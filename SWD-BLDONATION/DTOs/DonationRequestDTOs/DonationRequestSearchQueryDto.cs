namespace SWD_BLDONATION.DTOs.DonationRequestDTOs
{
    public class DonationRequestSearchQueryDto
    {
        public int? UserId { get; set; }
        public int? BloodTypeId { get; set; }
        public string? Status { get; set; }
        public string? Location { get; set; }
        public DateOnly? PreferredDate { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public int? QuantityMin { get; set; }
        public int? QuantityMax { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

}
