using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SWD_BLDONATION.Models.Generated;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using SWD_BLDONATION.DTOs.NotificationDTOs;

namespace SWD_BLDONATION.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly BloodDonationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(BloodDonationDbContext context, IMapper mapper, ILogger<NotificationsController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/Notifications/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<object>> GetNotificationsByUser(
            int userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null)
        {
            if (userId < 1 || page < 1 || pageSize < 1)
            {
                _logger.LogWarning("Invalid parameters: userId={UserId}, page={Page}, pageSize={PageSize}", userId, page, pageSize);
                return BadRequest(new { Message = "Invalid userId, page, or pageSize." });
            }

            var query = _context.Notifications
                .Where(n => n.UserId == userId)
                .Include(n => n.User)
                .AsQueryable();

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                var normalizedStatus = status.Trim().ToLowerInvariant();
                if (normalizedStatus == "read" || normalizedStatus == "unread")
                {
                    query = query.Where(n => n.Status != null && n.Status.ToLowerInvariant() == normalizedStatus);
                }
                else
                {
                    _logger.LogWarning("Invalid status filter: status={Status}", status);
                    return BadRequest(new { Message = "Invalid status value. Use 'read' or 'unread'." });
                }
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var notifications = await query
                .OrderByDescending(n => n.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var notificationDtos = notifications.Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                UserId = n.UserId,
                Message = n.Message,
                Type = n.Type,
                Status = n.Status,
                SentAt = n.SentAt
            }).ToList();

            _logger.LogInformation("Retrieved {Count} notifications for userId={UserId}", notificationDtos.Count, userId);

            return Ok(new
            {
                Data = notificationDtos,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            });
        }

        // PATCH: api/Notifications/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateNotificationStatus(int id, [FromBody] UpdateNotificationStatusDto dto)
        {
            _logger.LogInformation("UpdateNotificationStatus called with id={Id}, status={Status}", id, dto.Status);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid data provided for UpdateNotificationStatus: id={Id}", id);
                return BadRequest(new
                {
                    Message = "Invalid data submitted.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var normalizedStatus = dto.Status?.Trim().ToLowerInvariant();
            if (normalizedStatus != "read" && normalizedStatus != "unread")
            {
                _logger.LogWarning("Invalid status value: id={Id}, status={Status}", id, dto.Status);
                return BadRequest(new { Message = "Invalid status value. Use 'read' or 'unread'." });
            }

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                _logger.LogWarning("Notification not found: id={Id}", id);
                return NotFound(new { Message = "Notification not found" });
            }

            notification.Status = normalizedStatus == "read" ? "Read" : "Unread";
            _context.Entry(notification).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Notification status updated successfully: id={Id}, status={Status}", id, notification.Status);
            return Ok(new { Message = "Notification status updated successfully." });
        }

        [HttpPatch("bulk-status")]
        public async Task<IActionResult> UpdateBulkNotificationStatus([FromBody] UpdateBulkNotificationStatusDto dto)
        {
            _logger.LogInformation("UpdateBulkNotificationStatus called with notificationIds={NotificationIds}, userId={UserId}",
                dto.NotificationIds != null ? string.Join(",", dto.NotificationIds) : "null", dto.UserId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid data provided for UpdateBulkNotificationStatus");
                return BadRequest(new
                {
                    Message = "Invalid data submitted.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            if (dto.NotificationIds == null && !dto.UserId.HasValue)
            {
                _logger.LogWarning("Either notificationIds or userId must be provided");
                return BadRequest(new { Message = "Either notificationIds or userId must be provided." });
            }

            var query = _context.Notifications.AsQueryable();

            if (dto.UserId.HasValue)
            {
                if (!await _context.Users.AnyAsync(u => u.UserId == dto.UserId))
                {
                    _logger.LogWarning("User not found: userId={UserId}", dto.UserId);
                    return NotFound(new { Message = "User not found" });
                }
                query = query.Where(n => n.UserId == dto.UserId && n.Status == "Unread");
            }
            else if (dto.NotificationIds != null && dto.NotificationIds.Any())
            {
                query = query.Where(n => dto.NotificationIds.Contains(n.NotificationId) && n.Status == "Unread");
            }

            var notifications = await query.ToListAsync();
            if (!notifications.Any())
            {
                _logger.LogWarning("No unread notifications found for the provided criteria");
                return NotFound(new { Message = "No unread notifications found" });
            }

            foreach (var notification in notifications)
            {
                notification.Status = "Read";
                _context.Entry(notification).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated {Count} notifications to Read status", notifications.Count);
            return Ok(new { Message = $"{notifications.Count} notifications updated to Read status." });
        }

        // DELETE: api/Notifications/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            _logger.LogInformation("DeleteNotification called with id={Id}", id);

            var notification = await _context.Notifications.FindAsync(id);
            if (notification == null)
            {
                _logger.LogWarning("Notification not found: id={Id}", id);
                return NotFound(new { Message = "Notification not found" });
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Notification deleted successfully: id={Id}", id);
            return NoContent();
        }

        // POST: api/Notifications
        [HttpPost]
        public async Task<ActionResult<NotificationDto>> CreateNotification([FromBody] CreateNotificationDto createDto)
        {
            _logger.LogInformation("CreateNotification called for userId={UserId}", createDto.UserId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid data provided for CreateNotification: userId={UserId}", createDto.UserId);
                return BadRequest(new
                {
                    Message = "Invalid data submitted.",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            if (createDto.UserId.HasValue && !await _context.Users.AnyAsync(u => u.UserId == createDto.UserId))
            {
                _logger.LogWarning("User not found: userId={UserId}", createDto.UserId);
                return NotFound(new { Message = "User not found" });
            }

            var notification = new Notification
            {
                UserId = createDto.UserId,
                Message = createDto.Message,
                Type = createDto.Type,
                Status = "Unread",
                SentAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            var resultDto = new NotificationDto
            {
                NotificationId = notification.NotificationId,
                UserId = notification.UserId,
                Message = notification.Message,
                Type = notification.Type,
                Status = notification.Status,
                SentAt = notification.SentAt
            };

            _logger.LogInformation("Notification created successfully: id={Id}, userId={UserId}", notification.NotificationId, notification.UserId);
            return CreatedAtAction(nameof(GetNotificationsByUser), new { userId = notification.UserId }, resultDto);
        }
    }
}