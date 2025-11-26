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

namespace Nop.Plugin.NopStation.OrderPdfDesigner.Services;

/// <summary>
/// Custom PDF service that overrides default NopCommerce PDF service
/// to use the Order PDF Designer plugin templates
/// </summary>
public class CustomPdfService : IPdfService
{
    private readonly IPdfService _defaultPdfService;
    private readonly IOrderPdfDesignerService _orderPdfDesignerService;
    private readonly ILogger<CustomPdfService> _logger;

    public CustomPdfService(
        IPdfService defaultPdfService,
        IOrderPdfDesignerService orderPdfDesignerService,
        ILogger<CustomPdfService> logger)
    {
        _defaultPdfService = defaultPdfService;
        _orderPdfDesignerService = orderPdfDesignerService;
        _logger = logger;
    }

    /// <summary>
    /// Write PDF invoice to the specified stream using custom template
    /// </summary>
    public virtual async Task PrintOrderToPdfAsync(Stream stream, Order order, Language language = null, Store store = null, Vendor vendor = null)
    {
        try
        {
            // Use our custom PDF designer to generate the PDF
            var pdfBytes = await _orderPdfDesignerService.RenderTemplateToPdfAsync(order);
            await stream.WriteAsync(pdfBytes, 0, pdfBytes.Length);
        }
        catch (Exception ex)
        {
            // Log the error and fall back to default
            _logger.LogWarning(ex, "Custom PDF generation failed for order {OrderId}. Falling back to default PDF service.", order.Id);
            await _defaultPdfService.PrintOrderToPdfAsync(stream, order, language, store, vendor);
        }
    }

    /// <summary>
    /// Write ZIP archive with invoices to the specified stream
    /// </summary>
    public virtual async Task PrintOrdersToPdfAsync(Stream stream, IList<Order> orders, Language language = null, Vendor vendor = null)
    {
        // For multiple orders, we'll use our custom PDF for each order
        try
        {
            using var zipArchive = new ZipArchive(stream, ZipArchiveMode.Create, true);

            foreach (var order in orders)
            {
                var fileName = $"order_{order.CustomOrderNumber ?? order.Id.ToString()}.pdf";
                var entry = zipArchive.CreateEntry(fileName);

                using var entryStream = entry.Open();
                var pdfBytes = await _orderPdfDesignerService.RenderTemplateToPdfAsync(order);
                await entryStream.WriteAsync(pdfBytes, 0, pdfBytes.Length);
            }
        }
        catch (Exception ex)
        {
            // Log the error and fall back to default
            _logger.LogWarning(ex, "Custom PDF generation failed for bulk export. Falling back to default PDF service.");
            await _defaultPdfService.PrintOrdersToPdfAsync(stream, orders, language, vendor);
        }
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
        try
        {
            // Generate custom PDF
            var pdfBytes = await _orderPdfDesignerService.RenderTemplateToPdfAsync(order);
            
            // Save to temp file
            var fileName = $"order_{order.CustomOrderNumber ?? order.Id.ToString()}_{Guid.NewGuid()}.pdf";
            var filePath = Path.Combine(Path.GetTempPath(), fileName);
            
            await File.WriteAllBytesAsync(filePath, pdfBytes);
            
            return filePath;
        }
        catch (Exception ex)
        {
            // Log the error and fall back to default
            _logger.LogWarning(ex, "Custom PDF save to disk failed for order {OrderId}. Falling back to default PDF service.", order.Id);
            return await _defaultPdfService.SaveOrderPdfToDiskAsync(order, language, vendor);
        }
    }
}
