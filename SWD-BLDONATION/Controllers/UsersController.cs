using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWD_BLDONATION.DTOs.UserDTOs;
using SWD_BLDONATION.Models.Generated;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using SWD_BLDONATION.Models.Enums;

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
        public async Task<ActionResult<object>> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
                return BadRequest(new { message = "Invalid page or pageSize." });

            var query = _context.Users
                .Where(u => !u.IsDeleted)
                .GroupJoin(_context.BloodTypes,
                    u => u.BloodTypeId,
                    bt => bt.BloodTypeId,
                    (u, bt) => new { User = u, BloodType = bt })
                .SelectMany(
                    x => x.BloodType.DefaultIfEmpty(),
                    (u, bt) => new { u.User, BloodType = bt })
                .GroupJoin(_context.BloodComponents,
                    x => x.User.BloodComponentId,
                    bc => bc.BloodComponentId,
                    (x, bc) => new { x.User, x.BloodType, BloodComponent = bc })
                .SelectMany(
                    x => x.BloodComponent.DefaultIfEmpty(),
                    (x, bc) => new { x.User, x.BloodType, BloodComponent = bc });

            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new UserSearchDto
                {
                    UserId = x.User.UserId,
                    UserName = x.User.UserName,
                    Name = x.User.Name,
                    Email = x.User.Email,
                    Phone = x.User.Phone,
                    DateOfBirth = x.User.DateOfBirth,
                    Address = x.User.Address,
                    Identification = x.User.Identification,
                    StatusBit = x.User.StatusBit ?? 1,
                    RoleBit = x.User.RoleBit ?? 0,
                    HeightCm = x.User.HeightCm,
                    WeightKg = x.User.WeightKg,
                    MedicalHistory = x.User.MedicalHistory,
                    BloodTypeId = x.User.BloodTypeId,
                    BloodComponentId = x.User.BloodComponentId,
                    IsDeleted = x.User.IsDeleted,
                    Role = new EnumDto { Id = x.User.RoleBit ?? 0, Name = ((UserRole)(x.User.RoleBit ?? 0)).ToString() },
                    Status = new EnumDto { Id = x.User.StatusBit ?? 1, Name = ((UserStatus)(x.User.StatusBit ?? 1)).ToString() },
                    BloodType = x.BloodType != null ? new EnumDto { Id = x.BloodType.BloodTypeId, Name = x.BloodType.Name + x.BloodType.RhFactor } : null,
                    BloodComponent = x.BloodComponent != null ? new EnumDto { Id = x.BloodComponent.BloodComponentId, Name = x.BloodComponent.Name } : null
                })
                .ToListAsync();

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return Ok(new
            {
                Users = users,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            });
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

            // Clean input data
            string userName = dto.UserName.Trim();
            string email = dto.Email.Trim();
            string name = dto.Name.Trim();
            string? phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
            string? address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim();
            string? identification = string.IsNullOrWhiteSpace(dto.Identification) ? null : dto.Identification.Trim();
            string? medicalHistory = string.IsNullOrWhiteSpace(dto.MedicalHistory) ? null : dto.MedicalHistory.Trim();

            // Check for duplicate email
            if (await _context.Users.AnyAsync(u => u.Email == email && !u.IsDeleted))
                return BadRequest(new { message = "Email already exists." });

            // Check for duplicate username
            if (await _context.Users.AnyAsync(u => u.UserName == userName && !u.IsDeleted))
                return BadRequest(new { message = "Username already exists." });

            // Check for duplicate identification
            if (!string.IsNullOrEmpty(identification) && await _context.Users.AnyAsync(u => u.Identification == identification && !u.IsDeleted))
                return BadRequest(new { message = "Identification already exists." });

            // Create new user
            var user = new User
            {
                UserName = userName,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password), // Hash password
                Email = email,
                Name = name,
                Phone = phone,
                DateOfBirth = dto.DateOfBirth,
                Address = address,
                Identification = identification,
                StatusBit = 1,
                IsDeleted = false,
                RoleBit = dto.RoleBit,
                BloodTypeId = dto.BloodTypeId,
                BloodComponentId = dto.BloodComponentId,
                HeightCm = dto.HeightCm,
                WeightKg = dto.WeightKg,
                MedicalHistory = medicalHistory
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var userDto = new UserDto
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address,
                Identification = user.Identification,
                StatusBit = user.StatusBit,
                RoleBit = user.RoleBit.Value,
                HeightCm = user.HeightCm,
                WeightKg = user.WeightKg,
                MedicalHistory = user.MedicalHistory,
                BloodTypeId = user.BloodTypeId,
                BloodComponentId = user.BloodComponentId,
                IsDeleted = user.IsDeleted
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, userDto);
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

            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email.Trim() != user.Email)
            {
                var email = dto.Email.Trim();
                var emailExists = await _context.Users.AnyAsync(u => u.Email == email && !u.IsDeleted && u.UserId != id);
                if (emailExists)
                    return BadRequest(new { message = "Email đã tồn tại." });
                user.Email = email;
                updatedFields.Add("Email");
            }

            if (!string.IsNullOrWhiteSpace(dto.Identification) && dto.Identification.Trim() != user.Identification)
            {
                var identification = dto.Identification.Trim();
                var idExists = await _context.Users.AnyAsync(u => u.Identification == identification && !u.IsDeleted && u.UserId != id);
                if (idExists)
                    return BadRequest(new { message = "Identification đã tồn tại." });
                user.Identification = identification;
                updatedFields.Add("Identification");
            }

            if (!string.IsNullOrWhiteSpace(dto.UserName) && dto.UserName != user.UserName)
            {
                user.UserName = dto.UserName;
                updatedFields.Add("UserName");
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.Password = dto.Password.Trim(); // hoặc hash nếu cần
                updatedFields.Add("Password");
            }

            if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name != user.Name)
            {
                user.Name = dto.Name;
                updatedFields.Add("Name");
            }

            if (!string.IsNullOrWhiteSpace(dto.Phone) && dto.Phone != user.Phone)
            {
                user.Phone = dto.Phone;
                updatedFields.Add("Phone");
            }

            if (dto.DateOfBirth.HasValue && dto.DateOfBirth != user.DateOfBirth)
            {
                user.DateOfBirth = dto.DateOfBirth;
                updatedFields.Add("DateOfBirth");
            }

            if (!string.IsNullOrWhiteSpace(dto.Address) && dto.Address != user.Address)
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
                user.RoleBit = dto.RoleBit; // 0 là hợp lệ theo mặc định
                updatedFields.Add("RoleBit");
            }

            if (dto.BloodTypeId.HasValue && dto.BloodTypeId > 0 && dto.BloodTypeId != user.BloodTypeId)
            {
                user.BloodTypeId = dto.BloodTypeId;
                updatedFields.Add("BloodTypeId");
            }

            if (dto.BloodComponentId.HasValue && dto.BloodComponentId > 0 && dto.BloodComponentId != user.BloodComponentId)
            {
                user.BloodComponentId = dto.BloodComponentId;
                updatedFields.Add("BloodComponentId");
            }

            if (dto.HeightCm.HasValue && dto.HeightCm > 0 && dto.HeightCm != user.HeightCm)
            {
                user.HeightCm = dto.HeightCm;
                updatedFields.Add("HeightCm");
            }

            if (dto.WeightKg.HasValue && dto.WeightKg > 0 && dto.WeightKg != user.WeightKg)
            {
                user.WeightKg = dto.WeightKg;
                updatedFields.Add("WeightKg");
            }

            if (!string.IsNullOrWhiteSpace(dto.MedicalHistory) && dto.MedicalHistory != user.MedicalHistory)
            {
                user.MedicalHistory = dto.MedicalHistory;
                updatedFields.Add("MedicalHistory");
            }

            if (updatedFields.Any())
            {
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = updatedFields.Any() ? "Cập nhật thành công." : "Không có trường nào được cập nhật.",
                updatedFields = updatedFields.Any() ? updatedFields : new List<string>()
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

        [HttpGet("search")]
        public async Task<ActionResult<object>> SearchUsers([FromQuery] UserSearchQueryDto query)
        {
            if (query.Page < 1 || query.PageSize < 1)
                return BadRequest(new { message = "Invalid page or pageSize." });

            var dbQuery = _context.Users
                .Where(u => !u.IsDeleted)
                .GroupJoin(_context.BloodTypes,
                    u => u.BloodTypeId,
                    bt => bt.BloodTypeId,
                    (u, bt) => new { User = u, BloodType = bt })
                .SelectMany(
                    x => x.BloodType.DefaultIfEmpty(),
                    (u, bt) => new { u.User, BloodType = bt })
                .GroupJoin(_context.BloodComponents,
                    x => x.User.BloodComponentId,
                    bc => bc.BloodComponentId,
                    (x, bc) => new { x.User, x.BloodType, BloodComponent = bc })
                .SelectMany(
                    x => x.BloodComponent.DefaultIfEmpty(),
                    (x, bc) => new { x.User, x.BloodType, BloodComponent = bc });

            if (!string.IsNullOrEmpty(query.UserName))
                dbQuery = dbQuery.Where(x => x.User.UserName.Contains(query.UserName.Trim()));

            if (!string.IsNullOrEmpty(query.Name))
                dbQuery = dbQuery.Where(x => x.User.Name.Contains(query.Name.Trim()));

            if (!string.IsNullOrEmpty(query.Email))
                dbQuery = dbQuery.Where(x => x.User.Email != null && x.User.Email.Contains(query.Email.Trim()));

            if (!string.IsNullOrEmpty(query.Phone))
                dbQuery = dbQuery.Where(x => x.User.Phone != null && x.User.Phone.Contains(query.Phone.Trim()));

            if (!string.IsNullOrEmpty(query.Address))
                dbQuery = dbQuery.Where(x => x.User.Address != null && x.User.Address.Contains(query.Address.Trim()));

            if (query.StatusBit.HasValue)
                dbQuery = dbQuery.Where(x => x.User.StatusBit == query.StatusBit.Value);

            if (query.Id.HasValue)
                dbQuery = dbQuery.Where(x => x.User.UserId == query.Id.Value);

            var users = await dbQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(x => new UserSearchDto
                {
                    UserId = x.User.UserId,
                    UserName = x.User.UserName,
                    Name = x.User.Name,
                    Email = x.User.Email,
                    Phone = x.User.Phone,
                    DateOfBirth = x.User.DateOfBirth,
                    Address = x.User.Address,
                    Identification = x.User.Identification,
                    StatusBit = x.User.StatusBit ?? 1,
                    RoleBit = x.User.RoleBit ?? 0,
                    HeightCm = x.User.HeightCm,
                    WeightKg = x.User.WeightKg,
                    MedicalHistory = x.User.MedicalHistory,
                    BloodTypeId = x.User.BloodTypeId,
                    BloodComponentId = x.User.BloodComponentId,
                    IsDeleted = x.User.IsDeleted
                })
                .ToListAsync();

                    var totalCount = await dbQuery.CountAsync();
                    var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

            return Ok(new
            {
                Users = users,
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = query.Page,
                PageSize = query.PageSize
            });
        }
    }
}
