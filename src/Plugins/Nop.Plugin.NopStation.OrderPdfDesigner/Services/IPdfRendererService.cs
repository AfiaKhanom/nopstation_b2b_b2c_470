using System.Threading.Tasks;

namespace Nop.Plugin.NopStation.OrderPdfDesigner.Services;

/// <summary>
/// Represents the PDF renderer service interface
/// </summary>
public interface IPdfRendererService
{
    /// <summary>
    /// Converts HTML to PDF
    /// </summary>
    /// <param name="html">HTML content</param>
    /// <param name="paperSize">Paper size (A4, Letter, Legal, etc.)</param>
    /// <param name="marginTop">Top margin in millimeters</param>
    /// <param name="marginBottom">Bottom margin in millimeters</param>
    /// <param name="marginLeft">Left margin in millimeters</param>
    /// <param name="marginRight">Right margin in millimeters</param>
    /// <returns>PDF bytes</returns>
    Task<byte[]> ConvertHtmlToPdfAsync(
        string html, 
        string paperSize = "A4",
        int marginTop = 10, 
        int marginBottom = 10, 
        int marginLeft = 10, 
        int marginRight = 10);
}
