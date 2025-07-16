using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;

namespace NopStation.Plugin.Payments.B2BAccount.Infrastructure;

public class PluginNopStartup : INopStartup
{
    public static string NopStationPluginSystemName => "NopStation.Plugin.Payments.B2BAccount";

    public void Configure(IApplicationBuilder application)
    {
        
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNopStationServices(NopStationPluginSystemName);
    }

    public int Order => 20;
}
