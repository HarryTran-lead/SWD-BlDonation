using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD_BLDONATION.DTOs;
using SWD_BLDONATION.DTOs.BloodComponentDTOs;
using SWD_BLDONATION.Models.Generated;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWD_BLDONATION.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BloodComponentsController : ControllerBase
    {
        private readonly BloodDonationDbContext _context;

        public BloodComponentsController(BloodDonationDbContext context)
        {
            _context = context;
        }

        // GET: api/BloodComponents
        [HttpGet]
        [Authorize(Roles = "Admin,Staff,User")]
        public async Task<ActionResult<IEnumerable<BloodComponentDto>>> GetBloodComponents()
        {
            var list = await _context.BloodComponents
                .Select(bc => new BloodComponentDto
                {
                    BloodComponentId = bc.BloodComponentId,
                    Name = bc.Name
                })
                .ToListAsync();

            return Ok(list);
        }

        // GET: api/BloodComponents/search
        [HttpGet("search")]
        [Authorize(Roles = "Admin,Staff,User")]
        public async Task<ActionResult<object>> SearchBloodComponents(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? bloodComponentId = null,
            [FromQuery] string? name = null)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest(new { Message = "Invalid page or pageSize." });
            }

            var query = _context.BloodComponents.AsQueryable();

            if (bloodComponentId.HasValue)
                query = query.Where(bc => bc.BloodComponentId == bloodComponentId.Value);
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(bc => bc.Name != null && bc.Name.ToLower().Contains(name.Trim().ToLower()));

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var bloodComponents = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(bc => new BloodComponentDto
                {
                    BloodComponentId = bc.BloodComponentId,
                    Name = bc.Name
                })
                .ToListAsync();

            return Ok(new
            {
                Data = bloodComponents,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            });
        }

        // GET: api/BloodComponents/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff,User")]
        public async Task<ActionResult<BloodComponentDto>> GetBloodComponent(int id)
        {
            var bloodComponent = await _context.BloodComponents
                .Where(bc => bc.BloodComponentId == id)
                .Select(bc => new BloodComponentDto
                {
                    BloodComponentId = bc.BloodComponentId,
                    Name = bc.Name
                })
                .FirstOrDefaultAsync();

            if (bloodComponent == null)
                return NotFound();

            return Ok(bloodComponent);
        }

        // PUT: api/BloodComponents/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutBloodComponent(int id, [FromForm] UpdateBloodComponentDto dto)
        {
            var bloodComponent = await _context.BloodComponents.FindAsync(id);
            if (bloodComponent == null)
                return NotFound();

            bloodComponent.Name = dto.Name;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BloodComponentExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/BloodComponents
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BloodComponentDto>> PostBloodComponent([FromForm] CreateBloodComponentDto dto)
        {
            var bloodComponent = new BloodComponent
            {
                Name = dto.Name
            };

            _context.BloodComponents.Add(bloodComponent);
            await _context.SaveChangesAsync();

            var resultDto = new BloodComponentDto
            {
                BloodComponentId = bloodComponent.BloodComponentId,
                Name = bloodComponent.Name
            };

            return CreatedAtAction(nameof(GetBloodComponent), new { id = bloodComponent.BloodComponentId }, resultDto);
        }

        // DELETE: api/BloodComponents/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBloodComponent(int id)
        {
            var bloodComponent = await _context.BloodComponents.FindAsync(id);
            if (bloodComponent == null)
                return NotFound();

            _context.BloodComponents.Remove(bloodComponent);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BloodComponentExists(int id)
        {
            return _context.BloodComponents.Any(e => e.BloodComponentId == id);
        }
    }
}
