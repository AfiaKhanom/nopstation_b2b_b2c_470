using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Plugins;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    /// <summary>
    /// Represents a ErpIntegration plugin manager implementation
    /// </summary>
    public partial class ErpIntegrationPluginManager : PluginManager<IErpIntegrationPlugin>, IErpIntegrationPluginManager
    { 
        #region Fields
        private readonly ICustomerService _customerService;
        private readonly IPluginService _pluginService;
        private readonly ISettingService _settingService;
        private readonly ERPIntegrationCoreSettings _settings;
        private readonly IStoreContext _storeContext;
        #endregion

        #region Ctor

        public ErpIntegrationPluginManager(ICustomerService customerService,
            IPluginService pluginService,
            ISettingService settingService,
            ERPIntegrationCoreSettings settings,
            IStoreContext storeContext) : base(customerService, pluginService)
        {
            _customerService = customerService;
            _pluginService = pluginService;
            _settingService = settingService;
            _settings = settings;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        public virtual bool IsPluginActive(IErpIntegrationPlugin IntegrationMethod)
        {
            if(string.IsNullOrEmpty(_settings.SelectedErpIntegrationPlugin))
                return false;
            return IsPluginActive(IntegrationMethod, new List<string> { _settings.SelectedErpIntegrationPlugin });
        }

        public virtual async Task<IErpIntegrationPlugin> LoadActiveERPIntegrationPlugin(ErpSyncLevel erpSyncLevel)
        {
            var defaultIntegrationPlugin=_settings.SelectedErpIntegrationPlugin;
            var enableUseSinglePlugin = _settings.UseSingleIntegrationPluginForAllSync;
            if (enableUseSinglePlugin)
            {
                if (string.IsNullOrEmpty(defaultIntegrationPlugin))
                    return null;
                return await LoadPluginBySystemNameAsync(defaultIntegrationPlugin);
            }
            var pluginSystemName = erpSyncLevel switch
            {
                ErpSyncLevel.Order => _settings.SelectedErpIntegrationPluginForOrder,
                ErpSyncLevel.Product => _settings.SelectedErpIntegrationPluginForProduct,
                ErpSyncLevel.Account => _settings.SelectedErpIntegrationPluginForAccount,
                ErpSyncLevel.SpecialPrice => _settings.SelectedErpIntegrationPluginForSpecialPrice,
                ErpSyncLevel.Invoice => _settings.SelectedErpIntegrationPluginForInvoice,
                ErpSyncLevel.ShipToAddress => _settings.SelectedErpIntegrationPluginForShiptoAddress,
                ErpSyncLevel.GroupPrice => _settings.SelectedErpIntegrationPluginForGroupPrice,
                ErpSyncLevel.Stock => _settings.SelectedErpIntegrationPluginForStock,
                _ => null
            };
            if (string.IsNullOrEmpty(pluginSystemName) && string.IsNullOrEmpty(defaultIntegrationPlugin))
                return null;
            if (string.IsNullOrEmpty(pluginSystemName))
                pluginSystemName = defaultIntegrationPlugin;
            return  await LoadPluginBySystemNameAsync(pluginSystemName);  
        }

        #endregion
    }
}