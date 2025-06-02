using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class CreateUserDto
{
    [Required]
    [DefaultValue("")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    [DefaultValue("")]
    public string Password { get; set; } = string.Empty;

    [EmailAddress]
    [DefaultValue("")]
    public string? Email { get; set; }

    [JsonPropertyName("Full Name")]
    [DefaultValue("")]
    public string? Name { get; set; }

    [Phone]
    [DefaultValue("")]
    public string? Phone { get; set; }

    [DefaultValue("2000-01-01")]
    public DateOnly? DateOfBirth { get; set; }

    [DefaultValue("")]
    public string? Address { get; set; }

    [DefaultValue("")]
    public string? Identification { get; set; }

    [DefaultValue(0)]
    public byte? RoleBit { get; set; } = 0;

    public double? HeightCm { get; set; }

    public double? WeightKg { get; set; }

    [DefaultValue("")]
    public string? MedicalHistory { get; set; }

    public int? BloodTypeId { get; set; }

    public int? BloodComponentId { get; set; }
}
