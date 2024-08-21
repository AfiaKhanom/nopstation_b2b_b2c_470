using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Shipping;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public class ErpStockSyncService : IErpStockSyncService
{
    #region Fields

    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;
    private readonly IProductService _productService;
    private readonly IShippingService _shippingService;
    private readonly ISyncLogService _erpSyncLogService;
    private readonly IErpProductService _erpProductService;
    private readonly IErpDataClearCacheService _erpDataClearCacheService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginService;
    private readonly IErpSalesOrgService _erpSalesOrgService;

    #endregion

    #region Ctor

    public ErpStockSyncService(IStoreContext storeContext,
        ISettingService settingService,
        IProductService productService,
        IShippingService shippingService,
        ISyncLogService erpSyncLogService,
        IErpProductService erpProductService,
        IErpDataClearCacheService erpDataClearCacheService,
        IErpIntegrationPluginManager erpIntegrationPluginService,
        IErpSalesOrgService erpSalesOrgService)
    {
        _storeContext = storeContext;
        _settingService = settingService;
        _productService = productService;
        _shippingService = shippingService;
        _erpSyncLogService = erpSyncLogService;
        _erpProductService = erpProductService;
        _erpDataClearCacheService = erpDataClearCacheService;
        _erpIntegrationPluginService = erpIntegrationPluginService;
        _erpSalesOrgService = erpSalesOrgService;
    }

    #endregion

    #region Method

    public async virtual Task<bool> IsErpStockSyncSuccessfulAsync()
    {
        var erpIntegrationPlugin = await _erpIntegrationPluginService.LoadActiveERPIntegrationPlugin();

        if (erpIntegrationPlugin is null)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                ErpSyncLavel.Stock,
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
                    ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                    ErpSyncLavel.Stock,
                    $"No Sales org found with Sales org code: {salesOrgCode}. Unable to run {ErpDataSchedulerDefaults.ErpStockSyncTaskName}.");

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
                ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                ErpSyncLavel.Stock,
                "Erp Stock Sync started.");

            foreach (var salesOrg in listOfSalesOrgs)
            {
                var start = "0";
                var isError = false;
                var dateFrom = erpDataSchedulerSettings.SyncFromDate.HasValue ? erpDataSchedulerSettings.SyncFromDate.Value : DateTime.MinValue;
                var lastErpProductStockSynced = new Product();
                var totalSyncedSoFar = 0;
                var syncStartTime = DateTime.UtcNow.AddMinutes(-10);

                while (true)
                {
                    var erpGetRequestModel = new ErpGetRequestModel
                    {
                        Start = start,
                        DateFrom = dateFrom,
                        Location = salesOrg.Code
                    };

                    var response = await erpIntegrationPlugin.GetStocksFromErpAsync(erpGetRequestModel);

                    if (response.ErpResponseModel.IsError)
                    {
                        isError = true;

                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                            ErpSyncLavel.Stock,
                            response.ErpResponseModel?.ErrorShortMessage ?? string.Empty,
                            response.ErpResponseModel?.ErrorFullMessage ?? string.Empty);

                        break;
                    }
                    else if (response.Data is null)
                    {
                        isError = false;
                        break;
                    }

                    start = response.ErpResponseModel.Next;

                    foreach (var erpProduct in response.Data)
                    {
                        var oldErpProduct = await _productService.GetProductBySkuAsync(erpProduct.Sku) ?? new Product();

                        if (oldErpProduct.Id == 0)
                        {
                            continue;
                        }

                        oldErpProduct.StockQuantity = Convert.ToInt32(Math.Min(Math.Max(Math.Round(erpProduct.QuantityOnHand), int.MinValue), int.MaxValue));
                        oldErpProduct.UpdatedOnUtc = DateTime.UtcNow;

                        if (oldErpProduct.StockQuantity == 0)
                        {
                            oldErpProduct.Published = false;
                        }

                        lastErpProductStockSynced = oldErpProduct;
                        totalSyncedSoFar++;

                        await _productService.UpdateProductAsync(oldErpProduct);

                        #region Cache clear for this erp stock

                        await _erpDataClearCacheService.ClearCacheOfEntity(oldErpProduct, oldErpProduct.Id);

                        #endregion
                    }

                    if (lastErpProductStockSynced.Id != 0)
                    {
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                            ErpSyncLavel.Stock,
                            (lastErpProductStockSynced is not null ? $"The last synced Stock of Erp Product : {lastErpProductStockSynced.Sku} in this batch." : string.Empty) + $"Total product stock synced so far: {totalSyncedSoFar}");
                    }
                }

                if (!isError)
                {
                    await _erpProductService.UnpublishAllOldProduct(syncStartTime);

                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                        ErpSyncLavel.Stock,
                        $"Erp Stock sync successful. The products having stock quantity of 0 (zero) are unpublished for Sales Org: {salesOrg.Name}.");
                }
                else
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                        ErpSyncLavel.Stock,
                        $"Erp Stock sync is partially or not successful for Sales Org: {salesOrg.Name}.");
                }

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                    ErpSyncLavel.Stock,
                    (lastErpProductStockSynced is not null ? $"The last synced Erp Product Stock: {lastErpProductStockSynced.Sku}, for Sales Org: {salesOrg.Name}. " : string.Empty) + $"Total synced in this session: {totalSyncedSoFar}");
            }

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                ErpSyncLavel.Stock,
                "Erp Stock Sync ended.");

            return true;
        }
        catch (Exception ex)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                ErpSyncLavel.Stock,
                ex.Message,
                ex.StackTrace);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                ErpSyncLavel.Stock,
                "Erp Stock Sync ended.");

            return false;
        }
    }

    #endregion
}