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
        public async Task<ActionResult<UserDto>> PostUser([FromForm] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Làm sạch dữ liệu đầu vào
            string email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
            string identification = string.IsNullOrWhiteSpace(dto.Identification) ? null : dto.Identification.Trim();
            string userName = string.IsNullOrWhiteSpace(dto.UserName) ? null : dto.UserName.Trim();
            string password = string.IsNullOrWhiteSpace(dto.Password) ? null : dto.Password.Trim();
            string name = string.IsNullOrWhiteSpace(dto.Name) ? null : dto.Name.Trim();
            string phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
            string address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim();
            string medicalHistory = string.IsNullOrWhiteSpace(dto.MedicalHistory) ? null : dto.MedicalHistory.Trim();

            // Kiểm tra trùng email
            if (!string.IsNullOrEmpty(email))
            {
                var emailExists = await _context.Users.AnyAsync(u => u.Email == email && !u.IsDeleted);
                if (emailExists)
                    return BadRequest(new { message = "Email đã tồn tại." });
            }

            // Kiểm tra trùng identification
            if (!string.IsNullOrEmpty(identification))
            {
                var idExists = await _context.Users.AnyAsync(u => u.Identification == identification && !u.IsDeleted);
                if (idExists)
                    return BadRequest(new { message = "Identification đã tồn tại." });
            }

            // Tạo đối tượng User mới
            var user = new User
            {
                UserName = userName,
                Password = password,
                Email = email,
                Identification = identification,
                Name = name,
                Phone = phone,
                DateOfBirth = dto.DateOfBirth,
                Address = address,
                StatusBit = 1,
                IsDeleted = false,
                RoleBit = dto.RoleBit ?? 0,
                BloodTypeId = dto.BloodTypeId,
                BloodComponentId = dto.BloodComponentId,
                HeightCm = dto.HeightCm,
                WeightKg = dto.WeightKg,
                MedicalHistory = medicalHistory
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, [FromForm] UpdateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FindAsync(id);
            if (user == null || user.IsDeleted)
                return NotFound(new { message = "User not found" });

            var updatedFields = new List<string>();

            string? email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
            string? identification = string.IsNullOrWhiteSpace(dto.Identification) ? null : dto.Identification.Trim();
            string? password = string.IsNullOrWhiteSpace(dto.Password) ? null : dto.Password.Trim();

            if (!string.IsNullOrEmpty(email) && email != user.Email)
            {
                var emailExists = await _context.Users.AnyAsync(u => u.Email == email && !u.IsDeleted);
                if (emailExists)
                    return BadRequest(new { message = "Email đã tồn tại." });
                user.Email = email;
                updatedFields.Add("Email");
            }

            if (!string.IsNullOrEmpty(identification) && identification != user.Identification)
            {
                var idExists = await _context.Users.AnyAsync(u => u.Identification == identification && !u.IsDeleted);
                if (idExists)
                    return BadRequest(new { message = "Identification đã tồn tại." });
                user.Identification = identification;
                updatedFields.Add("Identification");
            }

            if (dto.UserName != null && dto.UserName != user.UserName)
            {
                user.UserName = dto.UserName;
                updatedFields.Add("UserName");
            }

            if (password != null)
            {
                user.Password = password; // hoặc hash nếu cần
                updatedFields.Add("Password");
            }

            if (dto.Name != null && dto.Name != user.Name)
            {
                user.Name = dto.Name;
                updatedFields.Add("Name");
            }

            if (dto.Phone != null && dto.Phone != user.Phone)
            {
                user.Phone = dto.Phone;
                updatedFields.Add("Phone");
            }

            if (dto.DateOfBirth.HasValue && dto.DateOfBirth != user.DateOfBirth)
            {
                user.DateOfBirth = dto.DateOfBirth;
                updatedFields.Add("DateOfBirth");
            }

            if (dto.Address != null && dto.Address != user.Address)
            {
                user.Address = dto.Address;
                updatedFields.Add("Address");
            }

            if (dto.StatusBit.HasValue)
            {
                user.StatusBit = dto.StatusBit.Value ? (byte)1 : (byte)0;
                updatedFields.Add("StatusBit");
            }

            if (dto.RoleBit.HasValue && dto.RoleBit != user.RoleBit)
            {
                user.RoleBit = dto.RoleBit;
                updatedFields.Add("RoleBit");
            }

            if (dto.BloodTypeId.HasValue && dto.BloodTypeId != user.BloodTypeId)
            {
                user.BloodTypeId = dto.BloodTypeId;
                updatedFields.Add("BloodTypeId");
            }

            if (dto.BloodComponentId.HasValue && dto.BloodComponentId != user.BloodComponentId)
            {
                user.BloodComponentId = dto.BloodComponentId;
                updatedFields.Add("BloodComponentId");
            }

            if (dto.HeightCm.HasValue && dto.HeightCm != user.HeightCm)
            {
                user.HeightCm = dto.HeightCm;
                updatedFields.Add("HeightCm");
            }

            if (dto.WeightKg.HasValue && dto.WeightKg != user.WeightKg)
            {
                user.WeightKg = dto.WeightKg;
                updatedFields.Add("WeightKg");
            }

            if (dto.MedicalHistory != null && dto.MedicalHistory != user.MedicalHistory)
            {
                user.MedicalHistory = dto.MedicalHistory;
                updatedFields.Add("MedicalHistory");
            }

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật thành công.",
                updatedFields = updatedFields.Count > 0 ? updatedFields : new List<string> { "Không có trường nào được cập nhật." }
            });
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
