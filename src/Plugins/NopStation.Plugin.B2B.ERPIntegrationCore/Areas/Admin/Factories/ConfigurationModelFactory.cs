using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Areas.Admin.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Areas.Admin.Factories;

public class ConfigurationModelFactory : IConfigurationModelFactory
{
    #region Fields

    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly IPluginService _pluginService;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public ConfigurationModelFactory(ISettingService settingService, 
        IStoreContext storeContext, 
        IPluginService pluginService, 
        ILocalizationService localizationService)
    {
        _settingService = settingService;
        _storeContext = storeContext;
        _pluginService = pluginService;
        _localizationService = localizationService;
    }

    #endregion

    #region Methods

    public async Task<ConfigurationModel> PrepareConfigurationModelAsync()
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var erpIntegrationCoreSettings = await _settingService.LoadSettingAsync<ERPIntegrationCoreSettings>(storeScope);

        var model = erpIntegrationCoreSettings.ToSettingsModel<ConfigurationModel>();

        //filter visible plugins
        var plugins = (await _pluginService.GetPluginDescriptorsAsync<IPlugin>(
            group: ERPIntegrationCoreDefaults.ERPIntegrationPluginGroupName))
            .Where(p => p.ShowInPluginsList)
            .OrderBy(plugin => plugin.Group).ToList();

        // Prepare erpIntegrationPlugins dropdown options
        model.AvailableErpIntegrationPlugins = plugins
        .Select(plugin => new SelectListItem
        {
            Value = plugin.SystemName,
            Text = plugin.FriendlyName
        }).ToList();

        model.AvailableErpIntegrationPlugins.Insert(0, new SelectListItem
        {
            Value = "",
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Select")
        });

        model.ActiveStoreScopeConfiguration = storeScope;
        if (storeScope == 0)
            return model;

        model.SelectedErpIntegrationPlugin_OverrideForStore = await _settingService.SettingExistsAsync(erpIntegrationCoreSettings, x => x.SelectedErpIntegrationPlugin, storeScope);

        return model;
    }

    #endregion
}