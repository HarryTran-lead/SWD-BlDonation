using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD_BLDONATION.Models;

[Table("BloodType")]
public partial class BloodType
{
    [Key]
    [Column("blood_type_id")]
    public int BloodTypeId { get; set; }

    [Column("name")]
    [StringLength(2)]
    [Unicode(false)]
    public string? Name { get; set; }

    [Column("rh_factor")]
    [StringLength(1)]
    [Unicode(false)]
    public string? RhFactor { get; set; }

    [InverseProperty("BloodType")]
    public virtual ICollection<BloodInventory> BloodInventories { get; set; } = new List<BloodInventory>();

    [InverseProperty("BloodType")]
    public virtual ICollection<BloodRequest> BloodRequests { get; set; } = new List<BloodRequest>();

    [InverseProperty("BloodType")]
    public virtual ICollection<DonationHistory> DonationHistories { get; set; } = new List<DonationHistory>();

    [InverseProperty("BloodType")]
    public virtual ICollection<DonationRequest> DonationRequests { get; set; } = new List<DonationRequest>();

    [InverseProperty("BloodType")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
