using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using NopStation.Plugin.Misc.Core.Infrastructure;
using NopStation.Plugin.Widgets.AdvanceCart.Factories;

namespace NopStation.Plugin.Widgets.AdvanceCart.Infrastructure;

public class PluginNopStartUp : INopStartup
{
    public int Order => int.MaxValue;

    public void Configure(IApplicationBuilder application)
    {

    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNopStationServices("NopStation.Plugin.Widgets.AdvanceCart");

        services.AddScoped<IAdvanceCartModelFactory, AdvanceCartModelFactory>();
        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new ViewLocationExpander());
        });
    }
}
