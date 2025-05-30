namespace SWD_BLDONATION.DTOs.BloodInventoryDTOs
{
    public class UpdateBloodInventoryDto
    {
        public int? BloodTypeId { get; set; }
        public int? BloodComponentId { get; set; }
        public int? Quantity { get; set; }
        public string? Unit { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string? InventoryLocation { get; set; }
    }
}
