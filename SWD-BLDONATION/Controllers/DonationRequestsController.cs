﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SWD_BLDONATION.DTOs.BloodComponentDTOs;
using SWD_BLDONATION.DTOs.BloodTypeDTOs;
using SWD_BLDONATION.DTOs.DonationRequestDTOs;
using SWD_BLDONATION.DTOs.UserDTOs;
using SWD_BLDONATION.Models.Enums;
using SWD_BLDONATION.Models.Generated;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SWD_BLDONATION.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonationRequestsController : ControllerBase
    {
        private readonly BloodDonationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<DonationRequestsController> _logger;

        public DonationRequestsController(BloodDonationDbContext context, IMapper mapper, ILogger<DonationRequestsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // Helper method to map byte? Status to string
        private static string? MapStatusToString(byte? status)
        {
            return status switch
            {
                0 => "Pending",
                1 => "Successful",
                2 => "Cancelled",
                _ => null
            };
        }

        // GET: api/DonationRequests
        [HttpGet]
        public async Task<ActionResult<object>> GetDonationRequests([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest(new { Message = "Invalid page or pageSize." });
            }

            var query = _context.DonationRequests
                .Include(dr => dr.BloodType)
                .Include(dr => dr.BloodComponent)
                .Include(dr => dr.User)
                .AsQueryable();

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var donationRequests = await query
                .OrderBy(dr => dr.Status == 0 ? 0 : dr.Status == 1 ? 1 : 2)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var donationRequestDtos = donationRequests.Select(dr => new DonationRequestDto
            {
                DonateRequestId = dr.DonateRequestId,
                UserId = dr.UserId,
                BloodTypeId = dr.BloodTypeId,
                BloodComponentId = dr.BloodComponentId,
                PreferredDate = dr.PreferredDate,
                StatusName = MapStatusToString(dr.Status),
                Status = dr.Status,
                Location = dr.Location,
                CreatedAt = dr.CreatedAt,
                Quantity = dr.Quantity,
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
                    UserName = dr.User.UserName ?? "Unknown",
                    Name = dr.User.Name ?? "Unknown",
                    Email = dr.User.Email ?? "No Email Provided",
                    Phone = dr.User.Phone ?? "No Phone Provided",
                    DateOfBirth = dr.User.DateOfBirth ?? DateOnly.FromDateTime(DateTime.UtcNow),
                    Address = dr.User.Address ?? "No Address Provided",
                    Identification = dr.User.Identification ?? "No Identification Provided",
                    IsDeleted = dr.User.IsDeleted,
                    RoleBit = dr.User.RoleBit ?? 0,
                    StatusBit = dr.User.StatusBit ?? 0,
                    HeightCm = dr.User.HeightCm ?? 0,
                    WeightKg = dr.User.WeightKg ?? 0,
                    MedicalHistory = dr.User.MedicalHistory ?? "No Medical History Provided",
                    BloodTypeId = dr.User.BloodTypeId ?? dr.BloodTypeId,
                    BloodComponentId = dr.User.BloodComponentId ?? dr.BloodComponentId
                } : null
            }).ToList();

            return Ok(new
            {
                Data = donationRequestDtos,
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
            var donationRequest = await _context.DonationRequests
                .Include(dr => dr.BloodType)
                .Include(dr => dr.BloodComponent)
                .Include(dr => dr.User)
                .FirstOrDefaultAsync(dr => dr.DonateRequestId == id);

            if (donationRequest == null)
            {
                return NotFound(new { Message = "Donation request not found." });
            }

            var donationRequestDto = _mapper.Map<DonationRequestDto>(donationRequest);
            donationRequestDto.StatusName = MapStatusToString(donationRequest.Status);
            donationRequestDto.Status = donationRequest.Status;
            return Ok(donationRequestDto);
        }

        // PUT: api/DonationRequests/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDonationRequest(int id, [FromBody] UpdateDonationRequestDto updateDto)
        {
            if (id != updateDto.DonateRequestId)
            {
                return BadRequest(new { Message = "ID mismatch." });
            }

            var donationRequest = await _context.DonationRequests.FindAsync(id);
            if (donationRequest == null)
            {
                return NotFound(new { Message = "Donation request not found." });
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
                    return NotFound(new { Message = "Donation request not found." });
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
        public async Task<ActionResult<DonationRequestDto>> PostDonationRequest([FromForm] CreateDonationRequestDto createDto)
        {
            _logger.LogInformation("PostDonationRequest called with data: {@CreateDto}", createDto);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("PostDonationRequest: Invalid data provided");
                return BadRequest(new
                {
                    Message = "Invalid data submitted.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var donationRequest = _mapper.Map<DonationRequest>(createDto);
            donationRequest.CreatedAt = DateTime.UtcNow;

            _context.DonationRequests.Add(donationRequest);

            try
            {
                await _context.SaveChangesAsync();

                if (donationRequest.UserId.HasValue)
                {
                    var notification = new Notification
                    {
                        UserId = donationRequest.UserId,
                        Message = $"Your donation request (ID: {donationRequest.DonateRequestId}) has been successfully created and is pending approval.",
                        Type = "DonationRequest",
                        Status = "Unread",
                        SentAt = DateTime.UtcNow
                    };

                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Notification created for userId={UserId} for donation request id={DonateRequestId}", donationRequest.UserId, donationRequest.DonateRequestId);
                }
                else
                {
                    _logger.LogWarning("No UserId provided for notification creation for donation request id={DonateRequestId}", donationRequest.DonateRequestId);
                }

                var resultDto = _mapper.Map<DonationRequestDto>(donationRequest);
                resultDto.StatusName = MapStatusToString(donationRequest.Status);
                resultDto.Status = donationRequest.Status;

                return CreatedAtAction(nameof(GetDonationRequest), new { id = resultDto.DonateRequestId }, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating donation request or notification");
                return StatusCode(500, new { Message = "An error occurred while processing the request." });
            }
        }


        // DELETE: api/DonationRequests/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDonationRequest(int id)
        {
            var donationRequest = await _context.DonationRequests.FindAsync(id);
            if (donationRequest == null)
            {
                return NotFound(new { Message = "Donation request not found." });
            }

            _context.DonationRequests.Remove(donationRequest);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/DonationRequests/status
        [HttpPatch("status")]
        public async Task<IActionResult> UpdateDonationRequestStatus([FromBody] UpdateDonationRequestStatusDto dto)
        {
            _logger.LogInformation("UpdateDonationRequestStatus called with id={Id}, status={Status}", dto.Id, dto.Status);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid data provided for UpdateDonationRequestStatus: id={Id}", dto.Id);
                return BadRequest(new
                {
                    Message = "Invalid data submitted.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            if (!Enum.IsDefined(typeof(BloodRequestStatus), dto.Status))
            {
                _logger.LogWarning("Invalid status value: id={Id}, status={Status}", dto.Id, dto.Status);
                return BadRequest(new { Message = "Invalid status value." });
            }

            var donationRequest = await _context.DonationRequests.FindAsync(dto.Id);
            if (donationRequest == null)
            {
                _logger.LogWarning("Donation request not found: id={Id}", dto.Id);
                return NotFound(new { Message = "Donation request not found" });
            }

            donationRequest.Status = (byte)dto.Status;
            _context.Entry(donationRequest).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Donation request status updated successfully: id={Id}, status={Status}", dto.Id, dto.Status);
            return Ok(new { Message = "Donation request status updated successfully." });
        }

        // GET: api/DonationRequests/search
        [HttpGet("search")]
        public async Task<ActionResult<object>> SearchDonationRequests(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? userId = null,
            [FromQuery] int? bloodTypeId = null,
            [FromQuery] int? bloodComponentId = null,
            [FromQuery] string? status = null,
            [FromQuery] string? location = null,
            [FromQuery] DateOnly? preferredDate = null,
            [FromQuery] DateTime? createdAfter = null,
            [FromQuery] DateTime? createdBefore = null,
            [FromQuery] int? quantityMin = null,
            [FromQuery] int? quantityMax = null)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest(new { Message = "Invalid page or pageSize." });
            }

            var query = _context.DonationRequests
                .Include(dr => dr.BloodType)
                .Include(dr => dr.BloodComponent)
                .Include(dr => dr.User)
                .AsQueryable();

            // Apply search filters
            if (userId.HasValue)
                query = query.Where(dr => dr.UserId == userId.Value);
            if (bloodTypeId.HasValue)
                query = query.Where(dr => dr.BloodTypeId == bloodTypeId.Value);
            if (bloodComponentId.HasValue)
                query = query.Where(dr => dr.BloodComponentId == bloodComponentId.Value);
            if (!string.IsNullOrWhiteSpace(status))
            {
                byte? statusByte = status.Trim().ToLowerInvariant() switch
                {
                    "pending" => 0,
                    "successful" => 1,
                    "cancelled" => 2,
                    _ => null
                };
                if (statusByte.HasValue)
                    query = query.Where(dr => dr.Status == statusByte.Value);
            }
            if (!string.IsNullOrWhiteSpace(location))
                query = query.Where(dr => dr.Location != null && dr.Location.Contains(location.Trim(), StringComparison.OrdinalIgnoreCase));
            if (preferredDate.HasValue)
                query = query.Where(dr => dr.PreferredDate == preferredDate.Value);
            if (createdAfter.HasValue)
                query = query.Where(dr => dr.CreatedAt >= createdAfter.Value);
            if (createdBefore.HasValue)
                query = query.Where(dr => dr.CreatedAt <= createdBefore.Value);
            if (quantityMin.HasValue)
                query = query.Where(dr => dr.Quantity >= quantityMin.Value);
            if (quantityMax.HasValue)
                query = query.Where(dr => dr.Quantity <= quantityMax.Value);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var donationRequests = await query
                .OrderBy(dr => dr.Status == 0 ? 0 : dr.Status == 1 ? 1 : 2)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var donationRequestDtos = donationRequests.Select(dr => new DonationRequestDto
            {
                DonateRequestId = dr.DonateRequestId,
                UserId = dr.UserId,
                BloodTypeId = dr.BloodTypeId,
                BloodComponentId = dr.BloodComponentId,
                PreferredDate = dr.PreferredDate,
                StatusName = MapStatusToString(dr.Status),
                Status = dr.Status,
                Location = dr.Location,
                CreatedAt = dr.CreatedAt,
                Quantity = dr.Quantity,
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
                    UserName = dr.User.UserName ?? "Unknown",
                    Name = dr.User.Name ?? "Unknown",
                    Email = dr.User.Email ?? "No Email Provided",
                    Phone = dr.User.Phone ?? "No Phone Provided",
                    DateOfBirth = dr.User.DateOfBirth ?? DateOnly.FromDateTime(DateTime.UtcNow),
                    Address = dr.User.Address ?? "No Address Provided",
                    Identification = dr.User.Identification ?? "No Identification Provided",
                    IsDeleted = dr.User.IsDeleted,
                    RoleBit = dr.User.RoleBit ?? 0,
                    StatusBit = dr.User.StatusBit ?? 0,
                    HeightCm = dr.User.HeightCm ?? 0,
                    WeightKg = dr.User.WeightKg ?? 0,
                    MedicalHistory = dr.User.MedicalHistory ?? "No Medical History Provided",
                    BloodTypeId = dr.User.BloodTypeId ?? dr.BloodTypeId,
                    BloodComponentId = dr.User.BloodComponentId ?? dr.BloodComponentId
                } : null
            }).ToList();

            return Ok(new
            {
                Requests = donationRequestDtos,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            });
        }

        // GET: api/DonationRequests/ByUser/search/{userId}
        [HttpGet("ByUser/search/{userId}")]
        public async Task<ActionResult<object>> SearchDonationRequestsByUser(
            int userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? bloodTypeId = null,
            [FromQuery] int? bloodComponentId = null,
            [FromQuery] string? status = null,
            [FromQuery] string? location = null,
            [FromQuery] DateOnly? preferredDate = null,
            [FromQuery] DateTime? createdAfter = null,
            [FromQuery] DateTime? createdBefore = null,
            [FromQuery] int? quantityMin = null,
            [FromQuery] int? quantityMax = null)
        {
            if (userId < 1 || page < 1 || pageSize < 1)
            {
                return BadRequest(new { Message = "Invalid userId, page, or pageSize." });
            }

            var query = _context.DonationRequests
                .Where(dr => dr.UserId == userId)
                .Include(dr => dr.BloodType)
                .Include(dr => dr.BloodComponent)
                .Include(dr => dr.User)
                .AsQueryable();

            // Apply search filters
            if (bloodTypeId.HasValue)
                query = query.Where(dr => dr.BloodTypeId == bloodTypeId.Value);
            if (bloodComponentId.HasValue)
                query = query.Where(dr => dr.BloodComponentId == bloodComponentId.Value);
            if (!string.IsNullOrWhiteSpace(status))
            {
                byte? statusByte = status.Trim().ToLowerInvariant() switch
                {
                    "pending" => 0,
                    "successful" => 1,
                    "cancelled" => 2,
                    _ => null
                };
                if (statusByte.HasValue)
                    query = query.Where(dr => dr.Status == statusByte.Value);
            }
            if (!string.IsNullOrWhiteSpace(location))
                query = query.Where(dr => dr.Location != null && dr.Location.Contains(location.Trim(), StringComparison.OrdinalIgnoreCase));
            if (preferredDate.HasValue)
                query = query.Where(dr => dr.PreferredDate == preferredDate.Value);
            if (createdAfter.HasValue)
                query = query.Where(dr => dr.CreatedAt >= createdAfter.Value);
            if (createdBefore.HasValue)
                query = query.Where(dr => dr.CreatedAt <= createdBefore.Value);
            if (quantityMin.HasValue)
                query = query.Where(dr => dr.Quantity >= quantityMin.Value);
            if (quantityMax.HasValue)
                query = query.Where(dr => dr.Quantity <= quantityMax.Value);

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var donationRequests = await query
                .OrderBy(dr => dr.Status == 0 ? 0 : dr.Status == 1 ? 1 : 2)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var donationRequestDtos = donationRequests.Select(dr => new DonationRequestDto
            {
                DonateRequestId = dr.DonateRequestId,
                UserId = dr.UserId,
                BloodTypeId = dr.BloodTypeId,
                BloodComponentId = dr.BloodComponentId,
                PreferredDate = dr.PreferredDate,
                StatusName = MapStatusToString(dr.Status),
                Status = dr.Status,
                Location = dr.Location,
                CreatedAt = dr.CreatedAt,
                Quantity = dr.Quantity,
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
                    UserName = dr.User.UserName ?? "Unknown",
                    Name = dr.User.Name ?? "Unknown",
                    Email = dr.User.Email ?? "No Email Provided",
                    Phone = dr.User.Phone ?? "No Phone Provided",
                    DateOfBirth = dr.User.DateOfBirth ?? DateOnly.FromDateTime(DateTime.UtcNow),
                    Address = dr.User.Address ?? "No Address Provided",
                    Identification = dr.User.Identification ?? "No Identification Provided",
                    IsDeleted = dr.User.IsDeleted,
                    RoleBit = dr.User.RoleBit ?? 0,
                    StatusBit = dr.User.StatusBit ?? 0,
                    HeightCm = dr.User.HeightCm ?? 0,
                    WeightKg = dr.User.WeightKg ?? 0,
                    MedicalHistory = dr.User.MedicalHistory ?? "No Medical History Provided",
                    BloodTypeId = dr.User.BloodTypeId ?? dr.BloodTypeId,
                    BloodComponentId = dr.User.BloodComponentId ?? dr.BloodComponentId
                } : null
            }).ToList();

            return Ok(new
            {
                Requests = donationRequestDtos,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            });
        }
    }
}