using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.NopStationSyncServices;
using Quartz;
using Quartz.AspNetCore;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Infrastructure;

public class SchedulerNopStartup : INopStartup
{
    public int Order => 1;
    public static string CorsClient = "B2bErpDataScheduler";

    public void Configure(IApplicationBuilder application)
    {
        application.UseCors(CorsClient);
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        #region Quartz Configs

        var connectionString = configuration.GetConnectionString("ConnectionString");

        services.AddQuartz(config =>
        {
            config.UsePersistentStore(opt =>
            {
                opt.UseProperties = true;
                opt.UseSqlServer(sqlOpt =>
                {
                    sqlOpt.ConnectionString = connectionString!;
                });

                opt.UseNewtonsoftJsonSerializer();
            });

            config.AddJobListener<NopStationJobListener>();
            config.AddTriggerListener<NopStationTriggerListener>();
        });

        services.AddQuartzServer(option =>
        {
            option.WaitForJobsToComplete = true;
        });

        #endregion

        services.AddCors(opt =>
        {
            opt.AddPolicy(CorsClient, policy =>
            {
                policy
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        services.AddScoped<INopStationScheduler, NopStationScheduler>();
    }
}
