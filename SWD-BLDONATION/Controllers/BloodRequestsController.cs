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
    public class BloodRequestsController : ControllerBase
    {
        private readonly BloodDonationContext _context;

        public BloodRequestsController(BloodDonationContext context)
        {
            _context = context;
        }

        // GET: api/BloodRequests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BloodRequest>>> GetBloodRequests()
        {
            return await _context.BloodRequests.ToListAsync();
        }

        // GET: api/BloodRequests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BloodRequest>> GetBloodRequest(int id)
        {
            var bloodRequest = await _context.BloodRequests.FindAsync(id);

            if (bloodRequest == null)
            {
                return NotFound();
            }

            return bloodRequest;
        }

        // PUT: api/BloodRequests/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBloodRequest(int id, BloodRequest bloodRequest)
        {
            if (id != bloodRequest.BloodRequestId)
            {
                return BadRequest();
            }

            _context.Entry(bloodRequest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BloodRequestExists(id))
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

        // POST: api/BloodRequests
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<BloodRequest>> PostBloodRequest(BloodRequest bloodRequest)
        {
            _context.BloodRequests.Add(bloodRequest);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBloodRequest", new { id = bloodRequest.BloodRequestId }, bloodRequest);
        }

        // DELETE: api/BloodRequests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBloodRequest(int id)
        {
            var bloodRequest = await _context.BloodRequests.FindAsync(id);
            if (bloodRequest == null)
            {
                return NotFound();
            }

            _context.BloodRequests.Remove(bloodRequest);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BloodRequestExists(int id)
        {
            return _context.BloodRequests.Any(e => e.BloodRequestId == id);
        }
    }
}
