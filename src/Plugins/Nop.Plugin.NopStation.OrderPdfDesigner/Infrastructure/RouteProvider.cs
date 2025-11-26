using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.NopStation.OrderPdfDesigner.Infrastructure;

/// <summary>
/// Represents plugin route provider
/// </summary>
public class RouteProvider : IRouteProvider
{
    /// <summary>
    /// Register routes
    /// </summary>
    /// <param name="endpointRouteBuilder">Route builder</param>
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapControllerRoute(
            "OrderPdfDesignerConfigure",
            "Admin/OrderPdfDesignerAdmin/Configure",
            new { controller = "OrderPdfDesignerAdmin", action = "Configure", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            "OrderPdfDesignerPreview",
            "Admin/OrderPdfDesignerAdmin/Preview",
            new { controller = "OrderPdfDesignerAdmin", action = "Preview", area = "Admin" });

        endpointRouteBuilder.MapControllerRoute(
            "OrderPdfDesignerGeneratePdf",
            "Admin/OrderPdfDesignerAdmin/GeneratePdf",
            new { controller = "OrderPdfDesignerAdmin", action = "GeneratePdf", area = "Admin" });
    }

    /// <summary>
    /// Gets a priority of route provider
    /// </summary>
    public int Priority => 0;
}
