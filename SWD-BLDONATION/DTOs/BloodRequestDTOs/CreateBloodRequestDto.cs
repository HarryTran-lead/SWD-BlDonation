using System.ComponentModel.DataAnnotations;
using SWD_BLDONATION.Models.Enums; // Đảm bảo bạn đã import enum BloodRequestStatus

namespace SWD_BLDONATION.DTOs.BloodRequestDTOs
{
    public class CreateBloodRequestDto
    {
        public int? UserId { get; set; }

        // Xác thực yêu cầu nhập Name
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(255, ErrorMessage = "Name cannot exceed 255 characters.")]
        public string Name { get; set; } = null!;

        // Xác thực yêu cầu nhập DateOfBirth
        [Required(ErrorMessage = "DateOfBirth is required.")]
        public DateOnly DateOfBirth { get; set; }

        // Xác thực yêu cầu nhập Phone
        [Required(ErrorMessage = "Phone is required.")]
        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters.")]
        public string Phone { get; set; } = null!;

        // Xác thực yêu cầu nhập BloodTypeId
        [Required(ErrorMessage = "BloodTypeId is required.")]
        public int BloodTypeId { get; set; }

        // Xác thực yêu cầu nhập BloodComponentId
        [Required(ErrorMessage = "BloodComponentId is required.")]
        public int BloodComponentId { get; set; }

        // Xác thực yêu cầu nhập IsEmergency
        [Required(ErrorMessage = "IsEmergency is required.")]
        public bool IsEmergency { get; set; }

        // Xác thực yêu cầu nhập Location và chiều dài chuỗi không vượt quá 255 ký tự
        [Required(ErrorMessage = "Location is required.")]
        [StringLength(255, ErrorMessage = "Location cannot exceed 255 characters.")]
        public string Location { get; set; } = null!;

        // Xác thực yêu cầu nhập Quantity, phải là số dương
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be positive.")]
        public int Quantity { get; set; }

        // Xác thực yêu cầu nhập HeightCm, phải nằm trong khoảng 0 đến 300 cm
        [Required(ErrorMessage = "HeightCm is required.")]
        [Range(0, 300, ErrorMessage = "HeightCm must be between 0 and 300 cm.")]
        public decimal HeightCm { get; set; }

        // Xác thực yêu cầu nhập WeightKg, phải nằm trong khoảng 0 đến 500 kg
        [Required(ErrorMessage = "WeightKg is required.")]
        [Range(0, 500, ErrorMessage = "WeightKg must be between 0 and 500 kg.")]
        public decimal WeightKg { get; set; }

        // Xác thực yêu cầu nhập HealthInfo, chiều dài chuỗi không vượt quá 1000 ký tự
        [Required(ErrorMessage = "HealthInfo is required.")]
        [StringLength(1000, ErrorMessage = "HealthInfo cannot exceed 1000 characters.")]
        public string HealthInfo { get; set; } = null!;

        // Thêm trường Status với kiểu BloodRequestStatus (enum)
        [Required(ErrorMessage = "Status is required.")]
        public BloodRequestStatus Status { get; set; }  // Enum cho Status
    }
}
