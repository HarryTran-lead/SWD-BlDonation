using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD_BLDONATION.Models;

[Table("BloodComponent")]
[Index("Name", Name = "UQ__BloodCom__72E12F1BC76DA3F1", IsUnique = true)]
public partial class BloodComponent
{
    [Key]
    [Column("blood_component_id")]
    public int BloodComponentId { get; set; }

    [Column("name")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Name { get; set; }

    [InverseProperty("BloodComponent")]
    public virtual ICollection<BloodInventory> BloodInventories { get; set; } = new List<BloodInventory>();

    [InverseProperty("BloodComponent")]
    public virtual ICollection<BloodRequest> BloodRequests { get; set; } = new List<BloodRequest>();

    [InverseProperty("BloodComponent")]
    public virtual ICollection<DonationHistory> DonationHistories { get; set; } = new List<DonationHistory>();

    [InverseProperty("BloodComponent")]
    public virtual ICollection<DonationRequest> DonationRequests { get; set; } = new List<DonationRequest>();

    [InverseProperty("BloodComponent")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
