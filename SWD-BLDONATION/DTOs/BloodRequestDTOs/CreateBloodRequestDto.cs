namespace SWD_BLDONATION.DTOs.BloodRequestDTOs
{
    public class CreateBloodRequestDto
    {
        public int? UserId { get; set; }
        public int? BloodTypeId { get; set; }
        public int? BloodComponentId { get; set; }
        public bool? IsEmergency { get; set; }
        public string? Location { get; set; }
        public int? Quantity { get; set; }
        public decimal? HeightCm { get; set; }
        public decimal? WeightKg { get; set; }
        public string? MedicalHistory { get; set; }
        public string? PatientCondition { get; set; }
        public string? ReasonForRequest { get; set; }
        public string? UrgencyLevel { get; set; }
    }
}
