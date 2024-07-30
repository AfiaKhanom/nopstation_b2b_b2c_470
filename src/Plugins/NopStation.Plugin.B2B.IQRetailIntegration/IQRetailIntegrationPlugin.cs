using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.IQRetailIntegration.ErpInterfaceImplementation;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.B2B.IQRetailIntegration
{
    public class IQRetailIntegrationPlugin : BasePlugin, IAdminMenuPlugin, IErpIntegrationPlugin, IMiscPlugin, INopStationPlugin
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly ErpIntegrationOrderService _erpIntegrationOrderService;
        private readonly ErpIntegrationAccountService _erpIntegrationAccountService;
        private readonly ErpIntegrationProductService _erpIntegrationProductService;
        private const string CONFIG_PAGE_URL_EXTENSION = "Admin/IQRetailIntegration/Configure";
        private const string THIRD_PARTY_PLUGINS = "Third party plugins";
        private const string PLUGIN_SYSTEM_NAME = "NopStation.IQRetailIntegration";
        private const string PLUGIN_TITLE = "IQ Retail Integration";
        private const string PLUGIN_ICON_CLASS = "nav-icon fas fa-cube";
        private const bool PLUGIN_VISIBLE = true;
        private const string PLUGIN_VERSION = "1.51";
        private const string CHILD_NODE_CONFIG_SYSTEM_NAME = "NopStation.IQRetailIntegration.Configuration";
        private const string CHILD_NODE_CONFIG_TITLE = "Configuration";
        private const string CHILD_NODE_CONFIG_CONTROLLER_NAME = "IQRetailIntegration";
        private const string CHILD_NODE_CONFIG_ACTION_NAME = "Configure";
        private const string CHILD_NODE_CONFIG_ICON_CLASS = "nav-icon fas fa-cogs";
        private const bool CHILD_NODE_CONFIG_VISIBLE = true;

        #endregion

        #region Ctor

        public IQRetailIntegrationPlugin(
            IWebHelper webHelper,
            IStoreContext storeContext,
            ISettingService settingService,
            ILocalizationService localizationService,
            ErpIntegrationOrderService erpIntegrationOrderService,
            ErpIntegrationAccountService erpIntegrationAccountService,
            ErpIntegrationProductService erpIntegrationProductService)
        {
            _webHelper = webHelper;
            _storeContext = storeContext;
            _settingService = settingService;
            _localizationService = localizationService;
            _erpIntegrationOrderService = erpIntegrationOrderService;
            _erpIntegrationAccountService = erpIntegrationAccountService;
            _erpIntegrationProductService = erpIntegrationProductService;
        }

        #endregion

        #region Methods

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync();

            await base.InstallAsync();
        }
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}{CONFIG_PAGE_URL_EXTENSION}";
        }
        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == THIRD_PARTY_PLUGINS);

            if (pluginNode is null)
            {
                return;
            }

            pluginNode.ChildNodes.Add(new()
            {
                SystemName = PLUGIN_SYSTEM_NAME,
                Title = PLUGIN_TITLE,
                IconClass = PLUGIN_ICON_CLASS,
                Visible = PLUGIN_VISIBLE,
                ChildNodes = new List<SiteMapNode>() {
                    new()
                    {
                        SystemName = CHILD_NODE_CONFIG_SYSTEM_NAME,
                        Title = CHILD_NODE_CONFIG_TITLE,
                        ControllerName = CHILD_NODE_CONFIG_CONTROLLER_NAME,
                        ActionName = CHILD_NODE_CONFIG_ACTION_NAME,
                        IconClass = CHILD_NODE_CONFIG_ICON_CLASS,
                        Visible = CHILD_NODE_CONFIG_VISIBLE
                    }
                }
            });
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync();

            await base.UninstallAsync();
        }

        public override async Task UpdateAsync(string currentVersion, string targetVersion)
        {
            if (targetVersion == currentVersion || targetVersion != PLUGIN_VERSION)
            {
                return;
            }
            var keyValuePairs = PluginResouces().ToDictionary(kv => kv.Key, kv => kv.Value);
            foreach (var keyValuePair in keyValuePairs)
            {
                await _localizationService.AddOrUpdateLocaleResourceAsync(keyValuePair.Key, keyValuePair.Value);
            }

            await base.UpdateAsync(currentVersion, targetVersion);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Admin.Configure.Title", "Configure"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Admin.Configure.Settings", "Configure Settings"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.BaseUrl", "Base Url"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.BaseUrl.Hint", "Enter Base Url"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.UserName", "Username"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.UserName.Hint", "Enter Username"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.Password", "Password"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.Password.Hint", "Enter Password"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.Location", "Location"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.Location.Hint", "Enter Location"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.B2cPriceCode", "B2C Price Code"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.B2cPriceCode.Hint", "Enter B2C Price Code"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.DefaultLimit", "Default Limit"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.DefaultLimit.Hint", "Enter Default Limit"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.ErpCallTimeOut", "Erp Call TimeOut"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.ErpCallTimeOut.Hint", "Enter Erp Call TimeOut"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.DefaultCustomerId", "Default Customer"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.DefaultCustomerId.Hint", "Choose Default Customer"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.CompanyId", "Company ID"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.CompanyId.Hint", "Enter Company ID"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.TerminalNumber", "Terminal Number"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.TerminalNumber.Hint", "Enter Terminal Number"),

                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Admin.Configure.SellPrices", "Configure Sell Prices"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice1Code", "Sell Price1 Code"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice1Code.Hint", "Enter Sell Price1 Code"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice2Code", "Sell Price2 Code"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice2Code.Hint", "Enter Sell Price2 Code"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice3Code", "Sell Price3 Code"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice3Code.Hint", "Enter Sell Price3 Code"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice4Code", "Sell Price4 Code"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice4Code.Hint", "Enter Sell Price4 Code"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice5Code", "Sell Price5 Code"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice5Code.Hint", "Enter Sell Price5 Code"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice6Code", "Sell Price6 Code"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice6Code.Hint", "Enter Sell Price6 Code"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice7Code", "Sell Price7 Code"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice7Code.Hint", "Enter Sell Price7 Code"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice8Code", "Sell Price8 Code"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice8Code.Hint", "Enter Sell Price8 Code"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice9Code", "Sell Price9 Code"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice9Code.Hint", "Enter Sell Price9 Code"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice10Code", "Sell Price10 Code"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice10Code.Hint", "Enter Sell Price10 Code"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.HttpCallRestTimeInMinutes", "HTTP Call Rest Time"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.HttpCallRestTimeInMinutes.Hint", "Enter HTTP Call Rest Time in Minutes"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.HttpCallMaxRetries", "HTTP Call Maximum Retries"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.HttpCallMaxRetries.Hint", "Enter HTTP Call Maximum Retries"),

                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.ActivityLog.EditConfigurations", "Edit IQ Retail Integration plugin configurations"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Updated", "The settings have been updated successfully."),
            };

            return list;
        }

        #endregion

        #region Account Sync Methods

        public async Task<ErpResponseModel> CreateAccountNoErpAsync(ErpCreateAccountModel erpCreateAccountModel)
        {
            return await _erpIntegrationAccountService.CreateAccountNoErpAsync(erpCreateAccountModel);
        }

        public async Task<ErpResponseData<ErpAccountDataModel>> GetAccountFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _erpIntegrationAccountService.GetAccountFromErpAsync(erpRequest);
        }

        public async Task<ErpResponseData<IList<ErpAccountDataModel>>> GetAccountsFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _erpIntegrationAccountService.GetAccountsFromErpAsync(erpRequest);
        }

        public async Task<ErpResponseData<IList<ErpInvoiceDataModel>>> GetInvoiceByAccountNoFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _erpIntegrationAccountService.GetInvoiceByAccountNoFromErpAsync(erpRequest);
        }

        public async Task<ErpResponseData<string>> GetInvoicePdfByteCodeByDocumentNoFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _erpIntegrationAccountService.GetInvoicePdfByteCodeByDocumentNoFromErpAsync(erpRequest);
        }

        public async Task<ErpResponseData<IList<ErpShipToAddressDataModel>>> GetShipToAddressByAccountNumberFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _erpIntegrationAccountService.GetShipToAddressByAccountNumberFromErpAsync(erpRequest);
        }

        #endregion

        #region Product Sync Methods

        public async Task<ErpResponseData<ErpProductDataModel>> GetProductByItemNoFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _erpIntegrationProductService.GetProductByItemNoFromErpAsync(erpRequest);
        }

        public async Task<ErpResponseData<IList<ErpProductDataModel>>> GetProductsFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _erpIntegrationProductService.GetProductsFromErpAsync(erpRequest);
        }

        public async Task<ErpResponseData<ErpProductDataModel>> GetStockByItemNoFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _erpIntegrationProductService.GetStockByItemNoFromErpAsync(erpRequest);
        }

        public async Task<ErpResponseData<IList<ErpProductDataModel>>> GetStocksFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _erpIntegrationProductService.GetStocksFromErpAsync(erpRequest);
        }

        public async Task<ErpResponseData<ErpPriceGroupPricingDataModel>> GetProductGroupPriceFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _erpIntegrationProductService.GetProductGroupPriceFromErpAsync(erpRequest);
        }

        public async Task<ErpResponseData<IList<ErpPriceGroupPricingDataModel>>> GetProductGroupPricesFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _erpIntegrationProductService.GetProductGroupPricesFromErpAsync(erpRequest);
        }

        public async Task<ErpResponseData<ErpPriceSpecialPricingDataModel>> GetProductSpecialPriceFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _erpIntegrationProductService.GetProductSpecialPriceFromErpAsync(erpRequest);
        }

        public async Task<ErpResponseData<IList<ErpPriceSpecialPricingDataModel>>> GetProductSpecialPricesFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _erpIntegrationProductService.GetProductSpecialPricesFromErpAsync(erpRequest);
        }

        #endregion

        #region Order Sync Methods

        public async Task<ErpResponseModel> CreateOrderOnErpAsync(ErpPlaceOrderDataModel erpRequest)
        {
            return await _erpIntegrationOrderService.CreateOrderOnErpAsync(erpRequest);
        }

        public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrderByAccountFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _erpIntegrationOrderService.GetOrderByAccountFromErpAsync(erpRequest);
        }

        public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrderByOrderNumberFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _erpIntegrationOrderService.GetOrderByOrderNumberFromErpAsync(erpRequest);
        }

        public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetQuoteByAccountFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _erpIntegrationOrderService.GetQuoteByAccountFromErpAsync(erpRequest);
        }

        public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetQuoteByQuoteNumberFromErpAsync(ErpGetRequestModel erpRequest)
        {
            return await _erpIntegrationOrderService.GetQuoteByQuoteNumberFromErpAsync(erpRequest);
        }

        #endregion

        #region SalesOrg Sync Methods

        public Task<ErpResponseModel> GetSalesOrgsFromErpAsync(ErpGetRequestModel erpRequest)
        {
            throw new NotImplementedException();
        }

        public Task<ErpResponseModel> GetSalesWarehouseFromErpAsync(ErpGetRequestModel erpRequest)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IQRetailIntegration Settings Methods

        public async Task<string> GetSalesOrgCodeFromIQIntegrationSettings()
        {
            var settings = await _settingService.LoadSettingAsync<IQRetailIntegrationSettings>(await _storeContext.GetActiveStoreScopeConfigurationAsync());
            return settings.CompanyId;
        }

        #endregion
    }
}