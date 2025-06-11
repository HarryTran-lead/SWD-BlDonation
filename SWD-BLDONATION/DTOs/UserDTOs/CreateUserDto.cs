using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SWD_BLDONATION.DTOs.UserDTOs
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "Username is required.")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters long.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name is required.")]
        [JsonPropertyName("Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string? Phone { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public string? Address { get; set; }

        public string? Identification { get; set; }

        [DefaultValue(1)]
        public byte RoleBit { get; set; } = 0;

        public double? HeightCm { get; set; }

        public double? WeightKg { get; set; }

        public string? MedicalHistory { get; set; }

        public int? BloodTypeId { get; set; }

        public int? BloodComponentId { get; set; }
    }
}