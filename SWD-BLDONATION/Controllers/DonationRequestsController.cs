using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SWD_BLDONATION.DTOs.BloodComponentDTOs;
using SWD_BLDONATION.DTOs.BloodRequestDTOs;
using SWD_BLDONATION.DTOs.BloodTypeDTOs;
using SWD_BLDONATION.DTOs.DonationRequestDTOs;
using SWD_BLDONATION.DTOs.UserDTOs;
using SWD_BLDONATION.Models.Enums;
using SWD_BLDONATION.Models.Generated;
using SWD_BLDONATION.Provider;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SWD_BLDONATION.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonationRequestsController : ControllerBase
    {
        private readonly BloodDonationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<DonationRequestsController> _logger;

        public DonationRequestsController(BloodDonationDbContext context, IMapper mapper, ILogger<DonationRequestsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // Helper method to map byte? Status to string
        private static string? MapStatusToString(byte? status)
        {
            return status switch
            {
                0 => "Pending",
                1 => "Successful",
                2 => "Cancelled",
                _ => null
            };
        }

        // GET: api/DonationRequests
        [HttpGet]
        public async Task<ActionResult<object>> GetDonationRequests([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest(new { Message = "Invalid page or pageSize." });
            }

            var query = _context.DonationRequests
                .Include(dr => dr.BloodType)
                .Include(dr => dr.BloodComponent)
                .Include(dr => dr.User)
                .AsQueryable();

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var donationRequests = await query
                .OrderBy(dr => dr.Status == 0 ? 0 : dr.Status == 1 ? 1 : 2)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var donationRequestDtos = donationRequests.Select(dr => new DonationRequestDto
            {
                DonateRequestId = dr.DonateRequestId,
                UserId = dr.UserId,
                BloodTypeId = dr.BloodTypeId,
                BloodComponentId = dr.BloodComponentId,
                PreferredDate = dr.PreferredDate,
                StatusName = MapStatusToString(dr.Status),
                Status = dr.Status,
                Location = dr.Location,
                CreatedAt = dr.CreatedAt,
                Quantity = dr.Quantity,
                HeightCm = dr.HeightCm,
                WeightKg = dr.WeightKg,
                LastDonationDate = dr.LastDonationDate,
                HealthInfo = dr.HealthInfo,
                DateOfBirth = dr.User?.DateOfBirth ?? DateOnly.FromDateTime(DateTime.UtcNow),
                BloodType = dr.BloodType != null ? new BloodTypeDto
                {
                    BloodTypeId = dr.BloodType.BloodTypeId,
                    Name = dr.BloodType.Name,
                    RhFactor = dr.BloodType.RhFactor
                } : null,
                BloodComponent = dr.BloodComponent != null ? new BloodComponentDto
                {
                    BloodComponentId = dr.BloodComponent.BloodComponentId,
                    Name = dr.BloodComponent.Name
                } : null,
                User = dr.User != null ? new UserDto
                {
                    UserId = dr.User.UserId,
                    UserName = dr.User.UserName ?? "Unknown",
                    Name = dr.User.Name ?? "Unknown",
                    Email = dr.User.Email ?? "No Email Provided",
                    Phone = dr.User.Phone ?? "No Phone Provided",
                    DateOfBirth = dr.User.DateOfBirth ?? DateOnly.FromDateTime(DateTime.UtcNow),
                    Address = dr.User.Address ?? "No Address Provided",
                    Identification = dr.User.Identification ?? "No Identification Provided",
                    IsDeleted = dr.User.IsDeleted,
                    RoleBit = dr.User.RoleBit ?? 0,
                    StatusBit = dr.User.StatusBit ?? 0,
                    HeightCm = dr.User.HeightCm ?? 0,
                    WeightKg = dr.User.WeightKg ?? 0,
                    MedicalHistory = dr.User.MedicalHistory ?? "No Medical History Provided",
                    BloodTypeId = dr.User.BloodTypeId ?? dr.BloodTypeId,
                    BloodComponentId = dr.User.BloodComponentId ?? dr.BloodComponentId
                } : null
            }).ToList();

            return Ok(new
            {
                Data = donationRequestDtos,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            });
        }

        // GET: api/DonationRequests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DonationRequestDto>> GetDonationRequest(int id)
        {
            var donationRequest = await _context.DonationRequests
                .Include(dr => dr.BloodType)
                .Include(dr => dr.BloodComponent)
                .Include(dr => dr.User)
                .FirstOrDefaultAsync(dr => dr.DonateRequestId == id);

            if (donationRequest == null)
            {
                return NotFound(new { Message = "Donation request not found." });
            }

            var donationRequestDto = _mapper.Map<DonationRequestDto>(donationRequest);
            donationRequestDto.StatusName = MapStatusToString(donationRequest.Status);
            donationRequestDto.Status = donationRequest.Status;
            return Ok(donationRequestDto);
        }

        // PUT: api/DonationRequests/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDonationRequest(int id, [FromForm] UpdateDonationRequestDto dto)
        {
            _logger.LogInformation("PutDonationRequest called with id={Id}, data={@Dto}", id, dto);

            if (dto.DonateRequestId > 0 && id != dto.DonateRequestId)
            {
                _logger.LogWarning("ID mismatch: route id={Id}, DTO DonateRequestId={DonateRequestId}", id, dto.DonateRequestId);
                return BadRequest(new { Message = "DonateRequestId in the body must match the ID in the route." });
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid data provided for PutDonationRequest: id={Id}, errors={@Errors}", id, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return BadRequest(new
                {
                    Message = "Invalid data submitted.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var donationRequest = await _context.DonationRequests.FindAsync(id);
            if (donationRequest == null)
            {
                _logger.LogWarning("Donation request not found: id={Id}", id);
                return NotFound(new { Message = "Donation request not found." });
            }

            var originalUserId = donationRequest.UserId;
            var originalBloodTypeId = donationRequest.BloodTypeId;
            var originalBloodComponentId = donationRequest.BloodComponentId;
            var originalQuantity = donationRequest.Quantity;

            _mapper.Map(dto, donationRequest);

            var updatedFields = new List<string>();
            var skippedFields = new List<string>();

            if (dto.UserId.HasValue)
            {
                if (dto.UserId.Value > 0)
                {
                    donationRequest.UserId = dto.UserId.Value;
                    updatedFields.Add("UserId");
                }
                else
                {
                    donationRequest.UserId = originalUserId;
                    skippedFields.Add($"UserId (value: {dto.UserId.Value})");
                }
            }
            if (dto.BloodTypeId.HasValue)
            {
                if (dto.BloodTypeId.Value > 0)
                {
                    donationRequest.BloodTypeId = dto.BloodTypeId.Value;
                    updatedFields.Add("BloodTypeId");
                }
                else
                {
                    donationRequest.BloodTypeId = originalBloodTypeId;
                    skippedFields.Add($"BloodTypeId (value: {dto.BloodTypeId.Value})");
                }
            }
            if (dto.BloodComponentId.HasValue)
            {
                if (dto.BloodComponentId.Value > 0)
                {
                    donationRequest.BloodComponentId = dto.BloodComponentId.Value;
                    updatedFields.Add("BloodComponentId");
                }
                else
                {
                    donationRequest.BloodComponentId = originalBloodComponentId;
                    skippedFields.Add($"BloodComponentId (value: {dto.BloodComponentId.Value})");
                }
            }
            if (dto.Quantity.HasValue)
            {
                if (dto.Quantity.Value > 0)
                {
                    donationRequest.Quantity = dto.Quantity.Value;
                    updatedFields.Add("Quantity");
                }
                else
                {
                    donationRequest.Quantity = originalQuantity;
                    skippedFields.Add($"Quantity (value: {dto.Quantity.Value})");
                }
            }

            if (dto.DonateRequestId > 0) updatedFields.Add("DonateRequestId");
            if (dto.PreferredDate.HasValue) updatedFields.Add("PreferredDate");
            if (dto.Location != null) updatedFields.Add("Location");
            if (dto.Status.HasValue) updatedFields.Add("Status");
            if (dto.HeightCm.HasValue) updatedFields.Add("HeightCm");
            if (dto.WeightKg.HasValue) updatedFields.Add("WeightKg");
            if (dto.HealthInfo != null) updatedFields.Add("HealthInfo");
            if (dto.LastDonationDate.HasValue) updatedFields.Add("LastDonationDate");
            if (dto.Name != null) updatedFields.Add("Name");
            if (dto.DateOfBirth.HasValue) updatedFields.Add("DateOfBirth");

            _context.Entry(donationRequest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Donation request updated successfully: id={Id}", id);

                var updatedDonationRequest = await _context.DonationRequests
                    .AsNoTracking()
                    .FirstOrDefaultAsync(dr => dr.DonateRequestId == id);

                if (updatedDonationRequest == null)
                {
                    _logger.LogError("Updated donation request not found after save: id={Id}", id);
                    return StatusCode(500, new { Message = "Failed to verify updated donation request." });
                }

                _logger.LogInformation("Verified updated donation request id={Id}: UserId={UserId}, BloodTypeId={BloodTypeId}, BloodComponentId={BloodComponentId}, Quantity={Quantity}, Location={Location}, Status={Status}",
                    id, updatedDonationRequest.UserId, updatedDonationRequest.BloodTypeId, updatedDonationRequest.BloodComponentId, updatedDonationRequest.Quantity, updatedDonationRequest.Location, updatedDonationRequest.Status);

                if (updatedFields.Any())
                {
                    _logger.LogInformation("Fields updated for donation request id={Id}: {Fields}", id, string.Join(", ", updatedFields));
                }
                if (skippedFields.Any())
                {
                    _logger.LogInformation("Fields skipped (value 0) for donation request id={Id}: {Fields}", id, string.Join(", ", skippedFields));
                }
                if (!updatedFields.Any() && !skippedFields.Any())
                {
                    _logger.LogInformation("No fields provided for update for donation request id={Id}", id);
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!_context.DonationRequests.Any(dr => dr.DonateRequestId == id))
                {
                    _logger.LogWarning("Concurrency issue: Donation request no longer exists: id={Id}", id);
                    return NotFound(new { Message = "Donation request not found." });
                }
                _logger.LogError(ex, "Concurrency error updating donation request: id={Id}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating donation request: id={Id}", id);
                return StatusCode(500, new { Message = "An error occurred while updating the donation request." });
            }

            return Ok(new { Message = "Donation request updated successfully." });
        }

        // POST: api/DonationRequests
        [HttpPost]
        public async Task<ActionResult<DonationRequestDto>> PostDonationRequest([FromForm] CreateDonationRequestDto createDto)
        {
            _logger.LogInformation("PostDonationRequest called with data: {@CreateDto}", createDto);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("PostDonationRequest: Invalid data provided");
                return BadRequest(new
                {
                    Message = "Invalid data submitted.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var donationRequest = _mapper.Map<DonationRequest>(createDto);
            donationRequest.CreatedAt = DateTime.UtcNow;

            _context.DonationRequests.Add(donationRequest);

            try
            {
                await _context.SaveChangesAsync();

                if (donationRequest.UserId.HasValue)
                {
                    var notification = new Notification
                    {
                        UserId = donationRequest.UserId,
                        Message = $"Your donation request (ID: {donationRequest.DonateRequestId}) has been successfully created and is pending approval.",
                        Type = "DonationRequest",
                        Status = "Unread",
                        SentAt = VietnamDateTimeProvider.Now
                    };

                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Notification created for userId={UserId} for donation request id={DonateRequestId}", donationRequest.UserId, donationRequest.DonateRequestId);
                }
                else
                {
                    _logger.LogWarning("No UserId provided for notification creation for donation request id={DonateRequestId}", donationRequest.DonateRequestId);
                }

                var resultDto = _mapper.Map<DonationRequestDto>(donationRequest);
                resultDto.StatusName = MapStatusToString(donationRequest.Status);
                resultDto.Status = donationRequest.Status;

                return CreatedAtAction(nameof(GetDonationRequest), new { id = resultDto.DonateRequestId }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating donation request or notification");
                return StatusCode(500, new { Message = "An error occurred while processing the request." });
            }
        }


        // DELETE: api/DonationRequests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDonationRequest(int id)
        {
            var donationRequest = await _context.DonationRequests.FindAsync(id);
            if (donationRequest == null)
            {
                return NotFound(new { Message = "Donation request not found." });
            }

            _context.DonationRequests.Remove(donationRequest);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/DonationRequests/status
        [HttpPatch("status")]
        public async Task<IActionResult> UpdateDonationRequestStatus([FromBody] UpdateDonationRequestStatusDto dto)
        {
            _logger.LogInformation("UpdateDonationRequestStatus called with id={Id}, status={Status}", dto.Id, dto.Status);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid data provided for UpdateDonationRequestStatus: id={Id}", dto.Id);
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

            var donationRequest = await _context.DonationRequests.FindAsync(dto.Id);
            if (donationRequest == null)
            {
                _logger.LogWarning("Donation request not found: id={Id}", dto.Id);
                return NotFound(new { Message = "Donation request not found" });
            }

            donationRequest.Status = (byte)dto.Status;
            _context.Entry(donationRequest).State = EntityState.Modified;

            try
            {
                if (donationRequest.UserId.HasValue)
                {
                    string notificationMessage = null;

                    switch ((BloodRequestStatus)dto.Status)
                    {
                        case BloodRequestStatus.Successful:
                            notificationMessage = "Your donation request has been approved.";
                            break;
                        case BloodRequestStatus.Cancelled:
                            notificationMessage = "Your donation request has been cancelled.";
                            break;
                        case BloodRequestStatus.Pending:
                            notificationMessage = "Your donation request is pending review.";
                            break;
                    }

                    if (!string.IsNullOrEmpty(notificationMessage))
                    {
                        var notification = new Notification
                        {
                            UserId = donationRequest.UserId.Value,
                            Message = notificationMessage,
                            Type = "DonationRequest",
                            Status = "Unread",
                            SentAt = VietnamDateTimeProvider.Now
                        };

                        _context.Notifications.Add(notification);
                        _logger.LogInformation("Notification created for userId={UserId} with message='{Message}'",
                            donationRequest.UserId.Value, notificationMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating notification for donation request id={Id}", dto.Id);
                return StatusCode(500, new { Message = "An error occurred while processing the donation request." });
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Donation request status updated successfully: id={Id}, status={Status}", dto.Id, dto.Status);
            return Ok(new { Message = "Donation request status updated successfully." });
        }


        // GET: api/DonationRequests/search
        [HttpGet("search")]
        public async Task<ActionResult<object>> SearchDonationRequests(
     [FromQuery] int page = 1,
     [FromQuery] int pageSize = 10,
     [FromQuery] string? keyword = null,
     [FromQuery] int? bloodTypeId = null,
     [FromQuery] int? bloodComponentId = null,
     [FromQuery] byte? status = null)
        {
            _logger.LogInformation("SearchDonationRequests called with page={Page}, pageSize={PageSize}, keyword={Keyword}, bloodTypeId={BloodTypeId}, bloodComponentId={BloodComponentId}, status={Status}",
                page, pageSize, keyword, bloodTypeId, bloodComponentId, status);

            if (page < 1 || pageSize < 1)
            {
                _logger.LogWarning("Invalid page or pageSize: page={Page}, pageSize={PageSize}", page, pageSize);
                return BadRequest(new { Message = "Invalid page or pageSize." });
            }

            var query = _context.DonationRequests
                .Join(_context.BloodTypes,
                    dr => dr.BloodTypeId,
                    bt => bt.BloodTypeId,
                    (dr, bt) => new
                    {
                        DonationRequest = dr,
                        BloodType = bt
                    })
                .Join(_context.BloodComponents,
                    x => x.DonationRequest.BloodComponentId,
                    bc => bc.BloodComponentId,
                    (x, bc) => new
                    {
                        x.DonationRequest,
                        x.BloodType,
                        BloodComponent = bc
                    })
                .Join(_context.Users,
                    x => x.DonationRequest.UserId,
                    u => u.UserId,
                    (x, u) => new
                    {
                        x.DonationRequest,
                        x.BloodType,
                        x.BloodComponent,
                        User = u
                    });

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string lowerKeyword = keyword.Trim().ToLower();
                query = query.Where(x =>
                    (x.User.Name != null && x.User.Name.ToLower().Contains(lowerKeyword)) ||
                    (x.User.Phone != null && x.User.Phone.ToLower().Contains(lowerKeyword)) ||
                    (x.DonationRequest.Location != null && x.DonationRequest.Location.ToLower().Contains(lowerKeyword)));
            }

            if (bloodTypeId.HasValue)
            {
                query = query.Where(x => x.DonationRequest.BloodTypeId == bloodTypeId.Value);
            }

            if (bloodComponentId.HasValue)
            {
                query = query.Where(x => x.DonationRequest.BloodComponentId == bloodComponentId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.DonationRequest.Status == status.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var donationRequests = await query
                .OrderBy(x => x.DonationRequest.Status == (byte)DonationRequestStatus.Pending ? 0
                    : x.DonationRequest.Status == (byte)DonationRequestStatus.Successful ? 1
                    : 2)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DonationRequestDto
                {
                    DonateRequestId = x.DonationRequest.DonateRequestId,
                    UserId = x.DonationRequest.UserId,
                    BloodTypeId = x.DonationRequest.BloodTypeId,
                    BloodComponentId = x.DonationRequest.BloodComponentId,
                    PreferredDate = x.DonationRequest.PreferredDate,
                    Status = x.DonationRequest.Status,
                    StatusName = x.DonationRequest.Status.HasValue ? ((DonationRequestStatus)x.DonationRequest.Status.Value).ToString() : null,
                    Location = x.DonationRequest.Location,
                    CreatedAt = x.DonationRequest.CreatedAt,
                    Quantity = x.DonationRequest.Quantity,
                    HeightCm = x.DonationRequest.HeightCm,
                    WeightKg = x.DonationRequest.WeightKg,
                    LastDonationDate = x.DonationRequest.LastDonationDate,
                    HealthInfo = x.DonationRequest.HealthInfo,
                    DateOfBirth = x.User.DateOfBirth,
                    BloodType = x.BloodType != null ? new BloodTypeDto
                    {
                        BloodTypeId = x.BloodType.BloodTypeId,
                        Name = x.BloodType.Name,
                        RhFactor = x.BloodType.RhFactor
                    } : null,
                    BloodComponent = x.BloodComponent != null ? new BloodComponentDto
                    {
                        BloodComponentId = x.BloodComponent.BloodComponentId,
                        Name = x.BloodComponent.Name
                    } : null,
                    User = x.User != null ? new UserDto
                    {
                        UserId = x.User.UserId,
                        UserName = x.User.UserName,
                        Name = x.User.Name,
                        Email = x.User.Email,
                        Phone = x.User.Phone,
                        DateOfBirth = x.User.DateOfBirth,
                        Address = x.User.Address,
                        Identification = x.User.Identification,
                        IsDeleted = x.User.IsDeleted,
                        RoleBit = x.User.RoleBit.Value,
                        StatusBit = x.User.StatusBit,
                        HeightCm = x.User.HeightCm,
                        WeightKg = x.User.WeightKg,
                        MedicalHistory = x.User.MedicalHistory,
                        BloodTypeId = x.User.BloodTypeId,
                        BloodComponentId = x.User.BloodComponentId
                    } : null
                })
                .ToListAsync();


            return Ok(new
            {
                Message = "Retrieved donation requests successfully.",
                Data = new
                {
                    Requests = donationRequests,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize
                }
            });
        }

        // GET: api/DonationRequests/ByUser/search/{userId}
        [HttpGet("ByUser/search/{userId}")]
        public async Task<ActionResult<object>> SearchDonationRequestsByUser(
            int userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? bloodTypeId = null,
            [FromQuery] int? bloodComponentId = null,
            [FromQuery] string? status = null,
            [FromQuery] string? location = null,
            [FromQuery] DateOnly? preferredDate = null,
            [FromQuery] DateTime? createdAfter = null,
            [FromQuery] DateTime? createdBefore = null,
            [FromQuery] int? quantityMin = null,
            [FromQuery] int? quantityMax = null)
        {
            if (userId < 1 || page < 1 || pageSize < 1)
            {
                return BadRequest(new { Message = "Invalid userId, page, or pageSize." });
            }

            var query = _context.DonationRequests
                .Where(dr => dr.UserId == userId)
                .Include(dr => dr.BloodType)
                .Include(dr => dr.BloodComponent)
                .Include(dr => dr.User)
                .AsQueryable();

            // Apply search filters
            if (bloodTypeId.HasValue)
                query = query.Where(dr => dr.BloodTypeId == bloodTypeId.Value);
            if (bloodComponentId.HasValue)
                query = query.Where(dr => dr.BloodComponentId == bloodComponentId.Value);
            if (!string.IsNullOrWhiteSpace(status))
            {
                byte? statusByte = status.Trim().ToLowerInvariant() switch
                {
                    "pending" => 0,
                    "successful" => 1,
                    "cancelled" => 2,
                    _ => null
                };
                if (statusByte.HasValue)
                    query = query.Where(dr => dr.Status == statusByte.Value);
            }
            if (!string.IsNullOrWhiteSpace(location))
                query = query.Where(dr => dr.Location != null && dr.Location.Contains(location.Trim(), StringComparison.OrdinalIgnoreCase));
            if (preferredDate.HasValue)
                query = query.Where(dr => dr.PreferredDate == preferredDate.Value);
            if (createdAfter.HasValue)
                query = query.Where(dr => dr.CreatedAt >= createdAfter.Value);
            if (createdBefore.HasValue)
                query = query.Where(dr => dr.CreatedAt <= createdBefore.Value);
            if (quantityMin.HasValue)
                query = query.Where(dr => dr.Quantity >= quantityMin.Value);
            if (quantityMax.HasValue)
                query = query.Where(dr => dr.Quantity <= quantityMax.Value);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var donationRequests = await query
                .OrderBy(dr => dr.Status == 0 ? 0 : dr.Status == 1 ? 1 : 2)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var donationRequestDtos = donationRequests.Select(dr => new DonationRequestDto
            {
                DonateRequestId = dr.DonateRequestId,
                UserId = dr.UserId,
                BloodTypeId = dr.BloodTypeId,
                BloodComponentId = dr.BloodComponentId,
                PreferredDate = dr.PreferredDate,
                StatusName = MapStatusToString(dr.Status),
                Status = dr.Status,
                Location = dr.Location,
                CreatedAt = dr.CreatedAt,
                Quantity = dr.Quantity,
                HeightCm = dr.HeightCm,
                WeightKg = dr.WeightKg,
                LastDonationDate = dr.LastDonationDate,
                HealthInfo = dr.HealthInfo,
                BloodType = dr.BloodType != null ? new BloodTypeDto
                {
                    BloodTypeId = dr.BloodType.BloodTypeId,
                    Name = dr.BloodType.Name,
                    RhFactor = dr.BloodType.RhFactor
                } : null,
                BloodComponent = dr.BloodComponent != null ? new BloodComponentDto
                {
                    BloodComponentId = dr.BloodComponent.BloodComponentId,
                    Name = dr.BloodComponent.Name
                } : null,
                User = dr.User != null ? new UserDto
                {
                    UserId = dr.User.UserId,
                    UserName = dr.User.UserName ?? "Unknown",
                    Name = dr.User.Name ?? "Unknown",
                    Email = dr.User.Email ?? "No Email Provided",
                    Phone = dr.User.Phone ?? "No Phone Provided",
                    DateOfBirth = dr.User.DateOfBirth ?? DateOnly.FromDateTime(DateTime.UtcNow),
                    Address = dr.User.Address ?? "No Address Provided",
                    Identification = dr.User.Identification ?? "No Identification Provided",
                    IsDeleted = dr.User.IsDeleted,
                    RoleBit = dr.User.RoleBit ?? 0,
                    StatusBit = dr.User.StatusBit ?? 0,
                    HeightCm = dr.User.HeightCm ?? 0,
                    WeightKg = dr.User.WeightKg ?? 0,
                    MedicalHistory = dr.User.MedicalHistory ?? "No Medical History Provided",
                    BloodTypeId = dr.User.BloodTypeId ?? dr.BloodTypeId,
                    BloodComponentId = dr.User.BloodComponentId ?? dr.BloodComponentId
                } : null
            }).ToList();

            return Ok(new
            {
                Requests = donationRequestDtos,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            });
        }
    }
}