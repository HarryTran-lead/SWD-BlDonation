using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SWD_BLDONATION.DTOs.BloodRequestDTOs;
using SWD_BLDONATION.Models.Generated;
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
        private readonly IMapper _mapper;
        private readonly ILogger<BloodRequestsController> _logger;

        public BloodRequestsController(BloodDonationContext context, IMapper mapper, ILogger<BloodRequestsController> logger)
        {
            _context = context;
            _mapper = mapper;
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
                    (x, u) => new { x.BloodRequest, UserName = u != null ? u.UserName : null })
                .Join(_context.BloodTypes,
                    x => x.BloodRequest.BloodTypeId,
                    bt => bt.BloodTypeId,
                    (x, bt) => new { x.BloodRequest, x.UserName, BloodTypeName = bt.Name + bt.RhFactor })
                .Join(_context.BloodComponents,
                    x => x.BloodRequest.BloodComponentId,
                    bc => bc.BloodComponentId,
                    (x, bc) => new { x.BloodRequest, x.UserName, x.BloodTypeName, BloodComponentName = bc.Name });

            var requests = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BloodRequestDto
                {
                    BloodRequestId = x.BloodRequest.BloodRequestId,
                    UserId = x.BloodRequest.UserId,
                    UserName = x.UserName,
                    BloodTypeId = x.BloodRequest.BloodTypeId,
                    BloodTypeName = x.BloodTypeName,
                    BloodComponentId = x.BloodRequest.BloodComponentId,
                    BloodComponentName = x.BloodComponentName,
                    IsEmergency = x.BloodRequest.IsEmergency.Value,
                    Status = x.BloodRequest.Status,
                    CreatedAt = x.BloodRequest.CreatedAt.Value,
                    Location = x.BloodRequest.Location,
                    Quantity = x.BloodRequest.Quantity.Value,
                    Fulfilled = x.BloodRequest.Fulfilled.Value,
                    FulfilledSource = x.BloodRequest.FulfilledSource,
                    HeightCm = x.BloodRequest.HeightCm.Value,
                    WeightKg = x.BloodRequest.WeightKg.Value,
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
                    (x, u) => new { x.BloodRequest, UserName = u != null ? u.UserName : null })
                .Join(_context.BloodTypes,
                    x => x.BloodRequest.BloodTypeId,
                    bt => bt.BloodTypeId,
                    (x, bt) => new { x.BloodRequest, x.UserName, BloodTypeName = bt.Name + bt.RhFactor })
                .Join(_context.BloodComponents,
                    x => x.BloodRequest.BloodComponentId,
                    bc => bc.BloodComponentId,
                    (x, bc) => new BloodRequestDto
                    {
                        BloodRequestId = x.BloodRequest.BloodRequestId,
                        UserId = x.BloodRequest.UserId,
                        UserName = x.UserName,
                        BloodTypeId = x.BloodRequest.BloodTypeId,
                        BloodTypeName = x.BloodTypeName,
                        BloodComponentId = x.BloodRequest.BloodComponentId,
                        BloodComponentName = bc.Name,
                        IsEmergency = x.BloodRequest.IsEmergency.Value,
                        Status = x.BloodRequest.Status,
                        CreatedAt = x.BloodRequest.CreatedAt.Value,
                        Location = x.BloodRequest.Location,
                        Quantity = x.BloodRequest.Quantity.Value,
                        Fulfilled = x.BloodRequest.Fulfilled.Value,
                        FulfilledSource = x.BloodRequest.FulfilledSource,
                        HeightCm = x.BloodRequest.HeightCm.Value,
                        WeightKg = x.BloodRequest.WeightKg.Value,
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

        // GET: api/BloodRequests/search
        [HttpGet("search")]
        public async Task<ActionResult<object>> SearchBloodRequests([FromQuery] BloodRequestSearchQueryDto query)
        {
            _logger.LogInformation("SearchBloodRequests called with query: {@Query}", query);

            if (query.Page < 1 || query.PageSize < 1)
            {
                _logger.LogWarning("Invalid page or pageSize: page={Page}, pageSize={PageSize}", query.Page, query.PageSize);
                return BadRequest(new { Message = "Invalid page or pageSize." });
            }

            var dbQuery = _context.BloodRequests
                .GroupJoin(_context.Users,
                    br => br.UserId,
                    u => u.UserId,
                    (br, u) => new { BloodRequest = br, Users = u })
                .SelectMany(
                    x => x.Users.DefaultIfEmpty(),
                    (x, u) => new { x.BloodRequest, UserName = u != null ? u.UserName : null })
                .Join(_context.BloodTypes,
                    x => x.BloodRequest.BloodTypeId,
                    bt => bt.BloodTypeId,
                    (x, bt) => new { x.BloodRequest, x.UserName, BloodTypeName = bt.Name + bt.RhFactor })
                .Join(_context.BloodComponents,
                    x => x.BloodRequest.BloodComponentId,
                    bc => bc.BloodComponentId,
                    (x, bc) => new { x.BloodRequest, x.UserName, x.BloodTypeName, BloodComponentName = bc.Name });

            if (query.Id.HasValue)
                dbQuery = dbQuery.Where(x => x.BloodRequest.BloodRequestId == query.Id.Value);

            if (query.UserId.HasValue)
                dbQuery = dbQuery.Where(x => x.BloodRequest.UserId == query.UserId.Value);

            if (query.BloodTypeId.HasValue)
                dbQuery = dbQuery.Where(x => x.BloodRequest.BloodTypeId == query.BloodTypeId.Value);

            if (query.BloodComponentId.HasValue)
                dbQuery = dbQuery.Where(x => x.BloodRequest.BloodComponentId == query.BloodComponentId.Value);

            if (!string.IsNullOrEmpty(query.Status))
                dbQuery = dbQuery.Where(x => x.BloodRequest.Status.ToLower() == query.Status.Trim().ToLower());

            if (!string.IsNullOrEmpty(query.Location))
                dbQuery = dbQuery.Where(x => x.BloodRequest.Location.Contains(query.Location.Trim()));

            var requests = await dbQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(x => new BloodRequestDto
                {
                    BloodRequestId = x.BloodRequest.BloodRequestId,
                    UserId = x.BloodRequest.UserId,
                    UserName = x.UserName,
                    BloodTypeId = x.BloodRequest.BloodTypeId,
                    BloodTypeName = x.BloodTypeName,
                    BloodComponentId = x.BloodRequest.BloodComponentId,
                    BloodComponentName = x.BloodComponentName,
                    IsEmergency = x.BloodRequest.IsEmergency.Value,
                    Status = x.BloodRequest.Status,
                    CreatedAt = x.BloodRequest.CreatedAt.Value,
                    Location = x.BloodRequest.Location,
                    Quantity = x.BloodRequest.Quantity.Value,
                    Fulfilled = x.BloodRequest.Fulfilled.Value,
                    FulfilledSource = x.BloodRequest.FulfilledSource,
                    HeightCm = x.BloodRequest.HeightCm.Value,
                    WeightKg = x.BloodRequest.WeightKg.Value,
                    HealthInfo = x.BloodRequest.HealthInfo
                })
                .ToListAsync();

            var totalCount = await dbQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            _logger.LogInformation("SearchBloodRequests returned {Count} items", requests.Count);

            return Ok(new
            {
                Message = "Search completed successfully.",
                Data = new
                {
                    Requests = requests,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = query.Page,
                    PageSize = query.PageSize
                }
            });
        }

        // POST: api/BloodRequests
        [HttpPost]
        public async Task<ActionResult<object>> PostBloodRequest([FromForm] CreateBloodRequestDto createDto)
        {
            _logger.LogInformation("PostBloodRequest called with data: {@CreateDto}", createDto);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("PostBloodRequest: Invalid data provided");
                return BadRequest(new
                {
                    message = "Invalid data submitted.",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            if (createDto.UserId.HasValue)
            {
                var userExists = await _context.Users.AnyAsync(u => u.UserId == createDto.UserId.Value && !u.IsDeleted);
                if (!userExists)
                {
                    _logger.LogWarning("PostBloodRequest: UserId={UserId} does not exist or is deleted", createDto.UserId);
                    return BadRequest(new { message = $"User with id = {createDto.UserId} not found or is deleted." });
                }
            }

            var bloodTypeExists = await _context.BloodTypes.AnyAsync(bt => bt.BloodTypeId == createDto.BloodTypeId);
            if (!bloodTypeExists)
            {
                _logger.LogWarning("PostBloodRequest: BloodTypeId={BloodTypeId} does not exist", createDto.BloodTypeId);
                return BadRequest(new { message = $"Blood type with id = {createDto.BloodTypeId} not found." });
            }

            var bloodComponentExists = await _context.BloodComponents.AnyAsync(bc => bc.BloodComponentId == createDto.BloodComponentId);
            if (!bloodComponentExists)
            {
                _logger.LogWarning("PostBloodRequest: BloodComponentId={BloodComponentId} does not exist", createDto.BloodComponentId);
                return BadRequest(new { message = $"Blood component with id = {createDto.BloodComponentId} not found." });
            }
            var  transaction = await _context.Database.BeginTransactionAsync();
            try
            {

                var bloodRequest = _mapper.Map<BloodRequest>(createDto);
                bloodRequest.CreatedAt = DateTime.UtcNow;
                bloodRequest.Status = "pending";
                bloodRequest.Fulfilled = false;

                var inventory = await _context.BloodInventories
                    .Where(i => i.BloodTypeId == createDto.BloodTypeId &&
                                i.BloodComponentId == createDto.BloodComponentId &&
                                i.Quantity >= createDto.Quantity)
                    .FirstOrDefaultAsync();

                if (inventory != null)
                {
                    bloodRequest.Fulfilled = true;
                    bloodRequest.FulfilledSource = "Inventory";

                    inventory.Quantity -= createDto.Quantity;
                    _context.BloodInventories.Update(inventory);
                    var bloodRequestInventory = new BloodRequestInventory
                    {
                        BloodRequestId = bloodRequest.BloodRequestId,
                        InventoryId = inventory.InventoryId,
                        QuantityUnit = createDto.Quantity,
                        QuantityAllocated = createDto.Quantity,
                        AllocatedAt = DateTime.UtcNow,
                        AllocatedBy = createDto.UserId ?? 1 
                    };

                    _context.BloodRequests.Add(bloodRequest);
                    await _context.SaveChangesAsync(); 

                    bloodRequestInventory.BloodRequestId = bloodRequest.BloodRequestId;
                    _context.BloodRequestInventories.Add(bloodRequestInventory);
                }
                else
                {
                    _context.BloodRequests.Add(bloodRequest);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                var resultDto = await _context.BloodRequests
                    .Where(br => br.BloodRequestId == bloodRequest.BloodRequestId)
                    .GroupJoin(_context.Users,
                        br => br.UserId,
                        u => u.UserId,
                        (br, u) => new { BloodRequest = br, Users = u })
                    .SelectMany(
                        x => x.Users.DefaultIfEmpty(),
                        (x, u) => new { x.BloodRequest, UserName = u != null ? u.UserName : null })
                    .Join(_context.BloodTypes,
                        x => x.BloodRequest.BloodTypeId,
                        bt => bt.BloodTypeId,
                        (x, bt) => new { x.BloodRequest, x.UserName, BloodTypeName = bt.Name + bt.RhFactor })
                    .Join(_context.BloodComponents,
                        x => x.BloodRequest.BloodComponentId,
                        bc => bc.BloodComponentId,
                        (x, bc) => new BloodRequestDto
                        {
                            BloodRequestId = x.BloodRequest.BloodRequestId,
                            UserId = x.BloodRequest.UserId,
                            UserName = x.UserName,
                            BloodTypeId = x.BloodRequest.BloodTypeId,
                            BloodTypeName = x.BloodTypeName,
                            BloodComponentId = x.BloodRequest.BloodComponentId,
                            BloodComponentName = bc.Name,
                            IsEmergency = x.BloodRequest.IsEmergency.Value,
                            Status = x.BloodRequest.Status,
                            CreatedAt = x.BloodRequest.CreatedAt.Value,
                            Location = x.BloodRequest.Location,
                            Quantity = x.BloodRequest.Quantity.Value,
                            Fulfilled = x.BloodRequest.Fulfilled.Value,
                            FulfilledSource = x.BloodRequest.FulfilledSource,
                            HeightCm = x.BloodRequest.HeightCm.Value,
                            WeightKg = x.BloodRequest.WeightKg.Value,
                            HealthInfo = x.BloodRequest.HealthInfo
                        })
                    .FirstAsync();

                _logger.LogInformation("PostBloodRequest: Created blood request with id={BloodRequestId}, Fulfilled={Fulfilled}",
                    resultDto.BloodRequestId, resultDto.Fulfilled);

                return CreatedAtAction(
                    nameof(GetBloodRequest),
                    new { id = resultDto.BloodRequestId },
                    new { message = "Blood request created successfully.", data = resultDto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostBloodRequest: Exception occurred while creating blood request");
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "An error occurred while creating the blood request.", detail = ex.Message });
            }
        }

        // DELETE: api/BloodRequests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBloodRequest(int id)
        {
            _logger.LogInformation("DeleteBloodRequest called with id={Id}", id);

            var entity = await _context.BloodRequests.FindAsync(id);
            if (entity == null)
            {
                _logger.LogWarning("DeleteBloodRequest: Blood request with id={Id} not found or already deleted", id);
                return NotFound(new { Message = $"Blood request with id = {id} not found or already deleted." });
            }

            entity.Status = "cancelled";

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("DeleteBloodRequest: Soft deleted blood request with id={Id}", id);
                return Ok(new { Message = "Blood request deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteBloodRequest: Exception occurred for id={Id}", id);
                return StatusCode(500, new { Message = "Error deleting blood request.", Detail = ex.Message });
            }
        }
    }
}

