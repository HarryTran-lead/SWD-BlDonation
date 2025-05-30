using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD_BLDONATION.DTOs.BloodInventoryDTOs;
using SWD_BLDONATION.Models.Generated;
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

        public BloodInventoriesController(BloodDonationContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/BloodInventories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BloodInventoryDto>>> GetBloodInventories()
        {
            var entities = await _context.BloodInventories.ToListAsync();
            var dtos = _mapper.Map<IEnumerable<BloodInventoryDto>>(entities);
            return Ok(dtos);
        }

        // GET: api/BloodInventories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BloodInventoryDto>> GetBloodInventory(int id)
        {
            var entity = await _context.BloodInventories.FindAsync(id);
            if (entity == null)
                return NotFound();

            var dto = _mapper.Map<BloodInventoryDto>(entity);
            return Ok(dto);
        }

        // PUT: api/BloodInventories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBloodInventory(int id, [FromForm] UpdateBloodInventoryDto updateDto)
        {
            var entity = await _context.BloodInventories.FindAsync(id);
            if (entity == null)
                return NotFound();

            // Map dữ liệu từ DTO vào Entity
            _mapper.Map(updateDto, entity);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BloodInventoryExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/BloodInventories
        [HttpPost]
        public async Task<ActionResult<BloodInventoryDto>> PostBloodInventory([FromForm] CreateBloodInventoryDto createDto)
        {
            var entity = _mapper.Map<BloodInventory>(createDto);
            _context.BloodInventories.Add(entity);
            await _context.SaveChangesAsync();

            var resultDto = _mapper.Map<BloodInventoryDto>(entity);

            return CreatedAtAction(nameof(GetBloodInventory), new { id = entity.InventoryId }, resultDto);
        }

        // DELETE: api/BloodInventories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBloodInventory(int id)
        {
            var entity = await _context.BloodInventories.FindAsync(id);
            if (entity == null)
                return NotFound();

            _context.BloodInventories.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BloodInventoryExists(int id)
        {
            return _context.BloodInventories.Any(e => e.InventoryId == id);
        }
    }
}
