using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD_BLDONATION.Models;

[Table("BloodRequest")]
public partial class BloodRequest
{
    [Key]
    [Column("blood_request_id")]
    public int BloodRequestId { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }

    [Column("blood_type_id")]
    public int? BloodTypeId { get; set; }

    [Column("blood_component_id")]
    public int? BloodComponentId { get; set; }

    [Column("is_emergency")]
    public bool? IsEmergency { get; set; }

    [Column("status")]
    [StringLength(10)]
    [Unicode(false)]
    public string? Status { get; set; }

    [Column("created_at", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column("location")]
    [StringLength(255)]
    [Unicode(false)]
    public string? Location { get; set; }

    [Column("quantity")]
    public int? Quantity { get; set; }

    [Column("fulfilled")]
    public bool? Fulfilled { get; set; }

    [Column("fulfilled_source")]
    [StringLength(255)]
    [Unicode(false)]
    public string? FulfilledSource { get; set; }

    [Column("height_cm", TypeName = "decimal(5, 2)")]
    public decimal? HeightCm { get; set; }

    [Column("weight_kg", TypeName = "decimal(5, 2)")]
    public decimal? WeightKg { get; set; }

    [Column("medical_history", TypeName = "text")]
    public string? MedicalHistory { get; set; }

    [Column("patient_condition", TypeName = "text")]
    public string? PatientCondition { get; set; }

    [Column("reason_for_request", TypeName = "text")]
    public string? ReasonForRequest { get; set; }

    [Column("urgency_level")]
    [StringLength(10)]
    [Unicode(false)]
    public string? UrgencyLevel { get; set; }

    [ForeignKey("BloodComponentId")]
    [InverseProperty("BloodRequests")]
    public virtual BloodComponent? BloodComponent { get; set; }

    [InverseProperty("BloodRequest")]
    public virtual ICollection<BloodRequestInventory> BloodRequestInventories { get; set; } = new List<BloodRequestInventory>();

    [ForeignKey("BloodTypeId")]
    [InverseProperty("BloodRequests")]
    public virtual BloodType? BloodType { get; set; }

    [InverseProperty("BloodRequest")]
    public virtual ICollection<RequestMatch> RequestMatches { get; set; } = new List<RequestMatch>();

    [ForeignKey("UserId")]
    [InverseProperty("BloodRequests")]
    public virtual User? User { get; set; }
}
