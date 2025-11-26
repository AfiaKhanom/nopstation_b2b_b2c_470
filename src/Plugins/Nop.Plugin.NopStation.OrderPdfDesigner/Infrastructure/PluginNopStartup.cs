using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nop.Core.Infrastructure;
using Nop.Plugin.NopStation.OrderPdfDesigner.Services;
using Nop.Services.Common;
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

        // Override the default IPdfService with our custom implementation
        // This will intercept all PDF generation calls and use our custom templates
        services.Replace(ServiceDescriptor.Scoped<IPdfService>(serviceProvider =>
        {
            // Get the default PdfService implementation
            var defaultPdfService = ActivatorUtilities.CreateInstance<PdfService>(serviceProvider);
            
            // Get our custom service
            var orderPdfDesignerService = serviceProvider.GetRequiredService<IOrderPdfDesignerService>();
            
            // Return wrapped service
            return new CustomPdfService(defaultPdfService, orderPdfDesignerService);
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
