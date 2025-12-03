using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Areas.Admin.Models;
using Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Domain;
using Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Services.Template;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Areas.Admin.Controllers
{
    [Area(AreaNames.ADMIN)]
    [AuthorizeAdmin]
    [AutoValidateAntiforgeryToken]
    public class InvoiceTemplateController : BasePluginController
    {
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ITemplateService _templateService;
        private readonly IStoreService _storeService;
        private readonly ILanguageService _languageService;

        public InvoiceTemplateController(
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ITemplateService templateService,
            IStoreService storeService,
            ILanguageService languageService)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _templateService = templateService;
            _storeService = storeService;
            _languageService = languageService;
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(ConfigureInvoiceAndPackSlipPermissionProvider.ManageInvoiceTemplates))
                return AccessDeniedView();

            var model = new InvoiceTemplateSearchModel();

            return View("~/Plugins/Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip/Areas/Admin/Views/InvoiceTemplate/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(InvoiceTemplateSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(ConfigureInvoiceAndPackSlipPermissionProvider.ManageInvoiceTemplates))
                return await AccessDeniedDataTablesJson();

            var templates = await _templateService.GetAllInvoiceTemplatesAsync();
            
            var pagedTemplates = templates.Skip((searchModel.Page - 1) * searchModel.PageSize)
                .Take(searchModel.PageSize)
                .ToList();

            var modelList = await Task.WhenAll(pagedTemplates.Select(async template =>
            {
                return new InvoiceTemplateModel
                {
                    Id = template.Id,
                    Name = template.Name,
                    StoreId = template.StoreId,
                    LanguageId = template.LanguageId,
                    IsDefault = template.IsDefault,
                    RenderModeId = template.RenderModeId,
                    CreatedOn = template.CreatedOnUtc.ToString("g"),
                    UpdatedOn = template.UpdatedOnUtc.ToString("g")
                };
            }));

            var model = new InvoiceTemplateListModel
            {
                Data = modelList,
                RecordsTotal = templates.Count,
                RecordsFiltered = templates.Count
            };

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(ConfigureInvoiceAndPackSlipPermissionProvider.ManageInvoiceTemplates))
                return AccessDeniedView();

            var model = new InvoiceTemplateModel();
            await PrepareModelAsync(model);

            return View("~/Plugins/Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip/Areas/Admin/Views/InvoiceTemplate/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(InvoiceTemplateModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(ConfigureInvoiceAndPackSlipPermissionProvider.ManageInvoiceTemplates))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var template = new InvoiceTemplate
                {
                    Name = model.Name,
                    StoreId = model.StoreId,
                    LanguageId = model.LanguageId,
                    IsDefault = model.IsDefault,
                    RenderModeId = model.RenderModeId,
                    TemplateHtml = model.TemplateHtml ?? string.Empty,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow
                };

                await _templateService.InsertInvoiceTemplateAsync(template);

                _notificationService.SuccessNotification(
                    await _localizationService.GetResourceAsync("Plugins.Comalytics.ConfigureInvoiceAndPackSlip.InvoiceTemplates.Created"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = template.Id });
            }

            await PrepareModelAsync(model);
            return View("~/Plugins/Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip/Areas/Admin/Views/InvoiceTemplate/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(ConfigureInvoiceAndPackSlipPermissionProvider.ManageInvoiceTemplates))
                return AccessDeniedView();

            var template = await _templateService.GetInvoiceTemplateByIdAsync(id);
            if (template == null)
                return RedirectToAction("List");

            var model = new InvoiceTemplateModel
            {
                Id = template.Id,
                Name = template.Name,
                StoreId = template.StoreId,
                LanguageId = template.LanguageId,
                IsDefault = template.IsDefault,
                RenderModeId = template.RenderModeId,
                TemplateHtml = template.TemplateHtml,
                CreatedOn = template.CreatedOnUtc.ToString("g"),
                UpdatedOn = template.UpdatedOnUtc.ToString("g")
            };

            await PrepareModelAsync(model);

            return View("~/Plugins/Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip/Areas/Admin/Views/InvoiceTemplate/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(InvoiceTemplateModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(ConfigureInvoiceAndPackSlipPermissionProvider.ManageInvoiceTemplates))
                return AccessDeniedView();

            var template = await _templateService.GetInvoiceTemplateByIdAsync(model.Id);
            if (template == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                template.Name = model.Name;
                template.StoreId = model.StoreId;
                template.LanguageId = model.LanguageId;
                template.IsDefault = model.IsDefault;
                template.RenderModeId = model.RenderModeId;
                template.TemplateHtml = model.TemplateHtml ?? string.Empty;
                template.UpdatedOnUtc = DateTime.UtcNow;

                await _templateService.UpdateInvoiceTemplateAsync(template);

                _notificationService.SuccessNotification(
                    await _localizationService.GetResourceAsync("Plugins.Comalytics.ConfigureInvoiceAndPackSlip.InvoiceTemplates.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = template.Id });
            }

            await PrepareModelAsync(model);
            return View("~/Plugins/Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip/Areas/Admin/Views/InvoiceTemplate/Edit.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(ConfigureInvoiceAndPackSlipPermissionProvider.ManageInvoiceTemplates))
                return AccessDeniedView();

            var template = await _templateService.GetInvoiceTemplateByIdAsync(id);
            if (template == null)
                return RedirectToAction("List");

            await _templateService.DeleteInvoiceTemplateAsync(template);

            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("Plugins.Comalytics.ConfigureInvoiceAndPackSlip.InvoiceTemplates.Deleted"));

            return RedirectToAction("List");
        }

        private async Task PrepareModelAsync(InvoiceTemplateModel model)
        {
            // Stores
            var stores = await _storeService.GetAllStoresAsync();
            model.AvailableStores.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = "0" });
            foreach (var store in stores)
            {
                model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString() });
            }

            // Languages
            var languages = await _languageService.GetAllLanguagesAsync();
            model.AvailableLanguages.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = "0" });
            foreach (var language in languages)
            {
                model.AvailableLanguages.Add(new SelectListItem { Text = language.Name, Value = language.Id.ToString() });
            }

            // Render Modes
            model.AvailableRenderModes.Add(new SelectListItem { Text = "QuestPDF", Value = "1" });
            model.AvailableRenderModes.Add(new SelectListItem { Text = "Chromium", Value = "2" });
        }
    }
}
