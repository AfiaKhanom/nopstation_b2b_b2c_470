using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Plugin.NopStation.OrderPdfDesigner.Areas.Admin.Models;
using Nop.Plugin.NopStation.OrderPdfDesigner.Services;

namespace Nop.Plugin.NopStation.OrderPdfDesigner.Areas.Admin.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class OrderPdfDesignerAdminController : BasePluginController
{
    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IOrderService _orderService;
    private readonly IOrderPdfDesignerService _orderPdfDesignerService;
    private readonly IPdfService _pdfService;

    public OrderPdfDesignerAdminController(
        ISettingService settingService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IOrderService orderService,
        IOrderPdfDesignerService orderPdfDesignerService,
        IPdfService pdfService)
    {
        _settingService = settingService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _orderService = orderService;
        _orderPdfDesignerService = orderPdfDesignerService;
        _pdfService = pdfService;
    }

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(OrderPdfDesignerPermissionProvider.ManageOrderPdfDesigner))
            return AccessDeniedView();

        var settings = await _settingService.LoadSettingAsync<OrderPdfDesignerSettings>();
        var model = new ConfigurationModel
        {
            ServerSectionHeader = settings.ServerSectionHeader,
            HeaderDetails = settings.HeaderDetails,
            AddressSection = settings.AddressSection,
            ProductSection = settings.ProductSection,
            NoteSection = settings.NoteSection,
            OrderSummarySection = settings.OrderSummarySection,
            Footer = settings.Footer,
            FooterDescription = settings.FooterDescription,
            PaperSize = settings.PaperSize,
            MarginTop = settings.MarginTop,
            MarginBottom = settings.MarginBottom,
            MarginLeft = settings.MarginLeft,
            MarginRight = settings.MarginRight,
            EnableImageInsertion = settings.EnableImageInsertion,
            ActiveTemplateVersion = settings.ActiveTemplateVersion,
            AvailableTokens = await _orderPdfDesignerService.GetAvailableTokensAsync()
        };

        return View("~/Plugins/Nop.Plugin.NopStation.OrderPdfDesigner/Areas/Admin/Views/OrderPdfDesignerAdmin/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(OrderPdfDesignerPermissionProvider.ManageOrderPdfDesigner))
            return AccessDeniedView();

        if (!ModelState.IsValid)
            return await Configure();

        var settings = await _settingService.LoadSettingAsync<OrderPdfDesignerSettings>();
        settings.ServerSectionHeader = model.ServerSectionHeader ?? string.Empty;
        settings.HeaderDetails = model.HeaderDetails ?? string.Empty;
        settings.AddressSection = model.AddressSection ?? string.Empty;
        settings.ProductSection = model.ProductSection ?? string.Empty;
        settings.NoteSection = model.NoteSection ?? string.Empty;
        settings.OrderSummarySection = model.OrderSummarySection ?? string.Empty;
        settings.Footer = model.Footer ?? string.Empty;
        settings.FooterDescription = model.FooterDescription ?? string.Empty;
        settings.PaperSize = model.PaperSize ?? "A4";
        settings.MarginTop = model.MarginTop;
        settings.MarginBottom = model.MarginBottom;
        settings.MarginLeft = model.MarginLeft;
        settings.MarginRight = model.MarginRight;
        settings.EnableImageInsertion = model.EnableImageInsertion;

        await _orderPdfDesignerService.SaveTemplateSettingsAsync(settings);

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    [HttpPost]
    public async Task<IActionResult> Preview(int orderId)
    {
        if (!await _permissionService.AuthorizeAsync(OrderPdfDesignerPermissionProvider.ManageOrderPdfDesigner))
            return Json(new { success = false, message = "Access denied" });

        try
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
                return Json(new { success = false, message = "Order not found" });

            var html = await _orderPdfDesignerService.RenderTemplateToHtmlAsync(order);

            return Json(new { success = true, html });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> GeneratePdf(int orderId)
    {
        if (!await _permissionService.AuthorizeAsync(OrderPdfDesignerPermissionProvider.ManageOrderPdfDesigner))
            return AccessDeniedView();

        try
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null)
            {
                _notificationService.ErrorNotification(
                    await _localizationService.GetResourceAsync("Plugins.NopStation.OrderPdfDesigner.OrderNotFound"));
                return RedirectToAction("Configure");
            }

            // Use the default NopCommerce PDF service to generate a valid PDF
            // The custom template HTML is available for preview, but PDF generation
            // requires a proper HTML-to-PDF library integration
            using var stream = new MemoryStream();
            await _pdfService.PrintOrderToPdfAsync(stream, order);
            var pdfBytes = stream.ToArray();
            
            var fileName = $"Order_{order.CustomOrderNumber ?? order.Id.ToString()}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _notificationService.ErrorNotification(ex.Message);
            return RedirectToAction("Configure");
        }
    }
}
