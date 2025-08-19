using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Widgets.AdvanceCart.Components;

public class AdvanceCartViewComponent : NopStationViewComponent
{
    private readonly AdvanceCartSettings _advanceCartSettings;

    public AdvanceCartViewComponent(AdvanceCartSettings advanceCartSettings)
    {
        _advanceCartSettings = advanceCartSettings;
    }

    public IViewComponentResult Invoke(string widgetZone, object additionalData)
    {
        if (!_advanceCartSettings.EnableAdvanceCartPlugin)
            return Content("");

        return View();
    }
}
