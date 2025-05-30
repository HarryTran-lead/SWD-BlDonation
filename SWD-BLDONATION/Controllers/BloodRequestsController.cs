using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using SWD_BLDONATION.Models.Generated;
using SWD_BLDONATION.DTOs.BloodRequestDTOs;

namespace SWD_BLDONATION.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BloodRequestsController : ControllerBase
    {
        private readonly BloodDonationContext _context;
        private readonly IMapper _mapper;

        public BloodRequestsController(BloodDonationContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/BloodRequests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BloodRequestDto>>> GetBloodRequests()
        {
            var entities = await _context.BloodRequests.ToListAsync();
            var dtos = _mapper.Map<List<BloodRequestDto>>(entities);
            return Ok(dtos);
        }

        // GET: api/BloodRequests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BloodRequestDto>> GetBloodRequest(int id)
        {
            var entity = await _context.BloodRequests.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            var dto = _mapper.Map<BloodRequestDto>(entity);
            return Ok(dto);
        }

        // POST: api/BloodRequests
        [HttpPost]
        public async Task<ActionResult<BloodRequestDto>> PostBloodRequest(CreateBloodRequestDto createDto)
        {
            var entity = _mapper.Map<BloodRequest>(createDto);
            entity.CreatedAt = System.DateTime.UtcNow;
            entity.Status = "pending";
            entity.Fulfilled = false;

            _context.BloodRequests.Add(entity);
            await _context.SaveChangesAsync();

            var dto = _mapper.Map<BloodRequestDto>(entity);
            return CreatedAtAction(nameof(GetBloodRequest), new { id = dto.BloodRequestId }, dto);
        }

        // PUT: api/BloodRequests/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBloodRequest(int id, UpdateBloodRequestDto updateDto)
        {
            if (id != updateDto.BloodRequestId)
            {
                return BadRequest("ID không khớp");
            }

            var entity = await _context.BloodRequests.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            // Map dữ liệu updateDto lên entity
            _mapper.Map(updateDto, entity);

            _context.Entry(entity).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.BloodRequests.Any(e => e.BloodRequestId == id))
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

        // DELETE: api/BloodRequests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBloodRequest(int id)
        {
            var entity = await _context.BloodRequests.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            _context.BloodRequests.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
