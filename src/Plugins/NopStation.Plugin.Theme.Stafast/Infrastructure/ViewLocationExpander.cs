using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace NopStation.Plugin.Theme.Stafast.Infrastructure
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.AreaName == "Admin")
            {
                viewLocations = new[] {
                    $"/Plugins/NopStation.Plugin.Theme.Stafast/Areas/Admin/Views/Shared/{{0}}.cshtml",
                    $"/Plugins/NopStation.Plugin.Theme.Stafast/Areas/Admin/Views/{{1}}/{{0}}.cshtml"
                }.Concat(viewLocations);
            }
            else
            {
                viewLocations = new[] {
                    $"/Plugins/NopStation.Plugin.Theme.Stafast/Views/Shared/{{0}}.cshtml",
                    $"/Plugins/NopStation.Plugin.Theme.Stafast/Views/{{1}}/{{0}}.cshtml"
                }.Concat(viewLocations);
            }
            return viewLocations;
        }
    }
}
