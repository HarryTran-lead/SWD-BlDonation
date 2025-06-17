using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD_BLDONATION.DTOs.BloodRequestDTOs;
using SWD_BLDONATION.Models.Generated;
using SWD_BLDONATION.Models.Enums; // Import BloodRequestStatus enum
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
        private readonly BloodDonationContext _context;
        private readonly ILogger<BloodRequestsController> _logger;

        public BloodRequestsController(BloodDonationContext context, ILogger<BloodRequestsController> logger)
        {
            _context = context;
            _logger = logger;
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
                    (x, u) => new {
                        x.BloodRequest,
                        Name = u != null ? u.Name : null, // Use Name instead of UserName
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
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BloodRequestDto
                {
                    BloodRequestId = x.BloodRequest.BloodRequestId,
                    UserId = x.BloodRequest.UserId,
                    Name = x.Name,  // Now correctly handled as Name
                    DateOfBirth = x.DateOfBirth,  // Now correctly handled for null
                    Phone = x.Phone,  // Now correctly handled for null
                    BloodTypeId = x.BloodRequest.BloodTypeId ?? 0,  // Use 0 if BloodTypeId is null
                    BloodTypeName = x.BloodTypeName,
                    BloodComponentId = x.BloodRequest.BloodComponentId ?? 0,  // Use 0 if BloodComponentId is null
                    BloodComponentName = x.BloodComponentName,
                    IsEmergency = x.BloodRequest.IsEmergency.HasValue ? x.BloodRequest.IsEmergency.Value : false,
                    Status = new StatusDto
                    {
                        Id = (byte)BloodRequestStatus.Pending, // Default to Pending (0)
                        Name = BloodRequestStatus.Pending.ToString() // Convert enum to string
                    },
                    CreatedAt = x.BloodRequest.CreatedAt.HasValue ? x.BloodRequest.CreatedAt.Value : DateTime.MinValue,
                    Location = x.BloodRequest.Location,
                    Quantity = x.BloodRequest.Quantity.HasValue ? (int)x.BloodRequest.Quantity.Value : 0, // Handle nullable decimal to int
                    Fulfilled = x.BloodRequest.Fulfilled.HasValue ? x.BloodRequest.Fulfilled.Value : false,
                    FulfilledSource = x.BloodRequest.FulfilledSource,
                    HeightCm = x.BloodRequest.HeightCm.HasValue ? (int)x.BloodRequest.HeightCm.Value : 0, // Handle nullable decimal to int
                    WeightKg = x.BloodRequest.WeightKg.HasValue ? (int)x.BloodRequest.WeightKg.Value : 0, // Handle nullable decimal to int
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

        // GET: api/BloodRequests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetBloodRequest(int id)
        {
            _logger.LogInformation("GetBloodRequest called with id={Id}", id);

            var request = await _context.BloodRequests
                .Where(br => br.BloodRequestId == id)
                .GroupJoin(_context.Users,
                    br => br.UserId,
                    u => u.UserId,
                    (br, u) => new { BloodRequest = br, Users = u })
                .SelectMany(
                    x => x.Users.DefaultIfEmpty(),
                    (x, u) => new { x.BloodRequest, Name = u != null ? u.Name : null, DateOfBirth = u != null ? u.DateOfBirth : (DateOnly?)null, Phone = u != null ? u.Phone : null })
                .Join(_context.BloodTypes,
                    x => x.BloodRequest.BloodTypeId,
                    bt => bt.BloodTypeId,
                    (x, bt) => new { x.BloodRequest, x.Name, x.DateOfBirth, x.Phone, BloodTypeName = bt.Name + bt.RhFactor })
                .Join(_context.BloodComponents,
                    x => x.BloodRequest.BloodComponentId,
                    bc => bc.BloodComponentId,
                    (x, bc) => new BloodRequestDto
                    {
                        BloodRequestId = x.BloodRequest.BloodRequestId,
                        UserId = x.BloodRequest.UserId,
                        Name = x.Name,  // Include Name
                        DateOfBirth = x.DateOfBirth,  // Include DateOfBirth
                        Phone = x.Phone,  // Include Phone
                        BloodTypeId = x.BloodRequest.BloodTypeId ?? 0,  // Use 0 if BloodTypeId is null
                        BloodTypeName = x.BloodTypeName,
                        BloodComponentId = x.BloodRequest.BloodComponentId ?? 0,  // Use 0 if BloodComponentId is null
                        BloodComponentName = bc.Name,
                        IsEmergency = x.BloodRequest.IsEmergency.HasValue ? x.BloodRequest.IsEmergency.Value : false,
                        Status = new StatusDto
                        {
                            Id = (byte)BloodRequestStatus.Pending,
                            Name = BloodRequestStatus.Pending.ToString()
                        },
                        CreatedAt = x.BloodRequest.CreatedAt.HasValue ? x.BloodRequest.CreatedAt.Value : DateTime.MinValue,
                        Location = x.BloodRequest.Location,
                        Quantity = x.BloodRequest.Quantity.HasValue ? (int)x.BloodRequest.Quantity.Value : 0, // Handle nullable decimal to int
                        Fulfilled = x.BloodRequest.Fulfilled.HasValue ? x.BloodRequest.Fulfilled.Value : false,
                        FulfilledSource = x.BloodRequest.FulfilledSource,
                        HeightCm = x.BloodRequest.HeightCm.HasValue ? (int)x.BloodRequest.HeightCm.Value : 0, // Handle nullable decimal to int
                        WeightKg = x.BloodRequest.WeightKg.HasValue ? (int)x.BloodRequest.WeightKg.Value : 0, // Handle nullable decimal to int
                        HealthInfo = x.BloodRequest.HealthInfo
                    })
                .FirstOrDefaultAsync();

            if (request == null)
            {
                _logger.LogWarning("GetBloodRequest: Blood request with id={Id} not found", id);
                return NotFound(new { Message = $"Blood request with id = {id} not found." });
            }

            _logger.LogInformation("GetBloodRequest: Found blood request with id={Id}", id);
            return Ok(new { Message = "Retrieved blood request successfully.", Data = request });
        }

        // POST: api/BloodRequests
        [HttpPost]
        public async Task<ActionResult<object>> PostBloodRequest([FromBody] CreateBloodRequestDto dto)
        {
            _logger.LogInformation("PostBloodRequest called with data: {@CreateDto}", dto);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("PostBloodRequest: Invalid data provided");
                return BadRequest(new { message = "Invalid data submitted.", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            var bloodRequest = new BloodRequest
            {
                BloodTypeId = dto.BloodTypeId,
                BloodComponentId = dto.BloodComponentId,
                IsEmergency = dto.IsEmergency,
                Status = (byte)BloodRequestStatus.Pending, // Default to Pending
                CreatedAt = DateTime.UtcNow,
                Location = dto.Location,
                Quantity = dto.Quantity,
                Fulfilled = false,
                HealthInfo = dto.HealthInfo
            };

            _context.BloodRequests.Add(bloodRequest);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBloodRequest), new { id = bloodRequest.BloodRequestId }, bloodRequest);
        }

        // PUT: api/BloodRequests/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBloodRequest(int id, [FromBody] UpdateBloodRequestDto dto)
        {
            var bloodRequest = await _context.BloodRequests.FindAsync(id);
            if (bloodRequest == null)
            {
                return NotFound(new { message = "Blood request not found" });
            }

            // Validate the status enum
            if (dto.Status.HasValue && !Enum.IsDefined(typeof(BloodRequestStatus), dto.Status))
            {
                return BadRequest(new { message = "Invalid status value." });
            }

            // Update status if valid
            bloodRequest.Status = dto.Status.HasValue
                ? (byte)dto.Status.Value
                : bloodRequest.Status;

            _context.Entry(bloodRequest).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Blood request updated successfully." });
        }

        // DELETE: api/BloodRequests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBloodRequest(int id)
        {
            var bloodRequest = await _context.BloodRequests.FindAsync(id);
            if (bloodRequest == null)
            {
                return NotFound(new { message = "Blood request not found" });
            }

            // Soft delete: Mark as cancelled and fulfilled
            bloodRequest.Status = (byte)BloodRequestStatus.Cancelled;
            bloodRequest.Fulfilled = true;

            _context.Entry(bloodRequest).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Blood request soft deleted: ID = {Id}, Status = Cancelled", id);

            return Ok(new { message = "Blood request deleted successfully." });
        }

        [HttpGet("ByUser/{userId}")]
        public async Task<ActionResult<object>> GetBloodRequestsByUser(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("GetBloodRequestsByUser called with userId={UserId}, page={Page}, pageSize={PageSize}", userId, page, pageSize);

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
                    (x, u) => new {
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
                        Id = (byte)BloodRequestStatus.Pending,
                        Name = BloodRequestStatus.Pending.ToString()
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

            _logger.LogInformation("GetBloodRequestsByUser returned {Count} items for userId={UserId}", requests.Count, userId);

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
    }
}
