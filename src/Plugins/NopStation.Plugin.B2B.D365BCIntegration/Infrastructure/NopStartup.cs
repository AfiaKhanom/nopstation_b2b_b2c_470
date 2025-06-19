using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Web.Framework.Infrastructure.Extensions;
using NopStation.Plugin.B2B.D365BCIntegration.D365BCImplementation;
using NopStation.Plugin.B2B.D365BCIntegration.Services;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace NopStation.Plugin.B2B.D365BCIntegration.Infrastructure;

public class NopStartup : INopStartup
{
    /// <summary>
    /// Add and configure any of the middleware
    /// </summary>
    /// <param name="services">Collection of service descriptors</param>
    /// <param name="configuration">Configuration of the application</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNopStationServices("NopStation.Plugin.B2B.D365BCIntegration");

        //add view location expander
        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new ViewLocationExpander());
        });

        //register services
        services.AddSingleton<ID365BCAuthService, D365BCAuthService>();
        services.AddHttpClient<D365BCHttpClient>().WithProxy();
        services.AddScoped<IErpNopMapperService, ErpNopMapperService>();
        services.AddScoped<ID365BCService, D365BCService>();
        services.AddScoped<ID365BCIntegrationAccountService, D365BCIntegrationAccountService>();
        services.AddScoped<ID365BCIntegrationProductService, D365BCIntegrationProductService>();
        services.AddScoped<ID365BCIntegrationStockService, D365BCIntegrationStockService>();
        services.AddScoped<ID365BCIntegrationOrderService, D365BCIntegrationOrderService>();
        services.AddScoped<ID365BCHttpService, D365BCHttpService>();

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
    public int Order => 3000;
}
