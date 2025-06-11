namespace SWD_BLDONATION.DTOs.BloodInventoryDTOs
{
    public class BloodInventoryDto
    {
        public int InventoryId { get; set; }
        public int BloodTypeId { get; set; }
        public string? BloodTypeName { get; set; }
        public int BloodComponentId { get; set; }
        public string? BloodComponentName { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; } = null!;
        public DateTime LastUpdated { get; set; }
        public string? InventoryLocation { get; set; }
    }
}