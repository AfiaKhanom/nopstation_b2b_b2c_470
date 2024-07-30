using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.B2B.IQRetailIntegration.ErpInterfaceImplementation;
using NopStation.Plugin.B2B.IQRetailIntegration.Service;
using Nop.Web.Framework.Infrastructure.Extensions;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Infrastructure
{
    public class NopStartup : INopStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddNopStationServices("NopStation.Plugin.B2B.IQRetailIntegration");

            //add view location expander
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });

            //register services
            services.AddHttpClient<IQRetailHttpClient>().WithProxy();
            services.AddScoped<IErpNopMapperService, ErpNopMapperService>();
            services.AddScoped<IIQRetailService, IQRetailService>();
            services.AddScoped<ErpIntegrationAccountService>();
            services.AddScoped<ErpIntegrationOrderService>();
            services.AddScoped<ErpIntegrationProductService>();
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
}
