using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace NopStation.Plugin.Widgets.AjaxCart.Infrastructure;

public class ViewLocationExpander : IViewLocationExpander
{
    private const string THEME_KEY = "nop.themename";

    public void PopulateValues(ViewLocationExpanderContext context)
    {
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        if (context.ControllerName.Equals("AjaxCart", System.StringComparison.InvariantCultureIgnoreCase))
        {
            viewLocations = new string[] {
                $"/Views/ShoppingCart/{{0}}.cshtml",
                $"/Views/Shared/{{0}}.cshtml"
            }.Concat(viewLocations);


            if (context.Values.TryGetValue(THEME_KEY, out string theme))
            {
                viewLocations = new[] {
                    $"/Themes/{theme}/Views/ShoppingCart/{{0}}.cshtml",
                    $"/Themes/{theme}/Views/Shared/{{0}}.cshtml",
                }
                .Concat(viewLocations);
            }
        }

        return viewLocations;
    }
}
