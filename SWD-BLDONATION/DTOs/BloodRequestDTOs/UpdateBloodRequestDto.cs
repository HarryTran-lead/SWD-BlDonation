using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SWD_BLDONATION.DTOs.BloodRequestDTOs
{
    public class UpdateBloodRequestDto
    {
        [Required(ErrorMessage = "BloodRequestId là bắt buộc")]
        public int BloodRequestId { get; set; }

        public int? BloodTypeId { get; set; }
        public int? BloodComponentId { get; set; }
        public bool? IsEmergency { get; set; }

        [DefaultValue("")]
        public string? Status { get; set; }

        [DefaultValue("")]
        public string? Location { get; set; }

        public int? Quantity { get; set; }

        public bool? Fulfilled { get; set; }

        [DefaultValue("")]
        public string? FulfilledSource { get; set; }

        public decimal? HeightCm { get; set; }
        public decimal? WeightKg { get; set; }

        [DefaultValue("")]
        public string? MedicalHistory { get; set; }

        [DefaultValue("")]
        public string? PatientCondition { get; set; }

        [DefaultValue("")]
        public string? ReasonForRequest { get; set; }

        [DefaultValue("")]
        public string? UrgencyLevel { get; set; }
    }
}
