using SWD_BLDONATION.DTOs.BloodComponentDTOs;
using SWD_BLDONATION.DTOs.BloodTypeDTOs;
using SWD_BLDONATION.DTOs.UserDTOs;

namespace SWD_BLDONATION.DTOs.DonationRequestDTOs
{
    public class DonationRequestDto
    {
        public int DonateRequestId { get; set; }
        public int? UserId { get; set; }
        public int? BloodTypeId { get; set; }
        public int? BloodComponentId { get; set; }
        public DateOnly? PreferredDate { get; set; }
        public string? StatusName { get; set; }
        public byte? Status { get; set; }
        public string? Location { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? Quantity { get; set; }
        public decimal? HeightCm { get; set; }
        public decimal? WeightKg { get; set; }
        public DateOnly? LastDonationDate { get; set; }
        public string? HealthInfo { get; set; }

        // Navigation properties (DTO objects)
        public BloodTypeDto? BloodType { get; set; }
        public BloodComponentDto? BloodComponent { get; set; }
        public UserDto? User { get; set; }
    }
}
