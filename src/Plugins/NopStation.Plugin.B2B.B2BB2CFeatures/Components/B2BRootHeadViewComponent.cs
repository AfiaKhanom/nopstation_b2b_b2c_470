using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Components;
public class B2BRootHeadViewComponent : NopViewComponent
{
    public B2BRootHeadViewComponent()
    {

    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        return View();
    }
}
