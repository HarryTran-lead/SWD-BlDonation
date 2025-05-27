namespace SWD_BLDONATION.DTOs.UserDTOs
{
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
        public bool StatusBit { get; set; }    // bool cho client biết active/inactive
        public byte RoleBit { get; set; }      // 0-user, 1-staff, 2-admin
    }
}
