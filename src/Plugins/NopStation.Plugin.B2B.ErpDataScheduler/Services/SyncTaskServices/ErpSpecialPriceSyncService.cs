using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public class ErpSpecialPriceSyncService : IErpSpecialPriceSyncService
{
    #region Fields

    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;
    private readonly IProductService _productService;
    private readonly ISyncLogService _erpSyncLogService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpSpecialPriceService _erpSpecialPriceService;
    private readonly IErpDataClearCacheService _erpDataClearCacheService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginService;

    #endregion

    #region Ctor

    public ErpSpecialPriceSyncService(IStoreContext storeContext,
        ISettingService settingService,
        IProductService productService,
        ISyncLogService erpSyncLogService,
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService,
        IErpSpecialPriceService erpSpecialPriceService,
        IErpDataClearCacheService erpDataClearCacheService,
        IErpIntegrationPluginManager erpIntegrationPluginService)
    {
        _storeContext = storeContext;
        _settingService = settingService;
        _productService = productService;
        _erpSyncLogService = erpSyncLogService;
        _erpAccountService = erpAccountService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpSpecialPriceService = erpSpecialPriceService;
        _erpDataClearCacheService = erpDataClearCacheService;
        _erpIntegrationPluginService = erpIntegrationPluginService;
    }

    #endregion

    #region Method

    public async virtual Task<bool> IsErpSpecialPriceSyncSuccessfulAsync()
    {
        var erpIntegrationPlugin = await _erpIntegrationPluginService.LoadActiveERPIntegrationPlugin();

        if (erpIntegrationPlugin is null)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                ErpSyncLavel.SpecialPrice,
                "No integration method found.");

            return false;
        }

        try
        {
            #region Data collection

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var erpDataSchedulerSettings = await _settingService.LoadSettingAsync<ErpDataSchedulerSettings>(storeScope);
            var listOfSalesOrgs = new List<ErpSalesOrg>();
            var salesOrgCode = await erpIntegrationPlugin.GetSalesOrgCodeFromIntegrationSettings();

            if (!string.IsNullOrWhiteSpace(salesOrgCode))
            {
                var salesOrg = (await _erpSalesOrgService.GetAllErpSalesOrgAsync(code: salesOrgCode)).FirstOrDefault();

                if (salesOrg == null)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                    ErpSyncLavel.SpecialPrice,
                    $"No Sales org found with Sales org code: {salesOrgCode}. Unable to run {ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName}.");

                    return false;
                }
                else
                {
                    listOfSalesOrgs.Add(salesOrg);
                }
            }
            else
            {
                var salesOrgs = await _erpSalesOrgService.GetAllErpSalesOrgsAsync();

                if (salesOrgs.Any())
                {
                    listOfSalesOrgs.AddRange(salesOrgs);
                }
            }

            #endregion

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                ErpSyncLavel.SpecialPrice,
                "Erp Special Price Sync started.");

            foreach (var salesOrg in listOfSalesOrgs)
            {
                var oldErpAccounts = (List<ErpAccount>)await _erpAccountService.GetAllErpAccountsAsync(salesOrgId: salesOrg.Id);
                if (!oldErpAccounts.Any())
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                        ErpSyncLavel.SpecialPrice,
                        $"No Erp Accounts found with the Sales org : {salesOrg.Name}");

                    return false;
                }

                var lastErpSpecialPriceSynced = new ErpSpecialPrice();
                var lastErpSpecialPriceSyncedOfErpAccount = "";
                var lastErpSpecialPriceSyncedofProduct = "";
                var totalSyncedSoFar = 0;
                var isError = false;
                var lastErrorMessage = "";

                foreach (var erpAccount in oldErpAccounts)
                {
                    var start = "0";
                    var dateFrom = erpDataSchedulerSettings.SyncFromDate.HasValue ? erpDataSchedulerSettings.SyncFromDate.Value : DateTime.MinValue;

                    while (true)
                    {
                        var erpGetRequestModel = new ErpGetRequestModel
                        {
                            Start = start,
                            Location = salesOrg.Code,
                            DateFrom = erpAccount.LastPriceRefresh ?? dateFrom
                        };

                        var response = await erpIntegrationPlugin.GetProductSpecialPricesFromErpAsync(erpGetRequestModel);

                        if (response.ErpResponseModel.IsError)
                        {
                            isError = true;
                            lastErrorMessage = $"The last error: {response.ErpResponseModel.ErrorShortMessage}";
                            break;
                        }
                        else if (response.Data is null)
                        {
                            isError = false;
                            break;
                        }

                        start = response.ErpResponseModel.Next;

                        foreach (var erpSpecialPrice in response.Data)
                        {
                            var product = await _productService.GetProductBySkuAsync(erpSpecialPrice.Sku);
                            if (product is null)
                            {
                                break;
                            }
                            var oldSpecialPrice = await _erpSpecialPriceService.GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(erpAccount.Id, product.Id);

                            if (oldSpecialPrice.Id <= 0)
                            {
                                oldSpecialPrice.ErpAccountId = erpAccount.Id;
                                oldSpecialPrice.NopProductId = product.Id;
                                oldSpecialPrice.Price = erpSpecialPrice.SpecialPrice;
                                oldSpecialPrice.ListPrice = erpSpecialPrice.ListPrice;
                                oldSpecialPrice.PercentageOfAllocatedStock = 0;
                                oldSpecialPrice.PercentageOfAllocatedStockResetTimeUtc = DateTime.MinValue;
                                oldSpecialPrice.VolumeDiscount = true;
                                oldSpecialPrice.DiscountPerc = erpSpecialPrice.DiscountPercentage;
                                oldSpecialPrice.PricingNote = erpSpecialPrice.PricingNotes;
                                await _erpSpecialPriceService.InsertErpSpecialPriceAsync(oldSpecialPrice);
                            }
                            else
                            {
                                oldSpecialPrice.Price = erpSpecialPrice.SpecialPrice;
                                oldSpecialPrice.ListPrice = erpSpecialPrice.ListPrice ;
                                oldSpecialPrice.DiscountPerc = erpSpecialPrice.DiscountPercentage;
                                oldSpecialPrice.PricingNote = erpSpecialPrice.PricingNotes;
                                await _erpSpecialPriceService.UpdateErpSpecialPriceAsync(oldSpecialPrice);
                            }

                            lastErpSpecialPriceSynced = oldSpecialPrice;
                            lastErpSpecialPriceSyncedOfErpAccount = erpAccount.AccountNumber;
                            lastErpSpecialPriceSyncedofProduct = product.Sku;
                            totalSyncedSoFar++;

                            #region Cache clear for this erp special price

                            await _erpDataClearCacheService.ClearCacheOfEntity(oldSpecialPrice, oldSpecialPrice.Id);

                            #endregion
                        }
                    }
                    erpAccount.LastPriceRefresh = DateTime.UtcNow;

                    await _erpAccountService.UpdateErpAccountAsync(erpAccount);

                    #region Cache clear for this erp account

                    await _erpDataClearCacheService.ClearCacheOfEntity(erpAccount, erpAccount.Id);

                    #endregion
                }

                if (!isError)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                        ErpSyncLavel.SpecialPrice,
                        $"Erp Special Price sync successful for Sales Org: {salesOrg.Name}");
                }
                else
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                        ErpSyncLavel.SpecialPrice,
                        $"Erp Special Price sync is partially or not successful for Sales Org: {salesOrg.Name}",
                        lastErrorMessage);
                }

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                    ErpSyncLavel.SpecialPrice,
                    (lastErpSpecialPriceSynced is not null ? $"The last synced Erp Special Price: {lastErpSpecialPriceSynced.Price}, on Product: {lastErpSpecialPriceSyncedofProduct}, of Erp Account: {lastErpSpecialPriceSyncedOfErpAccount} for Sales Org: {salesOrg.Name}. " : string.Empty) + $"Total synced in this session: {totalSyncedSoFar}");

            }

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                ErpSyncLavel.SpecialPrice,
                "Erp Special Price Sync ended.");

            return true;
        }
        catch (Exception ex)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                ErpSyncLavel.SpecialPrice,
                ex.Message,
                ex.StackTrace);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                ErpSyncLavel.SpecialPrice,
                "Erp Special Price Sync ended.");

            return false;
        }
    }

    #endregion
}