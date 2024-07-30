using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.Customers;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Componenets
{
    public class SalesRepHeaderViewComponent : NopViewComponent
    {
        private readonly ICommonHelperService _commonHelperService;

        public SalesRepHeaderViewComponent(ICommonHelperService commonHelperService)
        {
            _commonHelperService = commonHelperService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (await _commonHelperService.HasB2BSalesRepRoleAsync())
            {
                return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/Shared/Components/SalesRepHeader/Default.cshtml");
            }

            return Content(string.Empty);
        }
    }
}