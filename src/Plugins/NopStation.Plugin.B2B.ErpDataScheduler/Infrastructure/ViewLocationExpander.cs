using Microsoft.AspNetCore.Mvc.Razor;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Infrastructure;

public class ViewLocationExpander : IViewLocationExpander
{
    private const string THEME_KEY = "nop.themename";
    private const string ADMIN_AREA = "Admin";

    public void PopulateValues(ViewLocationExpanderContext context)
    {

    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        if (context.AreaName == ADMIN_AREA)
        {
            viewLocations = new[] {
                $"/Plugins/NopStation.Plugin.B2B.ErpDataScheduler/Areas/Admin/Views/Shared/{{0}}.cshtml",
                $"/Plugins/NopStation.Plugin.B2B.ErpDataScheduler/Areas/Admin/Views/{{1}}/{{0}}.cshtml"
            }.Concat(viewLocations);

            return viewLocations;
        }

        viewLocations = new[] {
            $"/Plugins/NopStation.Plugin.B2B.ErpDataScheduler/Views/Shared/{{0}}.cshtml",
            $"/Plugins/NopStation.Plugin.B2B.ErpDataScheduler/Views/{{1}}/{{0}}.cshtml"
        }.Concat(viewLocations);

        if (context.Values.TryGetValue(THEME_KEY, out string theme))
        {
            viewLocations = new[] {
                $"/Plugins/NopStation.Plugin.B2B.ErpDataScheduler/Themes/{theme}/Views/Shared/{{0}}.cshtml",
                $"/Plugins/NopStation.Plugin.B2B.ErpDataScheduler/Themes/{theme}/Views/{{1}}/{{0}}.cshtml"
            }.Concat(viewLocations);
        }            

        return viewLocations;
    }
}