using System.ComponentModel.DataAnnotations;

namespace SWD_BLDONATION.DTOs.UserDTOs
{
    public class UserSearchQueryDto
    {
        public string? UserName { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public byte? StatusBit { get; set; }

        public int? RoleBit { get; set; }
        public int? Id { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}