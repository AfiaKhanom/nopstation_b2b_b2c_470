using System.Threading.Tasks;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.NopStation.OrderPdfDesigner.Services;

/// <summary>
/// Represents the Order PDF Designer service interface
/// </summary>
public interface IOrderPdfDesignerService
{
    /// <summary>
    /// Gets the template settings
    /// </summary>
    /// <returns>Order PDF Designer settings</returns>
    Task<OrderPdfDesignerSettings> GetTemplateSettingsAsync();

    /// <summary>
    /// Saves the template settings
    /// </summary>
    /// <param name="settings">Settings to save</param>
    Task SaveTemplateSettingsAsync(OrderPdfDesignerSettings settings);

    /// <summary>
    /// Renders the template to HTML
    /// </summary>
    /// <param name="order">Order to render</param>
    /// <param name="settings">Optional settings (uses current if null)</param>
    /// <returns>Rendered HTML</returns>
    Task<string> RenderTemplateToHtmlAsync(Order order, OrderPdfDesignerSettings settings = null);

    /// <summary>
    /// Renders the template to PDF
    /// </summary>
    /// <param name="order">Order to render</param>
    /// <param name="settings">Optional settings (uses current if null)</param>
    /// <returns>PDF bytes</returns>
    Task<byte[]> RenderTemplateToPdfAsync(Order order, OrderPdfDesignerSettings settings = null);

    /// <summary>
    /// Gets all available tokens for order PDF templates
    /// </summary>
    /// <returns>Dictionary of token names and descriptions</returns>
    Task<System.Collections.Generic.Dictionary<string, string>> GetAvailableTokensAsync();
}
