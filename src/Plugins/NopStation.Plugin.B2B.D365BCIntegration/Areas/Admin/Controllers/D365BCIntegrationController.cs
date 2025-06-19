using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.D365BCIntegration.Areas.Admin.Models;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.B2B.D365BCIntegration.Areas.Admin.Controllers;

public class D365BCIntegrationController : NopStationAdminController
{
    #region Fields

    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;
    private readonly ICustomerService _customerService;
    private readonly IPermissionService _permissionService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IErpLogsService _erpLogsService;
    private readonly IErpActivityLogsService _erpActivityLogsService;

    private const string EDIT_SETTINGS_SYSTEM_KEYWORD = "EditSettings";

    #endregion

    #region Ctor

    public D365BCIntegrationController(
        IWorkContext workContext,
        IStoreContext storeContext,
        ISettingService settingService,
        ICustomerService customerService,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IErpLogsService erpLogsService,
        IErpActivityLogsService erpActivityLogsService)
    {
        _workContext = workContext;
        _storeContext = storeContext;
        _settingService = settingService;
        _customerService = customerService;
        _permissionService = permissionService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _erpLogsService = erpLogsService;
        _erpActivityLogsService = erpActivityLogsService;
    }

    #endregion

    #region Utilities

    private async Task PrepareAvailableCustomersAsync(ConfigurationModel model)
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

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<D365BCIntegrationSettings>(storeScope);
        var model = settings.ToSettingsModel<ConfigurationModel>();

        model.ActiveStoreScopeConfiguration = storeScope;
        await PrepareAvailableCustomersAsync(model);

        if (storeScope > 0)
        {
            model.ClientId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ClientId, storeScope);
            model.ClientSecret_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ClientSecret, storeScope);
            model.BaseApiUrl_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.BaseApiUrl, storeScope);
            model.CompanyName_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.CompanyName, storeScope);
            model.DefaultCustomerId_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.DefaultCustomerId, storeScope);
            model.ErpCallTimeOut_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ErpCallTimeOut, storeScope);
            model.HttpCallMaxRetries_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.HttpCallMaxRetries, storeScope);
            model.HttpCallRestTimeInMinutes_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.HttpCallRestTimeInMinutes, storeScope);
            model.ProductSyncLimit_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ProductSyncLimit, storeScope);
            model.StockSyncLimit_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.StockSyncLimit, storeScope);
            model.OrderSyncLimit_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.OrderSyncLimit, storeScope);
            model.OrderSyncLimit_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.Environment, storeScope);
            model.OrderSyncLimit_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.TenantId, storeScope);
        }

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

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = model.ToSettings(_settingService.LoadSettingAsync<D365BCIntegrationSettings>(storeScope).Result);

        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ClientId, model.ClientId_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ClientSecret, model.ClientSecret_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.BaseApiUrl, model.BaseApiUrl_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.CompanyName, model.CompanyName_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DefaultCustomerId, model.DefaultCustomerId_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ErpCallTimeOut, model.ErpCallTimeOut_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.HttpCallMaxRetries, model.HttpCallMaxRetries_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.HttpCallRestTimeInMinutes, model.HttpCallRestTimeInMinutes_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.CustomerSyncLimit, model.CustomerSyncLimit_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ProductSyncLimit, model.ProductSyncLimit_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.StockSyncLimit, model.StockSyncLimit_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.OrderSyncLimit, model.OrderSyncLimit_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.Environment, model.Environment_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.TenantId, model.TenantId_OverrideForStore, storeScope, false);

        await _settingService.ClearCacheAsync();

        await _erpActivityLogsService.InsertErpActivityAsync(EDIT_SETTINGS_SYSTEM_KEYWORD, await _localizationService.GetResourceAsync("Plugins.NopStation.D365BCIntegration.ActivityLog.EditConfigurations"));

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return RedirectToAction("Configure");
    }

    #endregion
}