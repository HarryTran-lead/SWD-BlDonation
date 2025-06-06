﻿namespace SWD_BLDONATION.DTOs.BloodRequestDTOs
{
    public class BloodRequestDto
    {
        public int BloodRequestId { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public int? BloodTypeId { get; set; }
        public string? BloodTypeName { get; set; }
        public int? BloodComponentId { get; set; }
        public string? BloodComponentName { get; set; }
        public bool IsEmergency { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string Location { get; set; } = null!;
        public int Quantity { get; set; }
        public bool Fulfilled { get; set; }
        public string? FulfilledSource { get; set; }
        public decimal HeightCm { get; set; }
        public decimal WeightKg { get; set; }
        public string MedicalHistory { get; set; } = null!;
        public string PatientCondition { get; set; } = null!;
        public string ReasonForRequest { get; set; } = null!;
        public string UrgencyLevel { get; set; } = null!;
    }
}