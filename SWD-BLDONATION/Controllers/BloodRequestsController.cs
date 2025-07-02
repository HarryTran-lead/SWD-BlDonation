using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD_BLDONATION.DTOs.BloodRequestDTOs;
using SWD_BLDONATION.Models.Generated;
using SWD_BLDONATION.Models.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                .GroupJoin(_context.Users,
                    br => br.UserId,
                    u => u.UserId,
                    (br, u) => new { BloodRequest = br, Users = u })
                .SelectMany(
                    x => x.Users.DefaultIfEmpty(),
                    (x, u) => new
                    {
                        x.BloodRequest,
                        Name = u != null ? u.Name : null,
                        DateOfBirth = u != null ? u.DateOfBirth : (DateOnly?)null,
                        Phone = u != null ? u.Phone : null
                    })
                .Join(_context.BloodTypes,
                    x => x.BloodRequest.BloodTypeId,
                    bt => bt.BloodTypeId,
                    (x, bt) => new { x.BloodRequest, x.Name, x.DateOfBirth, x.Phone, BloodTypeName = bt.Name + bt.RhFactor })
                .Join(_context.BloodComponents,
                    x => x.BloodRequest.BloodComponentId,
                    bc => bc.BloodComponentId,
                    (x, bc) => new { x.BloodRequest, x.Name, x.DateOfBirth, x.Phone, x.BloodTypeName, BloodComponentName = bc.Name });

            var request = await query
                .Select(x => new BloodRequestDto
                {
                    BloodRequestId = x.BloodRequest.BloodRequestId,
                    UserId = x.BloodRequest.UserId,
                    Name = x.Name,
                    DateOfBirth = x.DateOfBirth,
                    Phone = x.Phone,
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
                .FirstOrDefaultAsync();

            if (request == null)
            {
                _logger.LogWarning("Blood request not found: id={Id}", id);
                return NotFound(new { Message = "Blood request not found." });
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
        public async Task<ActionResult<object>> GetBloodRequests([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("GetBloodRequests called with page={Page}, pageSize={PageSize}", page, pageSize);

            if (page < 1 || pageSize < 1)
            {
                _logger.LogWarning("Invalid page or pageSize: page={Page}, pageSize={PageSize}", page, pageSize);
                return BadRequest(new { Message = "Invalid page or pageSize." });
            }

            var query = _context.BloodRequests
                .GroupJoin(_context.Users,
                    br => br.UserId,
                    u => u.UserId,
                    (br, u) => new { BloodRequest = br, Users = u })
                .SelectMany(
                    x => x.Users.DefaultIfEmpty(),
                    (x, u) => new
                    {
                        x.BloodRequest,
                        Name = u != null ? u.Name : null,
                        DateOfBirth = u != null ? u.DateOfBirth : (DateOnly?)null,
                        Phone = u != null ? u.Phone : null
                    })
                .Join(_context.BloodTypes,
                    x => x.BloodRequest.BloodTypeId,
                    bt => bt.BloodTypeId,
                    (x, bt) => new { x.BloodRequest, x.Name, x.DateOfBirth, x.Phone, BloodTypeName = bt.Name + bt.RhFactor })
                .Join(_context.BloodComponents,
                    x => x.BloodRequest.BloodComponentId,
                    bc => bc.BloodComponentId,
                    (x, bc) => new { x.BloodRequest, x.Name, x.DateOfBirth, x.Phone, x.BloodTypeName, BloodComponentName = bc.Name });

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
                    Name = x.Name,
                    DateOfBirth = x.DateOfBirth,
                    Phone = x.Phone,
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
                .GroupJoin(_context.Users,
                    br => br.UserId,
                    u => u.UserId,
                    (br, u) => new { BloodRequest = br, Users = u })
                .SelectMany(
                    x => x.Users.DefaultIfEmpty(),
                    (x, u) => new
                    {
                        x.BloodRequest,
                        Name = u != null ? u.Name : null,
                        DateOfBirth = u != null ? u.DateOfBirth : (DateOnly?)null,
                        Phone = u != null ? u.Phone : null
                    })
                .Join(_context.BloodTypes,
                    x => x.BloodRequest.BloodTypeId,
                    bt => bt.BloodTypeId,
                    (x, bt) => new { x.BloodRequest, x.Name, x.DateOfBirth, x.Phone, BloodTypeName = bt.Name + bt.RhFactor })
                .Join(_context.BloodComponents,
                    x => x.BloodRequest.BloodComponentId,
                    bc => bc.BloodComponentId,
                    (x, bc) => new { x.BloodRequest, x.Name, x.DateOfBirth, x.Phone, x.BloodTypeName, BloodComponentName = bc.Name });

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string lowerKeyword = keyword.Trim().ToLower();
                query = query.Where(x =>
                    (x.Name != null && x.Name.ToLower().Contains(lowerKeyword)) ||
                    (x.Phone != null && x.Phone.ToLower().Contains(lowerKeyword)) ||
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
                .OrderBy(x => x.BloodRequest.Status == (byte)BloodRequestStatus.Pending ? 0
        : x.BloodRequest.Status == (byte)BloodRequestStatus.Successful ? 1
        : 2)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BloodRequestDto
                {
                    BloodRequestId = x.BloodRequest.BloodRequestId,
                    UserId = x.BloodRequest.UserId,
                    Name = x.Name,
                    DateOfBirth = x.DateOfBirth,
                    Phone = x.Phone,
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
        public async Task<ActionResult<object>> PostBloodRequest([FromForm] CreateBloodRequestDto dto)
        {
            _logger.LogInformation("PostBloodRequest called with data: {@CreateDto}", dto);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("PostBloodRequest: Invalid data provided");
                return BadRequest(new { message = "Invalid data submitted.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            Console.WriteLine(dto.DateOfBirth);

            var bloodRequest = new BloodRequest
            {
                UserId = dto.UserId,
                BloodTypeId = dto.BloodTypeId,
                BloodComponentId = dto.BloodComponentId,
                IsEmergency = dto.IsEmergency,
                Status = (byte)BloodRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow,
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
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBloodRequestById), new { id = bloodRequest.BloodRequestId }, bloodRequest);
        }

        // PUT: api/BloodRequests/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBloodRequest(int id, [FromForm] UpdateBloodRequestDto dto)
        {
            var bloodRequest = await _context.BloodRequests.FindAsync(id);
            if (bloodRequest == null)
            {
                return NotFound(new { message = "Blood request not found" });
            }

            if (dto.Status.HasValue && !Enum.IsDefined(typeof(BloodRequestStatus), dto.Status))
            {
                return BadRequest(new { message = "Invalid status value." });
            }

            bloodRequest.Status = dto.Status.HasValue
                ? (byte)dto.Status.Value
                : bloodRequest.Status;

            _context.Entry(bloodRequest).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Blood request updated successfully." });
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateBloodRequestStatus(int id, [FromForm] UpdateBloodRequestStatusDto dto)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid data submitted.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            if (!Enum.IsDefined(typeof(BloodRequestStatus), dto.Status))
            {
                return BadRequest(new { message = "Invalid status value." });
            }

            var bloodRequest = await _context.BloodRequests.FindAsync(id);
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
                .GroupJoin(_context.Users,
                    br => br.UserId,
                    u => u.UserId,
                    (br, u) => new { BloodRequest = br, Users = u })
                .SelectMany(
                    x => x.Users.DefaultIfEmpty(),
                    (x, u) => new
                    {
                        x.BloodRequest,
                        Name = u != null ? u.Name : null,
                        DateOfBirth = u != null ? u.DateOfBirth : (DateOnly?)null,
                        Phone = u != null ? u.Phone : null
                    })
                .Join(_context.BloodTypes,
                    x => x.BloodRequest.BloodTypeId,
                    bt => bt.BloodTypeId,
                    (x, bt) => new
                    {
                        x.BloodRequest,
                        x.Name,
                        x.DateOfBirth,
                        x.Phone,
                        BloodTypeName = bt.Name + bt.RhFactor
                    })
                .Join(_context.BloodComponents,
                    x => x.BloodRequest.BloodComponentId,
                    bc => bc.BloodComponentId,
                    (x, bc) => new
                    {
                        x.BloodRequest,
                        x.Name,
                        x.DateOfBirth,
                        x.Phone,
                        x.BloodTypeName,
                        BloodComponentName = bc.Name
                    });

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string lowerKeyword = keyword.Trim().ToLower();
                query = query.Where(x =>
                    (x.Name != null && x.Name.ToLower().Contains(lowerKeyword)) ||
                    (x.Phone != null && x.Phone.ToLower().Contains(lowerKeyword)) ||
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
                    Name = x.Name,
                    DateOfBirth = x.DateOfBirth,
                    Phone = x.Phone,
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

        [HttpPatch("{id}/status/staff")]
        public async Task<IActionResult> UpdateBloodRequestFromStaffStatus(int id, [FromForm] UpdateBloodRequestStatusDto dto)
        {
            _logger.LogInformation("UpdateBloodRequestStatus called with id={Id}, status={Status}", id, dto.Status);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid data provided for UpdateBloodRequestStatus: id={Id}", id);
                return BadRequest(new
                {
                    Message = "Invalid data submitted.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            if (!Enum.IsDefined(typeof(BloodRequestStatus), dto.Status))
            {
                _logger.LogWarning("Invalid status value: id={Id}, status={Status}", id, dto.Status);
                return BadRequest(new { Message = "Invalid status value." });
            }

            var bloodRequest = await _context.BloodRequests
                .FirstOrDefaultAsync(br => br.BloodRequestId == id);

            if (bloodRequest == null)
            {
                _logger.LogWarning("Blood request not found: id={Id}", id);
                return NotFound(new { Message = "Blood request not found" });
            }

            bloodRequest.Status = dto.Status;
            _context.Entry(bloodRequest).State = EntityState.Modified;

            if ((BloodRequestStatus)dto.Status == BloodRequestStatus.Successful)
            {
                try
                {
                    await ProcessBloodRequestFulfillment(bloodRequest);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing blood request fulfillment: id={Id}", id);
                    return StatusCode(500, new { Message = "An error occurred while processing the blood request." });
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Blood request status updated successfully: id={Id}, status={Status}", id, dto.Status);
            return Ok(new { Message = "Blood request status updated successfully." });
        }

        private async Task ProcessBloodRequestFulfillment(BloodRequest bloodRequest)
        {
            _logger.LogInformation("Processing blood request fulfillment: id={Id}", bloodRequest.BloodRequestId);

            var matchedInventory = await _context.BloodInventories
                .FirstOrDefaultAsync(inv =>
                    inv.BloodTypeId == bloodRequest.BloodTypeId &&
                    inv.BloodComponentId == bloodRequest.BloodComponentId &&
                    inv.Quantity >= bloodRequest.Quantity);

            if (matchedInventory != null)
            {
                matchedInventory.Quantity -= bloodRequest.Quantity;
                bloodRequest.Fulfilled = true;
                bloodRequest.FulfilledSource = "Inventory";

                _context.Entry(matchedInventory).State = EntityState.Modified;
                _context.Entry(bloodRequest).State = EntityState.Modified;
            }
            else
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
                            Type = "Auto"
                        };

                        _context.RequestMatches.Add(match);
                    }
                }
                else
                {
                    _logger.LogWarning("No matching donation requests found for blood request: id={Id}", bloodRequest.BloodRequestId);
                }
            }
        }

        //public async Task HandleCompletedDonationRequest(int donationRequestId)
        //{
        //    _logger.LogInformation("Handling completed donation request: id={Id}", donationRequestId);

        //    var donationRequest = await _context.DonationRequests
        //        .FirstOrDefaultAsync(dr => dr.DonateRequestId == donationRequestId);

        //    if (donationRequest == null || donationRequest.Status != "Completed")
        //    {
        //        return;
        //    }

        //    var inventory = await _context.BloodInventories
        //        .FirstOrDefaultAsync(inv =>
        //            inv.BloodTypeId == donationRequest.BloodTypeId &&
        //            inv.BloodComponentId == donationRequest.BloodComponentId);

        //    if (inventory == null)
        //    {
        //        inventory = new BloodInventory
        //        {
        //            BloodTypeId = donationRequest.BloodTypeId,
        //            BloodComponentId = donationRequest.BloodComponentId,
        //            Quantity = donationRequest.Quantity, 
        //            LastUpdated = DateTime.UtcNow
        //        };
        //        _context.BloodInventories.Add(inventory);
        //    }
        //    else
        //    {
        //        inventory.Quantity += donationRequest.Quantity;
        //        inventory.LastUpdated = DateTime.UtcNow;
        //        _context.Entry(inventory).State = EntityState.Modified;
        //    }

        //    var pendingRequests = await _context.BloodRequests
        //        .Where(br =>
        //            br.BloodTypeId == donationRequest.BloodTypeId &&
        //            br.BloodComponentId == donationRequest.BloodComponentId &&
        //            br.Status == (byte)BloodRequestStatus.Pending &&
        //            br.Fulfilled == false)
        //        .ToListAsync();

        //    foreach (var bloodRequest in pendingRequests)
        //    {
        //        if (inventory.Quantity >= bloodRequest.Quantity)
        //        {
        //            inventory.Quantity -= bloodRequest.Quantity;
        //            bloodRequest.Fulfilled = true;
        //            bloodRequest.FulfilledSource = "Inventory";

        //            var match = new RequestMatch
        //            {
        //                BloodRequestId = bloodRequest.BloodRequestId,
        //                DonationRequestId = donationRequest.DonateRequestId,
        //                MatchStatus = "Completed",
        //                ScheduledDate = DateOnly.FromDateTime(DateTime.UtcNow),
        //                Notes = "Fulfilled from completed donation",
        //                Type = "Auto"
        //            };

        //            _context.RequestMatches.Add(match);
        //            _context.Entry(bloodRequest).State = EntityState.Modified;
        //            _context.Entry(inventory).State = EntityState.Modified;
        //        }
        //    }

        //    await _context.SaveChangesAsync();
        //    _logger.LogInformation("Completed donation request processed: id={Id}", donationRequestId);
        //}
    }
}