using System;
using System.Collections.Generic;

namespace SWD_BLDONATION.Models.Generated;

public partial class User
{
    public int UserId { get; set; }

    public int? BloodTypeId { get; set; }

    public int? BloodComponentId { get; set; }

    public string? UserName { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? Phone { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Address { get; set; }

    public string? Identification { get; set; }

    public bool IsDeleted { get; set; }

    public byte? RoleBit { get; set; }

    public byte? StatusBit { get; set; }

    public double? HeightCm { get; set; }

    public double? WeightKg { get; set; }

    public string? MedicalHistory { get; set; }

    public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();

    public virtual BloodComponent? BloodComponent { get; set; }

    public virtual ICollection<BloodRequestInventory> BloodRequestInventories { get; set; } = new List<BloodRequestInventory>();

    public virtual ICollection<BloodRequest> BloodRequests { get; set; } = new List<BloodRequest>();

    public virtual BloodType? BloodType { get; set; }

    public virtual ICollection<DonationHistory> DonationHistories { get; set; } = new List<DonationHistory>();

    public virtual ICollection<DonationRequest> DonationRequests { get; set; } = new List<DonationRequest>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
