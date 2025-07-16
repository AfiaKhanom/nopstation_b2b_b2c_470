using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.B2B.ERPIntegrationCore.Areas.Admin.Factories;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Infrastructure;

/// <summary>
/// Represents object for the configuring services on application startup
/// </summary>
public class NopStartup : INopStartup
{
    /// <summary>
    /// Add and configure any of the middleware
    /// </summary>
    /// <param name="services">Collection of service descriptors</param>
    /// <param name="configuration">Configuration of the application</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNopStationServices("NopStation.Plugin.B2B.ERPIntegrationCore");

        //add view location expander
        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new ViewLocationExpander());
        });

        //register services
        services.AddScoped<IErpAccountService, ErpAccountService>();
        services.AddScoped<IErpProductService, ErpProductService>();
        services.AddScoped<IErpGroupPriceCodeService, ErpGroupPriceCodeService>();
        services.AddScoped<IErpGroupPriceService, ErpGroupPriceService>();
        services.AddScoped<IErpInvoiceService, ErpInvoiceService>();
        services.AddScoped<IErpLogsService, ErpLogsService>();
        services.AddScoped<IErpNopUserAccountMapService, ErpNopUserAccountMapService>();
        services.AddScoped<IErpNopUserService, ErpNopUserService>();
        services.AddScoped<IErpOrderAdditionalDataService, ErpOrderAdditionalDataService>();
        services.AddScoped<IErpOrderItemAdditionalDataService, ErpOrderItemAdditionalDataService>();
        services.AddScoped<IErpSalesOrgService, ErpSalesOrgService>();
        services.AddScoped<IErpSalesRepSalesOrgMapService, ErpSalesRepSalesOrgMapService>();
        services.AddScoped<IErpSalesRepService, ErpSalesRepService>();
        services.AddScoped<IErpShipToAddressService, ErpShipToAddressService>();
        services.AddScoped<IErpSpecialPriceService, ErpSpecialPriceService>();
        services.AddScoped<IErpWarehouseAdditionalDataService, ErpWarehouseAdditionalDataService>();
        services.AddScoped<IErpWarehouseSalesOrgMapService, ErpWarehouseSalesOrgMapService>();
        services.AddScoped<IErpActivityLogsService, ErpActivityLogsService>();

        services.AddScoped<IErpAccountCustomerRegistrationFormService, ErpAccountCustomerRegistrationFormService>();
        services.AddScoped<IErpAccountCustomerRegistrationBankingDetailsService, ErpAccountCustomerRegistrationBankingDetailsService>();
        services.AddScoped<IErpAccountCustomerRegistrationPhysicalTradingAddressService, ErpAccountCustomerRegistrationPhysicalTradingAddressService>();
        services.AddScoped<IErpAccountCustomerRegistrationTradeReferencesService, ErpAccountCustomerRegistrationTradeReferencesService>();
        services.AddScoped<IErpAccountCustomerRegistrationPremisesService, ErpAccountCustomerRegistrationPremisesService>();

        services.AddScoped<IErpIntegrationPluginManager, ErpIntegrationPluginManager>();

        services.AddScoped<IConfigurationModelFactory, ConfigurationModelFactory>();
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
    public int Order => 1;
}