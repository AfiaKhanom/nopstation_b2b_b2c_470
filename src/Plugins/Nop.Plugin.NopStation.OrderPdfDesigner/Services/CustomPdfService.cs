using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Services.Common;
using Nop.Services.Configuration;

namespace Nop.Plugin.NopStation.OrderPdfDesigner.Services;

/// <summary>
/// Custom PDF service that overrides default NopCommerce PDF service
/// to use the Order PDF Designer plugin templates
/// </summary>
public class CustomPdfService : IPdfService
{
    private readonly IPdfService _defaultPdfService;
    private readonly IOrderPdfDesignerService _orderPdfDesignerService;
    private readonly ISettingService _settingService;
    private readonly ILogger<CustomPdfService> _logger;

    public CustomPdfService(
        IPdfService defaultPdfService,
        IOrderPdfDesignerService orderPdfDesignerService,
        ISettingService settingService,
        ILogger<CustomPdfService> logger)
    {
        _defaultPdfService = defaultPdfService;
        _orderPdfDesignerService = orderPdfDesignerService;
        _settingService = settingService;
        _logger = logger;
    }

    /// <summary>
    /// Check if plugin is properly configured with template content
    /// </summary>
    private async Task<bool> IsPluginConfiguredAsync()
    {
        try
        {
            var settings = await _settingService.LoadSettingAsync<OrderPdfDesignerSettings>();
            
            // Check if at least one section has content (not just the default placeholder)
            return !string.IsNullOrWhiteSpace(settings.ServerSectionHeader) ||
                   !string.IsNullOrWhiteSpace(settings.HeaderDetails) ||
                   !string.IsNullOrWhiteSpace(settings.AddressSection) ||
                   !string.IsNullOrWhiteSpace(settings.ProductSection) ||
                   !string.IsNullOrWhiteSpace(settings.OrderSummarySection);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Write PDF invoice to the specified stream using custom template
    /// </summary>
    public virtual async Task PrintOrderToPdfAsync(Stream stream, Order order, Language language = null, Store store = null, Vendor vendor = null)
    {
        // Always use the default NopCommerce PDF service for reliable PDF generation
        // The custom template HTML is available for preview, but PDF generation requires
        // a proper HTML-to-PDF library integration (QuestPDF, Puppeteer, wkhtmltopdf, etc.)
        // 
        // To enable custom PDF output:
        // 1. Install an HTML-to-PDF library (e.g., QuestPDF, IronPDF, SelectPdf)
        // 2. Update PdfRendererService.ConvertHtmlToPdfAsync() to use the library
        // 3. Uncomment the code below
        //
        // if (await IsPluginConfiguredAsync())
        // {
        //     try
        //     {
        //         var pdfBytes = await _orderPdfDesignerService.RenderTemplateToPdfAsync(order);
        //         await stream.WriteAsync(pdfBytes, 0, pdfBytes.Length);
        //         return;
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogWarning(ex, "Custom PDF generation failed for order {OrderId}. Falling back to default.", order.Id);
        //     }
        // }
        
        await _defaultPdfService.PrintOrderToPdfAsync(stream, order, language, store, vendor);
    }

    /// <summary>
    /// Write ZIP archive with invoices to the specified stream
    /// </summary>
    public virtual async Task PrintOrdersToPdfAsync(Stream stream, IList<Order> orders, Language language = null, Vendor vendor = null)
    {
        // Use default NopCommerce PDF service for reliable PDF generation
        await _defaultPdfService.PrintOrdersToPdfAsync(stream, orders, language, vendor);
    }

    /// <summary>
    /// Write packaging slip to the specified stream
    /// Packaging slips use default implementation
    /// </summary>
    public virtual async Task PrintPackagingSlipToPdfAsync(Stream stream, Shipment shipment, Language language = null)
    {
        await _defaultPdfService.PrintPackagingSlipToPdfAsync(stream, shipment, language);
    }

    /// <summary>
    /// Write ZIP archive with packaging slips to the specified stream
    /// Packaging slips use default implementation
    /// </summary>
    public virtual async Task PrintPackagingSlipsToPdfAsync(Stream stream, IList<Shipment> shipments, Language language = null)
    {
        await _defaultPdfService.PrintPackagingSlipsToPdfAsync(stream, shipments, language);
    }

    /// <summary>
    /// Write PDF catalog to the specified stream
    /// Product catalogs use default implementation
    /// </summary>
    public virtual async Task PrintProductsToPdfAsync(Stream stream, IList<Product> products)
    {
        await _defaultPdfService.PrintProductsToPdfAsync(stream, products);
    }

    /// <summary>
    /// Export an order to PDF and save to disk using custom template
    /// </summary>
    public virtual async Task<string> SaveOrderPdfToDiskAsync(Order order, Language language = null, Vendor vendor = null)
    {
        // Use default NopCommerce PDF service for reliable PDF generation
        return await _defaultPdfService.SaveOrderPdfToDiskAsync(order, language, vendor);
    }
}
