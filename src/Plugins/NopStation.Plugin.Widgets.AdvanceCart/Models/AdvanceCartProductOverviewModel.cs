using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.AdvanceCart.Models;

public record AdvanceCartProductOverviewModel : BaseNopEntityModel
{
    public AdvanceCartProductOverviewModel()
    {
        AllowedQuantities = new List<SelectListItem>();
    }

    //qty
    [NopResourceDisplayName("Products.Qty")]
    public int EnteredQuantity { get; set; }
    public string MinimumQuantityNotification { get; set; }
    public List<SelectListItem> AllowedQuantities { get; set; }

    public bool DisableBuyButton { get; set; }

    public bool EnableBuyNowButton { get; set; }
    public bool AllowCustomersToSelectQuantityFromProductBox { get; set; }
}
