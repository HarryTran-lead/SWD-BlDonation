using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD_BLDONATION.DTOs;
using SWD_BLDONATION.DTOs.BloodTypeDTOs;
using SWD_BLDONATION.Models.Generated;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWD_BLDONATION.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BloodTypesController : ControllerBase
    {
        private readonly BloodDonationDbContext _context;

        public BloodTypesController(BloodDonationDbContext context)
        {
            _context = context;
        }

        // GET: api/BloodTypes
        [HttpGet]
        [Authorize(Roles = "Admin,Staff,User")]
        public async Task<ActionResult<IEnumerable<BloodTypeDto>>> GetBloodTypes()
        {
            var bloodTypes = await _context.BloodTypes
                .Select(bt => new BloodTypeDto
                {
                    BloodTypeId = bt.BloodTypeId,
                    Name = bt.Name,
                    RhFactor = bt.RhFactor
                })
                .ToListAsync();

            return Ok(bloodTypes);
        }

        // GET: api/BloodTypes/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff,User")]
        public async Task<ActionResult<BloodTypeDto>> GetBloodType(int id)
        {
            var bloodType = await _context.BloodTypes
                .Where(bt => bt.BloodTypeId == id)
                .Select(bt => new BloodTypeDto
                {
                    BloodTypeId = bt.BloodTypeId,
                    Name = bt.Name,
                    RhFactor = bt.RhFactor
                })
                .FirstOrDefaultAsync();

            if (bloodType == null)
                return NotFound();

            return Ok(bloodType);
        }

        // PUT: api/BloodTypes/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutBloodType(int id, [FromForm] UpdateBloodTypeDto dto)
        {
            var bloodType = await _context.BloodTypes.FindAsync(id);
            if (bloodType == null)
                return NotFound();

            bloodType.Name = dto.Name;
            bloodType.RhFactor = dto.RhFactor;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BloodTypeExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/BloodTypes
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BloodTypeDto>> PostBloodType([FromForm] CreateBloodTypeDto dto)
        {
            var bloodType = new BloodType
            {
                Name = dto.Name,
                RhFactor = dto.RhFactor
            };

            _context.BloodTypes.Add(bloodType);
            await _context.SaveChangesAsync();

            var resultDto = new BloodTypeDto
            {
                BloodTypeId = bloodType.BloodTypeId,
                Name = bloodType.Name,
                RhFactor = bloodType.RhFactor
            };

            return CreatedAtAction(nameof(GetBloodType), new { id = bloodType.BloodTypeId }, resultDto);
        }

        // DELETE: api/BloodTypes/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBloodType(int id)
        {
            var bloodType = await _context.BloodTypes.FindAsync(id);
            if (bloodType == null)
                return NotFound();

            _context.BloodTypes.Remove(bloodType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BloodTypeExists(int id)
        {
            return _context.BloodTypes.Any(e => e.BloodTypeId == id);
        }
    }
}
