using System;
using System.ComponentModel.DataAnnotations;

namespace SWD_BLDONATION.DTOs
{
    public class CreateUserDto
    {
        [Required]
        public string UserName { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;

        public string? Email { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Identification { get; set; }
        public byte? RoleBit { get; set; }  // có thể truyền role từ client, không bắt buộc
    }
}
