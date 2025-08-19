using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Widgets.AjaxCart.Components;

public class AjaxCartViewComponent : NopStationViewComponent
{
    private readonly AjaxCartSettings _ajaxCartSettings;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AjaxCartViewComponent(AjaxCartSettings ajaxCartSettings,
        IHttpContextAccessor httpContextAccessor)
    {
        _ajaxCartSettings = ajaxCartSettings;
        _httpContextAccessor = httpContextAccessor;
    }

    public IViewComponentResult Invoke(string widgetZone, object additionalData)
    {
        if (!_ajaxCartSettings.EnableAjaxCartPlugin)
            return Content("");

        var controller = _httpContextAccessor.HttpContext.Request.RouteValues["controller"].ToString();
        var action = _httpContextAccessor.HttpContext.Request.RouteValues["action"].ToString();

        if (controller.Equals("ShoppingCart", StringComparison.InvariantCultureIgnoreCase) &&
            (action.Equals("Cart", StringComparison.InvariantCultureIgnoreCase) ||
            action.Equals("Wishlist", StringComparison.InvariantCultureIgnoreCase)))
            return View();

        return Content("");
    }
}
