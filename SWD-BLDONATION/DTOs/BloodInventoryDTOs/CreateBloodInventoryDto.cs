using System.ComponentModel.DataAnnotations;

namespace SWD_BLDONATION.DTOs.BloodInventoryDTOs
{
    public class CreateBloodInventoryDto
    {
        [Required(ErrorMessage = "BloodTypeId is required.")]
        public int BloodTypeId { get; set; }

        [Required(ErrorMessage = "BloodComponentId is required.")]
        public int BloodComponentId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be non-negative.")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Unit is required.")]
        [MaxLength(50, ErrorMessage = "Unit cannot exceed 50 characters.")]
        public string Unit { get; set; } = null!;

        [MaxLength(255, ErrorMessage = "InventoryLocation cannot exceed 255 characters.")]
        public string? InventoryLocation { get; set; }
    }
}