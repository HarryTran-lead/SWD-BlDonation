namespace SWD_BLDONATION.DTOs.DonationRequestDTOs
{
    public class CreateDonationRequestDto
    {
        public int? UserId { get; set; }
        public int? BloodTypeId { get; set; }
        public int? BloodComponentId { get; set; }
        public DateOnly? PreferredDate { get; set; }
        public string? Status { get; set; }
        public string? Location { get; set; }
        public int? Quantity { get; set; }
        public string? Note { get; set; }
        public decimal? HeightCm { get; set; }
        public decimal? WeightKg { get; set; }
        public string? HealthInfo { get; set; }  // Renamed from 'MedicalHistory' for consistency with model
        public DateOnly? LastDonationDate { get; set; }

        // Properties like HemoglobinLevel, BloodPressure, and PulseRate have been removed
        // since they are not in the original DonationRequest model
    }
}
