using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD_BLDONATION.DTOs.UserDTOs;
using SWD_BLDONATION.Models.Generated;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace SWD_BLDONATION.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly BloodDonationContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(BloodDonationContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users
                .Where(u => !u.IsDeleted)
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    Name = u.Name,
                    Email = u.Email,
                    Phone = u.Phone,
                    DateOfBirth = u.DateOfBirth,
                    Address = u.Address,
                    Identification = u.Identification,
                    StatusBit = u.StatusBit ?? 1,
                    RoleBit = u.RoleBit ?? 0,
                    HeightCm = u.HeightCm,
                    WeightKg = u.WeightKg,
                    MedicalHistory = u.MedicalHistory,
                    BloodTypeId = u.BloodTypeId,
                    BloodComponentId = u.BloodComponentId,
                    IsDeleted = u.IsDeleted 
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users
                .Where(u => u.UserId == id && !u.IsDeleted)
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    UserName = u.UserName,
                    Name = u.Name,
                    Email = u.Email,
                    Phone = u.Phone,
                    DateOfBirth = u.DateOfBirth,
                    Address = u.Address,
                    Identification = u.Identification,
                    StatusBit = u.StatusBit ?? 1,
                    RoleBit = u.RoleBit ?? 0,
                    HeightCm = u.HeightCm,
                    WeightKg = u.WeightKg,
                    MedicalHistory = u.MedicalHistory,
                    BloodTypeId = u.BloodTypeId,
                    BloodComponentId = u.BloodComponentId,
                    IsDeleted = u.IsDeleted 
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(user);
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<UserDto>> PostUser([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var emailExists = await _context.Users.AnyAsync(u => u.Email == dto.Email && !u.IsDeleted);
                if (emailExists)
                    return BadRequest(new { message = "Email đã tồn tại." });
            }

            if (!string.IsNullOrWhiteSpace(dto.Identification))
            {
                var idExists = await _context.Users.AnyAsync(u => u.Identification == dto.Identification && !u.IsDeleted);
                if (idExists)
                    return BadRequest(new { message = "Identification đã tồn tại." });
            }

            var user = new User
            {
                UserName = dto.UserName,
                Password = dto.Password,
                Email = dto.Email,
                Identification = dto.Identification,
                Name = dto.Name,
                Phone = dto.Phone,
                DateOfBirth = dto.DateOfBirth,
                Address = dto.Address,
                StatusBit = 1,
                IsDeleted = false,
                RoleBit = dto.RoleBit ?? 0,
                BloodTypeId = dto.BloodTypeId,
                BloodComponentId = dto.BloodComponentId,
                HeightCm = dto.HeightCm,
                WeightKg = dto.WeightKg,
                MedicalHistory = dto.MedicalHistory
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = new UserDto
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address,
                Identification = user.Identification,
                StatusBit = user.StatusBit ?? 1,
                RoleBit = user.RoleBit ?? 0,
                HeightCm = user.HeightCm,
                WeightKg = user.WeightKg,
                MedicalHistory = user.MedicalHistory,
                BloodTypeId = user.BloodTypeId,
                BloodComponentId = user.BloodComponentId,
                IsDeleted = user.IsDeleted 
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, result);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, [FromBody] UpdateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FindAsync(id);
            if (user == null || user.IsDeleted)
                return NotFound(new { message = "User not found" });

            user.UserName = dto.UserName ?? user.UserName;
            user.Name = dto.Name ?? user.Name;
            user.Email = dto.Email ?? user.Email;
            user.Phone = dto.Phone ?? user.Phone;
            user.DateOfBirth = dto.DateOfBirth ?? user.DateOfBirth;
            user.Address = dto.Address ?? user.Address;
            user.Identification = dto.Identification ?? user.Identification;

            if (dto.StatusBit.HasValue)
                user.StatusBit = dto.StatusBit.Value ? (byte)1 : (byte)0;

            if (dto.RoleBit.HasValue)
                user.RoleBit = dto.RoleBit.Value;

            user.BloodTypeId = dto.BloodTypeId ?? user.BloodTypeId;
            user.BloodComponentId = dto.BloodComponentId ?? user.BloodComponentId;
            user.HeightCm = dto.HeightCm ?? user.HeightCm;
            user.WeightKg = dto.WeightKg ?? user.WeightKg;
            user.MedicalHistory = dto.MedicalHistory ?? user.MedicalHistory;

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation($"User updated: ID = {user.UserId}, UserName = {user.UserName}");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                    return NotFound(new { message = "User not found" });
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Users/5 (soft delete)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null || user.IsDeleted)
                return NotFound(new { message = "User not found" });

            user.IsDeleted = true;
            _context.Entry(user).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"User soft deleted: ID = {user.UserId}, UserName = {user.UserName}");

            return Ok(new { message = "User deleted successfully." });
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id && !e.IsDeleted);
        }
    }
}
