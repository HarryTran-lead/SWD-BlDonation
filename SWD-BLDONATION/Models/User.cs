using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD_BLDONATION.Models;

[Table("User")]
[Index("Identification", Name = "UQ__User__AAA7C1F571AA5E41", IsUnique = true)]
[Index("Email", Name = "UQ__User__AB6E616478514D0E", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("blood_type_id")]
    public int? BloodTypeId { get; set; }

    [Column("blood_component_id")]
    public int? BloodComponentId { get; set; }

    [Column("user_name")]
    [StringLength(255)]
    [Unicode(false)]
    public string? UserName { get; set; }

    [Column("name")]
    [StringLength(255)]
    [Unicode(false)]
    public string? Name { get; set; }

    [Column("email")]
    [StringLength(255)]
    [Unicode(false)]
    public string? Email { get; set; }

    [Column("password")]
    [StringLength(255)]
    [Unicode(false)]
    public string? Password { get; set; }

    [Column("phone")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Phone { get; set; }

    [Column("date_of_birth")]
    public DateOnly? DateOfBirth { get; set; }

    [Column("role")]
    [StringLength(10)]
    [Unicode(false)]
    public string? Role { get; set; }

    [Column("address")]
    [StringLength(255)]
    [Unicode(false)]
    public string? Address { get; set; }

    [Column("identification")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Identification { get; set; }

    [Column("status")]
    [StringLength(10)]
    [Unicode(false)]
    public string? Status { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();

    [ForeignKey("BloodComponentId")]
    [InverseProperty("Users")]
    public virtual BloodComponent? BloodComponent { get; set; }

    [InverseProperty("AllocatedByNavigation")]
    public virtual ICollection<BloodRequestInventory> BloodRequestInventories { get; set; } = new List<BloodRequestInventory>();

    [InverseProperty("User")]
    public virtual ICollection<BloodRequest> BloodRequests { get; set; } = new List<BloodRequest>();

    [ForeignKey("BloodTypeId")]
    [InverseProperty("Users")]
    public virtual BloodType? BloodType { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<DonationHistory> DonationHistories { get; set; } = new List<DonationHistory>();

    [InverseProperty("User")]
    public virtual ICollection<DonationRequest> DonationRequests { get; set; } = new List<DonationRequest>();

    [InverseProperty("User")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
