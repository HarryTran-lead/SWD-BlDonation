using SWD_BLDONATION.Models.Enums;

namespace SWD_BLDONATION.DTOs.BloodRequestDTOs
{
    public class BloodRequestSearchQueryDto
    {
        public int? Id { get; set; }
        public int? UserId { get; set; }
        public int? BloodTypeId { get; set; }
        public int? BloodComponentId { get; set; }

        // Sử dụng enum BloodRequestStatus cho Status
        public BloodRequestStatus Status { get; set; }  // Changed to use enum instead of byte

        public string? Location { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
