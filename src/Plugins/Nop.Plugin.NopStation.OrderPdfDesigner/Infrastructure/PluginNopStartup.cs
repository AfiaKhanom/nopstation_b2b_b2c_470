using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Nop.Core.Infrastructure;
using Nop.Plugin.NopStation.OrderPdfDesigner.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace Nop.Plugin.NopStation.OrderPdfDesigner.Infrastructure;

/// <summary>
/// Represents object for the configuring plugin on application startup
/// </summary>
public class PluginNopStartup : INopStartup
{
    /// <summary>
    /// Add and configure any of the middleware
    /// </summary>
    /// <param name="services">Collection of service descriptors</param>
    /// <param name="configuration">Configuration of the application</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNopStationServices("Nop.Plugin.NopStation.OrderPdfDesigner");

        services.AddScoped<IOrderPdfDesignerService, OrderPdfDesignerService>();
        services.AddScoped<IPdfRendererService, PdfRendererService>();

        // Use the Decorate pattern to wrap the original IPdfService
        // First, register the original PdfService with a different key
        services.AddScoped<PdfService>();
        
        // Then replace IPdfService with our custom implementation that wraps the original
        services.Replace(ServiceDescriptor.Scoped<IPdfService>(serviceProvider =>
        {
            // Get the original PdfService (now registered as concrete type)
            var defaultPdfService = serviceProvider.GetRequiredService<PdfService>();
            
            // Get our custom service, setting service and logger
            var orderPdfDesignerService = serviceProvider.GetRequiredService<IOrderPdfDesignerService>();
            var settingService = serviceProvider.GetRequiredService<ISettingService>();
            var logger = serviceProvider.GetRequiredService<ILogger<CustomPdfService>>();
            
            // Return wrapped service
            return new CustomPdfService(defaultPdfService, orderPdfDesignerService, settingService, logger);
        }));
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
