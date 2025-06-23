using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD_BLDONATION.DTOs.BloodComponentDTOs;
using SWD_BLDONATION.DTOs.BloodRequestDTOs;
using SWD_BLDONATION.DTOs.BloodTypeDTOs;
using SWD_BLDONATION.DTOs.DonationRequestDTOs;
using SWD_BLDONATION.DTOs.UserDTOs;
using SWD_BLDONATION.Models.Generated;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
        public async Task<IActionResult> PutDonationRequest(int id, [FromBody] UpdateDonationRequestDto updateDto)
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
        public async Task<ActionResult<DonationRequestDto>> PostDonationRequest([FromBody] CreateDonationRequestDto createDto)
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

        [HttpPost("search")]
        public async Task<ActionResult<object>> SearchDonationRequests([FromForm] DonationRequestSearchQueryDto query)
        {
            // Validate pagination parameters
            if (query.Page < 1 || query.PageSize < 1)
            {
                return BadRequest(new { message = "Invalid page or pageSize." });
            }

            // Build query with related entities
            var dbQuery = _context.DonationRequests
                .Include(dr => dr.BloodType)
                .Include(dr => dr.BloodComponent)
                .Include(dr => dr.User)
                .AsQueryable();

            if (query.UserId.HasValue)
                dbQuery = dbQuery.Where(dr => dr.UserId == query.UserId.Value);
            if (query.BloodTypeId.HasValue)
                dbQuery = dbQuery.Where(dr => dr.BloodTypeId == query.BloodTypeId.Value);
            if (!string.IsNullOrWhiteSpace(query.Status))
                dbQuery = dbQuery.Where(dr => dr.Status != null && dr.Status.ToLower() == query.Status.Trim().ToLower());
            if (!string.IsNullOrWhiteSpace(query.Location))
                dbQuery = dbQuery.Where(dr => dr.Location != null && dr.Location.Contains(query.Location.Trim()));
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

            // Pagination logic
            var totalCount = await dbQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            var donationRequests = await dbQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            // Manual mapping to DonationRequestDto
            var donationRequestDtos = donationRequests.Select(dr => new DonationRequestDto
            {
                DonateRequestId = dr.DonateRequestId,
                UserId = dr.UserId,
                BloodTypeId = dr.BloodTypeId,
                BloodComponentId = dr.BloodComponentId,
                PreferredDate = dr.PreferredDate,
                Status = dr.Status,
                Location = dr.Location,
                CreatedAt = dr.CreatedAt,
                Quantity = dr.Quantity,
                Note = dr.Note,
                HeightCm = dr.HeightCm,
                WeightKg = dr.WeightKg,
                LastDonationDate = dr.LastDonationDate,
                HealthInfo = dr.HealthInfo,
                BloodType = dr.BloodType != null ? new BloodTypeDto
                {
                    BloodTypeId = dr.BloodType.BloodTypeId,
                    Name = dr.BloodType.Name,
                    RhFactor = dr.BloodType.RhFactor
                } : null,
                BloodComponent = dr.BloodComponent != null ? new BloodComponentDto
                {
                    BloodComponentId = dr.BloodComponent.BloodComponentId,
                    Name = dr.BloodComponent.Name
                } : null,
                User = dr.User != null ? new UserDto
                {
                    UserId = dr.User.UserId,
                    UserName = dr.User.UserName,
                    Name = dr.User.Name,
                    Email = dr.User.Email,
                    Phone = dr.User.Phone,
                    DateOfBirth = dr.User.DateOfBirth,
                    Address = dr.User.Address,
                    Identification = dr.User.Identification,
                    StatusBit = dr.User.StatusBit ?? 1,
                    RoleBit = dr.User.RoleBit ?? 0,
                    HeightCm = dr.User.HeightCm,
                    WeightKg = dr.User.WeightKg,
                    MedicalHistory = dr.User.MedicalHistory,
                    BloodTypeId = dr.User.BloodTypeId,
                    BloodComponentId = dr.User.BloodComponentId,
                    IsDeleted = dr.User.IsDeleted
                } : null
            }).ToList();

            return Ok(new
            {
                Requests = donationRequestDtos,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = query.Page,
                PageSize = query.PageSize
            });
        }
    }
}
