using Microsoft.AspNetCore.Mvc.Razor;

namespace NopStation.Plugin.B2B.D365BCIntegration.Infrastructure;

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
                $"/Plugins/NopStation.Plugin.B2B.D365BCIntegration/Areas/Admin/Views/Shared/{{0}}.cshtml",
                $"/Plugins/NopStation.Plugin.B2B.D365BCIntegration/Areas/Admin/Views/{{1}}/{{0}}.cshtml"
            }.Concat(viewLocations);

            return viewLocations;
        }

        return viewLocations;
    }
}