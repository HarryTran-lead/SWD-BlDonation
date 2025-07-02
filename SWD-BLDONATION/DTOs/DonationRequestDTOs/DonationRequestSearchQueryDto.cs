namespace SWD_BLDONATION.DTOs.DonationRequestDTOs
{
    public class DonationRequestSearchQueryDto
    {
        public int? UserId { get; set; }  // Filter by user ID (nullable for optional filter)
        public int? BloodTypeId { get; set; }  // Filter by blood type (nullable for optional filter)
        public byte? Status { get; set; }
        public string? Location { get; set; }  // Filter by location (nullable for optional filter)
        public DateOnly? PreferredDate { get; set; }  // Filter by preferred donation date (nullable for optional filter)
        public DateTime? CreatedAfter { get; set; }  // Filter by creation date range (after this date)
        public DateTime? CreatedBefore { get; set; }  // Filter by creation date range (before this date)
        public int? QuantityMin { get; set; }  // Minimum quantity filter (nullable for optional filter)
        public int? QuantityMax { get; set; }  // Maximum quantity filter (nullable for optional filter)

        public int Page { get; set; } = 1;  // Default page number is 1
        public int PageSize { get; set; } = 10;  // Default page size is 10
    }
}
