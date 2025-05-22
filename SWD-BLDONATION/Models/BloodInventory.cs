using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SWD_BLDONATION.Models;

[Table("BloodInventory")]
public partial class BloodInventory
{
    [Key]
    [Column("inventory_id")]
    public int InventoryId { get; set; }

    [Column("blood_type_id")]
    public int? BloodTypeId { get; set; }

    [Column("blood_component_id")]
    public int? BloodComponentId { get; set; }

    [Column("quantity")]
    public int? Quantity { get; set; }

    [Column("unit")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Unit { get; set; }

    [Column("last_updated", TypeName = "datetime")]
    public DateTime? LastUpdated { get; set; }

    [Column("inventory_location")]
    [StringLength(255)]
    [Unicode(false)]
    public string? InventoryLocation { get; set; }

    [ForeignKey("BloodComponentId")]
    [InverseProperty("BloodInventories")]
    public virtual BloodComponent? BloodComponent { get; set; }

    [InverseProperty("Inventory")]
    public virtual ICollection<BloodRequestInventory> BloodRequestInventories { get; set; } = new List<BloodRequestInventory>();

    [ForeignKey("BloodTypeId")]
    [InverseProperty("BloodInventories")]
    public virtual BloodType? BloodType { get; set; }
}
