using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD_BLDONATION.Models;

[Table("DonationHistory")]
public partial class DonationHistory
{
    [Key]
    [Column("donation_id")]
    public int DonationId { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }

    [Column("blood_type_id")]
    public int? BloodTypeId { get; set; }

    [Column("blood_component_id")]
    public int? BloodComponentId { get; set; }

    [Column("date")]
    public DateOnly? Date { get; set; }

    [Column("volume_ml")]
    public int? VolumeMl { get; set; }

    [Column("status")]
    [StringLength(10)]
    [Unicode(false)]
    public string? Status { get; set; }

    [ForeignKey("BloodComponentId")]
    [InverseProperty("DonationHistories")]
    public virtual BloodComponent? BloodComponent { get; set; }

    [ForeignKey("BloodTypeId")]
    [InverseProperty("DonationHistories")]
    public virtual BloodType? BloodType { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("DonationHistories")]
    public virtual User? User { get; set; }
}
