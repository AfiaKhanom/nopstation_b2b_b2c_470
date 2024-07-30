using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Infrastructure
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        private const string THEME_KEY = "nop.themename";

        public void PopulateValues(ViewLocationExpanderContext context)
        {

        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.AreaName == "Admin")
            {
                viewLocations = new[] {
                    $"/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Areas/Admin/Views/Shared/{{0}}.cshtml",
                    $"/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Areas/Admin/Views/{{1}}/{{0}}.cshtml"
                }.Concat(viewLocations);
            }
            else
            {
                viewLocations = new[] {
                    $"/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/Shared/{{0}}.cshtml",
                    $"/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/{{1}}/{{0}}.cshtml",
                    $"/Views/Checkout/{{0}}.cshtml"
                }.Concat(viewLocations);

                if (context.Values.TryGetValue(THEME_KEY, out string theme))
                {
                    viewLocations = new[] {
                        $"/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Themes/{theme}/Views/Shared/{{0}}.cshtml",
                        $"/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Themes/{theme}/Views/{{1}}/{{0}}.cshtml"
                    }.Concat(viewLocations);
                }
            }

            return viewLocations;
        }
    }
}