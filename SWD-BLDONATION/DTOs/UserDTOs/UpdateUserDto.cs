using System;

namespace SWD_BLDONATION.DTOs
{
    public class UpdateUserDto
    {
        public string? UserName { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Identification { get; set; }
        public bool? StatusBit { get; set; }  // bool? để cập nhật trạng thái active/inactive
        public byte? RoleBit { get; set; }    // cập nhật quyền nếu cần
    }
}
