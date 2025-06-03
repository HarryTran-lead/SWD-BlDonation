using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SWD_BLDONATION.DTOs.BloodRequestDTOs
{
    public class CreateBloodRequestDto
    {
        // UserId có thể optional, ví dụ do đăng nhập hay không
        public int? UserId { get; set; }

        [Required(ErrorMessage = "BloodTypeId là bắt buộc")]
        public int BloodTypeId { get; set; }

        [Required(ErrorMessage = "BloodComponentId là bắt buộc")]
        public int BloodComponentId { get; set; }

        [Required(ErrorMessage = "IsEmergency là bắt buộc")]
        public bool IsEmergency { get; set; }

        [Required(ErrorMessage = "Location là bắt buộc")]
        [StringLength(255)]
        [DefaultValue("")]
        public string Location { get; set; } = "";

        [Required(ErrorMessage = "Quantity là bắt buộc")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "HeightCm là bắt buộc")]
        public decimal HeightCm { get; set; }

        [Required(ErrorMessage = "WeightKg là bắt buộc")]
        public decimal WeightKg { get; set; }

        [Required(ErrorMessage = "MedicalHistory là bắt buộc")]
        [DefaultValue("")]
        public string MedicalHistory { get; set; } = "";

        [Required(ErrorMessage = "PatientCondition là bắt buộc")]
        [DefaultValue("")]
        public string PatientCondition { get; set; } = "";

        [Required(ErrorMessage = "ReasonForRequest là bắt buộc")]
        [DefaultValue("")]
        public string ReasonForRequest { get; set; } = "";

        [Required(ErrorMessage = "UrgencyLevel là bắt buộc")]
        [DefaultValue("")]
        public string UrgencyLevel { get; set; } = "";
    }
}
