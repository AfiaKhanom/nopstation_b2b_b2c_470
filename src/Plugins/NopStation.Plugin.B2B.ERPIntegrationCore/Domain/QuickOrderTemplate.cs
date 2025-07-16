using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Common;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class QuickOrderTemplate : BaseEntity, ISoftDeletedEntity
{
    private ICollection<QuickOrderItem> _quickOrderItems;

    public string Name { get; set; }

    public int CustomerId { get; set; }

    public decimal TotalPriceOfItems { get; set; }

    public DateTime? LastPriceCalculatedOnUtc { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? EditedOnUtc { get; set; }

    public bool Deleted { get; set; }

    public DateTime? LastOrderDate { get; set; }

    public virtual ICollection<QuickOrderItem> QuickOrderItems
    {
        get => _quickOrderItems ??= new List<QuickOrderItem>();
        protected set => _quickOrderItems = value;
    }
}
