using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.B2B.D365BCIntegration.D365BCImplementation;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Services;


namespace NopStation.Plugin.B2B.D365BCIntegration;

public class D365BCIntegrationPlugin : BasePlugin, IAdminMenuPlugin, IErpIntegrationPlugin, IMiscPlugin, INopStationPlugin
{
    #region Fields

    private readonly ISettingService _settingService;
    private readonly IWebHelper _webHelper;
    private readonly ILocalizationService _localizationService;
    private readonly ID365BCIntegrationAccountService _D365BCIntegrationAccountService;
    private readonly ID365BCIntegrationProductService _D365BCIntegrationProductService;
    private readonly ID365BCIntegrationStockService _D365BCIntegrationStockService;
    private readonly ID365BCIntegrationOrderService _D365BCIntegrationOrderService;
    private readonly IStoreContext _storeContext;



    private const string THIRD_PARTY_PLUGINS = "Third party plugins";


    #endregion

    #region Ctor

    public D365BCIntegrationPlugin(
        ISettingService settingService,
        IWebHelper webHelper,
        ILocalizationService localizationService,
        ID365BCIntegrationAccountService D365BCIntegrationAccountService,
        ID365BCIntegrationProductService D365BCIntegrationProductService,
        ID365BCIntegrationStockService D365BCIntegrationStockService,
        ID365BCIntegrationOrderService D365BCIntegrationOrderService,
        IStoreContext storeContext)

    {
        _settingService = settingService;
        _webHelper = webHelper;
        _localizationService = localizationService;
        _D365BCIntegrationAccountService = D365BCIntegrationAccountService;
        _D365BCIntegrationProductService = D365BCIntegrationProductService;
        _D365BCIntegrationStockService = D365BCIntegrationStockService;
        _D365BCIntegrationOrderService = D365BCIntegrationOrderService;
        _storeContext = storeContext;
    }

    #endregion

    #region Utilities

