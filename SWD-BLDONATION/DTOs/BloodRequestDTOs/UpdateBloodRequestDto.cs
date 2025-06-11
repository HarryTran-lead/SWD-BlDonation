using System.ComponentModel.DataAnnotations;

namespace SWD_BLDONATION.DTOs.BloodRequestDTOs
{
    public class UpdateBloodRequestDto
    {
        [Required(ErrorMessage = "BloodRequestId is required.")]
        public int BloodRequestId { get; set; }

        public int? UserId { get; set; }
        public int? BloodTypeId { get; set; }
        public int? BloodComponentId { get; set; }
        public bool? IsEmergency { get; set; }

        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters.")]
        public string? Status { get; set; }

        [StringLength(255, ErrorMessage = "Location cannot exceed 255 characters.")]
        public string? Location { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be positive.")]
        public int? Quantity
        {
            get; set;
        }

        public bool? Fulfilled { get; set; }

        [StringLength(255, ErrorMessage = "FulfilledSource cannot exceed 255 characters.")]
        public string? FulfilledSource { get; set; }

        [Range(0, 300, ErrorMessage = "HeightCm must be between 0 and 300 cm.")]
        public decimal? HeightCm { get; set; }

        [Range(0, 500, ErrorMessage = "WeightKg must be between 0 and 500 kg.")]
        public decimal? WeightKg { get; set; }

        [StringLength(1000, ErrorMessage = "HealthInfo cannot exceed 1000 characters.")]
        public string? HealthInfo { get; set; }
    }
}
