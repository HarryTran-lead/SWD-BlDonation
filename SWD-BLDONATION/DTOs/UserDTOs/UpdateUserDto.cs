using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SWD_BLDONATION.DTOs.UserDTOs
{
    public class UpdateUserDto
    {
        [DefaultValue("")]
        public string? UserName { get; set; }

        [DefaultValue("")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string? Password { get; set; }

        [JsonPropertyName("Full Name")]
        [DefaultValue("")]
        public string? Name { get; set; }

        [DefaultValue("")]
        public string? Email { get; set; }

        [DefaultValue("")]
        public string? Phone { get; set; }

        [DefaultValue("2000-01-01")]
        public DateOnly? DateOfBirth { get; set; }

        [DefaultValue("")]
        public string? Address { get; set; }

        [DefaultValue("")]
        public string? Identification { get; set; }

        public bool? StatusBit { get; set; }

        [DefaultValue(0)]
        public byte? RoleBit { get; set; }

        public double? HeightCm { get; set; }

        public double? WeightKg { get; set; }

        [DefaultValue("")]
        public string? MedicalHistory { get; set; }

        public int? BloodTypeId { get; set; }

        public int? BloodComponentId { get; set; }
    }
}