    private async Task InsertLocalStringResourcesAsync()
    {
        // Locales
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.AccessTokenUrl", "Access Token URL");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ClientId", "Client ID");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ClientSecret", "Client Secret");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.BaseApiUrl", "Base API URL");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.CompanyName", "Company Name");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.DefaultCustomerId", "Default Customer ID");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ErpCallTimeOut", "API Call Timeout (seconds)");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.HttpCallMaxRetries", "Max API Call Retries");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.HttpCallRestTimeInMinutes", "Rest Time Between Retries (minutes)");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.CustomerSyncLimit", "Customer Sync Batch Size");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ProductSyncLimit", "Product Sync Batch Size");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ProductSyncLimit.Hint", "Specify the number of products to sync in each batch. Default is 100.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.AccessTokenUrl", "Access Token URL");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.AccessTokenUrl.Hint", "Enter the OAuth2 access token URL for authentication with Dynamics 365.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ClientId", "Client ID");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ClientId.Hint", "Enter the client ID (application ID) from your Azure AD application registration.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ClientSecret", "Client Secret");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ClientSecret.Hint", "Enter the client secret from your Azure AD application registration.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.BaseApiUrl", "Base API URL");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.BaseApiUrl.Hint", "Enter the base URL for Dynamics 365 Business Central API endpoints.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.CompanyName", "Company Id");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.CompanyName.Hint", "Enter your Dynamics 365 Business Central company id.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.DefaultCustomerId", "Default Customer ID");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.DefaultCustomerId.Hint", "Select the default customer account to use for system operations.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ErpCallTimeOut", "API Call Timeout (seconds)");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ErpCallTimeOut.Hint", "Specify the timeout period in seconds for API calls to Dynamics 365.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.HttpCallMaxRetries", "Max API Call Retries");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.HttpCallMaxRetries.Hint", "Specify the maximum number of retry attempts for failed API calls.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.HttpCallRestTimeInMinutes", "Rest Time Between Retries (minutes)");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.HttpCallRestTimeInMinutes.Hint", "Specify the waiting time in minutes between retry attempts.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.CustomerSyncLimit", "Customer Sync Limit");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.CustomerSyncLimit.Hint", "Specify the number of customers to sync in each batch. Default is 100.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ProductSyncLimit", "Product Sync Limit");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ProductSyncLimit.Hint", "Specify the number of products to sync in each batch. Default is 100.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.StockSyncLimit", "Stock Sync Limit");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.StockSyncLimit.Hint", "Specify the number of stock items to sync in each batch. Default is 100.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.OrderSyncLimit", "Order Sync BLimit");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.OrderSyncLimit.Hint", "Specify the number of Order Sync in each batch. Default is 100.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.AccessTokenUrl", "Access Token URL");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.AccessTokenUrl.Hint", "Enter the OAuth2 access token URL for authentication with Dynamics 365.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ClientId", "Client ID");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ClientId.Hint", "Enter the client ID (application ID) from your Azure AD application registration.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ClientSecret", "Client Secret");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ClientSecret.Hint", "Enter the client secret from your Azure AD application registration.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.BaseApiUrl", "Base API URL");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.BaseApiUrl.Hint", "Enter the base URL for Dynamics 365 Business Central API endpoints.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.CompanyName", "Company Id");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.CompanyName.Hint", "Enter your Dynamics 365 Business Central company id.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.DefaultCustomerId", "Default Customer ID");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.DefaultCustomerId.Hint", "Select the default customer account to use for system operations.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ErpCallTimeOut", "API Call Timeout (seconds)");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ErpCallTimeOut.Hint", "Specify the timeout period in seconds for API calls to Dynamics 365.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.HttpCallMaxRetries", "Max API Call Retries");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.HttpCallMaxRetries.Hint", "Specify the maximum number of retry attempts for failed API calls.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.HttpCallRestTimeInMinutes", "Rest Time Between Retries (minutes)");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.HttpCallRestTimeInMinutes.Hint", "Specify the waiting time in minutes between retry attempts.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.CustomerSyncLimit", "Customer Sync Limit");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.CustomerSyncLimit.Hint", "Specify the number of customers to sync in each batch. Default is 100.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ProductSyncLimit", "Product Sync Limit");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ProductSyncLimit.Hint", "Specify the number of products to sync in each batch. Default is 100.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.StockSyncLimit", "Stock Sync Limit");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.StockSyncLimit.Hint", "Specify the number of stock items to sync in each batch. Default is 100.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.OrderSyncLimit", "Order Sync BLimit");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.OrderSyncLimit.Hint", "Specify the number of Order Sync in each batch. Default is 100.");

        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Admin.Configure.Title", "Dynamics 365 Business Central configuration page");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Admin.Configure.Settings", "Dynamics 365 Business Central settings");
        
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.Environment", "Environment");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.Environment.Hint", "Business Central environment. For instance - Sandbox, Production etc");
        
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.TenantId", "Tenant Id (Directory Id)");
        await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.Environment.Hint", "Tenant Id (Directory Id)");
    }

    #endregion

    #region Methods

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/D365BCIntegration/Configure";
    }

    public override async Task InstallAsync()
    {
        // Settings
        var settings = new D365BCIntegrationSettings
        {
            BaseApiUrl = D365BCIntegrationDefaults.DefaultBaseApiUrl,
            CompanyName = D365BCIntegrationDefaults.DefaultCompanyName,
            ErpCallTimeOut = D365BCIntegrationDefaults.DefaultTimeOutPeriod,
            HttpCallMaxRetries = 3,
            HttpCallRestTimeInMinutes = 1
        };

        await _settingService.SaveSettingAsync(settings);

        // Locales
        await InsertLocalStringResourcesAsync();

        await base.InstallAsync();
    }

    public override async Task UpdateAsync(string currentVersion, string targetVersion)
    {
        await base.UninstallAsync();
    }

    public override async Task UninstallAsync()
    {
        // Settings
        await _settingService.DeleteSettingAsync<D365BCIntegrationSettings>();

        // Locales
        await _localizationService.DeleteLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.AccessTokenUrl");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ClientId");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ClientSecret");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.BaseApiUrl");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.CompanyName");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.DefaultCustomerId");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ErpCallTimeOut");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.HttpCallMaxRetries");
        await _localizationService.DeleteLocaleResourceAsync("Plugins.NopStation.D365BCIntegration.Configuration.Fields.HttpCallRestTimeInMinutes");


        await base.UninstallAsync();
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        return [];
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == THIRD_PARTY_PLUGINS);

        if (pluginNode is null)
            return;

        pluginNode.ChildNodes.Add(new()
        {
            SystemName = D365BCIntegrationDefaults.AdminMenuSystemName,
            Title = "Business Central",
            IconClass = "nav-icon fas fa-cube",
            Visible = true,
            ChildNodes = new List<SiteMapNode>() {
                new()
                {
                    SystemName = $"{D365BCIntegrationDefaults.AdminMenuSystemName}.Configure",
                    Title = "Configure",
                    ControllerName = "D365BCIntegration",
                    ActionName = "Configure",
                    IconClass = "nav-icon fas fa-cogs",
                    Visible = true
                }
            }
        });
    }

    public async Task<ErpResponseModel> CreateAccountNoErpAsync(ErpCreateAccountModel erpCreateAccountModel)
    {
        return await _D365BCIntegrationAccountService.CreateAccountNoErpAsync(erpCreateAccountModel);
    }

    public async Task<ErpResponseData<ErpAccountDataModel>> GetAccountFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _D365BCIntegrationAccountService.GetAccountFromErpAsync(erpRequest);
    }

    public async Task<ErpResponseData<IList<ErpAccountDataModel>>> GetAccountsFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _D365BCIntegrationAccountService.GetAccountsFromErpAsync(erpRequest);
    }

    public Task<ErpResponseData<IList<ErpInvoiceDataModel>>> GetInvoiceByAccountNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return Task.FromResult(new ErpResponseData<IList<ErpInvoiceDataModel>>());
    }

    public Task<ErpResponseData<string>> GetInvoicePdfByteCodeByDocumentNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return Task.FromResult(new ErpResponseData<string>());
    }

    public Task<ErpResponseData<IList<ErpShipToAddressDataModel>>> GetShipToAddressByAccountNumberFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return Task.FromResult(new ErpResponseData<IList<ErpShipToAddressDataModel>>());
    }

    public async Task<ErpResponseData<ErpProductDataModel>> GetProductByItemNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _D365BCIntegrationProductService.GetProductByItemNoFromErpAsync(erpRequest);
    }

    public async Task<ErpResponseData<IList<ErpProductDataModel>>> GetProductsFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _D365BCIntegrationProductService.GetProductsFromErpAsync(erpRequest);
    }

    public async Task<ErpResponseData<ErpStockDataModel>> GetStockByItemNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _D365BCIntegrationStockService.GetStockByItemNoFromErpAsync(erpRequest);
    }

    public async Task<ErpResponseData<IList<ErpStockDataModel>>> GetStocksFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _D365BCIntegrationStockService.GetStocksFromErpAsync(erpRequest);
    }

    public Task<ErpResponseData<ErpPriceGroupPricingDataModel>> GetProductGroupPriceFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return Task.FromResult(new ErpResponseData<ErpPriceGroupPricingDataModel>());
    }

    public Task<ErpResponseData<IList<ErpPriceGroupPricingDataModel>>> GetProductGroupPricesFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return Task.FromResult(new ErpResponseData<IList<ErpPriceGroupPricingDataModel>>());
    }

    public Task<ErpResponseData<ErpPriceSpecialPricingDataModel>> GetProductSpecialPriceFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return Task.FromResult(new ErpResponseData<ErpPriceSpecialPricingDataModel>());
    }

    public Task<ErpResponseData<IList<ErpPriceSpecialPricingDataModel>>> GetProductSpecialPricesFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return Task.FromResult(new ErpResponseData<IList<ErpPriceSpecialPricingDataModel>>());
    }

    public Task ProductListLiveStockDataAsync(ErpAccount erpAccount, IList<Product> products, IProductService productService)
    {
        return Task.FromResult(new ErpResponseData<IList<ErpInvoiceDataModel>>());
    }

    public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrderByAccountFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _D365BCIntegrationOrderService.GetOrdersByAccountFromErpAsync(erpRequest);
    }

    public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrderByOrderNumberFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _D365BCIntegrationOrderService.GetOrderByOrderNumberFromErpAsync(erpRequest);
    }

    public Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetQuoteByAccountFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return Task.FromResult(new ErpResponseData<IList<ErpPlaceOrderDataModel>>());
    }

    public Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetQuoteByQuoteNumberFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return Task.FromResult(new ErpResponseData<IList<ErpPlaceOrderDataModel>>());
    }

    public async Task<ErpResponseModel> CreateOrderOnErpAsync(ErpPlaceOrderDataModel erpRequest)
    {
        var result = await _D365BCIntegrationOrderService.CreateOrderAsync(erpRequest);
        return result.ErpResponseModel;
    }

    public Task<ErpResponseModel> GetSalesOrgsFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return Task.FromResult(new ErpResponseModel());
    }

    public Task<ErpResponseModel> GetSalesWarehouseFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return Task.FromResult(new ErpResponseModel());
    }

    public async Task<string> GetSalesOrgCodeFromIntegrationSettings()
    {
        var settings = await _settingService.LoadSettingAsync<D365BCIntegrationSettings>(
            await _storeContext.GetActiveStoreScopeConfigurationAsync());

        return settings.CompanyName;
    }

    public Task<ErpResponseData<string>> GetStatementPdfByteCodeFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    public Task<ErpResponseData<IList<ErpShipToAddressDataModel>>> GetShipToAddressesFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    public Task<ErpResponseData<IList<ErpAccountDataModel>>> GetAllAccountCreditFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    #endregion
}
