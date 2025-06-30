using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SWD_BLDONATION.DTOs.AuthDTOs;
using SWD_BLDONATION.Models.Generated;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.CodeAnalysis.Scripting;
using SWD_BLDONATION.Models.Enums;

namespace SWD_BLDONATION.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly BloodDonationContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(BloodDonationContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string userName = dto.UserName.Trim();
            string email = dto.Email.Trim();
            string name = dto.Name.Trim();

            if (await _context.Users.AnyAsync(u => u.UserName == userName && !u.IsDeleted))
                return BadRequest(new { message = "Username already exists." });
            if (await _context.Users.AnyAsync(u => u.Email == email && !u.IsDeleted))
                return BadRequest(new { message = "Email already exists." });

            var user = new User
            {
                UserName = userName,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Email = email,
                Name = name,
                StatusBit = 1,
                IsDeleted = false,
                RoleBit = 0
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                UserName = user.UserName,
                Name = user.Name,
                Email = user.Email,
                Role = GetRoleName(user.RoleBit.Value)
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromForm] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users
                .Where(u => u.UserName == dto.UserName && !u.IsDeleted)
            .FirstOrDefaultAsync();

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                return Unauthorized(new { message = "Invalid username or password." });

            var token = GenerateJwtToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                UserName = user.UserName,
                Name = user.Name,
                Email = user.Email,
                Role = GetRoleName(user.RoleBit.Value)
            });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, GetRoleName(user.RoleBit.Value)),
                new Claim("RoleBit", user.RoleBit.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GetRoleName(byte roleBit)
        {
            return Enum.IsDefined(typeof(UserRole), roleBit)
                ? ((UserRole)roleBit).ToString()
                : "User";
        }

    }
}