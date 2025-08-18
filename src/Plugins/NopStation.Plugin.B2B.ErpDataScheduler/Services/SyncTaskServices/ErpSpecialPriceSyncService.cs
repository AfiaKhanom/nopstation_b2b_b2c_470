using FluentValidation;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncWorkflowMessage;
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.ERPIntegrationCore.Validators.Helpers;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public class ErpSpecialPriceSyncService : IErpSpecialPriceSyncService
{
    #region Fields

    private readonly IErpProductService _erpProductService;
    private readonly ISyncLogService _erpSyncLogService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpSpecialPriceService _erpSpecialPriceService;
    private readonly IErpDataClearCacheService _erpDataClearCacheService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly ISyncWorkflowMessageService _syncWorkflowMessageService;
    private readonly IValidator<ErpSpecialPrice> _erpSpecialPriceValidator;

    #endregion

    #region Ctor

    public ErpSpecialPriceSyncService(IErpProductService erpProductService,
        ISyncLogService erpSyncLogService,
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService,
        IErpSpecialPriceService erpSpecialPriceService,
        IErpDataClearCacheService erpDataClearCacheService,
        IErpIntegrationPluginManager erpIntegrationPluginService,
        IValidator<ErpSpecialPrice> erpSpecialPriceValidator,
        IStaticCacheManager staticCacheManager,
        ISyncWorkflowMessageService syncWorkflowMessageService)
    {
        _erpProductService = erpProductService;
        _erpSyncLogService = erpSyncLogService;
        _erpAccountService = erpAccountService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpSpecialPriceService = erpSpecialPriceService;
        _erpDataClearCacheService = erpDataClearCacheService;
        _erpIntegrationPluginService = erpIntegrationPluginService;
        _staticCacheManager = staticCacheManager;
        _syncWorkflowMessageService = syncWorkflowMessageService;
        _erpSpecialPriceValidator = erpSpecialPriceValidator;
    }

    #endregion

    #region Utilities

    private async Task<bool> IsValidErpSpecialPriceAsync(ErpSpecialPrice erpSpecialPrice)
    {
        if (erpSpecialPrice is null)
            return false;

        var validationResult = await _erpSpecialPriceValidator.ValidateAsync(erpSpecialPrice);

        if (!validationResult.IsValid)
        {
            var errorMessages = ErpDataValidationHelper.PrepareValidationLog(validationResult);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                ErpSyncLevel.SpecialPrice,
                $"Data mapping skipped for {nameof(ErpSpecialPrice)}, of {nameof(ErpSpecialPrice.ErpAccountId)}: {erpSpecialPrice.ErpAccountId} " +
                $"and {nameof(ErpSpecialPrice.NopProductId)}: {erpSpecialPrice.NopProductId}.\r\n {errorMessages}");
        }

        return validationResult.IsValid;
    }

    #endregion

    #region Method

    public virtual async Task<bool> IsErpSpecialPriceSyncSuccessfulAsync(string? erpAccountNumber, string? stockCode, bool isManualTrigger = false, bool isIncrementalSync = true, CancellationToken cancellationToken = default)
    {
        var erpIntegrationPlugin = await _erpIntegrationPluginService.LoadActiveERPIntegrationPlugin(ErpSyncLevel.SpecialPrice);

        if (erpIntegrationPlugin is null)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                ErpSyncLevel.SpecialPrice,
                $"No integration method found. Unable to run {ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName}.");

            return false;
        }

        try
        {
            #region Data collection

            var salesOrgs = await _erpSalesOrgService.GetAllErpSalesOrgsAsync();
            if (!salesOrgs.Any())
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                    ErpSyncLevel.SpecialPrice,
                    $"No Sales org found. Unable to run {ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName}.");

                return false;
            }

            IList<ErpAccount> specificErpAccounts = null;
            var specificErpAccountSalesOrgFound = false;
            if (!string.IsNullOrWhiteSpace(erpAccountNumber))
            {
                specificErpAccounts = await _erpAccountService.GetErpAccountsOfOnlyActiveErpNopUsersAsync(accountNumber: erpAccountNumber);

                if (specificErpAccounts == null || specificErpAccounts != null && specificErpAccounts.Count == 0)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                        ErpSyncLevel.SpecialPrice,
                        $"No Erp Account found with Account Number: {erpAccountNumber}");

                    return false;
                }
            }

            var erpSpecialPriceInsertList = new List<ErpSpecialPrice>();
            var erpSpecialPriceUpdateList = new List<ErpSpecialPrice>();

            #endregion

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                ErpSyncLevel.SpecialPrice,
                "Erp Special Price Sync started.");

            foreach (var salesOrg in salesOrgs)
            {
                IList<ErpAccount> oldErpAccounts;

                if (specificErpAccounts != null)
                {
                    if (specificErpAccounts.FirstOrDefault(x => x.ErpSalesOrgId == salesOrg.Id) != null)
                    {
                        specificErpAccountSalesOrgFound = true;
                        oldErpAccounts = specificErpAccounts;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    oldErpAccounts = (await _erpAccountService.GetErpAccountsOfOnlyActiveErpNopUsersAsync(salesOrgId: salesOrg.Id)).ToList();
                }

                if (oldErpAccounts.Count == 0)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                        ErpSyncLevel.SpecialPrice,
                        $"No Erp Accounts found with Active Nop Users for Sales org : {salesOrg.Name}");

                    if (specificErpAccounts != null)
                        return false;

                    continue;
                }

                var lastErpSpecialPriceSynced = (decimal)0.0;
                var lastErpSpecialPriceSyncedOfErpAccount = "";
                var lastErpSpecialPriceSyncedOfProduct = "";
                var totalSyncedSoFar = 0;
                var totalNotSyncedSoFar = 0;
                var isError = false;
                var lastErrorMessage = "";
                List<Product> products;

                foreach (var erpAccount in oldErpAccounts)
                {
                    var totalSyncedSoFarForThisAccount = 0;
                    var start = "0";
                    lastErpSpecialPriceSyncedOfErpAccount = erpAccount.AccountNumber;

                    while (true)
                    {
                        var erpGetRequestModel = new ErpGetRequestModel
                        {
                            Start = start,
                            Location = salesOrg.Code,
                            DateFrom = isIncrementalSync ? erpAccount.LastPriceRefresh : null,
                            AccountNumber = erpAccount.AccountNumber,
                            ProductSku = stockCode,
                            
                        };

                        var response = await erpIntegrationPlugin.GetProductSpecialPricesFromErpAsync(erpGetRequestModel);

                        if (response.ErpResponseModel.IsError)
                        {
                            isError = true;
                            lastErrorMessage = $"The last error: {response.ErpResponseModel.ErrorShortMessage}";

                            await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                                DateTime.UtcNow,
                                ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                                response.ErpResponseModel.ErrorShortMessage + "\n\n" + response.ErpResponseModel.ErrorFullMessage);

                            break;
                        }
                        else if (response.Data is null)
                        {
                            isError = false;
                            break;
                        }

                        start = response.ErpResponseModel.Next;

                        var responseData = response.Data
                            .Where(x => !string.IsNullOrWhiteSpace(x.Sku.Trim().ToLower()) && !string.IsNullOrWhiteSpace(x.AccountNumber.Trim()))
                            .GroupBy(x => new { StockCode = x.Sku.Trim().ToLower(), AccountNumber = x.AccountNumber.Trim() })
                            .Select(g => g.Last());

                        totalNotSyncedSoFar += response.Data.Count - responseData.Count();

                        products = (List<Product>?)await _erpProductService
                            .GetProductsBySkuAsync(
                                responseData.Select(x => x.Sku.Trim().ToLower()).ToArray(), 
                                filterOutDeleted: true,
                                filterOutUnpublished: true);

                        foreach (var erpSpecialPrice in responseData)
                        {
                            var product = products?.FirstOrDefault(x => x.Sku.Trim().ToLower() == erpSpecialPrice.Sku.Trim().ToLower());
                            if (product is null)
                            {
                                totalNotSyncedSoFar++;
                                continue;
                            }

                            var oldSpecialPrice = await _erpSpecialPriceService.GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(erpAccount.Id, product.Id);

                            if (oldSpecialPrice is null)
                            {
                                oldSpecialPrice = new ErpSpecialPrice();
                                oldSpecialPrice.ErpAccountId = erpAccount.Id;
                                oldSpecialPrice.NopProductId = product.Id;
                                oldSpecialPrice.Price = erpSpecialPrice.SpecialPrice ?? 0;
                                oldSpecialPrice.ListPrice = erpSpecialPrice.ListPrice ?? 0;
                                oldSpecialPrice.PercentageOfAllocatedStock = 0;
                                oldSpecialPrice.PercentageOfAllocatedStockResetTimeUtc = DateTime.MinValue;
                                oldSpecialPrice.VolumeDiscount = true;
                                oldSpecialPrice.PricingNote = erpSpecialPrice.PricingNotes;
                                oldSpecialPrice.DiscountPerc = erpSpecialPrice.DiscountPercentage ?? 0;                                

                                if (await IsValidErpSpecialPriceAsync(oldSpecialPrice))
                                {
                                    erpSpecialPriceInsertList.Add(oldSpecialPrice);
                                }
                                else
                                    totalNotSyncedSoFar++;
                            }
                            else
                            {
                                oldSpecialPrice.Price = erpSpecialPrice.SpecialPrice ?? 0;
                                oldSpecialPrice.ListPrice = erpSpecialPrice.ListPrice ?? 0;
                                oldSpecialPrice.VolumeDiscount = true;
                                oldSpecialPrice.PricingNote = erpSpecialPrice.PricingNotes;
                                oldSpecialPrice.DiscountPerc = erpSpecialPrice.DiscountPercentage ?? 0;                                

                                if (await IsValidErpSpecialPriceAsync(oldSpecialPrice))
                                {
                                    erpSpecialPriceUpdateList.Add(oldSpecialPrice);
                                }
                                else
                                    totalNotSyncedSoFar++;
                            }
                            lastErpSpecialPriceSyncedOfProduct = product.Sku;
                        }

                        if (erpSpecialPriceInsertList.Count != 0)
                        {
                            await _erpSpecialPriceService.InsertErpSpecialPricesAsync(erpSpecialPriceInsertList);

                            lastErpSpecialPriceSynced = erpSpecialPriceInsertList.LastOrDefault()?.Price ?? 0;
                            totalSyncedSoFar += erpSpecialPriceInsertList.Count;
                            totalSyncedSoFarForThisAccount += erpSpecialPriceInsertList.Count;
                            erpSpecialPriceInsertList.Clear();
                        }

                        if (erpSpecialPriceUpdateList.Count != 0)
                        {
                            await _erpSpecialPriceService.UpdateErpSpecialPricesAsync(erpSpecialPriceUpdateList);

                            lastErpSpecialPriceSynced = erpSpecialPriceUpdateList.LastOrDefault()?.Price ?? 0;
                            totalSyncedSoFar += erpSpecialPriceUpdateList.Count;
                            totalSyncedSoFarForThisAccount += erpSpecialPriceUpdateList.Count;
                            await _erpDataClearCacheService.ClearCacheOfEntities(erpSpecialPriceUpdateList);
                            erpSpecialPriceUpdateList.Clear();
                        }

                        if (cancellationToken.IsCancellationRequested)
                        {
                            await _erpDataClearCacheService.ClearCacheOfEntity(erpAccount);

                            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                                ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                                ErpSyncLevel.SpecialPrice,
                                "The Erp Special Price Sync run is cancelled. " +
                                (!string.IsNullOrWhiteSpace(lastErpSpecialPriceSyncedOfErpAccount) &&
                                !string.IsNullOrWhiteSpace(lastErpSpecialPriceSyncedOfProduct) ?
                                ($"The last synced Erp Special Price: {lastErpSpecialPriceSynced}, on Product: {lastErpSpecialPriceSyncedOfProduct}, " +
                                $"of Erp Account: {lastErpSpecialPriceSyncedOfErpAccount} for Sales Org: ({salesOrg.Code}) {salesOrg.Name}. ") : string.Empty) +
                                $"Total special prices synced in this session: {totalSyncedSoFar}, " +
                                $"And total not synced due to invalid data or product not found - {totalNotSyncedSoFar}");

                            return false;
                        }
                    }

                    erpAccount.LastPriceRefresh = DateTime.UtcNow;
                    await _erpAccountService.UpdateErpAccountAsync(erpAccount);

                    await _erpDataClearCacheService.ClearCacheOfEntity(erpAccount);
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                        ErpSyncLevel.SpecialPrice,
                        $"Total {totalSyncedSoFarForThisAccount} Erp Special Prices synced " +
                        $"for Erp Account: {erpAccount.AccountNumber} ({erpAccount.AccountName}), for Sales Org: ({salesOrg.Code}) {salesOrg.Name}");
                }

                if (!isError)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                        ErpSyncLevel.SpecialPrice,
                        $"Erp Special Price sync successful for Sales Org: ({salesOrg.Code}) {salesOrg.Name}");
                }
                else
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                        ErpSyncLevel.SpecialPrice,
                        $"Erp Special Price sync is partially or not successful for Sales Org: ({salesOrg.Code}) {salesOrg.Name}",
                        lastErrorMessage);
                }

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                    ErpSyncLevel.SpecialPrice,
                    (!string.IsNullOrWhiteSpace(lastErpSpecialPriceSyncedOfErpAccount)
                    && !string.IsNullOrWhiteSpace(lastErpSpecialPriceSyncedOfProduct) ?
                    ($"The last synced Erp Special Price: {lastErpSpecialPriceSynced}, on Product: {lastErpSpecialPriceSyncedOfProduct}, " +
                    $"of Erp Account: {lastErpSpecialPriceSyncedOfErpAccount} for Sales Org: ({salesOrg.Code}) {salesOrg.Name}. ") : string.Empty) +
                    $"Total synced in this session: {totalSyncedSoFar}, " +
                    $"And total not synced due to invalid data or product not found: {totalNotSyncedSoFar}");
            }

            if (!string.IsNullOrWhiteSpace(erpAccountNumber) && !specificErpAccountSalesOrgFound)
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                    ErpSyncLevel.SpecialPrice,
                    $"No Sales org found for the Erp Account : {erpAccountNumber} to sync Special Prices.");
            }

            await _staticCacheManager.RemoveByPrefixAsync("nop.pres.jcarousel.");
            await _staticCacheManager.RemoveByPrefixAsync("Nop.totals.productprice.");
            await _staticCacheManager.RemoveByPrefixAsync(ERPIntegrationCoreDefaults.ErpProductPricingPrefix);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                ErpSyncLevel.SpecialPrice,
                "Erp Special Price Sync ended.");

            return true;
        }
        catch (Exception ex)
        {
            await _staticCacheManager.RemoveByPrefixAsync("nop.pres.jcarousel.");
            await _staticCacheManager.RemoveByPrefixAsync("Nop.totals.productprice.");
            await _staticCacheManager.RemoveByPrefixAsync(ERPIntegrationCoreDefaults.ErpProductPricingPrefix);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                ErpSyncLevel.SpecialPrice,
                ex.Message,
                ex.StackTrace ?? string.Empty);

            await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                DateTime.UtcNow,
                ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                ex.Message + "\n\n" + ex.StackTrace);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                ErpSyncLevel.SpecialPrice,
                "Erp Special Price Sync ended.");

            return false;
        }
    }

    #endregion
}