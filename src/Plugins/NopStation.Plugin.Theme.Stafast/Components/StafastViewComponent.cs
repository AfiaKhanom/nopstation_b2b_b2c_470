using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Theme.Stafast.Components
{
    public class StafastViewComponent : NopStationViewComponent
    {
        public IViewComponentResult Invoke(string widgetZone)
        {
            return View();
        }
    }
}
