using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Services.Barcode;
using Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Services.Pdf;
using Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Services.Template;
using Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Services.Token;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Infrastructure
{
    /// <summary>
    /// Represents plugin startup class for DI registration
    /// </summary>
    public class PluginNopStartup : INopStartup
    {
        public int Order => 3000;

        public void Configure(IApplicationBuilder application)
        {
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Register services
            services.AddScoped<ITemplateService, TemplateService>();
            services.AddScoped<IBarcodeService, BarcodeService>();
            services.AddScoped<ITokenResolverService, TokenResolverService>();
            
            // Register PDF renderers
            services.AddScoped<QuestPdfAdapter>();
            services.AddScoped<ChromiumAdapter>();
            
            // Register PDF renderer factory
            services.AddScoped<IPdfRenderer>(serviceProvider =>
            {
                var settings = serviceProvider.GetRequiredService<ConfigureInvoiceAndPackSlipSettings>();
                
                return settings.PdfRendererProvider?.ToLowerInvariant() switch
                {
                    "chromium" => serviceProvider.GetRequiredService<ChromiumAdapter>(),
                    _ => serviceProvider.GetRequiredService<QuestPdfAdapter>()
                };
            });
        }
    }
}
