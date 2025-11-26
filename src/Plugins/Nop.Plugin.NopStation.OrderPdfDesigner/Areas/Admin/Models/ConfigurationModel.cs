using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.NopStation.OrderPdfDesigner.Areas.Admin.Models;

/// <summary>
/// Represents a configuration model
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    public ConfigurationModel()
    {
        AvailableTokens = new Dictionary<string, string>();
        PaperSizes = new List<string> { "A4", "Letter", "Legal", "A5", "A3" };
    }

    [NopResourceDisplayName("Plugins.NopStation.OrderPdfDesigner.Fields.ServerSectionHeader")]
    public string ServerSectionHeader { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.OrderPdfDesigner.Fields.HeaderDetails")]
    public string HeaderDetails { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.OrderPdfDesigner.Fields.AddressSection")]
    public string AddressSection { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.OrderPdfDesigner.Fields.ProductSection")]
    public string ProductSection { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.OrderPdfDesigner.Fields.NoteSection")]
    public string NoteSection { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.OrderPdfDesigner.Fields.OrderSummarySection")]
    public string OrderSummarySection { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.OrderPdfDesigner.Fields.Footer")]
    public string Footer { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.OrderPdfDesigner.Fields.FooterDescription")]
    public string FooterDescription { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.OrderPdfDesigner.Fields.PaperSize")]
    public string PaperSize { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.OrderPdfDesigner.Fields.MarginTop")]
    public int MarginTop { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.OrderPdfDesigner.Fields.MarginBottom")]
    public int MarginBottom { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.OrderPdfDesigner.Fields.MarginLeft")]
    public int MarginLeft { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.OrderPdfDesigner.Fields.MarginRight")]
    public int MarginRight { get; set; }

    [NopResourceDisplayName("Plugins.NopStation.OrderPdfDesigner.Fields.EnableImageInsertion")]
    public bool EnableImageInsertion { get; set; }

    public int ActiveTemplateVersion { get; set; }

    public Dictionary<string, string> AvailableTokens { get; set; }

    public List<string> PaperSizes { get; set; }
}
