using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD_BLDONATION.Models;

namespace SWD_BLDONATION.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BloodComponentsController : ControllerBase
    {
        private readonly BloodDonationContext _context;

        public BloodComponentsController(BloodDonationContext context)
        {
            _context = context;
        }

        // GET: api/BloodComponents
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BloodComponent>>> GetBloodComponents()
        {
            return await _context.BloodComponents.ToListAsync();
        }

        // GET: api/BloodComponents/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BloodComponent>> GetBloodComponent(int id)
        {
            var bloodComponent = await _context.BloodComponents.FindAsync(id);

            if (bloodComponent == null)
            {
                return NotFound();
            }

            return bloodComponent;
        }

        // PUT: api/BloodComponents/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBloodComponent(int id, BloodComponent bloodComponent)
        {
            if (id != bloodComponent.BloodComponentId)
            {
                return BadRequest();
            }

            _context.Entry(bloodComponent).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BloodComponentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/BloodComponents
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<BloodComponent>> PostBloodComponent(BloodComponent bloodComponent)
        {
            _context.BloodComponents.Add(bloodComponent);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBloodComponent", new { id = bloodComponent.BloodComponentId }, bloodComponent);
        }

        // DELETE: api/BloodComponents/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBloodComponent(int id)
        {
            var bloodComponent = await _context.BloodComponents.FindAsync(id);
            if (bloodComponent == null)
            {
                return NotFound();
            }

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
