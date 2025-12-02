using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Areas.Admin.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Areas.Admin.Controllers
{
    [Area(AreaNames.ADMIN)]
    [AuthorizeAdmin]
    [AutoValidateAntiforgeryToken]
    public class ConfigureInvoiceAndPackSlipAdminController : BasePluginController
    {
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;

        public ConfigureInvoiceAndPackSlipAdminController(
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
        }

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var settings = await _settingService.LoadSettingAsync<ConfigureInvoiceAndPackSlipSettings>();

            var model = new ConfigurationModel
            {
                PdfRendererProvider = settings.PdfRendererProvider,
                PageOrientation = settings.PageOrientation,
                PageSize = settings.PageSize,
                TopMargin = settings.TopMargin,
                BottomMargin = settings.BottomMargin,
                LeftMargin = settings.LeftMargin,
                RightMargin = settings.RightMargin,
                FontFamily = settings.FontFamily,
                FontSize = settings.FontSize,
                EnableRtl = settings.EnableRtl,
                MaxPictureWidth = settings.MaxPictureWidth,
                MaxPictureHeight = settings.MaxPictureHeight,
                EmbedFonts = settings.EmbedFonts,
                NumberOfCopies = settings.NumberOfCopies,
                EnableBackgroundProcessing = settings.EnableBackgroundProcessing
            };

            model.AvailableRenderers = new List<SelectListItem>
            {
                new SelectListItem { Text = "QuestPDF", Value = "QuestPdf" },
                new SelectListItem { Text = "Chromium", Value = "Chromium" }
            };

            model.AvailablePageOrientations = new List<SelectListItem>
            {
                new SelectListItem { Text = "Portrait", Value = "Portrait" },
                new SelectListItem { Text = "Landscape", Value = "Landscape" }
            };

            model.AvailablePageSizes = new List<SelectListItem>
            {
                new SelectListItem { Text = "A4", Value = "A4" },
                new SelectListItem { Text = "Letter", Value = "Letter" },
                new SelectListItem { Text = "Legal", Value = "Legal" }
            };

            return View("~/Plugins/Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip/Areas/Admin/Views/Configure/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            var settings = await _settingService.LoadSettingAsync<ConfigureInvoiceAndPackSlipSettings>();

            settings.PdfRendererProvider = model.PdfRendererProvider;
            settings.PageOrientation = model.PageOrientation;
            settings.PageSize = model.PageSize;
            settings.TopMargin = model.TopMargin;
            settings.BottomMargin = model.BottomMargin;
            settings.LeftMargin = model.LeftMargin;
            settings.RightMargin = model.RightMargin;
            settings.FontFamily = model.FontFamily;
            settings.FontSize = model.FontSize;
            settings.EnableRtl = model.EnableRtl;
            settings.MaxPictureWidth = model.MaxPictureWidth;
            settings.MaxPictureHeight = model.MaxPictureHeight;
            settings.EmbedFonts = model.EmbedFonts;
            settings.NumberOfCopies = model.NumberOfCopies;
            settings.EnableBackgroundProcessing = model.EnableBackgroundProcessing;

            await _settingService.SaveSettingAsync(settings);
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("Plugins.Comalytics.ConfigureInvoiceAndPackSlip.Configure.SaveSuccess"));

            return await Configure();
        }
    }
}
