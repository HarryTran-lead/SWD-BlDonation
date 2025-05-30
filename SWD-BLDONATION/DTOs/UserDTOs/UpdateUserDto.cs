using System;

namespace SWD_BLDONATION.DTOs.UserDTOs
{
    public class UpdateUserDto
    {
        public string? UserName { get; set; }

        public string? Name { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public DateOnly? DateOfBirth { get; set; }  // dùng DateTime? nếu không phải .NET 6+

        public string? Address { get; set; }

        public string? Identification { get; set; }

        public bool? StatusBit { get; set; }  // bool? cho phép cập nhật true/false hoặc bỏ qua

        public byte? RoleBit { get; set; }    // 0-user, 1-staff, 2-admin

        public decimal? HeightCm { get; set; }

        public decimal? WeightKg { get; set; }

        public string? MedicalHistory { get; set; }

        public int? BloodTypeId { get; set; }

        public int? BloodComponentId { get; set; }
    }
}
