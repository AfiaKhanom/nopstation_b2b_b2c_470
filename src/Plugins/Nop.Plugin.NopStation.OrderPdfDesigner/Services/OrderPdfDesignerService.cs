using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;

namespace Nop.Plugin.NopStation.OrderPdfDesigner.Services;

/// <summary>
/// Represents the Order PDF Designer service implementation
/// </summary>
public class OrderPdfDesignerService : IOrderPdfDesignerService
{
    private readonly ISettingService _settingService;
    private readonly IMessageTokenProvider _messageTokenProvider;
    private readonly ITokenizer _tokenizer;
    private readonly ILocalizationService _localizationService;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly IPdfRendererService _pdfRendererService;
    private readonly IEmailAccountService _emailAccountService;

    public OrderPdfDesignerService(
        ISettingService settingService,
        IMessageTokenProvider messageTokenProvider,
        ITokenizer tokenizer,
        ILocalizationService localizationService,
        IWorkContext workContext,
        IStoreContext storeContext,
        IPdfRendererService pdfRendererService,
        IEmailAccountService emailAccountService)
    {
        _settingService = settingService;
        _messageTokenProvider = messageTokenProvider;
        _tokenizer = tokenizer;
        _localizationService = localizationService;
        _workContext = workContext;
        _storeContext = storeContext;
        _pdfRendererService = pdfRendererService;
        _emailAccountService = emailAccountService;
    }

    /// <summary>
    /// Gets the template settings
    /// </summary>
    public virtual async Task<OrderPdfDesignerSettings> GetTemplateSettingsAsync()
    {
        return await _settingService.LoadSettingAsync<OrderPdfDesignerSettings>();
    }

    /// <summary>
    /// Saves the template settings
    /// </summary>
    public virtual async Task SaveTemplateSettingsAsync(OrderPdfDesignerSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        // Increment template version for cache-busting
        settings.ActiveTemplateVersion++;

        await _settingService.SaveSettingAsync(settings);
    }

