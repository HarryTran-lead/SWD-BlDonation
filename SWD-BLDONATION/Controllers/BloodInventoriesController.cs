using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SWD_BLDONATION.DTOs.BloodInventoryDTOs;
using SWD_BLDONATION.Models.Generated;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SWD_BLDONATION.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BloodInventoriesController : ControllerBase
    {
        private readonly BloodDonationContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<BloodInventoriesController> _logger;

        public BloodInventoriesController(BloodDonationContext context, IMapper mapper, ILogger<BloodInventoriesController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/BloodInventories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BloodInventoryDto>>> GetBloodInventories()
        {
            _logger.LogInformation("GetBloodInventories called");
            var entities = await _context.BloodInventories.ToListAsync();
            var dtos = _mapper.Map<IEnumerable<BloodInventoryDto>>(entities);
            _logger.LogInformation("GetBloodInventories returned {Count} items", dtos == null ? 0 : (dtos as ICollection<BloodInventoryDto>)?.Count ?? 0);
            return Ok(dtos);
        }

        // GET: api/BloodInventories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BloodInventoryDto>> GetBloodInventory(int id)
        {
            _logger.LogInformation("GetBloodInventory called with id={Id}", id);
            var entity = await _context.BloodInventories.FindAsync(id);
            if (entity == null)
            {
                _logger.LogWarning("GetBloodInventory: Inventory with id={Id} not found", id);
                return NotFound();
            }

            var dto = _mapper.Map<BloodInventoryDto>(entity);
            _logger.LogInformation("GetBloodInventory: Found inventory with id={Id}", id);
            return Ok(dto);
        }

        // PUT: api/BloodInventories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBloodInventory(int id, [FromForm] UpdateBloodInventoryDto updateDto)
        {
            _logger.LogInformation("PutBloodInventory called with id={Id}", id);
            var entity = await _context.BloodInventories.FindAsync(id);
            if (entity == null)
            {
                _logger.LogWarning("PutBloodInventory: Inventory with id={Id} not found", id);
                return NotFound();
            }

            _mapper.Map(updateDto, entity);

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("PutBloodInventory: Updated inventory with id={Id}", id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!BloodInventoryExists(id))
                {
                    _logger.LogWarning("PutBloodInventory: Inventory with id={Id} no longer exists", id);
                    return NotFound();
                }
                else
                {
                    _logger.LogError(ex, "PutBloodInventory: Concurrency exception for id={Id}", id);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PutBloodInventory: Exception occurred for id={Id}", id);
                throw;
            }

            return NoContent();
        }

        // POST: api/BloodInventories
        [HttpPost]
        public async Task<ActionResult<BloodInventoryDto>> PostBloodInventory([FromForm] CreateBloodInventoryDto createDto)
        {
            _logger.LogInformation("PostBloodInventory called with data: {@CreateDto}", createDto);

            try
            {
                var entity = _mapper.Map<BloodInventory>(createDto);
                _context.BloodInventories.Add(entity);
                await _context.SaveChangesAsync();

                var resultDto = _mapper.Map<BloodInventoryDto>(entity);

                _logger.LogInformation("PostBloodInventory: Created inventory with id={InventoryId}", entity.InventoryId);

                return CreatedAtAction(nameof(GetBloodInventory), new { id = entity.InventoryId }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PostBloodInventory: Exception occurred while creating inventory");
                throw;
            }
        }

        // DELETE: api/BloodInventories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBloodInventory(int id)
        {
            _logger.LogInformation("DeleteBloodInventory called with id={Id}", id);
            var entity = await _context.BloodInventories.FindAsync(id);
            if (entity == null)
            {
                _logger.LogWarning("DeleteBloodInventory: Inventory with id={Id} not found", id);
                return NotFound();
            }

            _context.BloodInventories.Remove(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("DeleteBloodInventory: Deleted inventory with id={Id}", id);
            return NoContent();
        }

        private bool BloodInventoryExists(int id)
        {
            var exists = _context.BloodInventories.Any(e => e.InventoryId == id);
            _logger.LogDebug("BloodInventoryExists check for id={Id}: {Exists}", id, exists);
            return exists;
        }
    }
}
