using System;
using System.Collections.Generic;

namespace SWD_BLDONATION.Models.Generated;

public partial class BloodRequestInventory
{
    public int Id { get; set; }

    public int? BloodRequestId { get; set; }

    public int? InventoryId { get; set; }

    public int? QuantityUnit { get; set; }

    public int? QuantityAllocated { get; set; }

    public DateTime? AllocatedAt { get; set; }

    public int? AllocatedBy { get; set; }

    public virtual User? AllocatedByNavigation { get; set; }

    public virtual BloodRequest? BloodRequest { get; set; }

    public virtual BloodInventory? Inventory { get; set; }
}
