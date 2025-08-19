using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.AdvanceCart.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.AdvanceCart.Areas.Admin.Controllers;

public class AdvanceCartController : NopStationAdminController
{
    #region Fields

    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly IWidgetPluginManager _widgetPluginManager;

    #endregion

    #region Ctor

    public AdvanceCartController(IStoreContext storeContext,
        ISettingService settingService,
        INotificationService notificationService,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        IWidgetPluginManager widgetPluginManager)
    {
        _storeContext = storeContext;
        _settingService = settingService;
        _notificationService = notificationService;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _widgetPluginManager = widgetPluginManager;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> ConfigureAsync()
    {
        if (!await _permissionService.AuthorizeAsync(AdvanceCartPermissionProvider.ManageAdvanceCart))
            return AccessDeniedView();

        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var advanceCartSettings = await _settingService.LoadSettingAsync<AdvanceCartSettings>(storeId);

        var model = advanceCartSettings.ToSettingsModel<ConfigurationModel>();

        model.QuickViewPluginActivated = await _widgetPluginManager.IsPluginActiveAsync("NopStation.Plugin.Widgets.QuickView");
        model.ActiveStoreScopeConfiguration = storeId;

        if (storeId > 0)
        {
            model.EnableAdvanceCartPlugin_OverrideForStore = await _settingService.SettingExistsAsync(advanceCartSettings, x => x.EnableAdvanceCartPlugin, storeId);
            model.EnableAdvanceFlyoutCart_OverrideForStore = await _settingService.SettingExistsAsync(advanceCartSettings, x => x.EnableAdvanceFlyoutCart, storeId);
            model.EnableBuyNowButton_OverrideForStore = await _settingService.SettingExistsAsync(advanceCartSettings, x => x.EnableBuyNowButton, storeId);
            model.AllowCustomersToSelectQuantityFromProductBox_OverrideForStore = await _settingService.SettingExistsAsync(advanceCartSettings, x => x.AllowCustomersToSelectQuantityFromProductBox, storeId);

            if (model.QuickViewPluginActivated)
                model.OpenQuickViewIfRedirectToDetailsPageRequired_OverrideForStore = await _settingService.SettingExistsAsync(advanceCartSettings, x => x.OpenQuickViewIfRedirectToDetailsPageRequired, storeId);

            model.EnableAddedToCartNotificationPopup_OverrideForStore = await _settingService.SettingExistsAsync(advanceCartSettings, x => x.EnableAddedToCartNotificationPopup, storeId);
            model.NotificationPopupProductImageSize_OverrideForStore = await _settingService.SettingExistsAsync(advanceCartSettings, x => x.NotificationPopupProductImageSize, storeId);
            model.ProductBoxAdditionalInfoWidgetZone_OverrideForStore = await _settingService.SettingExistsAsync(advanceCartSettings, x => x.ProductBoxAdditionalInfoWidgetZone, storeId);
            model.ProductDetailsAdditionalInfoWidgetZone_OverrideForStore = await _settingService.SettingExistsAsync(advanceCartSettings, x => x.ProductDetailsAdditionalInfoWidgetZone, storeId);
            model.ProductBoxAddToCartButtonSelector_OverrideForStore = await _settingService.SettingExistsAsync(advanceCartSettings, x => x.ProductBoxAddToCartButtonSelector, storeId);
            model.TopCartSelector_OverrideForStore = await _settingService.SettingExistsAsync(advanceCartSettings, x => x.TopCartSelector, storeId);
            model.FlyoutCartSelector_OverrideForStore = await _settingService.SettingExistsAsync(advanceCartSettings, x => x.FlyoutCartSelector, storeId);
            model.ProductBoxSelector_OverrideForStore = await _settingService.SettingExistsAsync(advanceCartSettings, x => x.ProductBoxSelector, storeId);
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ConfigureAsync(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(AdvanceCartPermissionProvider.ManageAdvanceCart))
            return AccessDeniedView();

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var advanceCartSettings = await _settingService.LoadSettingAsync<AdvanceCartSettings>(storeScope);
        advanceCartSettings = model.ToSettings(advanceCartSettings);

        await _settingService.SaveSettingOverridablePerStoreAsync(advanceCartSettings, x => x.EnableAdvanceCartPlugin, model.EnableAdvanceCartPlugin_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(advanceCartSettings, x => x.EnableAdvanceFlyoutCart, model.EnableAdvanceFlyoutCart_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(advanceCartSettings, x => x.EnableBuyNowButton, model.EnableBuyNowButton_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(advanceCartSettings, x => x.AllowCustomersToSelectQuantityFromProductBox, model.AllowCustomersToSelectQuantityFromProductBox_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(advanceCartSettings, x => x.OpenQuickViewIfRedirectToDetailsPageRequired, model.OpenQuickViewIfRedirectToDetailsPageRequired_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(advanceCartSettings, x => x.EnableAddedToCartNotificationPopup, model.EnableAddedToCartNotificationPopup_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(advanceCartSettings, x => x.NotificationPopupProductImageSize, model.NotificationPopupProductImageSize_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(advanceCartSettings, x => x.ProductBoxAdditionalInfoWidgetZone, model.ProductBoxAdditionalInfoWidgetZone_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(advanceCartSettings, x => x.ProductDetailsAdditionalInfoWidgetZone, model.ProductDetailsAdditionalInfoWidgetZone_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(advanceCartSettings, x => x.ProductBoxAddToCartButtonSelector, model.ProductBoxAddToCartButtonSelector_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(advanceCartSettings, x => x.TopCartSelector, model.TopCartSelector_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(advanceCartSettings, x => x.FlyoutCartSelector, model.FlyoutCartSelector_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(advanceCartSettings, x => x.ProductBoxSelector, model.ProductBoxSelector_OverrideForStore, storeScope, false);

        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

        return RedirectToAction("Configure");

    }

    #endregion
}
