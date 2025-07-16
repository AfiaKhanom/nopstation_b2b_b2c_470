using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Areas.Admin.Factories;
using NopStation.Plugin.B2B.ERPIntegrationCore.Areas.Admin.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Areas.Admin.Controllers;

public class ERPIntegrationCoreController : NopStationAdminController
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly IConfigurationModelFactory _configurationModelFactory;

    #endregion

    #region Ctor

    public ERPIntegrationCoreController(ILocalizationService localizationService,
        INotificationService notificationService,
        ISettingService settingService,
        IStoreContext storeContext,
        IConfigurationModelFactory configurationModelFactory)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _settingService = settingService;
        _storeContext = storeContext;
        _configurationModelFactory = configurationModelFactory;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> Configure()
    {
        var model = await _configurationModelFactory.PrepareConfigurationModelAsync();
        return View("~/Plugins/NopStation.Plugin.B2B.ERPIntegrationCore/Areas/Admin/Views/ERPIntegrationCore/Configure.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var erpIntegrationCoreSettings = await _settingService.LoadSettingAsync<ERPIntegrationCoreSettings>(storeScope);
        erpIntegrationCoreSettings = model.ToSettings(erpIntegrationCoreSettings);

        //save setting
        await _settingService.SaveSettingOverridablePerStoreAsync(erpIntegrationCoreSettings, x => x.SelectedErpIntegrationPlugin, model.SelectedErpIntegrationPlugin_OverrideForStore, storeScope, false);

        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Misc.NopStation.ERPIntegrationCore.Configuration.Updated"));

        return RedirectToAction("Configure");
    }

    #endregion
}