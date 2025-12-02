using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.NopStation.OrderPdfDesigner.Areas.Admin.Models;

/// <summary>
/// Represents a preview model
/// </summary>
public record PreviewModel : BaseNopModel
{
    [NopResourceDisplayName("Plugins.NopStation.OrderPdfDesigner.Fields.OrderId")]
    public int OrderId { get; set; }

    public string RenderedHtml { get; set; }
}
