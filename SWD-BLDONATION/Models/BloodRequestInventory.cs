using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD_BLDONATION.Models;

[Table("BloodRequestInventory")]
public partial class BloodRequestInventory
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("blood_request_id")]
    public int? BloodRequestId { get; set; }

    [Column("inventory_id")]
    public int? InventoryId { get; set; }

    [Column("quantity_unit")]
    public int? QuantityUnit { get; set; }

    [Column("quantity_allocated")]
    public int? QuantityAllocated { get; set; }

    [Column("allocated_at", TypeName = "datetime")]
    public DateTime? AllocatedAt { get; set; }

    [Column("allocated_by")]
    public int? AllocatedBy { get; set; }

    [ForeignKey("AllocatedBy")]
    [InverseProperty("BloodRequestInventories")]
    public virtual User? AllocatedByNavigation { get; set; }

    [ForeignKey("BloodRequestId")]
    [InverseProperty("BloodRequestInventories")]
    public virtual BloodRequest? BloodRequest { get; set; }

    [ForeignKey("InventoryId")]
    [InverseProperty("BloodRequestInventories")]
    public virtual BloodInventory? Inventory { get; set; }
}
