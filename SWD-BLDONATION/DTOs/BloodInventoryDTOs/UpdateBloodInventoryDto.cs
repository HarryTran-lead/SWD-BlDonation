using System.ComponentModel.DataAnnotations;

namespace SWD_BLDONATION.DTOs.BloodInventoryDTOs
{
    public class UpdateBloodInventoryDto
    {
        public int? BloodTypeId { get; set; }

        public int? BloodComponentId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be non-negative.")]
        public int? Quantity { get; set; }

        [MaxLength(50, ErrorMessage = "Unit cannot exceed 50 characters.")]
        public string? Unit { get; set; }

        [MaxLength(255, ErrorMessage = "InventoryLocation cannot exceed 255 characters.")]
        public string? InventoryLocation { get; set; }
    }
}