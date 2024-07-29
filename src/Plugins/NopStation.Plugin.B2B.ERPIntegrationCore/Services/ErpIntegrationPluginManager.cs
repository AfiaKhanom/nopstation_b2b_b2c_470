using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Plugins;

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

        #endregion

        #region Ctor

        public ErpIntegrationPluginManager(ICustomerService customerService,
            IPluginService pluginService,
            ISettingService settingService,
            ERPIntegrationCoreSettings settings) : base(customerService, pluginService)
        {
            _customerService = customerService;
            _pluginService = pluginService;
            _settingService = settingService;
            _settings = settings;
        }

        #endregion

        #region Methods

        public virtual bool IsPluginActive(IErpIntegrationPlugin IntegrationMethod)
        {
            if(string.IsNullOrEmpty(_settings.SelectedErpIntegrationPlugin))
                return false;
            return IsPluginActive(IntegrationMethod, new List<string> { _settings.SelectedErpIntegrationPlugin });
        }

        public virtual async Task<IErpIntegrationPlugin> LoadActiveERPIntegrationPlugin()
        {
            if (string.IsNullOrEmpty(_settings.SelectedErpIntegrationPlugin))
                return null;

          return  await LoadPluginBySystemNameAsync(_settings.SelectedErpIntegrationPlugin);  
        }

        #endregion
    }
}