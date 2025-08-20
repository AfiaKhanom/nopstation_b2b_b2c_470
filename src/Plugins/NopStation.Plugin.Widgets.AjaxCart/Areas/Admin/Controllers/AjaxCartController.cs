using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.AjaxCart.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.AjaxCart.Areas.Admin.Controllers;

public class AjaxCartController : NopStationAdminController
{
    #region Fields

    private readonly IPermissionService _permissionService;
    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public AjaxCartController(IPermissionService permissionService,
        IStoreContext storeContext,
        ISettingService settingService,
        INotificationService notificationService,
        ILocalizationService localizationService)
    {
        _permissionService = permissionService;
        _storeContext = storeContext;
        _settingService = settingService;
        _notificationService = notificationService;
        _localizationService = localizationService;
    }

    #endregion

    public async Task<IActionResult> ConfigureAsync()
    {
        if (!await _permissionService.AuthorizeAsync(AjaxCartPermissionProvider.ManageAjaxCart))
            return AccessDeniedView();

        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var ajaxCartSettings = await _settingService.LoadSettingAsync<AjaxCartSettings>(storeId);

        var model = ajaxCartSettings.ToSettingsModel<ConfigurationModel>();
        model.ActiveStoreScopeConfiguration = storeId;

        if (storeId > 0)
        {
            model.EnableAjaxCartPlugin_OverrideForStore = await _settingService.SettingExistsAsync(ajaxCartSettings, x => x.EnableAjaxCartPlugin, storeId);
            model.ShoppingCartFormSelector_OverrideForStore = await _settingService.SettingExistsAsync(ajaxCartSettings, x => x.ShoppingCartFormSelector, storeId);
            model.ShoppingCartInputQuantitySelector_OverrideForStore = await _settingService.SettingExistsAsync(ajaxCartSettings, x => x.ShoppingCartInputQuantitySelector, storeId);
            model.ShoppingCartSelectQuantitySelector_OverrideForStore = await _settingService.SettingExistsAsync(ajaxCartSettings, x => x.ShoppingCartSelectQuantitySelector, storeId);
            model.ApplyDiscountCouponCodeButtonSelector_OverrideForStore = await _settingService.SettingExistsAsync(ajaxCartSettings, x => x.ApplyDiscountCouponCodeButtonSelector, storeId);
            model.ApplyDiscountCouponCodeInputSelector_OverrideForStore = await _settingService.SettingExistsAsync(ajaxCartSettings, x => x.ApplyDiscountCouponCodeInputSelector, storeId);
            model.ApplyGiftCardCouponCodeButtonSelector_OverrideForStore = await _settingService.SettingExistsAsync(ajaxCartSettings, x => x.ApplyGiftCardCouponCodeButtonSelector, storeId);
            model.ApplyGiftCardCouponCodeInputSelector_OverrideForStore = await _settingService.SettingExistsAsync(ajaxCartSettings, x => x.ApplyGiftCardCouponCodeInputSelector, storeId);
            model.OrderSummaryContainerSelector_OverrideForStore = await _settingService.SettingExistsAsync(ajaxCartSettings, x => x.OrderSummaryContainerSelector, storeId);
            model.UpdateShoppingCartButtonSelector_OverrideForStore = await _settingService.SettingExistsAsync(ajaxCartSettings, x => x.UpdateShoppingCartButtonSelector, storeId);
        }

        return View(model);

    }

    [HttpPost]
    public async Task<IActionResult> ConfigureAsync(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(AjaxCartPermissionProvider.ManageAjaxCart))
            return AccessDeniedView();

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var ajaxCartSettings = await _settingService.LoadSettingAsync<AjaxCartSettings>(storeScope);
        ajaxCartSettings = model.ToSettings(ajaxCartSettings);

        await _settingService.SaveSettingOverridablePerStoreAsync(ajaxCartSettings, x => x.EnableAjaxCartPlugin, model.EnableAjaxCartPlugin_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(ajaxCartSettings, x => x.ShoppingCartFormSelector, model.ShoppingCartFormSelector_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(ajaxCartSettings, x => x.ShoppingCartInputQuantitySelector, model.ShoppingCartInputQuantitySelector_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(ajaxCartSettings, x => x.ShoppingCartSelectQuantitySelector, model.ShoppingCartSelectQuantitySelector_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(ajaxCartSettings, x => x.ApplyDiscountCouponCodeButtonSelector, model.ApplyDiscountCouponCodeButtonSelector_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(ajaxCartSettings, x => x.ApplyDiscountCouponCodeInputSelector, model.ApplyDiscountCouponCodeInputSelector_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(ajaxCartSettings, x => x.ApplyGiftCardCouponCodeButtonSelector, model.ApplyGiftCardCouponCodeButtonSelector_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(ajaxCartSettings, x => x.ApplyGiftCardCouponCodeInputSelector, model.ApplyGiftCardCouponCodeInputSelector_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(ajaxCartSettings, x => x.OrderSummaryContainerSelector, model.OrderSummaryContainerSelector_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(ajaxCartSettings, x => x.UpdateShoppingCartButtonSelector, model.UpdateShoppingCartButtonSelector_OverrideForStore, storeScope, false);
        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

        return RedirectToAction("Configure");
    }
}
