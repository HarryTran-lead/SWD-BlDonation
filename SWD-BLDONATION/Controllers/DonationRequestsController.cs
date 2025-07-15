using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SWD_BLDONATION.DTOs.BloodComponentDTOs;
using SWD_BLDONATION.DTOs.BloodRequestDTOs;
using SWD_BLDONATION.DTOs.BloodTypeDTOs;
using SWD_BLDONATION.DTOs.DonationRequestDTOs;
using SWD_BLDONATION.DTOs.UserDTOs;
using SWD_BLDONATION.Models.Enums;
using SWD_BLDONATION.Models.Generated;
using SWD_BLDONATION.Provider;

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
                (byte)DonationRequestStatus.Pending => "Pending",
                (byte)DonationRequestStatus.Successful => "Successful",
                (byte)DonationRequestStatus.Cancelled => "Cancelled",
                (byte)DonationRequestStatus.Done => "Done",
                (byte)DonationRequestStatus.Stocked => "Stocked",
                _ => null
            };
        }

        // GET: api/DonationRequests
        [HttpGet]
        public async Task<ActionResult<object>> GetDonationRequests([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("GetDonationRequests called with page={Page}, pageSize={PageSize}", page, pageSize);

            if (page < 1 || pageSize < 1)
            {
                _logger.LogWarning("Invalid parameters: page={Page}, pageSize={PageSize}", page, pageSize);
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
                .OrderBy(dr => dr.Status == (byte)DonationRequestStatus.Pending ? 0 : dr.Status == (byte)DonationRequestStatus.Successful ? 1 : 2)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var donationRequestDtos = donationRequests.Select(dr => new DonationRequestDto
            {
                DonateRequestId = dr.DonateRequestId,
                UserId = dr.UserId,
                Name = dr.Name,
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
                Phone = dr.Phone,
                DateOfBirth = dr.DateOfBirth,
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
                    DateOfBirth = dr.User.DateOfBirth,
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

            _logger.LogInformation("Retrieved {Count} donation requests", donationRequestDtos.Count);

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
            _logger.LogInformation("GetDonationRequest called with id={Id}", id);

            var donationRequest = await _context.DonationRequests
                .Include(dr => dr.BloodType)
                .Include(dr => dr.BloodComponent)
                .Include(dr => dr.User)
                .FirstOrDefaultAsync(dr => dr.DonateRequestId == id);

            if (donationRequest == null)
            {
                _logger.LogWarning("Donation request not found: id={Id}", id);
                return NotFound(new { Message = "Donation request not found." });
            }

            var donationRequestDto = _mapper.Map<DonationRequestDto>(donationRequest);
            donationRequestDto.StatusName = MapStatusToString(donationRequest.Status);
            donationRequestDto.Status = donationRequest.Status;
            donationRequestDto.Phone = donationRequest.Phone;
            donationRequestDto.DateOfBirth = donationRequest.DateOfBirth;

            _logger.LogInformation("Retrieved donation request: id={Id}", id);
            return Ok(donationRequestDto);
        }

        // PUT: api/DonationRequests/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDonationRequest(int id, [FromForm] UpdateDonationRequestDto dto)
        {
            _logger.LogInformation("PutDonationRequest called with id={Id}, data={@Dto}", id, dto);

            if (dto.DonateRequestId > 0 && id != dto.DonateRequestId)
            {
                _logger.LogWarning("ID mismatch: route id={Id}, DTO DonateRequestId={DonateRequestId}", id, dto.DonateRequestId);
                return BadRequest(new { Message = "DonateRequestId in the body must match the ID in the route." });
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid data provided for PutDonationRequest: id={Id}, errors={@Errors}", id, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return BadRequest(new
                {
                    Message = "Invalid data submitted.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var donationRequest = await _context.DonationRequests.FindAsync(id);
            if (donationRequest == null)
            {
                _logger.LogWarning("Donation request not found: id={Id}", id);
                return NotFound(new { Message = "Donation request not found." });
            }

            var originalUserId = donationRequest.UserId;
            var originalBloodTypeId = donationRequest.BloodTypeId;
            var originalBloodComponentId = donationRequest.BloodComponentId;
            var originalQuantity = donationRequest.Quantity;

            _mapper.Map(dto, donationRequest);

            var updatedFields = new List<string>();
            var skippedFields = new List<string>();

            if (dto.UserId.HasValue)
            {
                if (dto.UserId.Value > 0)
                {
                    donationRequest.UserId = dto.UserId.Value;
                    updatedFields.Add("UserId");
                }
                else
                {
                    donationRequest.UserId = originalUserId;
                    skippedFields.Add($"UserId (value: {dto.UserId.Value})");
                }
            }
            if (dto.BloodTypeId.HasValue)
            {
                if (dto.BloodTypeId.Value > 0)
                {
                    donationRequest.BloodTypeId = dto.BloodTypeId.Value;
                    updatedFields.Add("BloodTypeId");
                }
                else
                {
                    donationRequest.BloodTypeId = originalBloodTypeId;
                    skippedFields.Add($"BloodTypeId (value: {dto.BloodTypeId.Value})");
                }
            }
            if (dto.BloodComponentId.HasValue)
            {
                if (dto.BloodComponentId.Value > 0)
                {
                    donationRequest.BloodComponentId = dto.BloodComponentId.Value;
                    updatedFields.Add("BloodComponentId");
                }
                else
                {
                    donationRequest.BloodComponentId = originalBloodComponentId;
                    skippedFields.Add($"BloodComponentId (value: {dto.BloodComponentId.Value})");
                }
            }
            if (dto.Quantity.HasValue)
            {
                if (dto.Quantity.Value > 0)
                {
                    donationRequest.Quantity = dto.Quantity.Value;
                    updatedFields.Add("Quantity");
                }
                else
                {
                    donationRequest.Quantity = originalQuantity;
                    skippedFields.Add($"Quantity (value: {dto.Quantity.Value})");
                }
            }

            if (dto.DonateRequestId > 0) updatedFields.Add("DonateRequestId");
            if (dto.PreferredDate.HasValue) updatedFields.Add("PreferredDate");
            if (dto.Location != null) updatedFields.Add("Location");
            if (dto.Status.HasValue) updatedFields.Add("Status");
            if (dto.HeightCm.HasValue) updatedFields.Add("HeightCm");
            if (dto.WeightKg.HasValue) updatedFields.Add("WeightKg");
            if (dto.HealthInfo != null) updatedFields.Add("HealthInfo");
            if (dto.LastDonationDate.HasValue) updatedFields.Add("LastDonationDate");
            if (dto.Phone != null) updatedFields.Add("Phone");
            if (dto.DateOfBirth.HasValue) updatedFields.Add("DateOfBirth");

            _context.Entry(donationRequest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Donation request updated successfully: id={Id}", id);

                var updatedDonationRequest = await _context.DonationRequests
                    .AsNoTracking()
                    .FirstOrDefaultAsync(dr => dr.DonateRequestId == id);

                if (updatedDonationRequest == null)
                {
                    _logger.LogError("Updated donation request not found after save: id={Id}", id);
                    return StatusCode(500, new { Message = "Failed to verify updated donation request." });
                }

                _logger.LogInformation("Verified updated donation request id={Id}: UserId={UserId}, BloodTypeId={BloodTypeId}, BloodComponentId={BloodComponentId}, Quantity={Quantity}, Location={Location}, Status={Status}, Phone={Phone}, DateOfBirth={DateOfBirth}",
                    id, updatedDonationRequest.UserId, updatedDonationRequest.BloodTypeId, updatedDonationRequest.BloodComponentId, updatedDonationRequest.Quantity, updatedDonationRequest.Location, updatedDonationRequest.Status, updatedDonationRequest.Phone, updatedDonationRequest.DateOfBirth);

                if (updatedFields.Any())
                {
                    _logger.LogInformation("Fields updated for donation request id={Id}: {Fields}", id, string.Join(", ", updatedFields));
                }
                if (skippedFields.Any())
                {
                    _logger.LogInformation("Fields skipped (value 0) for donation request id={Id}: {Fields}", id, string.Join(", ", skippedFields));
                }
                if (!updatedFields.Any() && !skippedFields.Any())
                {
                    _logger.LogInformation("No fields provided for update for donation request id={Id}", id);
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!_context.DonationRequests.Any(dr => dr.DonateRequestId == id))
                {
                    _logger.LogWarning("Concurrency issue: Donation request no longer exists: id={Id}", id);
                    return NotFound(new { Message = "Donation request not found." });
                }
                _logger.LogError(ex, "Concurrency error updating donation request: id={Id}", id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating donation request: id={Id}", id);
                return StatusCode(500, new { Message = "An error occurred while updating the donation request." });
            }

            return Ok(new { Message = "Donation request updated successfully." });
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
                        SentAt = VietnamDateTimeProvider.Now
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
                resultDto.Phone = donationRequest.Phone;
                resultDto.DateOfBirth = donationRequest.DateOfBirth;

                _logger.LogInformation("Donation request created successfully: id={Id}", donationRequest.DonateRequestId);
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
            _logger.LogInformation("DeleteDonationRequest called with id={Id}", id);

            var donationRequest = await _context.DonationRequests.FindAsync(id);
            if (donationRequest == null)
            {
                _logger.LogWarning("Donation request not found: id={Id}", id);
                return NotFound(new { Message = "Donation request not found." });
            }

            donationRequest.Status = (byte)DonationRequestStatus.Cancelled;
            _context.Entry(donationRequest).State = EntityState.Modified;

            try
            {
                if (donationRequest.UserId.HasValue)
                {
                    var notification = new Notification
                    {
                        UserId = donationRequest.UserId.Value,
                        Message = $"Your donation request (ID: {donationRequest.DonateRequestId}) has been cancelled.",
                        Type = "DonationRequest",
                        Status = "Unread",
                        SentAt = VietnamDateTimeProvider.Now
                    };
                    _context.Notifications.Add(notification);
                    _logger.LogInformation("Notification created for userId={UserId} for cancelled donation request id={Id}", donationRequest.UserId, id);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Donation request status updated to Cancelled: id={Id}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while cancelling donation request or creating notification: id={Id}", id);
                return StatusCode(500, new { Message = "An error occurred while processing the request." });
            }
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

            if (!Enum.IsDefined(typeof(DonationRequestStatus), dto.Status))
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

            try
            {
                if (donationRequest.UserId.HasValue)
                {
                    string notificationMessage = null;

                    switch ((DonationRequestStatus)dto.Status)
                    {
                        case DonationRequestStatus.Successful:
                            notificationMessage = "Your donation request has been approved.";
                            break;
                        case DonationRequestStatus.Cancelled:
                            notificationMessage = "Your donation request has been cancelled.";
                            break;
                        case DonationRequestStatus.Pending:
                            notificationMessage = "Your donation request is pending review.";
                            break;
                        case DonationRequestStatus.Done:
                            notificationMessage = "Thank you for your donation! Your blood has been successfully donated at the hospital.";
                            break;
                    }

                    if (!string.IsNullOrEmpty(notificationMessage))
                    {
                        var notification = new Notification
                        {
                            UserId = donationRequest.UserId.Value,
                            Message = notificationMessage,
                            Type = "DonationRequest",
                            Status = "Unread",
                            SentAt = VietnamDateTimeProvider.Now
                        };

                        _context.Notifications.Add(notification);
                        _logger.LogInformation("Notification created for userId={UserId} with message='{Message}'",
                            donationRequest.UserId.Value, notificationMessage);
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Donation request status updated successfully: id={Id}, status={Status}", dto.Id, dto.Status);
                return Ok(new { Message = "Donation request status updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating donation request status or creating notification: id={Id}", dto.Id);
                return StatusCode(500, new { Message = "An error occurred while processing the donation request." });
            }
        }

        // GET: api/DonationRequests/search
        [HttpGet("search")]
        public async Task<ActionResult<object>> SearchDonationRequests(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? keyword = null,
            [FromQuery] int? bloodTypeId = null,
            [FromQuery] int? bloodComponentId = null,
            [FromQuery] byte? status = null)
        {
            _logger.LogInformation("SearchDonationRequests called with page={Page}, pageSize={PageSize}, keyword={Keyword}, bloodTypeId={BloodTypeId}, bloodComponentId={BloodComponentId}, status={Status}",
                page, pageSize, keyword, bloodTypeId, bloodComponentId, status);

            if (page < 1 || pageSize < 1)
            {
                _logger.LogWarning("Invalid page or pageSize: page={Page}, pageSize={PageSize}", page, pageSize);
                return BadRequest(new { Message = "Invalid page or pageSize." });
            }

            var query = _context.DonationRequests
                .Include(dr => dr.BloodType)
                .Include(dr => dr.BloodComponent)
                .Include(dr => dr.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string lowerKeyword = keyword.Trim().ToLower();
                query = query.Where(dr =>
                    (dr.User != null && dr.User.Name != null && dr.User.Name.ToLower().Contains(lowerKeyword)) ||
                    (dr.Phone != null && dr.Phone.ToLower().Contains(lowerKeyword)) ||
                    (dr.Location != null && dr.Location.ToLower().Contains(lowerKeyword)));
            }

            if (bloodTypeId.HasValue)
            {
                query = query.Where(dr => dr.BloodTypeId == bloodTypeId.Value);
            }

            if (bloodComponentId.HasValue)
            {
                query = query.Where(dr => dr.BloodComponentId == bloodComponentId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(dr => dr.Status == status.Value);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var donationRequests = await query
                .OrderBy(dr => dr.Status == (byte)DonationRequestStatus.Pending ? 0
                    : dr.Status == (byte)DonationRequestStatus.Successful ? 1 : 2)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var donationRequestDtos = donationRequests.Select(dr => new DonationRequestDto
            {
                DonateRequestId = dr.DonateRequestId,
                UserId = dr.UserId,
                Name = dr.Name,
                BloodTypeId = dr.BloodTypeId,
                BloodComponentId = dr.BloodComponentId,
                PreferredDate = dr.PreferredDate,
                Status = dr.Status,
                StatusName = MapStatusToString(dr.Status),
                Location = dr.Location,
                CreatedAt = dr.CreatedAt,
                Quantity = dr.Quantity,
                HeightCm = dr.HeightCm,
                WeightKg = dr.WeightKg,
                LastDonationDate = dr.LastDonationDate,
                HealthInfo = dr.HealthInfo,
                Phone = dr.Phone,
                DateOfBirth = dr.DateOfBirth,
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
                    IsDeleted = dr.User.IsDeleted,
                    RoleBit = dr.User.RoleBit ?? 0,
                    StatusBit = dr.User.StatusBit,
                    HeightCm = dr.User.HeightCm,
                    WeightKg = dr.User.WeightKg,
                    MedicalHistory = dr.User.MedicalHistory,
                    BloodTypeId = dr.User.BloodTypeId,
                    BloodComponentId = dr.User.BloodComponentId
                } : null
            }).ToList();

            _logger.LogInformation("Retrieved {Count} donation requests from search", donationRequestDtos.Count);

            return Ok(new
            {
                Message = "Retrieved donation requests successfully.",
                Data = new
                {
                    Requests = donationRequestDtos,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize
                }
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
            _logger.LogInformation("SearchDonationRequestsByUser called with userId={UserId}, page={Page}, pageSize={PageSize}, bloodTypeId={BloodTypeId}, bloodComponentId={BloodComponentId}, status={Status}, location={Location}, preferredDate={PreferredDate}, createdAfter={CreatedAfter}, createdBefore={CreatedBefore}, quantityMin={QuantityMin}, quantityMax={QuantityMax}",
                userId, page, pageSize, bloodTypeId, bloodComponentId, status, location, preferredDate, createdAfter, createdBefore, quantityMin, quantityMax);

            if (userId < 1 || page < 1 || pageSize < 1)
            {
                _logger.LogWarning("Invalid parameters: userId={UserId}, page={Page}, pageSize={PageSize}", userId, page, pageSize);
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
                    "pending" => (byte)DonationRequestStatus.Pending,
                    "successful" => (byte)DonationRequestStatus.Successful,
                    "cancelled" => (byte)DonationRequestStatus.Cancelled,
                    _ => null
                };
                if (statusByte.HasValue)
                    query = query.Where(dr => dr.Status == statusByte.Value);
                else
                {
                    _logger.LogWarning("Invalid status filter: status={Status}", status);
                    return BadRequest(new { Message = "Invalid status value. Use 'pending', 'successful', or 'cancelled'." });
                }
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
                .OrderBy(dr => dr.Status == (byte)DonationRequestStatus.Pending ? 0 : dr.Status == (byte)DonationRequestStatus.Successful ? 1 : 2)
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
                Phone = dr.Phone,
                DateOfBirth = dr.DateOfBirth,
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
                    DateOfBirth = dr.User.DateOfBirth,
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