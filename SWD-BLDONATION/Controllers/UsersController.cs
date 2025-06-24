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

        // GET: api/Users/search
        [HttpGet("search")]
        public async Task<ActionResult<object>> SearchUsers([FromQuery] UserSearchQueryDto filter)
        {
            // Validate pagination parameters
            int page = filter.Page < 1 ? 1 : filter.Page;
            int pageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

            // Build query
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

            // Apply search filters
            if (!string.IsNullOrWhiteSpace(filter.UserName))
            {
                query = query.Where(x => x.User.UserName.Contains(filter.UserName.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(filter.Email))
            {
                query = query.Where(x => x.User.Email.Contains(filter.Email.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                query = query.Where(x => x.User.Name.Contains(filter.Name.Trim()));
            }

            if (!string.IsNullOrWhiteSpace(filter.Phone))
            {
                query = query.Where(x => x.User.Phone != null && x.User.Phone.Contains(filter.Phone.Trim()));
            }

            if (filter.StatusBit.HasValue)
            {
                query = query.Where(x => x.User.StatusBit == filter.StatusBit.Value);
            }

            // Execute query with pagination
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
        public async Task<ActionResult<UserDto>> PostUser([FromForm] CreateUserDto dto)
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

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, [FromForm] UpdateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users.FindAsync(id);
            if (user == null || user.IsDeleted)
                return BadRequest(new { message = "User not found" });

            var updatedFields = new List<string>();

            if (!string.IsNullOrWhiteSpace(dto.UserName))
            {
                var newUserName = dto.UserName.Trim();
                if (newUserName != user.UserName && await _context.Users.AnyAsync(u => u.UserName == newUserName && !u.IsDeleted && u.UserId != id))
                    return BadRequest(new { message = "Username already exists." });
                user.UserName = newUserName;
                updatedFields.Add("UserName");
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                updatedFields.Add("Password");
            }

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var newEmail = dto.Email.Trim();
                if (newEmail != user.Email && await _context.Users.AnyAsync(u => u.Email == newEmail && !u.IsDeleted && u.UserId != id))
                    return BadRequest(new { message = "Email already exists." });
                user.Email = newEmail;
                updatedFields.Add("Email");
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                user.Name = dto.Name.Trim();
                updatedFields.Add("Name");
            }

            if (dto.Phone != null)
            {
                user.Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim();
                updatedFields.Add("Phone");
            }

            if (dto.DateOfBirth.HasValue)
            {
                user.DateOfBirth = dto.DateOfBirth.Value;
                updatedFields.Add("DateOfBirth");
            }

            if (dto.Address != null)
            {
                user.Address = string.IsNullOrWhiteSpace(dto.Address) ? null : dto.Address.Trim();
                updatedFields.Add("Address");
            }

            if (dto.Identification != null)
            {
                var newId = string.IsNullOrWhiteSpace(dto.Identification) ? null : dto.Identification.Trim();
                if (newId != null && newId != user.Identification && await _context.Users.AnyAsync(u => u.Identification == newId && !u.IsDeleted && u.UserId != id))
                    return BadRequest(new { message = "Identification already exists." });
                user.Identification = newId;
                updatedFields.Add("Identification");
            }

            if (dto.StatusBit.HasValue)
            {
                user.StatusBit = dto.StatusBit.Value;
                updatedFields.Add("StatusBit");
            }

            if (dto.RoleBit.HasValue)
            {
                user.RoleBit = dto.RoleBit.Value;
                updatedFields.Add("RoleBit");
            }

            if (dto.HeightCm.HasValue && dto.HeightCm > 0)
            {
                user.HeightCm = dto.HeightCm.Value;
                updatedFields.Add("HeightCm");
            }

            if (dto.WeightKg.HasValue && dto.WeightKg > 0)
            {
                user.WeightKg = dto.WeightKg.Value;
                updatedFields.Add("WeightKg");
            }

            if (dto.MedicalHistory != null)
            {
                user.MedicalHistory = string.IsNullOrWhiteSpace(dto.MedicalHistory) ? null : dto.MedicalHistory.Trim();
                updatedFields.Add("MedicalHistory");
            }

            if (dto.BloodTypeId.HasValue && dto.BloodTypeId > 0)
            {
                user.BloodTypeId = dto.BloodTypeId.Value;
                updatedFields.Add("BloodTypeId");
            }

            if (dto.BloodComponentId.HasValue && dto.BloodComponentId > 0)
            {
                user.BloodComponentId = dto.BloodComponentId.Value;
                updatedFields.Add("BloodComponentId");
            }

            if (updatedFields.Any())
            {
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "User updated successfully.",
                    updatedFields
                });
            }

            return Ok(new { message = "No fields were updated." });
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