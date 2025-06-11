using System;
using System.Collections.Generic;

namespace SWD_BLDONATION.Models.Generated;

public partial class BloodType
{
    public int BloodTypeId { get; set; }

    public string? Name { get; set; }

    public string? RhFactor { get; set; }

    public virtual ICollection<BloodInventory> BloodInventories { get; set; } = new List<BloodInventory>();

    public virtual ICollection<BloodRequest> BloodRequests { get; set; } = new List<BloodRequest>();

    public virtual ICollection<DonationHistory> DonationHistories { get; set; } = new List<DonationHistory>();

    public virtual ICollection<DonationRequest> DonationRequests { get; set; } = new List<DonationRequest>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
