using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Data;
using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Factories;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.NopStationSyncServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.QuartzServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncWorkflowMessage;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Infrastructure;

public class NopStartup : INopStartup
{
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
        services.AddScoped<IQuartzJobDetailService, QuartzJobDetailService>();
        services.AddScoped<IQrtzFiredTriggersService, QrtzFiredTriggersService>();
        services.AddScoped<ISyncWorkflowMessageService, SyncWorkflowMessageService>();
    }

    public void Configure(IApplicationBuilder application)
    {
        //further actions are performed only when the database is installed
        if (!DataSettingsManager.IsDatabaseInstalled())
        {
            return;
        }
    }

    public int Order => 3000;
}
