using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Infrastructure;

public class ViewLocationExpander : IViewLocationExpander
{
    private const string THEME_KEY = "nop.themename";
    private const string ADMIN_AREA = "Admin";

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        if (context.AreaName == ADMIN_AREA)
        {
            viewLocations = new[] {
                $"/Plugins/NopStation.Plugin.B2B.ERPIntegrationCore/Areas/Admin/Views/Shared/{{0}}.cshtml",
                $"/Plugins/NopStation.Plugin.B2B.ERPIntegrationCore/Areas/Admin/Views/{{1}}/{{0}}.cshtml"
            }.Concat(viewLocations);

            return viewLocations;
        }

        viewLocations = new[] {
            $"/Plugins/NopStation.Plugin.B2B.ERPIntegrationCore/Views/Shared/{{0}}.cshtml",
            $"/Plugins/NopStation.Plugin.B2B.ERPIntegrationCore/Views/{{1}}/{{0}}.cshtml"
        }.Concat(viewLocations);

        if (context.Values.TryGetValue(THEME_KEY, out string theme))
        {
            viewLocations = new[] {
                $"/Plugins/NopStation.Plugin.B2B.ERPIntegrationCore/Themes/{theme}/Views/Shared/{{0}}.cshtml",
                $"/Plugins/NopStation.Plugin.B2B.ERPIntegrationCore/Themes/{theme}/Views/{{1}}/{{0}}.cshtml"
            }.Concat(viewLocations);
        }

        return viewLocations;
    }

    public void PopulateValues(ViewLocationExpanderContext context)
    {
        
    }
}
