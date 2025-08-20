using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;
using Nop.Core.Infrastructure;

namespace NopStation.Plugin.Widgets.AdvanceCart.Infrastructure
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
                viewLocations = new string[] {
                    $"/Plugins/NopStation.Plugin.Widgets.AdvanceCart/Areas/Admin/Views/Shared/{{0}}.cshtml",
                    $"/Plugins/NopStation.Plugin.Widgets.AdvanceCart/Areas/Admin/Views/{{1}}/{{0}}.cshtml"
                }.Concat(viewLocations);
            }
            else if (context.Values.TryGetValue(THEME_KEY, out string theme))
            {
                var smartShoppingCartSettings = EngineContext.Current.Resolve<AdvanceCartSettings>();

                if (context.ViewName == "Components/FlyoutShoppingCart/Default" && smartShoppingCartSettings.EnableAdvanceFlyoutCart)
                    viewLocations = new string[] { $"~/Plugins/NopStation.Plugin.Widgets.AdvanceCart/Themes/{theme}/Views/Shared/{{0}}.cshtml" };

                viewLocations = new string[] {
                        $"/Plugins/NopStation.Plugin.Widgets.AdvanceCart/Themes/{theme}/Views/Shared/{{0}}.cshtml",
                        $"/Plugins/NopStation.Plugin.Widgets.AdvanceCart/Themes/{theme}/Views/{{1}}/{{0}}.cshtml"
                    }.Concat(viewLocations);
            }
            return viewLocations;
        }
    }
}
