using Microsoft.AspNetCore.Mvc;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.AdvanceCart.Models;

namespace NopStation.Plugin.Widgets.AdvanceCart.Components;

public class AdvanceCartDetailsViewComponent : NopStationViewComponent
{
    private readonly AdvanceCartSettings _advanceCartSettings;

    public AdvanceCartDetailsViewComponent(AdvanceCartSettings advanceCartSettings)
    {
        _advanceCartSettings = advanceCartSettings;
    }

    public IViewComponentResult Invoke(string widgetZone, object additionalData)
    {
        if (!_advanceCartSettings.EnableAdvanceCartPlugin)
            return Content("");

        if (additionalData is not ProductDetailsModel.AddToCartModel addToCartModel)
            return Content("");

        var model = new AdvanceCartProductDetailsModel
        {
            Id = addToCartModel.ProductId,
            EnableBuyNowButton = _advanceCartSettings.EnableBuyNowButton
        };

        return View(model);
    }
}
