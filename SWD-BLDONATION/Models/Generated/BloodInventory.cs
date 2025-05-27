using System;
using System.Collections.Generic;

namespace SWD_BLDONATION.Models.Generated;

public partial class BloodInventory
{
    public int InventoryId { get; set; }

    public int? BloodTypeId { get; set; }

    public int? BloodComponentId { get; set; }

    public int? Quantity { get; set; }

    public string? Unit { get; set; }

    public DateTime? LastUpdated { get; set; }

    public string? InventoryLocation { get; set; }

    public virtual BloodComponent? BloodComponent { get; set; }

    public virtual ICollection<BloodRequestInventory> BloodRequestInventories { get; set; } = new List<BloodRequestInventory>();

    public virtual BloodType? BloodType { get; set; }
}
