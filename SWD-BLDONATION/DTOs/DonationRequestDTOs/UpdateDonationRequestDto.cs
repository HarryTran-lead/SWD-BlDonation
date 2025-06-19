namespace SWD_BLDONATION.DTOs.DonationRequestDTOs
{
    public class UpdateDonationRequestDto
    {
        public int DonateRequestId { get; set; }  // ID of the donation request to update
        public int? UserId { get; set; }  // User ID associated with the donation request
        public int? BloodTypeId { get; set; }  // Blood type ID
        public int? BloodComponentId { get; set; }  // Blood component ID
        public DateOnly? PreferredDate { get; set; }  // Preferred donation date
        public string? Status { get; set; }  // Current status of the donation request
        public string? Location { get; set; }  // Location for the donation
        public int? Quantity { get; set; }  // Quantity of blood to be donated
        public string? Note { get; set; }  // Additional notes for the donation request
        public decimal? HeightCm { get; set; }  // Height of the person making the donation
        public decimal? WeightKg { get; set; }  // Weight of the person making the donation
        public string? HealthInfo { get; set; }  // Health information of the donor (renamed from 'MedicalHistory' for consistency)
        public DateOnly? LastDonationDate { get; set; }  // Date of the last donation made by the donor
    }
}
