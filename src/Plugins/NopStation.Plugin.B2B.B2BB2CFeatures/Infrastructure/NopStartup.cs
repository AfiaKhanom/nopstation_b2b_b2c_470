using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.ActionFilters;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.B2BB2CFeatures.Controllers;
using NopStation.Plugin.B2B.B2BB2CFeatures.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Factories.ErpOrderDetails;
using NopStation.Plugin.B2B.B2BB2CFeatures.Factories.QuickOrder;
using NopStation.Plugin.B2B.B2BB2CFeatures.Helpers;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.Customers;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpPriceSyncFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpSpecificationAttributeService;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpWorkflowMessage;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ExportManager;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.Overriden;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services.QuickOrderServices;
using NopStation.Plugin.Misc.Core.Infrastructure;
using Nop.Services.Common;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpAccountCreditSyncFunctionality;
using Nop.Services.Localization;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Infrastructure;

public class NopStartup : INopStartup
{
    /// <summary>
    /// Add and configure any of the middleware
    /// </summary>
    /// <param name="services">Collection of service descriptors</param>
    /// <param name="configuration">Configuration of the application</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNopStationServices("NopStation.Plugin.B2B.B2BB2CFeatures");

        //add view location expander
        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new ViewLocationExpander());
        });

        //register services
        services.AddScoped<ICommonHelper, CommonHelper>();
        services.AddScoped<IB2BB2CWorkContext, B2BB2CWebWorkContext>();
        services.AddScoped<ICustomerRegistrationService, B2BB2CCustomerRegistrationService>();
        services.AddScoped<ICommonHelperService, CommonHelperService>();
        services.AddScoped<IErpCustomerFunctionalityService, ErpCustomerFunctionalityService>();
        services.AddScoped<IErpPriceSyncFunctionalityService, ErpPriceSyncFunctionalityService>();
        services.AddScoped<IPriceCalculationService, OverridenPriceCalculationService>();
        services.AddScoped<IProductService, OverridenProductService>();
        services.AddScoped<IErpSpecificationAttributeService, ErpSpecificationAttributeService>();

        services.AddScoped<IOverriddenOrderProcessingService, OverriddenOrderProcessingService>();
        services.AddScoped<IOrderProcessingService, OverriddenOrderProcessingService>();
        services.AddScoped<IAddressService, OverridenAddressService>();

        services.AddScoped<IB2BRegisterModelFactory, B2BRegisterModelFactory>();
        services.AddScoped<IErpShipToAddressModelFactory, ErpShipToAddressModelFactory>();
        services.AddScoped<IErpAccountModelFactory, ErpAccountModelFactory>();
        services.AddScoped<IErpSalesOrgModelFactory, ErpSalesOrgModelFactory>();
        services.AddScoped<IErpNopUserModelFactory, ErpNopUserModelFactory>();
        services.AddScoped<IErpInvoiceModelFactory, ErpInvoiceModelFactory>();
        services.AddScoped<ISalesRepUserModelFactory, SalesRepUserModelFactory>();

        services.AddScoped<CustomerController, B2BB2CCustomerController>();
        services.AddScoped<IErpGroupPriceCodeModelFactory, ErpGroupPriceCodeModelFactory>();
        services.AddScoped<IErpGroupPriceModelFactory, ErpGroupPriceModelFactory>();
        services.AddScoped<IErpSpecialPriceModelFactory, ErpSpecialPriceModelFactory>();
        services.AddScoped<IErpOrderModelFactory, ErpOrderModelFactory>();
        services.AddScoped<IErpSalesRepModelFactory, ErpSalesRepModelFactory>();

        services.AddScoped<IErpAccountPublicModelFactory, ErpAccountPublicModelFactory>();
        services.AddScoped<ICustomerModelFactory, Factories.OverridenCustomerModelFactory>();
        services.AddScoped<IProductModelFactory, OverridenProductModelFactory>();
        services.AddScoped<Nop.Web.Areas.Admin.Factories.ICustomerModelFactory, Areas.Admin.Factories.OverridenCustomerModelFactory>();
        services.AddScoped<IErpLogsModelFactory, ErpLogsModelFactory>();
        services.AddScoped<IErpCheckoutModelFactory, ErpCheckoutModelFactory>();
        services.AddScoped<IErpOrderItemModelFactory, ErpOrderItemModelFactory>();
        services.AddScoped<IErpProductModelFactory, ErpProductModelFactory>();

        services.AddScoped<IErpRegistrationApplicationModelFactory, ErpRegistrationApplicationModelFactory>();

        services.AddScoped<OrderController, Controllers.OverridenOrderController>();
        services.AddScoped<ShoppingCartController, OverridenShoppingCartController>();

        services.AddScoped<CheckoutController, ErpCheckoutController>();
        services.AddScoped<Nop.Web.Areas.Admin.Controllers.ProductController, OverridenProductController>();
        services.AddScoped<Nop.Web.Areas.Admin.Controllers.CustomerController, OverridenCustomerController>();
        services.AddScoped<Nop.Web.Areas.Admin.Controllers.OrderController, Areas.Admin.Controllers.OverridenOrderController>();

        services.AddScoped<IQuickOrderTemplateService, QuickOrderTemplateService>();
        services.AddScoped<IQuickOrderItemService, QuickOrderItemService>();
        services.AddScoped<IQuickOrderTemplateModelFactory, QuickOrderTemplateModelFactory>();
        services.AddScoped<IQuickOrderItemModelFactory, QuickOrderItemModelFactory>();

        services.AddScoped<IErpOrderDetailsModelFactory, ErpOrderDetailsModelFactory>();

        services.AddScoped<ICategoryProductsExportManager, CategoryProductsExportManager>();

        // add custom action filter
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<ErpSalesRepActionFilterAttribute>();
            options.Filters.Add<ErpNopUserActionFilterAttribute>();
        });

        services.AddScoped<IOverridenCustomerModelFactory, Areas.Admin.Factories.OverridenCustomerModelFactory>();

        services.AddScoped<IErpWorkflowMessageService, ErpWorkflowMessageService>();
        services.AddScoped<IErpActivityLogsModelFactory, ErpActivityLogsModelFactory>();
        services.AddScoped<IErpAccountCreditSyncFunctionality, ErpAccountCreditSyncFunctionality>();
        services.AddScoped<ILocalizationService, OverriddenLocalizationService>();

        services.AddScoped<IPermissionService, OverriddenPermissionService>();
        services.AddScoped<ICustomerService, OverriddenCustomerService>();
    }

    /// <summary>
    /// Configure the using of added middleware
    /// </summary>
    /// <param name="application">Builder for configuring an application's request pipeline</param>
    public void Configure(IApplicationBuilder application)
    {
    }

    /// <summary>
    /// Gets order of this startup configuration implementation
    /// </summary>
    public int Order => 30000;
}