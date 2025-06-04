namespace SWD_BLDONATION.DTOs.BloodInventoryDTOs
{
    public class BloodInventorySearchQueryDto
    {
        public int? Id { get; set; }
        public int? BloodTypeId { get; set; }
        public int? BloodComponentId { get; set; }
        public string? InventoryLocation { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}