using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Data;
using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Factories;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskScheduler;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Infrastructure
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
            services.AddNopStationServices("NopStation.Plugin.B2B.ErpDataScheduler");

            //add view location expander
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });

            //register services
            services.AddScoped<ISyncTaskModelFactory, SyncTaskModelFactory>();

            services.AddScoped<ISyncTaskService, SyncTaskService>();
            services.AddSingleton<ISyncTaskScheduler, SyncTaskScheduler>();
            services.AddTransient<ISyncTaskRunner, SyncTaskRunner>();

            services.AddScoped<IErpDataClearCacheService, ErpDataClearCacheService>();
            services.AddScoped<IErpAccountSyncService, ErpAccountSyncService>();
            services.AddScoped<IErpInvoiceSyncService, ErpInvoiceSyncService>();
            services.AddScoped<IErpShipToAddressSyncService, ErpShipToAddressSyncService>();
            services.AddScoped<IErpSpecialPriceSyncService, ErpSpecialPriceSyncService>();
            services.AddScoped<IErpGroupPriceSyncService, ErpGroupPriceSyncService>();
            services.AddScoped<IErpOrderSyncService, ErpOrderSyncService>();
            services.AddScoped<IErpProductSyncService, ErpProductSyncService>();
            services.AddScoped<IErpStockSyncService, ErpStockSyncService>();

            services.AddScoped<ISyncLogService, SyncLogService>();
        }

        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public void Configure(IApplicationBuilder application)
        {
            var engine = EngineContext.Current;

            //further actions are performed only when the database is installed
            if (!DataSettingsManager.IsDatabaseInstalled())
            {
                return;
            }

            var taskScheduler = engine.Resolve<ISyncTaskScheduler>();
            var result = false;

            try
            {
                result = taskScheduler.InitializeAsync().Result;
            }
            catch(Exception ex)
            {
                return;
            }

            if (!result)
                return;
            
            taskScheduler.StartSyncTaskScheduler();
        }

        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order => 3000;
    }
}
