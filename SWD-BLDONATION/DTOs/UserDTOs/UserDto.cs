namespace SWD_BLDONATION.DTOs.UserDTOs
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public DateOnly? DateOfBirth { get; set; }   // dùng DateOnly nếu bạn đang ở .NET 6+; nếu không thì đổi về DateTime?
        public string? Address { get; set; }
        public string? Identification { get; set; }
        public byte IsDeleted { get; set; }          // cho phép kiểm tra nếu cần lọc soft-delete
        public byte RoleBit { get; set; }            // 0-user, 1-staff, 2-admin
        public byte? StatusBit { get; set; }         // nullable vì model là byte?; 1-active, 0-inactive
        public decimal? HeightCm { get; set; }
        public decimal? WeightKg { get; set; }
        public string? MedicalHistory { get; set; }
    }
}
