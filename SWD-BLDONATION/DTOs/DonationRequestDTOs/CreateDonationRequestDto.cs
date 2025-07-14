namespace SWD_BLDONATION.DTOs.DonationRequestDTOs
{
    public class CreateDonationRequestDto
    {
        public int? UserId { get; set; }
        public int? BloodTypeId { get; set; }
        public int? BloodComponentId { get; set; }
        public DateOnly? PreferredDate { get; set; }
        public string? Location { get; set; }
        public int? Quantity { get; set; }
        public byte? Status { get; set; }
        public decimal? HeightCm { get; set; }
        public decimal? WeightKg { get; set; }
        public string? HealthInfo { get; set; }
        public DateOnly? LastDonationDate { get; set; }

        public DateOnly? DateOfBirth { get; set; }
        public string? Name { get; set; }

    }
}
