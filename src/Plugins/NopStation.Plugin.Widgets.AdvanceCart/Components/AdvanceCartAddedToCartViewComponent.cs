using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.AdvanceCart.Models;

namespace NopStation.Plugin.Widgets.AdvanceCart.Components;

public class AdvanceCartAddedToCartViewComponent : NopStationViewComponent
{
    private readonly AdvanceCartSettings _advanceCartSettings;

    public AdvanceCartAddedToCartViewComponent(AdvanceCartSettings advanceCartSettings)
    {
        _advanceCartSettings = advanceCartSettings;
    }

    public IViewComponentResult Invoke(AdvanceCartAddedToCartModel model)
    {
        if (!_advanceCartSettings.EnableAdvanceCartPlugin)
            return Content("");

        if (!_advanceCartSettings.EnableAddedToCartNotificationPopup)
            return Content("");

        return View(model);
    }
}
