using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace NopStation.Plugin.Theme.Stafast.Components
{
    public class StafastHeaderLoginViewComponent : NopViewComponent
    {
        #region Fields

        private readonly StafastSettings _StafastSettings;
        private readonly ICustomerModelFactory _customerModelFactory;

        #endregion

        #region Ctor

        public StafastHeaderLoginViewComponent(StafastSettings StafastSettings,
            ICustomerModelFactory customerModelFactory)
        {
            _StafastSettings = StafastSettings;
            _customerModelFactory = customerModelFactory;
        }

        #endregion

        #region Methods

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!_StafastSettings.EnableLoginBoxAtHeader)
                return Content("");

            var model = await _customerModelFactory.PrepareLoginModelAsync(null);
            return View(model);
        }

        #endregion
    }
}
