using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD_BLDONATION.DTOs.DonationRequestDTOs;
using SWD_BLDONATION.Models.Generated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SWD_BLDONATION.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonationRequestsController : ControllerBase
    {
        private readonly BloodDonationContext _context;
        private readonly IMapper _mapper;

        public DonationRequestsController(BloodDonationContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/DonationRequests
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DonationRequestDto>>> GetDonationRequests([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var query = _context.DonationRequests.AsQueryable();

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var donationRequests = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(dr => _mapper.Map<DonationRequestDto>(dr))
                .ToListAsync();

            return Ok(new
            {
                Data = donationRequests,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            });
        }

        // GET: api/DonationRequests/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DonationRequestDto>> GetDonationRequest(int id)
        {
            var donationRequest = await _context.DonationRequests.FindAsync(id);
            if (donationRequest == null)
            {
                return NotFound();
            }

            var donationRequestDto = _mapper.Map<DonationRequestDto>(donationRequest);
            return Ok(donationRequestDto);
        }

        // PUT: api/DonationRequests/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDonationRequest(int id, UpdateDonationRequestDto updateDto)
        {
            if (id != updateDto.DonateRequestId)
            {
                return BadRequest("ID mismatch.");
            }

            var donationRequest = await _context.DonationRequests.FindAsync(id);
            if (donationRequest == null)
            {
                return NotFound();
            }

            _mapper.Map(updateDto, donationRequest);
            _context.Entry(donationRequest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.DonationRequests.Any(e => e.DonateRequestId == id))
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

        // POST: api/DonationRequests
        [HttpPost]
        public async Task<ActionResult<DonationRequestDto>> PostDonationRequest(CreateDonationRequestDto createDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var donationRequest = _mapper.Map<DonationRequest>(createDto);
            donationRequest.CreatedAt = DateTime.UtcNow;

            _context.DonationRequests.Add(donationRequest);
            await _context.SaveChangesAsync();

            var resultDto = _mapper.Map<DonationRequestDto>(donationRequest);
            return CreatedAtAction(nameof(GetDonationRequest), new { id = resultDto.DonateRequestId }, resultDto);
        }

        // DELETE: api/DonationRequests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDonationRequest(int id)
        {
            var donationRequest = await _context.DonationRequests.FindAsync(id);
            if (donationRequest == null)
            {
                return NotFound();
            }

            _context.DonationRequests.Remove(donationRequest);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/DonationRequests/search
        [HttpGet("search")]
        public async Task<ActionResult<object>> SearchDonationRequests([FromQuery] DonationRequestSearchQueryDto query)
        {
            var dbQuery = _context.DonationRequests.AsQueryable();

            if (query.UserId.HasValue)
                dbQuery = dbQuery.Where(dr => dr.UserId == query.UserId.Value);
            if (query.BloodTypeId.HasValue)
                dbQuery = dbQuery.Where(dr => dr.BloodTypeId == query.BloodTypeId.Value);
            if (!string.IsNullOrEmpty(query.Status))
                dbQuery = dbQuery.Where(dr => dr.Status.ToLower() == query.Status.Trim().ToLower());
            if (!string.IsNullOrEmpty(query.Location))
                dbQuery = dbQuery.Where(dr => dr.Location.Contains(query.Location.Trim()));
            if (query.PreferredDate.HasValue)
                dbQuery = dbQuery.Where(dr => dr.PreferredDate == query.PreferredDate.Value);
            if (query.CreatedAfter.HasValue)
                dbQuery = dbQuery.Where(dr => dr.CreatedAt >= query.CreatedAfter.Value);
            if (query.CreatedBefore.HasValue)
                dbQuery = dbQuery.Where(dr => dr.CreatedAt <= query.CreatedBefore.Value);
            if (query.QuantityMin.HasValue)
                dbQuery = dbQuery.Where(dr => dr.Quantity >= query.QuantityMin.Value);
            if (query.QuantityMax.HasValue)
                dbQuery = dbQuery.Where(dr => dr.Quantity <= query.QuantityMax.Value);

            // Phân trang đúng cách
            var totalCount = await dbQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            var donationRequests = await dbQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(dr => _mapper.Map<DonationRequestDto>(dr))
                .ToListAsync();

            return Ok(new
            {
                Requests = donationRequests,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = query.Page,
                PageSize = query.PageSize
            });
        }

    }
}
