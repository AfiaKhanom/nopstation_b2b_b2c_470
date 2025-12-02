using Nop.Core.Configuration;

namespace Nop.Plugin.NopStation.OrderPdfDesigner;

/// <summary>
/// Represents Order PDF Designer settings
/// </summary>
public class OrderPdfDesignerSettings : ISettings
{
    /// <summary>
    /// Gets or sets the server section header HTML
    /// </summary>
    public string ServerSectionHeader { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the header details HTML
    /// </summary>
    public string HeaderDetails { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the address section HTML
    /// </summary>
    public string AddressSection { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the product section HTML
    /// </summary>
    public string ProductSection { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the note section HTML
    /// </summary>
    public string NoteSection { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the order summary section HTML
    /// </summary>
    public string OrderSummarySection { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the footer HTML
    /// </summary>
    public string Footer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the footer description HTML
    /// </summary>
    public string FooterDescription { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the active template version for cache-busting
    /// </summary>
    public int ActiveTemplateVersion { get; set; } = 1;

    /// <summary>
    /// Gets or sets the paper size (A4, Letter, Legal, etc.)
    /// </summary>
    public string PaperSize { get; set; } = "A4";

    /// <summary>
    /// Gets or sets the top margin in millimeters
    /// </summary>
    public int MarginTop { get; set; } = 10;

    /// <summary>
    /// Gets or sets the bottom margin in millimeters
    /// </summary>
    public int MarginBottom { get; set; } = 10;

    /// <summary>
    /// Gets or sets the left margin in millimeters
    /// </summary>
    public int MarginLeft { get; set; } = 10;

    /// <summary>
    /// Gets or sets the right margin in millimeters
    /// </summary>
    public int MarginRight { get; set; } = 10;

    /// <summary>
    /// Gets or sets whether to enable image insertion support
    /// </summary>
    public bool EnableImageInsertion { get; set; } = false;
}
