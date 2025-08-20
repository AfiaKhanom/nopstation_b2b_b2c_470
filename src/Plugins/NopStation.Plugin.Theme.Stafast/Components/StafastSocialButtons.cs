using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Theme.Stafast.Models;

namespace NopStation.Plugin.Theme.Stafast.Components
{
    public class StafastSocialButtonsViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly StafastSettings _StafastSettings;

        #endregion

        #region Ctor

        public StafastSocialButtonsViewComponent(StafastSettings StafastSettings)
        {
            _StafastSettings = StafastSettings;
        }

        #endregion

        #region Methods

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var model = new StafastSocialModel()
            {
                LinkedInLink = _StafastSettings.LinkedInLink,
                PinterestLink = _StafastSettings.PinterestLink
            };

            return View(model);
        }

        #endregion
    }
}
