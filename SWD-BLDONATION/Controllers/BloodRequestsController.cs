﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD_BLDONATION.DTOs.BloodRequestDTOs;
using SWD_BLDONATION.Models.Generated;
using SWD_BLDONATION.Models.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SWD_BLDONATION.Provider;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SWD_BLDONATION.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BloodRequestsController : ControllerBase
    {
        private readonly BloodDonationDbContext _context;
        private readonly ILogger<BloodRequestsController> _logger;

        public BloodRequestsController(BloodDonationDbContext context, ILogger<BloodRequestsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/BloodRequests/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff,User")]
        public async Task<ActionResult<object>> GetBloodRequestById(int id)
        {
            _logger.LogInformation("GetBloodRequestById called with id={Id}", id);

            if (id < 1)
            {
                _logger.LogWarning("Invalid id: id={Id}", id);
                return BadRequest(new { Message = "Invalid blood request ID." });
            }

            var query = _context.BloodRequests
                .Where(br => br.BloodRequestId == id)
                .Join(_context.BloodTypes,
                    br => br.BloodTypeId,
                    bt => bt.BloodTypeId,
                    (br, bt) => new
                    {
                        BloodRequest = br,
                        BloodTypeName = bt.Name + bt.RhFactor
                    })
                .Join(_context.BloodComponents,
                    x => x.BloodRequest.BloodComponentId,
                    bc => bc.BloodComponentId,
                    (x, bc) => new
                    {
                        x.BloodRequest,
                        x.BloodTypeName,
                        BloodComponentName = bc.Name
                    });

            var request = await query
                .Select(x => new BloodRequestDto
                {
                    BloodRequestId = x.BloodRequest.BloodRequestId,
                    UserId = x.BloodRequest.UserId,
                    Name = x.BloodRequest.Name,
                    DateOfBirth = x.BloodRequest.DateOfBirth,
                    Phone = x.BloodRequest.Phone,
                    BloodTypeId = x.BloodRequest.BloodTypeId ?? 0,
                    BloodTypeName = x.BloodTypeName,
                    BloodComponentId = x.BloodRequest.BloodComponentId ?? 0,
                    BloodComponentName = x.BloodComponentName,
                    IsEmergency = x.BloodRequest.IsEmergency ?? false,
                    Status = new StatusDto
                    {
                        Id = x.BloodRequest.Status ?? (byte)BloodRequestStatus.Pending,
                        Name = x.BloodRequest.Status.HasValue ? ((BloodRequestStatus)x.BloodRequest.Status.Value).ToString() : BloodRequestStatus.Pending.ToString()
                    },
                    CreatedAt = x.BloodRequest.CreatedAt ?? DateTime.MinValue,
                    Location = x.BloodRequest.Location,
                    Quantity = x.BloodRequest.Quantity ?? 0,
                    Fulfilled = x.BloodRequest.Fulfilled ?? false,
                    FulfilledSource = x.BloodRequest.FulfilledSource,
                    HeightCm = x.BloodRequest.HeightCm ?? 0,
                    WeightKg = x.BloodRequest.WeightKg ?? 0,
                    HealthInfo = x.BloodRequest.HealthInfo
                })
                .FirstOrDefaultAsync();

            if (request == null)
            {
                _logger.LogWarning("Blood request not found: id={Id}", id);
                return NotFound(new { Message = "Blood request not found." });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role == nameof(UserRole.User) && int.TryParse(userIdClaim, out int userId))
            {
                if (request.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} tried to access unauthorized blood request {RequestId}", userId, id);
                    return Forbid();
                }
            }

            _logger.LogInformation("GetBloodRequestById returned blood request with id={Id}", id);

            return Ok(new
            {
                Message = "Retrieved blood request successfully.",
                Data = request
            });
        }


        // GET: api/BloodRequests
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<object>> GetBloodRequests([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("GetBloodRequests called with page={Page}, pageSize={PageSize}", page, pageSize);

            if (page < 1 || pageSize < 1)
            {
                _logger.LogWarning("Invalid page or pageSize: page={Page}, pageSize={PageSize}", page, pageSize);
                return BadRequest(new { Message = "Invalid page or pageSize." });
            }

            var query = _context.BloodRequests
                .Join(_context.BloodTypes,
                    br => br.BloodTypeId,
                    bt => bt.BloodTypeId,
                    (br, bt) => new
                    {
                        BloodRequest = br,
                        BloodTypeName = bt.Name + bt.RhFactor
                    })
                .Join(_context.BloodComponents,
                    x => x.BloodRequest.BloodComponentId,
                    bc => bc.BloodComponentId,
                    (x, bc) => new
                    {
                        x.BloodRequest,
                        x.BloodTypeName,
                        BloodComponentName = bc.Name
                    });

            var requests = await query
                .OrderBy(x => x.BloodRequest.Status == (byte)BloodRequestStatus.Pending ? 0
                    : x.BloodRequest.Status == (byte)BloodRequestStatus.Successful ? 1
                    : 2)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BloodRequestDto
                {
                    BloodRequestId = x.BloodRequest.BloodRequestId,
                    UserId = x.BloodRequest.UserId,
                    Name = x.BloodRequest.Name,
                    DateOfBirth = x.BloodRequest.DateOfBirth,
                    Phone = x.BloodRequest.Phone,
                    BloodTypeId = x.BloodRequest.BloodTypeId ?? 0,
                    BloodTypeName = x.BloodTypeName,
                    BloodComponentId = x.BloodRequest.BloodComponentId ?? 0,
                    BloodComponentName = x.BloodComponentName,
                    IsEmergency = x.BloodRequest.IsEmergency.HasValue ? x.BloodRequest.IsEmergency.Value : false,
                    Status = new StatusDto
                    {
                        Id = x.BloodRequest.Status ?? (byte)BloodRequestStatus.Pending,
                        Name = x.BloodRequest.Status.HasValue ? ((BloodRequestStatus)x.BloodRequest.Status.Value).ToString() : BloodRequestStatus.Pending.ToString()
                    },
                    CreatedAt = x.BloodRequest.CreatedAt.HasValue ? x.BloodRequest.CreatedAt.Value : DateTime.MinValue,
                    Location = x.BloodRequest.Location,
                    Quantity = x.BloodRequest.Quantity.HasValue ? (int)x.BloodRequest.Quantity.Value : 0,
                    Fulfilled = x.BloodRequest.Fulfilled.HasValue ? x.BloodRequest.Fulfilled.Value : false,
                    FulfilledSource = x.BloodRequest.FulfilledSource,
                    HeightCm = x.BloodRequest.HeightCm.HasValue ? (int)x.BloodRequest.HeightCm.Value : 0,
                    WeightKg = x.BloodRequest.WeightKg.HasValue ? (int)x.BloodRequest.WeightKg.Value : 0,
                    HealthInfo = x.BloodRequest.HealthInfo
                })
                .ToListAsync();

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            _logger.LogInformation("GetBloodRequests returned {Count} items", requests.Count);

            return Ok(new
            {
                Message = "Retrieved blood requests successfully.",
                Data = new
                {
                    Requests = requests,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize
                }
            });
        }

        // GET: api/BloodRequests/search
        [HttpGet("search")]
        [Authorize(Roles = "Admin,Staff,User")]
        public async Task<ActionResult<object>> SearchBloodRequests(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] int? bloodTypeId = null,
            [FromQuery] int? bloodComponentId = null,
            [FromQuery] bool? isEmergency = null,
            [FromQuery] byte? status = null)
        {
            _logger.LogInformation("SearchBloodRequests called with page={Page}, pageSize={PageSize}, keyword={Keyword}, bloodTypeId={BloodTypeId}, bloodComponentId={BloodComponentId}, isEmergency={IsEmergency}, status={Status}",
                page, pageSize, keyword, bloodTypeId, bloodComponentId, isEmergency, status);

            if (page < 1 || pageSize < 1)
            {
                _logger.LogWarning("Invalid page or pageSize: page={Page}, pageSize={PageSize}", page, pageSize);
                return BadRequest(new { Message = "Invalid page or pageSize." });
            }

            var query = _context.BloodRequests
                .Join(_context.BloodTypes,
                    br => br.BloodTypeId,
                    bt => bt.BloodTypeId,
                    (br, bt) => new
                    {
                        BloodRequest = br,
                        BloodTypeName = bt.Name + bt.RhFactor
                    })
                .Join(_context.BloodComponents,
                    x => x.BloodRequest.BloodComponentId,
                    bc => bc.BloodComponentId,
                    (x, bc) => new
                    {
                        x.BloodRequest,
                        x.BloodTypeName,
                        BloodComponentName = bc.Name
                    });

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string lowerKeyword = keyword.Trim().ToLower();
                query = query.Where(x =>
                    (x.BloodRequest.Name != null && x.BloodRequest.Name.ToLower().Contains(lowerKeyword)) ||
                    (x.BloodRequest.Phone != null && x.BloodRequest.Phone.ToLower().Contains(lowerKeyword)) ||
                    (x.BloodRequest.Location != null && x.BloodRequest.Location.ToLower().Contains(lowerKeyword)));
            }

            if (bloodTypeId.HasValue)
            {
                query = query.Where(x => x.BloodRequest.BloodTypeId == bloodTypeId.Value);
            }

            if (bloodComponentId.HasValue)
            {
                query = query.Where(x => x.BloodRequest.BloodComponentId == bloodComponentId.Value);
            }

            if (isEmergency.HasValue)
            {
                query = query.Where(x => x.BloodRequest.IsEmergency == isEmergency.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.BloodRequest.Status == status.Value);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role == nameof(UserRole.User) && int.TryParse(userIdClaim, out int userId))
            {
                query = query.Where(x => x.BloodRequest.UserId == userId);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var requests = await query
                .OrderBy(x => x.BloodRequest.Status == (byte)BloodRequestStatus.Pending ? 0
                    : x.BloodRequest.Status == (byte)BloodRequestStatus.Successful ? 1
                    : 2)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BloodRequestDto
                {
                    BloodRequestId = x.BloodRequest.BloodRequestId,
                    UserId = x.BloodRequest.UserId,
                    Name = x.BloodRequest.Name,
                    DateOfBirth = x.BloodRequest.DateOfBirth,
                    Phone = x.BloodRequest.Phone,
                    BloodTypeId = x.BloodRequest.BloodTypeId ?? 0,
                    BloodTypeName = x.BloodTypeName,
                    BloodComponentId = x.BloodRequest.BloodComponentId ?? 0,
                    BloodComponentName = x.BloodComponentName,
                    IsEmergency = x.BloodRequest.IsEmergency.HasValue ? x.BloodRequest.IsEmergency.Value : false,
                    Status = new StatusDto
                    {
                        Id = x.BloodRequest.Status ?? (byte)BloodRequestStatus.Pending,
                        Name = x.BloodRequest.Status.HasValue ? ((BloodRequestStatus)x.BloodRequest.Status.Value).ToString() : BloodRequestStatus.Pending.ToString()
                    },
                    CreatedAt = x.BloodRequest.CreatedAt.HasValue ? x.BloodRequest.CreatedAt.Value : DateTime.MinValue,
                    Location = x.BloodRequest.Location,
                    Quantity = x.BloodRequest.Quantity.HasValue ? (int)x.BloodRequest.Quantity.Value : 0,
                    Fulfilled = x.BloodRequest.Fulfilled.HasValue ? x.BloodRequest.Fulfilled.Value : false,
                    FulfilledSource = x.BloodRequest.FulfilledSource,
                    HeightCm = x.BloodRequest.HeightCm.HasValue ? (int)x.BloodRequest.HeightCm.Value : 0,
                    WeightKg = x.BloodRequest.WeightKg.HasValue ? (int)x.BloodRequest.WeightKg.Value : 0,
                    HealthInfo = x.BloodRequest.HealthInfo
                })
                .ToListAsync();

            _logger.LogInformation("SearchBloodRequests returned {Count} items", requests.Count);

            return Ok(new
            {
                Message = "Retrieved blood requests successfully.",
                Data = new
                {
                    Requests = requests,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize
                }
            });
        }

        // POST: api/BloodRequests
        [HttpPost]
        [Authorize(Roles = "Admin,Staff,User")]
        public async Task<ActionResult<object>> PostBloodRequest([FromForm] CreateBloodRequestDto dto)
        {
            _logger.LogInformation("PostBloodRequest called with data: {@CreateDto}", dto);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("PostBloodRequest: Invalid data provided");
                return BadRequest(new { message = "Invalid data submitted.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            var bloodRequest = new BloodRequest
            {
                UserId = dto.UserId,
                BloodTypeId = dto.BloodTypeId,
                BloodComponentId = dto.BloodComponentId,
                IsEmergency = dto.IsEmergency,
                Status = (byte)BloodRequestStatus.Pending,
                CreatedAt = VietnamDateTimeProvider.Now,
                Location = dto.Location,
                Quantity = dto.Quantity,
                Fulfilled = false,
                HealthInfo = dto.HealthInfo,
                Name = dto.Name,
                Phone = dto.Phone,
                DateOfBirth = dto.DateOfBirth,
                HeightCm = dto.HeightCm,
                WeightKg = dto.WeightKg
            };

            _context.BloodRequests.Add(bloodRequest);

            try
            {
                await _context.SaveChangesAsync();

                if (dto.UserId.HasValue)
                {
                    var notification = new Notification
                    {
                        UserId = dto.UserId,
                        Message = $"Your blood request (ID: {bloodRequest.BloodRequestId}) has been successfully created and is pending approval.",
                        Type = "BloodRequest",
                        Status = "Unread",
                        SentAt = VietnamDateTimeProvider.Now
                    };

                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Notification created for userId={UserId} for blood request id={BloodRequestId}", dto.UserId, bloodRequest.BloodRequestId);
                }
                else
                {
                    _logger.LogWarning("No UserId provided for notification creation for blood request id={BloodRequestId}", bloodRequest.BloodRequestId);
                }

                return CreatedAtAction(nameof(GetBloodRequestById), new { id = bloodRequest.BloodRequestId }, bloodRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating blood request or notification");
                return StatusCode(500, new { message = "An error occurred while processing the request." });
            }
        }

        // PUT: api/BloodRequests/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> PutBloodRequest(int id, [FromForm] UpdateBloodRequestDto dto)
        {
            _logger.LogInformation("PutBloodRequest called with id={Id}, data={@Dto}", id, dto);

            if (dto.BloodRequestId.HasValue && id != dto.BloodRequestId)
            {
                return BadRequest(new { message = "BloodRequestId in the body must match the ID in the route." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Invalid data submitted.",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var bloodRequest = await _context.BloodRequests.FindAsync(id);
            if (bloodRequest == null)
            {
                return NotFound(new { message = "Blood request not found" });
            }

            if (dto.Status.HasValue && !Enum.IsDefined(typeof(BloodRequestStatus), dto.Status.Value))
            {
                return BadRequest(new { message = "Invalid status value." });
            }

            var updatedFields = new List<string>();
            var skippedFields = new List<string>();

            if (dto.BloodRequestId.HasValue)
            {
                if (dto.BloodRequestId.Value > 0)
                {
                    bloodRequest.BloodRequestId = dto.BloodRequestId.Value;
                    updatedFields.Add("BloodRequestId");
                }
                else
                {
                    skippedFields.Add($"BloodRequestId (value: {dto.BloodRequestId.Value})");
                }
            }
            if (dto.UserId.HasValue)
            {
                if (dto.UserId.Value > 0)
                {
                    bloodRequest.UserId = dto.UserId.Value;
                    updatedFields.Add("UserId");
                }
                else
                {
                    skippedFields.Add($"UserId (value: {dto.UserId.Value})");
                }
            }
            if (dto.BloodTypeId.HasValue)
            {
                if (dto.BloodTypeId.Value > 0)
                {
                    bloodRequest.BloodTypeId = dto.BloodTypeId.Value;
                    updatedFields.Add("BloodTypeId");
                }
                else
                {
                    skippedFields.Add($"BloodTypeId (value: {dto.BloodTypeId.Value})");
                }
            }
            if (dto.BloodComponentId.HasValue)
            {
                if (dto.BloodComponentId.Value > 0)
                {
                    bloodRequest.BloodComponentId = dto.BloodComponentId.Value;
                    updatedFields.Add("BloodComponentId");
                }
                else
                {
                    skippedFields.Add($"BloodComponentId (value: {dto.BloodComponentId.Value})");
                }
            }
            if (dto.Quantity.HasValue)
            {
                if (dto.Quantity.Value > 0)
                {
                    bloodRequest.Quantity = dto.Quantity.Value;
                    updatedFields.Add("Quantity");
                }
                else
                {
                    skippedFields.Add($"Quantity (value: {dto.Quantity.Value})");
                }
            }

            if (dto.Name != null)
            {
                bloodRequest.Name = dto.Name;
                updatedFields.Add("Name");
            }
            if (dto.DateOfBirth.HasValue)
            {
                bloodRequest.DateOfBirth = dto.DateOfBirth;
                updatedFields.Add("DateOfBirth");
            }
            if (dto.Phone != null)
            {
                bloodRequest.Phone = dto.Phone;
                updatedFields.Add("Phone");
            }
            if (dto.IsEmergency.HasValue)
            {
                bloodRequest.IsEmergency = dto.IsEmergency;
                updatedFields.Add("IsEmergency");
            }
            if (dto.Status.HasValue)
            {
                bloodRequest.Status = (byte)dto.Status.Value;
                updatedFields.Add("Status");
            }
            if (dto.Location != null)
            {
                bloodRequest.Location = dto.Location;
                updatedFields.Add("Location");
            }
            if (dto.Fulfilled.HasValue)
            {
                bloodRequest.Fulfilled = dto.Fulfilled;
                updatedFields.Add("Fulfilled");
            }
            if (dto.FulfilledSource != null)
            {
                bloodRequest.FulfilledSource = dto.FulfilledSource;
                updatedFields.Add("FulfilledSource");
            }
            if (dto.HeightCm.HasValue)
            {
                bloodRequest.HeightCm = dto.HeightCm;
                updatedFields.Add("HeightCm");
            }
            if (dto.WeightKg.HasValue)
            {
                bloodRequest.WeightKg = dto.WeightKg;
                updatedFields.Add("WeightKg");
            }
            if (dto.HealthInfo != null)
            {
                bloodRequest.HealthInfo = dto.HealthInfo;
                updatedFields.Add("HealthInfo");
            }

            // Log updated and skipped fields
            if (updatedFields.Any())
            {
                _logger.LogInformation("Fields updated for blood request id={Id}: {Fields}", id, string.Join(", ", updatedFields));
            }
            if (skippedFields.Any())
            {
                _logger.LogInformation("Fields skipped (value 0) for blood request id={Id}: {Fields}", id, string.Join(", ", skippedFields));
            }
            if (!updatedFields.Any() && !skippedFields.Any())
            {
                _logger.LogInformation("No fields provided for update for blood request id={Id}", id);
            }

            // Mark the entity as modified
            _context.Entry(bloodRequest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!_context.BloodRequests.Any(br => br.BloodRequestId == id))
                {
                    return NotFound(new { message = "Blood request not found." });
                }
                throw;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the blood request." });
            }

            return Ok(new { message = "Blood request updated successfully." });
        }

        [HttpPatch("status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBloodRequestStatus([FromBody] UpdateBloodRequestStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid data submitted.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            if (!Enum.IsDefined(typeof(BloodRequestStatus), dto.Status))
            {
                return BadRequest(new { message = "Invalid status value." });
            }

            var bloodRequest = await _context.BloodRequests.FindAsync(dto.Id);
            if (bloodRequest == null)
            {
                return NotFound(new { message = "Blood request not found" });
            }

            bloodRequest.Status = dto.Status;
            _context.Entry(bloodRequest).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Blood request status updated successfully." });
        }

        // DELETE: api/BloodRequests/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> DeleteBloodRequest(int id)
        {
            var bloodRequest = await _context.BloodRequests.FindAsync(id);
            if (bloodRequest == null)
            {
                return NotFound(new { message = "Blood request not found" });
            }

            bloodRequest.Status = (byte)BloodRequestStatus.Cancelled;
            bloodRequest.Fulfilled = true;

            _context.Entry(bloodRequest).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Blood request soft deleted: ID = {Id}, Status = Cancelled", id);

            return Ok(new { message = "Blood request deleted successfully." });
        }

        // GET: api/BloodRequests/ByUser/search/{userId}
        [HttpGet("ByUser/search/{userId}")]
        [Authorize(Roles = "Admin,Staff,User")]
        public async Task<ActionResult<object>> SearchBloodRequestsByUser(
            int userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] int? bloodTypeId = null,
            [FromQuery] int? bloodComponentId = null,
            [FromQuery] bool? isEmergency = null,
            [FromQuery] byte? status = null)
        {
            _logger.LogInformation("SearchBloodRequestsByUser called with userId={UserId}, page={Page}, pageSize={PageSize}, keyword={Keyword}, bloodTypeId={BloodTypeId}, bloodComponentId={BloodComponentId}, isEmergency={IsEmergency}, status={Status}",
                userId, page, pageSize, keyword, bloodTypeId, bloodComponentId, isEmergency, status);

            if (userId < 1 || page < 1 || pageSize < 1)
            {
                _logger.LogWarning("Invalid userId, page, or pageSize: userId={UserId}, page={Page}, pageSize={PageSize}", userId, page, pageSize);
                return BadRequest(new { Message = "Invalid userId, page, or pageSize." });
            }

            var query = _context.BloodRequests
                .Where(br => br.UserId == userId)
                .Join(_context.BloodTypes,
                    br => br.BloodTypeId,
                    bt => bt.BloodTypeId,
                    (br, bt) => new
                    {
                        BloodRequest = br,
                        BloodTypeName = bt.Name + bt.RhFactor
                    })
                .Join(_context.BloodComponents,
                    x => x.BloodRequest.BloodComponentId,
                    bc => bc.BloodComponentId,
                    (x, bc) => new
                    {
                        x.BloodRequest,
                        x.BloodTypeName,
                        BloodComponentName = bc.Name
                    });

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string lowerKeyword = keyword.Trim().ToLower();
                query = query.Where(x =>
                    (x.BloodRequest.Name != null && x.BloodRequest.Name.ToLower().Contains(lowerKeyword)) ||
                    (x.BloodRequest.Phone != null && x.BloodRequest.Phone.ToLower().Contains(lowerKeyword)) ||
                    (x.BloodRequest.Location != null && x.BloodRequest.Location.ToLower().Contains(lowerKeyword)));
            }

            if (bloodTypeId.HasValue)
            {
                query = query.Where(x => x.BloodRequest.BloodTypeId == bloodTypeId.Value);
            }

            if (bloodComponentId.HasValue)
            {
                query = query.Where(x => x.BloodRequest.BloodComponentId == bloodComponentId.Value);
            }

            if (isEmergency.HasValue)
            {
                query = query.Where(x => x.BloodRequest.IsEmergency == isEmergency.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.BloodRequest.Status == status.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var requests = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BloodRequestDto
                {
                    BloodRequestId = x.BloodRequest.BloodRequestId,
                    UserId = x.BloodRequest.UserId,
                    Name = x.BloodRequest.Name,
                    DateOfBirth = x.BloodRequest.DateOfBirth,
                    Phone = x.BloodRequest.Phone,
                    BloodTypeId = x.BloodRequest.BloodTypeId ?? 0,
                    BloodTypeName = x.BloodTypeName,
                    BloodComponentId = x.BloodRequest.BloodComponentId ?? 0,
                    BloodComponentName = x.BloodComponentName,
                    IsEmergency = x.BloodRequest.IsEmergency ?? false,
                    Status = new StatusDto
                    {
                        Id = x.BloodRequest.Status ?? (byte)BloodRequestStatus.Pending,
                        Name = x.BloodRequest.Status.HasValue ? ((BloodRequestStatus)x.BloodRequest.Status.Value).ToString() : BloodRequestStatus.Pending.ToString()
                    },
                    CreatedAt = x.BloodRequest.CreatedAt ?? DateTime.MinValue,
                    Location = x.BloodRequest.Location,
                    Quantity = x.BloodRequest.Quantity ?? 0,
                    Fulfilled = x.BloodRequest.Fulfilled ?? false,
                    FulfilledSource = x.BloodRequest.FulfilledSource,
                    HeightCm = x.BloodRequest.HeightCm ?? 0,
                    WeightKg = x.BloodRequest.WeightKg ?? 0,
                    HealthInfo = x.BloodRequest.HealthInfo
                })
                .ToListAsync();

            _logger.LogInformation("SearchBloodRequestsByUser returned {Count} items for userId={UserId}", requests.Count, userId);

            return Ok(new
            {
                Message = $"Retrieved blood requests for userId={userId} successfully.",
                Data = new
                {
                    Requests = requests,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize
                }
            });
        }


        [HttpGet("search/location")]
        [Authorize(Roles = "Admin,Staff,User")]
        public async Task<ActionResult<object>> SearchByLocation(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? keyword = null,
    [FromQuery] int? bloodTypeId = null,
    [FromQuery] int? bloodComponentId = null,
    [FromQuery] bool? isEmergency = null,
    [FromQuery] byte? status = null,
    [FromQuery] string? location = null)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest(new { Message = "Invalid page or pageSize." });
            }

            var query = _context.BloodRequests
                .Join(_context.BloodTypes,
                    br => br.BloodTypeId,
                    bt => bt.BloodTypeId,
                    (br, bt) => new
                    {
                        BloodRequest = br,
                        BloodTypeName = bt.Name + bt.RhFactor
                    })
                .Join(_context.BloodComponents,
                    x => x.BloodRequest.BloodComponentId,
                    bc => bc.BloodComponentId,
                    (x, bc) => new
                    {
                        x.BloodRequest,
                        x.BloodTypeName,
                        BloodComponentName = bc.Name
                    });

            // Keyword search
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string lowerKeyword = keyword.Trim().ToLower();
                query = query.Where(x =>
                    (x.BloodRequest.Name != null && x.BloodRequest.Name.ToLower().Contains(lowerKeyword)) ||
                    (x.BloodRequest.Phone != null && x.BloodRequest.Phone.ToLower().Contains(lowerKeyword)) ||
                    (x.BloodRequest.Location != null && x.BloodRequest.Location.ToLower().Contains(lowerKeyword)));
            }

            // Optional filters
            if (bloodTypeId.HasValue)
                query = query.Where(x => x.BloodRequest.BloodTypeId == bloodTypeId.Value);

            if (bloodComponentId.HasValue)
                query = query.Where(x => x.BloodRequest.BloodComponentId == bloodComponentId.Value);

            if (isEmergency.HasValue)
                query = query.Where(x => x.BloodRequest.IsEmergency == isEmergency.Value);

            if (status.HasValue)
                query = query.Where(x => x.BloodRequest.Status == status.Value);

            var allData = await query.ToListAsync();

            // Location matching logic (client-side)
            string[] parts = location?.Trim().ToLower().Split('_', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
            string province = parts.ElementAtOrDefault(0);
            string district = parts.ElementAtOrDefault(1);
            string ward = parts.ElementAtOrDefault(2);

            var filtered = allData.Select(x =>
            {
                string loc = x.BloodRequest.Location?.ToLower() ?? "";
                int priority = 0;
                if (!string.IsNullOrEmpty(province) && loc.Contains(province)) priority++;
                if (!string.IsNullOrEmpty(district) && loc.Contains(district)) priority++;
                if (!string.IsNullOrEmpty(ward) && loc.Contains(ward)) priority++;

                return new
                {
                    x.BloodRequest,
                    x.BloodTypeName,
                    x.BloodComponentName,
                    LocationPriority = priority
                };
            })
            .Where(x => x.LocationPriority > 0)
            .OrderByDescending(x => x.LocationPriority)
            .ThenByDescending(x => x.BloodRequest.CreatedAt);

            int totalCount = filtered.Count();
            int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var paged = filtered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BloodRequestDto
                {
                    BloodRequestId = x.BloodRequest.BloodRequestId,
                    UserId = x.BloodRequest.UserId,
                    Name = x.BloodRequest.Name,
                    DateOfBirth = x.BloodRequest.DateOfBirth,
                    Phone = x.BloodRequest.Phone,
                    BloodTypeId = x.BloodRequest.BloodTypeId ?? 0,
                    BloodTypeName = x.BloodTypeName,
                    BloodComponentId = x.BloodRequest.BloodComponentId ?? 0,
                    BloodComponentName = x.BloodComponentName,
                    IsEmergency = x.BloodRequest.IsEmergency ?? false,
                    Status = new StatusDto
                    {
                        Id = x.BloodRequest.Status ?? (byte)BloodRequestStatus.Pending,
                        Name = x.BloodRequest.Status.HasValue ?
                            ((BloodRequestStatus)x.BloodRequest.Status.Value).ToString() :
                            BloodRequestStatus.Pending.ToString()
                    },
                    CreatedAt = x.BloodRequest.CreatedAt ?? DateTime.MinValue,
                    Location = x.BloodRequest.Location,
                    Quantity = x.BloodRequest.Quantity ?? 0,
                    Fulfilled = x.BloodRequest.Fulfilled ?? false,
                    FulfilledSource = x.BloodRequest.FulfilledSource,
                    HeightCm = x.BloodRequest.HeightCm ?? 0,
                    WeightKg = x.BloodRequest.WeightKg ?? 0,
                    HealthInfo = x.BloodRequest.HealthInfo
                })
                .ToList();

            return Ok(new
            {
                Message = "Retrieved blood requests by location successfully.",
                Data = new
                {
                    Requests = paged,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize
                }
            });
        }



        [HttpPatch("status/staff")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateBloodRequestFromStaffStatus([FromBody] UpdateBloodRequestStatusDto dto)
        {
            _logger.LogInformation("UpdateBloodRequestStatus called with id={Id}, status={Status}", dto.Id, dto.Status);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid data provided for UpdateBloodRequestStatus: id={Id}", dto.Id);
                return BadRequest(new
                {
                    Message = "Invalid data submitted.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            if (!Enum.IsDefined(typeof(BloodRequestStatus), dto.Status))
            {
                _logger.LogWarning("Invalid status value: id={Id}, status={Status}", dto.Id, dto.Status);
                return BadRequest(new { Message = "Invalid status value." });
            }

            var bloodRequest = await _context.BloodRequests
                .FirstOrDefaultAsync(br => br.BloodRequestId == dto.Id);

            if (bloodRequest == null)
            {
                _logger.LogWarning("Blood request not found: id={Id}", dto.Id);
                return NotFound(new { Message = "Blood request not found" });
            }

            bloodRequest.Status = dto.Status;
            _context.Entry(bloodRequest).State = EntityState.Modified;

            try
            {
                if (bloodRequest.UserId.HasValue)
                {
                    if ((BloodRequestStatus)dto.Status == BloodRequestStatus.Successful)
                    {
                        var approvalNotification = new Notification
                        {
                            UserId = bloodRequest.UserId.Value,
                            Message = "Your blood request has been approved and will be fulfilled from our inventory. Our medical staff will contact you shortly with further instructions.",
                            Type = "BloodRequest",
                            Status = "Unread",
                            SentAt = VietnamDateTimeProvider.Now
                        };
                        _context.Notifications.Add(approvalNotification);
                        _logger.LogInformation("Approval notification created for userId={UserId} for blood request id={BloodRequestId}", bloodRequest.UserId, bloodRequest.BloodRequestId);

                        await ProcessBloodRequestFulfillment(bloodRequest);
                    }
                    else if ((BloodRequestStatus)dto.Status == BloodRequestStatus.Cancelled)
                    {
                        var cancellationNotification = new Notification
                        {
                            UserId = bloodRequest.UserId.Value,
                            Message = "Your blood request has been cancelled. If you believe this is a mistake or your situation has changed, please contact our support team.",
                            Type = "BloodRequest",
                            Status = "Unread",
                            SentAt = VietnamDateTimeProvider.Now
                        };
                        _context.Notifications.Add(cancellationNotification);
                        _logger.LogInformation("Cancellation notification created for userId={UserId} for blood request id={BloodRequestId}", bloodRequest.UserId, bloodRequest.BloodRequestId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing blood request status change: id={Id}", dto.Id);
                return StatusCode(500, new { Message = "An error occurred while processing the blood request." });
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Blood request status updated successfully: id={Id}, status={Status}", dto.Id, dto.Status);
            return Ok(new { Message = "Blood request status updated successfully." });
        }


        private async Task ProcessBloodRequestFulfillment(BloodRequest bloodRequest)
        {
            _logger.LogInformation("🔍 Processing blood request fulfillment: id={Id}", bloodRequest.BloodRequestId);

            if (bloodRequest.Fulfilled == true)
                return;

            string requestLocation = bloodRequest.Location?.ToLower()?.Trim() ?? "";

            var candidateInventories = await _context.BloodInventories
                .Where(inv =>
                    inv.BloodTypeId == bloodRequest.BloodTypeId &&
                    inv.BloodComponentId == bloodRequest.BloodComponentId &&
                    inv.Quantity >= bloodRequest.Quantity)
                .ToListAsync();

            if (candidateInventories.Count == 0)
            {
                await TryMatchDonationRequests(bloodRequest);
                return;
            }

            var bestInventory = candidateInventories
                .Select(inv =>
                {
                    int locationScore = 0;
                    string invLoc = inv.InventoryLocation?.ToLower()?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(requestLocation))
                    {
                        var parts = requestLocation.Split('_');
                        foreach (var part in parts)
                        {
                            if (!string.IsNullOrEmpty(part) && invLoc.Contains(part.Trim()))
                                locationScore++;
                        }
                    }
                    return new { Inventory = inv, Score = locationScore };
                })
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Inventory.LastUpdated)
                .FirstOrDefault()?.Inventory;

            if (bestInventory != null)
            {
                bestInventory.Quantity -= bloodRequest.Quantity;
                bestInventory.LastUpdated = DateTime.UtcNow;
                bloodRequest.Fulfilled = true;
                bloodRequest.FulfilledSource = $"Inventory";

                var bloodRequestInventory = new BloodRequestInventory
                {
                    BloodRequestId = bloodRequest.BloodRequestId,
                    InventoryId = bestInventory.InventoryId,
                    QuantityUnit = bloodRequest.Quantity,
                    QuantityAllocated = bloodRequest.Quantity,
                    AllocatedAt = DateTime.UtcNow,
                    AllocatedBy = null
                };
                _context.BloodRequestInventories.Add(bloodRequestInventory);

                _context.Entry(bestInventory).State = EntityState.Modified;
                _context.Entry(bloodRequest).State = EntityState.Modified;

                if (bloodRequest.UserId.HasValue)
                {
                    var fulfillmentNotification = new Notification
                    {
                        UserId = bloodRequest.UserId.Value,
                        Message = "Your blood request has been fulfilled from a nearby inventory. Our medical team will reach out to arrange delivery or collection.",
                        Type = "BloodRequest",
                        Status = "Unread",
                        SentAt = VietnamDateTimeProvider.Now
                    };
                    _context.Notifications.Add(fulfillmentNotification);
                }

                _logger.LogInformation(" Fulfilled blood request from inventory id={InventoryId}", bestInventory.InventoryId);
            }
            else
            {
                await TryMatchDonationRequests(bloodRequest);
            }
        }

        private async Task TryMatchDonationRequests(BloodRequest bloodRequest)
        {
            var potentialDonations = await _context.DonationRequests
                .Where(dr =>
                    dr.BloodTypeId == bloodRequest.BloodTypeId &&
                    dr.BloodComponentId == bloodRequest.BloodComponentId &&
                    dr.Status == 1)
                .ToListAsync();

            if (potentialDonations.Any())
            {
                foreach (var donation in potentialDonations)
                {
                    var match = new RequestMatch
                    {
                        BloodRequestId = bloodRequest.BloodRequestId,
                        DonationRequestId = donation.DonateRequestId,
                        MatchStatus = "Pending",
                        ScheduledDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                        Notes = "Auto-matched for blood need",
                        Type = "donation_to_request"
                    };
                    _context.RequestMatches.Add(match);
                }

                if (bloodRequest.UserId.HasValue)
                {
                    var message = $"Your blood request is being processed.\nCurrently, we do not have a suitable blood unit available in the storage. However, the system has found {potentialDonations.Count} matching donors, and invitations have been sent to them.\nWe are now waiting for a donor to arrive at the center for confirmation and procedures.\nYou will be notified as soon as someone confirms.\nThank you for your patience!";

                    var matchingNotification = new Notification
                    {
                        UserId = bloodRequest.UserId.Value,
                        Message = message,
                        Type = "BloodRequest",
                        Status = "Unread",
                        SentAt = VietnamDateTimeProvider.Now
                    };
                    _context.Notifications.Add(matchingNotification);
                }

                _logger.LogInformation("🧪 Created {Count} donation request matches for blood request id={Id}", potentialDonations.Count, bloodRequest.BloodRequestId);
            }
            else
            {
                _logger.LogWarning("⚠️ No matching donation requests found for blood request id={Id}", bloodRequest.BloodRequestId);
            }
        }
    }
}

