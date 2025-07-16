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

public class ErpSalesRepActionFilterAttribute : ActionFilterAttribute
{
    private IWorkContext _workContext;
    private ICustomerService _customerService;
    private IUrlHelperFactory _urlHelperFactory;
    private IActionContextAccessor _actionContextAccessor;

    private IWorkContext WorkContext => _workContext ?? (_workContext = EngineContext.Current.Resolve<IWorkContext>());
    private ICustomerService CustomerService => _customerService ?? (_customerService = EngineContext.Current.Resolve<ICustomerService>());
    private IUrlHelperFactory UrlHelperFactory => _urlHelperFactory ?? (_urlHelperFactory = EngineContext.Current.Resolve<IUrlHelperFactory>());
    private IActionContextAccessor ActionContextAccessor => _actionContextAccessor ?? (_actionContextAccessor = EngineContext.Current.Resolve<IActionContextAccessor>());

    public override async void OnActionExecuting(ActionExecutingContext context)
    {
        if (!(context.ActionDescriptor is ControllerActionDescriptor actionDescriptor))
            return;

        var controllerName = actionDescriptor.ControllerName;

        try
        {
            var customer = await WorkContext.GetCurrentCustomerAsync();

            if (!await CustomerService.IsInCustomerRoleAsync(customer, ERPIntegrationCoreDefaults.B2BSalesRepRoleSystemName))
            {
                return;
            }
            else if (controllerName == "CustomerImpersonate")
            {
                return;
            }

            //generate the relative URL
            var urlHelper = UrlHelperFactory.GetUrlHelper(ActionContextAccessor.ActionContext);
            var url = urlHelper.RouteUrl("CustomerImpersonateList");
            context.Result = new RedirectResult(url);
        }
        catch
        {
            return;
        }
    }
}
