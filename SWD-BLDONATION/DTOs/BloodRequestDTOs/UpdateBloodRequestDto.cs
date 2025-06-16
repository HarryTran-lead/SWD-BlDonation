using System.ComponentModel.DataAnnotations;
using SWD_BLDONATION.Models.Enums; // Đảm bảo bạn đã import enum BloodRequestStatus

namespace SWD_BLDONATION.DTOs.BloodRequestDTOs
{
    public class UpdateBloodRequestDto
    {
        // Yêu cầu nhập BloodRequestId khi cập nhật yêu cầu
        [Required(ErrorMessage = "BloodRequestId is required.")]
        public int BloodRequestId { get; set; }

        // Thêm trường Name cho người yêu cầu
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(255, ErrorMessage = "Name cannot exceed 255 characters.")]
        public string Name { get; set; } = null!;

        // Thêm trường DateOfBirth cho người yêu cầu
        [Required(ErrorMessage = "DateOfBirth is required.")]
        public DateOnly DateOfBirth { get; set; }

        // Thêm trường Phone cho người yêu cầu
        [Required(ErrorMessage = "Phone is required.")]
        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters.")]
        public string Phone { get; set; } = null!;

        // UserId, BloodTypeId, BloodComponentId có thể không có giá trị trong một số trường hợp, do đó chúng được khai báo là nullable
        public int? UserId { get; set; }
        public int? BloodTypeId { get; set; }
        public int? BloodComponentId { get; set; }
        public bool? IsEmergency { get; set; }

        // Sử dụng enum BloodRequestStatus cho Status thay vì string, kiểu nullable để cho phép giá trị null
        [Required(ErrorMessage = "Status is required.")]
        public BloodRequestStatus? Status { get; set; }  // Enum cho Status

        // Đảm bảo Location không quá 255 ký tự nếu có giá trị
        [StringLength(255, ErrorMessage = "Location cannot exceed 255 characters.")]
        public string? Location { get; set; }

        // Quantity phải là số dương, có thể không cần thiết phải bắt buộc (nullable)
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be positive.")]
        public int? Quantity { get; set; }

        // Fulfilled có thể là true hoặc false, có thể nullable nếu không có giá trị
        public bool? Fulfilled { get; set; }

        // FulfilledSource không quá 255 ký tự nếu có giá trị
        [StringLength(255, ErrorMessage = "FulfilledSource cannot exceed 255 characters.")]
        public string? FulfilledSource { get; set; }

        // Chiều cao trong phạm vi 0 đến 300 cm
        [Range(0, 300, ErrorMessage = "HeightCm must be between 0 and 300 cm.")]
        public decimal? HeightCm { get; set; }

        // Cân nặng trong phạm vi 0 đến 500 kg
        [Range(0, 500, ErrorMessage = "WeightKg must be between 0 and 500 kg.")]
        public decimal? WeightKg { get; set; }

        // HealthInfo không quá 1000 ký tự
        [StringLength(1000, ErrorMessage = "HealthInfo cannot exceed 1000 characters.")]
        public string? HealthInfo { get; set; }
    }
}