    /// <summary>
    /// Renders the template to HTML
    /// </summary>
    public virtual async Task<string> RenderTemplateToHtmlAsync(Order order, OrderPdfDesignerSettings settings = null)
    {
        ArgumentNullException.ThrowIfNull(order);

        settings ??= await GetTemplateSettingsAsync();

        // Get tokens for order
        var tokens = new List<Token>();
        var store = await _storeContext.GetCurrentStoreAsync();
        var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
        var emailAccount = (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();

        await _messageTokenProvider.AddOrderTokensAsync(tokens, order, languageId);
        
        if (emailAccount != null)
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

        // Assemble sections in the specified order
        var htmlBuilder = new StringBuilder();
        
        htmlBuilder.AppendLine("<!DOCTYPE html>");
        htmlBuilder.AppendLine("<html>");
        htmlBuilder.AppendLine("<head>");
        htmlBuilder.AppendLine("<meta charset=\"utf-8\" />");
        htmlBuilder.AppendLine("<style>");
        htmlBuilder.AppendLine("body { font-family: Arial, sans-serif; margin: 0; padding: 20px; }");
        htmlBuilder.AppendLine("table { width: 100%; border-collapse: collapse; }");
        htmlBuilder.AppendLine("th, td { padding: 8px; text-align: left; border-bottom: 1px solid #ddd; }");
        htmlBuilder.AppendLine("</style>");
        htmlBuilder.AppendLine("</head>");
        htmlBuilder.AppendLine("<body>");

        // Append sections in order
        if (!string.IsNullOrWhiteSpace(settings.ServerSectionHeader))
            htmlBuilder.AppendLine(await ReplaceTokensAsync(settings.ServerSectionHeader, tokens));

        if (!string.IsNullOrWhiteSpace(settings.HeaderDetails))
            htmlBuilder.AppendLine(await ReplaceTokensAsync(settings.HeaderDetails, tokens));

        if (!string.IsNullOrWhiteSpace(settings.AddressSection))
            htmlBuilder.AppendLine(await ReplaceTokensAsync(settings.AddressSection, tokens));

        if (!string.IsNullOrWhiteSpace(settings.ProductSection))
            htmlBuilder.AppendLine(await ReplaceTokensAsync(settings.ProductSection, tokens));

        if (!string.IsNullOrWhiteSpace(settings.NoteSection))
            htmlBuilder.AppendLine(await ReplaceTokensAsync(settings.NoteSection, tokens));

        if (!string.IsNullOrWhiteSpace(settings.OrderSummarySection))
            htmlBuilder.AppendLine(await ReplaceTokensAsync(settings.OrderSummarySection, tokens));

        if (!string.IsNullOrWhiteSpace(settings.Footer))
            htmlBuilder.AppendLine(await ReplaceTokensAsync(settings.Footer, tokens));

        if (!string.IsNullOrWhiteSpace(settings.FooterDescription))
            htmlBuilder.AppendLine(await ReplaceTokensAsync(settings.FooterDescription, tokens));

        htmlBuilder.AppendLine("</body>");
        htmlBuilder.AppendLine("</html>");

        return htmlBuilder.ToString();
    }

    /// <summary>
    /// Renders the template to PDF
    /// </summary>
    public virtual async Task<byte[]> RenderTemplateToPdfAsync(Order order, OrderPdfDesignerSettings settings = null)
    {
        ArgumentNullException.ThrowIfNull(order);

        settings ??= await GetTemplateSettingsAsync();

        // Render HTML first
        var html = await RenderTemplateToHtmlAsync(order, settings);

        // Convert to PDF using the renderer service
        var pdfBytes = await _pdfRendererService.ConvertHtmlToPdfAsync(
            html,
            settings.PaperSize,
            settings.MarginTop,
            settings.MarginBottom,
            settings.MarginLeft,
            settings.MarginRight);

        return pdfBytes;
    }

    /// <summary>
    /// Gets all available tokens for order PDF templates
    /// </summary>
    public virtual async Task<Dictionary<string, string>> GetAvailableTokensAsync()
    {
        var tokens = new Dictionary<string, string>();

        // Order tokens
        tokens.Add("%Order.OrderNumber%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.Order.OrderNumber"));
        tokens.Add("%Order.OrderId%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.Order.OrderId"));
        tokens.Add("%Order.CustomerFullName%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.Order.CustomerFullName"));
        tokens.Add("%Order.CustomerEmail%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.Order.CustomerEmail"));
        tokens.Add("%Order.OrderDate%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.Order.OrderDate"));
        tokens.Add("%Order.OrderTotal%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.Order.OrderTotal"));
        tokens.Add("%Order.OrderSubTotal%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.Order.OrderSubTotal"));
        tokens.Add("%Order.OrderShipping%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.Order.OrderShipping"));
        tokens.Add("%Order.OrderTax%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.Order.OrderTax"));
        tokens.Add("%Order.OrderDiscount%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.Order.OrderDiscount"));
        tokens.Add("%Order.PaymentMethod%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.Order.PaymentMethod"));
        tokens.Add("%Order.ShippingMethod%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.Order.ShippingMethod"));

        // Billing Address tokens
        tokens.Add("%Order.BillingFirstName%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.BillingAddress.FirstName"));
        tokens.Add("%Order.BillingLastName%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.BillingAddress.LastName"));
        tokens.Add("%Order.BillingAddress1%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.BillingAddress.Address1"));
        tokens.Add("%Order.BillingAddress2%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.BillingAddress.Address2"));
        tokens.Add("%Order.BillingCity%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.BillingAddress.City"));
        tokens.Add("%Order.BillingStateProvince%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.BillingAddress.StateProvince"));
        tokens.Add("%Order.BillingZipPostalCode%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.BillingAddress.ZipPostalCode"));
        tokens.Add("%Order.BillingCountry%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.BillingAddress.Country"));

        // Shipping Address tokens
        tokens.Add("%Order.ShippingFirstName%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.ShippingAddress.FirstName"));
        tokens.Add("%Order.ShippingLastName%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.ShippingAddress.LastName"));
        tokens.Add("%Order.ShippingAddress1%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.ShippingAddress.Address1"));
        tokens.Add("%Order.ShippingAddress2%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.ShippingAddress.Address2"));
        tokens.Add("%Order.ShippingCity%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.ShippingAddress.City"));
        tokens.Add("%Order.ShippingStateProvince%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.ShippingAddress.StateProvince"));
        tokens.Add("%Order.ShippingZipPostalCode%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.ShippingAddress.ZipPostalCode"));
        tokens.Add("%Order.ShippingCountry%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.ShippingAddress.Country"));

        // Store tokens
        tokens.Add("%Store.Name%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.Store.Name"));
        tokens.Add("%Store.URL%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.Store.URL"));
        tokens.Add("%Store.Email%", await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.Tokens.Store.Email"));

        return tokens;
    }

    /// <summary>
    /// Replaces tokens in the template
    /// </summary>
    private async Task<string> ReplaceTokensAsync(string template, IEnumerable<Token> tokens)
    {
        if (string.IsNullOrWhiteSpace(template))
            return string.Empty;

        return await Task.FromResult(_tokenizer.Replace(template, tokens, false));
    }
}
