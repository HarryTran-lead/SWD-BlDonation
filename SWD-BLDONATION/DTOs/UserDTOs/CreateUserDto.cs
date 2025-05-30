using System.ComponentModel.DataAnnotations;

public class CreateUserDto
{
    [Required]
    public string UserName { get; set; } = null!;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = null!;

    [EmailAddress]
    public string? Email { get; set; }

    public string? Name { get; set; }

    [Phone]
    public string? Phone { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Address { get; set; }

    public string? Identification { get; set; }

    public byte? RoleBit { get; set; } = 0;


    public decimal? HeightCm { get; set; }

    public decimal? WeightKg { get; set; }

    public string? MedicalHistory { get; set; }

    public int? BloodTypeId { get; set; }

    public int? BloodComponentId { get; set; }
}
