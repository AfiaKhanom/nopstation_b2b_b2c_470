using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Customers;
using NopStation.Plugin.B2B.ERPIntegrationCore;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.ActionFilters;

public class ErpNopUserActionFilterAttribute : ActionFilterAttribute
{
    private IWorkContext _workContext;
    private IWebHelper _webHelper;
    private ICustomerService _customerService;
    private IUrlHelperFactory _urlHelperFactory;
    private IActionContextAccessor _actionContextAccessor;

    private IWorkContext WorkContext => _workContext ?? (_workContext = EngineContext.Current.Resolve<IWorkContext>());
    private IWebHelper WebHelper => _webHelper ?? (_webHelper = EngineContext.Current.Resolve<IWebHelper>());
    private ICustomerService CustomerService => _customerService ?? (_customerService = EngineContext.Current.Resolve<ICustomerService>());
    private IUrlHelperFactory UrlHelperFactory => _urlHelperFactory ?? (_urlHelperFactory = EngineContext.Current.Resolve<IUrlHelperFactory>());
    private IActionContextAccessor ActionContextAccessor => _actionContextAccessor ?? (_actionContextAccessor = EngineContext.Current.Resolve<IActionContextAccessor>());

    public override async void OnActionExecuting(ActionExecutingContext context)
    {
        if (!(context.ActionDescriptor is ControllerActionDescriptor actionDescriptor))
            return;

        var controllerName = actionDescriptor.ControllerName;
        var actionName = actionDescriptor.ActionName;

        var area = context.RouteData.Values["area"] as string;
        var assembly = actionDescriptor.ControllerTypeInfo.Assembly;

        try
        {
            var customer = await WorkContext.GetCurrentCustomerAsync();

            if (!await CustomerService.IsInCustomerRoleAsync(customer, ERPIntegrationCoreDefaults.B2BB2CAdminRoleSystemName))
            {
                return;
            }
            if (controllerName == "ErpNopUser" || controllerName == "Home" || controllerName == "Common" ||
                controllerName == "ErpAccount" && actionName == "ErpAccountSearchAutoComplete" ||
                controllerName == "Security" && actionName == "AccessDenied" ||
                controllerName == "CustomerImpersonate" && actionName == "Logout")
            {
                return;
            }

            var url = $"{WebHelper.GetStoreLocation().TrimEnd('/')}/{((area == "Admin") ? "Admin/" : string.Empty)}{controllerName}/{actionName}";

            if (area is not null && area == "Admin")
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Security", new { pageUrl = url });
            }
            else
            {
                if (assembly.GetName().Name.StartsWith("NopStation.Plugin.", StringComparison.InvariantCultureIgnoreCase))
                {
                    var urlHelper = UrlHelperFactory.GetUrlHelper(ActionContextAccessor.ActionContext);
                    var deniedUrl = urlHelper.RouteUrl("PageNotFound");
                    context.Result = new RedirectResult(deniedUrl);
                }
            }
        }
        catch
        {
            return;
        }
    }
}
