using System.ComponentModel.DataAnnotations;

namespace SWD_BLDONATION.DTOs.BloodRequestDTOs
{
    public class CreateBloodRequestDto
    {
        public int? UserId { get; set; }

        [Required(ErrorMessage = "BloodTypeId is required.")]
        public int BloodTypeId { get; set; }

        [Required(ErrorMessage = "BloodComponentId is required.")]
        public int BloodComponentId { get; set; }

        [Required(ErrorMessage = "IsEmergency is required.")]
        public bool IsEmergency { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [StringLength(255, ErrorMessage = "Location cannot exceed 255 characters.")]
        public string Location { get; set; } = null!;

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be positive.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "HeightCm is required.")]
        [Range(0, 300, ErrorMessage = "HeightCm must be between 0 and 300 cm.")]
        public decimal HeightCm { get; set; }

        [Required(ErrorMessage = "WeightKg is required.")]
        [Range(0, 500, ErrorMessage = "WeightKg must be between 0 and 500 kg.")]
        public decimal WeightKg { get; set; }

        [Required(ErrorMessage = "HealthInfo is required.")]
        [StringLength(1000, ErrorMessage = "HealthInfo cannot exceed 1000 characters.")]
        public string HealthInfo { get; set; } = null!;
    }
}
