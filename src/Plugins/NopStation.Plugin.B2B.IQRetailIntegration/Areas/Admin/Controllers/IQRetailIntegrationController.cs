using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.IQRetailIntegration.Areas.Admin.Model;
using NopStation.Plugin.B2B.IQRetailIntegration.ErpInterfaceImplementation;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Areas.Admin.Controllers
{
    public class IQRetailIntegrationController : NopStationAdminController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ICustomerService _customerService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IErpLogsService _erpLogsService;
        private readonly IErpActivityLogsService _erpActivityLogsService;
        private readonly ErpIntegrationAccountService _erpIntegrationAccountService;
        private const string EDIT_SETTINGS_SYSTEM_KEYWORD = "EditSettings";

        #endregion

        #region Ctor

        public IQRetailIntegrationController(
            IWorkContext workContext,
            IStoreContext storeContext,
            ISettingService settingService,
            ICustomerService customerService,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            ICustomerActivityService customerActivityService,
            IErpLogsService erpLogsService,
            IErpActivityLogsService erpActivityLogsService,
            ErpIntegrationAccountService erpIntegrationAccountService)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _settingService = settingService;
            _customerService = customerService;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _customerActivityService = customerActivityService;
            _erpLogsService = erpLogsService;
            _erpActivityLogsService = erpActivityLogsService;
            _erpIntegrationAccountService = erpIntegrationAccountService;
        }

        #endregion

        #region Utilities

        public async Task PrepareAvailableCustomersAsync(ConfigurationModel model)
        {
            var administrativeCustomerRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName);
            var allCustomers = await _customerService.GetAllCustomersAsync(customerRoleIds: new int[] { administrativeCustomerRole.Id });

            if (!allCustomers.Any())
            {
                model.AvailableCustomers.Add(new SelectListItem { Value = "0", Text = "No Customer available", Selected = true });
                return;
            }

            model.DefaultCustomerId = model.DefaultCustomerId > 0 ? model.DefaultCustomerId : allCustomers?[0].Id ?? 0;

            model.AvailableCustomers = allCustomers.Select(customer => new SelectListItem
            {
                Text = $"{customer.FirstName} {customer.LastName}",
                Value = customer.Id.ToString()
            }).ToList();
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var settings = await _settingService.LoadSettingAsync<IQRetailIntegrationSettings>();

            var model = settings is not null ? settings.ToSettingsModel<ConfigurationModel>() : new ConfigurationModel();

            await PrepareAvailableCustomersAsync(model);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                await PrepareAvailableCustomersAsync(model);
                return View(model);
            }

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = model.ToSettings(await _settingService.LoadSettingAsync<IQRetailIntegrationSettings>(storeScope));

            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.BaseUrl, model.BaseUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.UserName, model.UserName_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.Password, model.Password_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.Location, model.Location_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.B2cPriceCode, model.B2cPriceCode_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DefaultLimit, model.DefaultLimit_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ErpCallTimeOut, model.ErpCallTimeOut_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DefaultCustomerId, model.DefaultCustomerId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.CompanyId, model.CompanyId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TerminalNumber, model.TerminalNumber_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.SellPrice1, model.SellPrice1_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.SellPrice2, model.SellPrice2_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.SellPrice3, model.SellPrice3_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.SellPrice4, model.SellPrice4_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.SellPrice5, model.SellPrice5_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.SellPrice6, model.SellPrice6_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.SellPrice7, model.SellPrice7_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.SellPrice8, model.SellPrice8_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.SellPrice9, model.SellPrice9_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.SellPrice10, model.SellPrice10_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.HttpCallMaxRetries, model.HttpCallMaxRetries_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.HttpCallRestTimeInMinutes, model.HttpCallRestTimeInMinutes_OverrideForStore, storeScope, false);
            await _settingService.ClearCacheAsync();

            //activity log
            //await _customerActivityService.InsertActivityAsync(EDIT_SETTINGS_SYSTEM_KEYWORD, await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.IQRetailIntegration.ActivityLog.EditConfigurations"));
            await _erpActivityLogsService.InsertErpActivityAsync(EDIT_SETTINGS_SYSTEM_KEYWORD, await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.IQRetailIntegration.ActivityLog.EditConfigurations"));

            var successMsg = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Updated");
            _notificationService.SuccessNotification(successMsg);

            await _erpLogsService.InformationAsync(successMsg, ErpSyncLavel.Account, customer: await _workContext.GetCurrentCustomerAsync());

            await PrepareAvailableCustomersAsync(model);

            return View(model);
        }
         
    }

    #endregion
}