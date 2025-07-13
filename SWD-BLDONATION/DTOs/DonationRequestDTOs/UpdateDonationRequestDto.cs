using SWD_BLDONATION.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SWD_BLDONATION.DTOs.DonationRequestDTOs
{
    public class UpdateDonationRequestDto
    {
        public int DonateRequestId { get; set; }
        public int? UserId { get; set; }

        public int? BloodTypeId { get; set; }

        public int? BloodComponentId { get; set; }

        public DateOnly? PreferredDate { get; set; }

        [StringLength(255, ErrorMessage = "Location cannot exceed 255 characters.")]
        [DefaultValue("")]
        public string? Location { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be positive.")]
        public int? Quantity { get; set; }

        public BloodRequestStatus? Status { get; set; }

        [Range(0, 300, ErrorMessage = "HeightCm must be between 0 and 300 cm.")]
        public decimal? HeightCm { get; set; }

        [Range(0, 500, ErrorMessage = "WeightKg must be between 0 and 500 kg.")]
        public decimal? WeightKg { get; set; }

        [StringLength(1000, ErrorMessage = "HealthInfo cannot exceed 1000 characters.")]
        [DefaultValue("")]
        public string? HealthInfo { get; set; }

        public DateOnly? LastDonationDate { get; set; }

        [StringLength(255, ErrorMessage = "Name cannot exceed 255 characters.")]
        [DefaultValue("")]
        public string? Name { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters.")]
        [DefaultValue("")]
        public string? Phone { get; set; }

    }
}
