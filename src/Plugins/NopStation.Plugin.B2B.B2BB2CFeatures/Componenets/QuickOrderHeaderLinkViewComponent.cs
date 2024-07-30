using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.QuickOrder.Components
{
    [ViewComponent(Name = "QuickOrderHeaderLink")]
    public class QuickOrderHeaderLinkViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return Content(string.Empty);
        }
    }
}