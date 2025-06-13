using SWD_BLDONATION.Models.Enums;

namespace SWD_BLDONATION.DTOs.BloodRequestDTOs
{
    public class BloodRequestDto
    {
        public int BloodRequestId { get; set; }
        public int? UserId { get; set; }
        public string Name { get; set; }
        public int BloodTypeId { get; set; }
        public string BloodTypeName { get; set; }
        public int BloodComponentId { get; set; }
        public string BloodComponentName { get; set; }
        public bool IsEmergency { get; set; }
        public StatusDto Status { get; set; } // Status will be an object containing Id and Name
        public DateTime CreatedAt { get; set; }
        public string Location { get; set; }
        public int Quantity { get; set; }
        public bool Fulfilled { get; set; }
        public string FulfilledSource { get; set; }
        public decimal HeightCm { get; set; }  // Changed to decimal for more precision
        public decimal WeightKg { get; set; }  // Changed to decimal for more precision
        public string HealthInfo { get; set; }
        public DateOnly? DateOfBirth { get; set; }  // Adding DateOfBirth with DateOnly
        public string Phone { get; set; }  // Add phone number if needed
    }

    public class StatusDto
    {
        public byte Id { get; set; }
        public string Name { get; set; }
    }
}
