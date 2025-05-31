public class UserDto
{
    public int UserId { get; set; }
    public string? UserName { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? Identification { get; set; }
    public bool IsDeleted { get; set; }
    public byte RoleBit { get; set; }
    public byte? StatusBit { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? WeightKg { get; set; }
    public string? MedicalHistory { get; set; }

    public int? BloodTypeId { get; set; }         // Thêm dòng này
    public int? BloodComponentId { get; set; }    // Thêm dòng này
}
