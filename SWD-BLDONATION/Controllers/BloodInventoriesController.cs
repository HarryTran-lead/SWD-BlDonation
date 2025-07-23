using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD_BLDONATION.DTOs.BloodInventoryDTOs;
using SWD_BLDONATION.Models.Generated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWD_BLDONATION.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BloodInventoriesController : ControllerBase
    {
        private readonly BloodDonationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<BloodInventoriesController> _logger;

        public BloodInventoriesController(BloodDonationDbContext context, IMapper mapper, ILogger<BloodInventoriesController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/BloodInventories
        [HttpGet]
        [Authorize(Roles = "Admin,Staff,User")]
        public async Task<ActionResult<object>> GetBloodInventories([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("GetBloodInventories called with page={Page}, pageSize={PageSize}", page, pageSize);

            if (page < 1 || pageSize < 1)
            {
                _logger.LogWarning("Invalid page or pageSize: page={Page}, pageSize={PageSize}", page, pageSize);
                return BadRequest(new { Message = "Invalid page or pageSize." });
            }

            var query = _context.BloodInventories
                .Join(_context.BloodTypes,
                    bi => bi.BloodTypeId,
                    bt => bt.BloodTypeId,
                    (bi, bt) => new { BloodInventory = bi, BloodTypeName = bt.Name + bt.RhFactor })
                .Join(_context.BloodComponents,
                    x => x.BloodInventory.BloodComponentId,
                    bc => bc.BloodComponentId,
                    (x, bc) => new { x.BloodInventory, x.BloodTypeName, BloodComponentName = bc.Name });

            var inventories = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BloodInventoryDto
                {
                    InventoryId = x.BloodInventory.InventoryId,
                    BloodTypeId = x.BloodInventory.BloodTypeId.Value,
                    BloodTypeName = x.BloodTypeName,
                    BloodComponentId = x.BloodInventory.BloodComponentId.Value,
                    BloodComponentName = x.BloodComponentName,
                    Quantity = x.BloodInventory.Quantity.Value,
                    Unit = x.BloodInventory.Unit,
                    LastUpdated = x.BloodInventory.LastUpdated.Value,
                    InventoryLocation = x.BloodInventory.InventoryLocation
                })
                .ToListAsync();

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            _logger.LogInformation("GetBloodInventories returned {Count} items", inventories.Count);

            return Ok(new
            {
                Message = "Retrieved blood inventories successfully.",
                Data = new
                {
                    Inventories = inventories,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize
                }
            });
        }

        // GET: api/BloodInventories/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff,User")]
        public async Task<ActionResult<object>> GetBloodInventory(int id)
        {
            _logger.LogInformation("GetBloodInventory called with id={Id}", id);

            var inventory = await _context.BloodInventories
                .Where(bi => bi.InventoryId == id)
                .Join(_context.BloodTypes,
                    bi => bi.BloodTypeId,
                    bt => bt.BloodTypeId,
                    (bi, bt) => new { BloodInventory = bi, BloodTypeName = bt.Name + bt.RhFactor })
                .Join(_context.BloodComponents,
                    x => x.BloodInventory.BloodComponentId,
                    bc => bc.BloodComponentId,
                    (x, bc) => new { x.BloodInventory, x.BloodTypeName, BloodComponentName = bc.Name })
                .Select(x => new BloodInventoryDto
                {
                    InventoryId = x.BloodInventory.InventoryId,
                    BloodTypeId = x.BloodInventory.BloodTypeId.Value,
                    BloodTypeName = x.BloodTypeName,
                    BloodComponentId = x.BloodInventory.BloodComponentId.Value,
                    BloodComponentName = x.BloodComponentName,
                    Quantity = x.BloodInventory.Quantity.Value,
                    Unit = x.BloodInventory.Unit,
                    LastUpdated = x.BloodInventory.LastUpdated.Value,
                    InventoryLocation = x.BloodInventory.InventoryLocation
                })
                .FirstOrDefaultAsync();

            if (inventory == null)
            {
                _logger.LogWarning("GetBloodInventory: Inventory with id={Id} not found", id);
                return NotFound(new { Message = $"Blood inventory with id = {id} not found." });
            }

            _logger.LogInformation("GetBloodInventory: Found inventory with id={Id}", id);
            return Ok(new { Message = "Retrieved blood inventory successfully.", Data = inventory });
        }

        // GET: api/BloodInventories/search
        [HttpGet("search")]
        [Authorize(Roles = "Admin,Staff,User")]
        public async Task<ActionResult<object>> SearchBloodInventories([FromQuery] BloodInventorySearchQueryDto query)
        {
            _logger.LogInformation("SearchBloodInventories called with query: {@Query}", query);

            if (query.Page < 1 || query.PageSize < 1)
            {
                _logger.LogWarning("Invalid page or pageSize: page={Page}, pageSize={PageSize}", query.Page, query.PageSize);
                return BadRequest(new { Message = "Invalid page or pageSize." });
            }

            var dbQuery = _context.BloodInventories
                .Join(_context.BloodTypes,
                    bi => bi.BloodTypeId,
                    bt => bt.BloodTypeId,
                    (bi, bt) => new { BloodInventory = bi, BloodTypeName = bt.Name + bt.RhFactor })
                .Join(_context.BloodComponents,
                    x => x.BloodInventory.BloodComponentId,
                    bc => bc.BloodComponentId,
                    (x, bc) => new { x.BloodInventory, x.BloodTypeName, BloodComponentName = bc.Name });

            if (query.Id.HasValue)
                dbQuery = dbQuery.Where(x => x.BloodInventory.InventoryId == query.Id.Value);

            if (query.BloodTypeId.HasValue)
                dbQuery = dbQuery.Where(x => x.BloodInventory.BloodTypeId == query.BloodTypeId.Value);

            if (query.BloodComponentId.HasValue)
                dbQuery = dbQuery.Where(x => x.BloodInventory.BloodComponentId == query.BloodComponentId.Value);

            if (!string.IsNullOrEmpty(query.InventoryLocation))
                dbQuery = dbQuery.Where(x => x.BloodInventory.InventoryLocation != null && x.BloodInventory.InventoryLocation.Contains(query.InventoryLocation.Trim()));

            var inventories = await dbQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(x => new BloodInventoryDto
                {
                    InventoryId = x.BloodInventory.InventoryId,
                    BloodTypeId = x.BloodInventory.BloodTypeId.Value,
                    BloodTypeName = x.BloodTypeName,
                    BloodComponentId = x.BloodInventory.BloodComponentId.Value,
                    BloodComponentName = x.BloodComponentName,
                    Quantity = x.BloodInventory.Quantity.Value,
                    Unit = x.BloodInventory.Unit,
                    LastUpdated = x.BloodInventory.LastUpdated.Value,
                    InventoryLocation = x.BloodInventory.InventoryLocation
                })
                .ToListAsync();

            var totalCount = await dbQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            _logger.LogInformation("SearchBloodInventories returned {Count} items", inventories.Count);

            return Ok(new
            {
                Message = "Search completed successfully.",
                Data = new
                {
                    Inventories = inventories,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = query.Page,
                    PageSize = query.PageSize
                }
            });
        }

        // POST: api/BloodInventories
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<object>> PostBloodInventory([FromForm] CreateBloodInventoryDto createDto)
        {
            _logger.LogInformation("PostBloodInventory called with data: {@CreateDto}", createDto);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("PostBloodInventory: Invalid data provided");
                return BadRequest(new
                {
                    message = "Invalid data submitted.",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var bloodTypeExists = await _context.BloodTypes.AnyAsync(bt => bt.BloodTypeId == createDto.BloodTypeId);
            if (!bloodTypeExists)
            {
                _logger.LogWarning("PostBloodInventory: BloodTypeId={BloodTypeId} does not exist", createDto.BloodTypeId);
                return BadRequest(new { message = $"Blood type with id = {createDto.BloodTypeId} not found." });
            }

            var bloodComponentExists = await _context.BloodComponents.AnyAsync(bc => bc.BloodComponentId == createDto.BloodComponentId);
            if (!bloodComponentExists)
            {
                _logger.LogWarning("PostBloodInventory: BloodComponentId={BloodComponentId} does not exist", createDto.BloodComponentId);
                return BadRequest(new { message = $"Blood component with id = {createDto.BloodComponentId} not found." });
            }

            try
            {
                var entity = _mapper.Map<BloodInventory>(createDto);
                entity.LastUpdated = DateTime.UtcNow;

                _context.BloodInventories.Add(entity);
                await _context.SaveChangesAsync();

                var responseDto = await _context.BloodInventories
                    .Where(bi => bi.InventoryId == entity.InventoryId)
                    .Join(
                        _context.BloodTypes,
                        bi => bi.BloodTypeId,
                        bt => bt.BloodTypeId,
                        (bi, bt) => new { BloodInventory = bi, BloodTypeName = bt.Name + bt.RhFactor })
                    .Join(
                        _context.BloodComponents,
                        x => x.BloodInventory.BloodComponentId,
                        bc => bc.BloodComponentId,
                        (x, bc) => new BloodInventoryDto
                        {
                            InventoryId = x.BloodInventory.InventoryId,
                            BloodTypeId = x.BloodInventory.BloodTypeId.Value,
                            BloodTypeName = x.BloodTypeName,
                            BloodComponentId = x.BloodInventory.BloodComponentId.Value,
                            BloodComponentName = x.BloodInventory.BloodComponent.Name,
                            Quantity = x.BloodInventory.Quantity.Value,
                            Unit = x.BloodInventory.Unit,
                            LastUpdated = x.BloodInventory.LastUpdated.Value,
                            InventoryLocation = x.BloodInventory.InventoryLocation
                        })
                    .FirstAsync();

                _logger.LogInformation("PostBloodInventory: Created inventory with id={InventoryId}", entity.InventoryId);

                return CreatedAtAction(
                    nameof(GetBloodInventory),
                    new { id = entity.BloodTypeId },
                    new { message = "Blood inventory created successfully.", data = responseDto });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostBloodInventory: Exception occurred while creating inventory");
                return StatusCode(500, new { message = "An error occurred while creating the blood inventory.", ex.Message });
            }
        }

        // PUT: api/BloodInventories/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> PutBloodInventory(int id, [FromForm] UpdateBloodInventoryDto updateDto)
        {
            _logger.LogInformation("PutBloodInventory called with id={Id}, data: {@UpdateDto}", id, updateDto);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("PutBloodInventory: Invalid data provided for id={Id}", id);
                return BadRequest(new
                {
                    Message = "Invalid data.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var entity = await _context.BloodInventories.FindAsync(id);
            if (entity == null)
            {
                _logger.LogWarning("PutBloodInventory: Inventory with id={Id} not found", id);
                return NotFound(new { Message = $"Blood inventory with id = {id} not found." });
            }

            if (updateDto.BloodTypeId.HasValue)
            {
                var bloodTypeExists = await _context.BloodTypes.AnyAsync(bt => bt.BloodTypeId == updateDto.BloodTypeId.Value);
                if (!bloodTypeExists)
                {
                    _logger.LogWarning("PutBloodInventory: BloodTypeId={BloodTypeId} does not exist for id={Id}", updateDto.BloodTypeId, id);
                    return BadRequest(new { Message = $"Blood type with id = {updateDto.BloodTypeId} does not exist." });
                }
            }

            if (updateDto.BloodComponentId.HasValue)
            {
                var bloodComponentExists = await _context.BloodComponents.AnyAsync(bc => bc.BloodComponentId == updateDto.BloodComponentId.Value);
                if (!bloodComponentExists)
                {
                    _logger.LogWarning("PutBloodInventory: BloodComponentId={BloodComponentId} does not exist for id={Id}", updateDto.BloodComponentId, id);
                    return BadRequest(new { Message = $"Blood component with id = {updateDto.BloodComponentId} does not exist." });
                }
            }

            var updatedFields = new List<string>();

            if (updateDto.BloodTypeId.HasValue && updateDto.BloodTypeId != entity.BloodTypeId)
            {
                entity.BloodTypeId = updateDto.BloodTypeId.Value;
                updatedFields.Add("BloodTypeId");
            }

            if (updateDto.BloodComponentId.HasValue && updateDto.BloodComponentId != entity.BloodComponentId)
            {
                entity.BloodComponentId = updateDto.BloodComponentId.Value;
                updatedFields.Add("BloodComponentId");
            }

            if (updateDto.Quantity.HasValue && updateDto.Quantity != entity.Quantity)
            {
                entity.Quantity = updateDto.Quantity.Value;
                updatedFields.Add("Quantity");
            }

            if (!string.IsNullOrEmpty(updateDto.Unit) && updateDto.Unit != entity.Unit)
            {
                entity.Unit = updateDto.Unit;
                updatedFields.Add("Unit");
            }

            if (updateDto.InventoryLocation != null && updateDto.InventoryLocation != entity.InventoryLocation)
            {
                entity.InventoryLocation = updateDto.InventoryLocation;
                updatedFields.Add("InventoryLocation");
            }

            if (updatedFields.Count > 0)
            {
                entity.LastUpdated = DateTime.UtcNow;

                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("PutBloodInventory: Updated inventory with id={Id}, fields={Fields}", id, string.Join(", ", updatedFields));
                    return Ok(new
                    {
                        Message = "Blood inventory updated successfully.",
                        UpdatedFields = updatedFields
                    });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!BloodInventoryExists(id))
                    {
                        _logger.LogWarning("PutBloodInventory: Inventory with id={Id} no longer exists", id);
                        return NotFound(new { Message = $"Blood inventory with id = {id} not found." });
                    }
                    _logger.LogError(ex, "PutBloodInventory: Concurrency exception for id={Id}", id);
                    return StatusCode(500, new { Message = "Concurrency error occurred.", Detail = ex.Message });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "PutBloodInventory: Exception occurred for id={Id}", id);
                    return StatusCode(500, new { Message = "Error updating blood inventory.", Detail = ex.Message });
                }
            }

            _logger.LogInformation("PutBloodInventory: No fields updated for id={Id}", id);
            return Ok(new
            {
                Message = "No fields were updated.",
                UpdatedFields = new List<string>()
            });
        }

        // DELETE: api/BloodInventories/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBloodInventory(int id)
        {
            _logger.LogInformation("DeleteBloodInventory called with id={Id}", id);

            var entity = await _context.BloodInventories.FindAsync(id);
            if (entity == null)
            {
                _logger.LogWarning("DeleteBloodInventory: Inventory with id={Id} not found", id);
                return NotFound(new { Message = $"Blood inventory with id = {id} not found." });
            }

            try
            {
                _context.BloodInventories.Remove(entity);
                await _context.SaveChangesAsync();
                _logger.LogInformation("DeleteBloodInventory: Deleted inventory with id={Id}", id);
                return Ok(new { Message = "Blood inventory deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteBloodInventory: Exception occurred for id={Id}", id);
                return StatusCode(500, new { Message = "Error deleting blood inventory.", Detail = ex.Message });
            }
        }

        private bool BloodInventoryExists(int id)
        {
            var exists = _context.BloodInventories.Any(e => e.InventoryId == id);
            _logger.LogDebug("BloodInventoryExists check for id={Id}: {Exists}", id, exists);
            return exists;
        }
    }
}