using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD_BLDONATION.Models;

[Table("DonationRequest")]
public partial class DonationRequest
{
    [Key]
    [Column("donate_request_id")]
    public int DonateRequestId { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }

    [Column("blood_type_id")]
    public int? BloodTypeId { get; set; }

    [Column("blood_component_id")]
    public int? BloodComponentId { get; set; }

    [Column("preferred_date")]
    public DateOnly? PreferredDate { get; set; }

    [Column("status")]
    [StringLength(10)]
    [Unicode(false)]
    public string? Status { get; set; }

    [Column("location")]
    [StringLength(255)]
    [Unicode(false)]
    public string? Location { get; set; }

    [Column("created_at", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column("quantity")]
    public int? Quantity { get; set; }

    [Column("note", TypeName = "text")]
    public string? Note { get; set; }

    [Column("height_cm", TypeName = "decimal(5, 2)")]
    public decimal? HeightCm { get; set; }

    [Column("weight_kg", TypeName = "decimal(5, 2)")]
    public decimal? WeightKg { get; set; }

    [Column("medical_history", TypeName = "text")]
    public string? MedicalHistory { get; set; }

    [Column("general_health_status", TypeName = "text")]
    public string? GeneralHealthStatus { get; set; }

    [Column("hemoglobin_level", TypeName = "decimal(4, 2)")]
    public decimal? HemoglobinLevel { get; set; }

    [Column("last_donation_date")]
    public DateOnly? LastDonationDate { get; set; }

    [Column("blood_pressure")]
    [StringLength(10)]
    [Unicode(false)]
    public string? BloodPressure { get; set; }

    [Column("pulse_rate")]
    public int? PulseRate { get; set; }

    [ForeignKey("BloodComponentId")]
    [InverseProperty("DonationRequests")]
    public virtual BloodComponent? BloodComponent { get; set; }

    [ForeignKey("BloodTypeId")]
    [InverseProperty("DonationRequests")]
    public virtual BloodType? BloodType { get; set; }

    [InverseProperty("DonationRequest")]
    public virtual ICollection<RequestMatch> RequestMatches { get; set; } = new List<RequestMatch>();

    [ForeignKey("UserId")]
    [InverseProperty("DonationRequests")]
    public virtual User? User { get; set; }
}
