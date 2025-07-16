using FluentValidation;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncWorkflowMessage;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.ERPIntegrationCore.Validators.Helpers;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public class ErpGroupPriceSyncService : IErpGroupPriceSyncService
{
    #region Fields

    private readonly ISyncLogService _erpSyncLogService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpGroupPriceService _erpGroupPriceService;
    private readonly IErpGroupPriceCodeService _erpGroupPriceCodeService;
    private readonly IErpDataClearCacheService _erpDataClearCacheService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginService;
    private readonly IValidator<ErpGroupPrice> _groupedPriceValidator;
    private readonly IValidator<ErpGroupPriceCode> _groupedPriceCodeValidator;
    private readonly ISyncWorkflowMessageService _syncWorkflowMessageService;
    private readonly IErpProductService _erpProductService;

    #endregion

    #region Ctor

    public ErpGroupPriceSyncService(
        ISyncLogService erpSyncLogService,
        IErpSalesOrgService erpSalesOrgService,
        IErpGroupPriceService erpGroupPriceService,
        IErpGroupPriceCodeService erpGroupPriceCodeService,
        IErpDataClearCacheService erpDataClearCacheService,
        IErpIntegrationPluginManager erpIntegrationPluginService,
        IValidator<ErpGroupPrice> groupedPriceValidator,
        IValidator<ErpGroupPriceCode> groupedPriceCodeValidator,
        ISyncWorkflowMessageService syncWorkflowMessageService,
        IErpProductService erpProductService)
    {
        _erpSyncLogService = erpSyncLogService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpGroupPriceService = erpGroupPriceService;
        _erpGroupPriceCodeService = erpGroupPriceCodeService;
        _erpDataClearCacheService = erpDataClearCacheService;
        _erpIntegrationPluginService = erpIntegrationPluginService;
        _groupedPriceValidator = groupedPriceValidator;
        _groupedPriceCodeValidator = groupedPriceCodeValidator;
        _syncWorkflowMessageService = syncWorkflowMessageService;
        _erpProductService = erpProductService;
    }

    #endregion

    #region Utilities

    private async Task<bool> IsValidErpGroupPrice(ErpGroupPrice erpGroupPrice)
    {
        if (erpGroupPrice is null)
            return false;

        var validationResult = await _groupedPriceValidator.ValidateAsync(erpGroupPrice);

        if (!validationResult.IsValid)
        {
            var errorMessages = ErpDataValidationHelper.PrepareValidationLog(validationResult);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                ErpSyncLevel.GroupPrice,
                $"Data mapping skipped for {nameof(ErpGroupPrice)}, {nameof(ErpGroupPrice.NopProductId)}: {erpGroupPrice.NopProductId}. \r\n {errorMessages}");
        }

        return validationResult.IsValid;
    }

    private async Task<bool> IsValidErpGroupPriceCode(ErpGroupPriceCode erpGroupPriceCode)
    {
        if (erpGroupPriceCode is null)
            return false;

        var validationResult = await _groupedPriceCodeValidator.ValidateAsync(erpGroupPriceCode);

        if (!validationResult.IsValid)
        {
            var errorMessages = ErpDataValidationHelper.PrepareValidationLog(validationResult);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                ErpSyncLevel.GroupPrice,
                $"Data mapping skipped for {nameof(ErpGroupPriceCode)}, {nameof(ErpGroupPriceCode.Code)}: {erpGroupPriceCode.Code}. \r\n {errorMessages}");
        }

        return validationResult.IsValid;
    }

    #endregion

    #region Method

    public virtual async Task<bool> IsErpGroupPriceSyncSuccessfulAsync(string? priceCode, string? stockCode, bool isManualTrigger = false, bool isIncrementalSync = true, CancellationToken cancellationToken = default)
    {
        var erpIntegrationPlugin = await _erpIntegrationPluginService.LoadActiveERPIntegrationPlugin();

        if (erpIntegrationPlugin is null)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                ErpSyncLevel.GroupPrice,
                $"No integration method found. Unable to run {ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName}.");

            return false;
        }

        try
        {
            var allErpSalesOrgs = await _erpSalesOrgService.GetAllErpSalesOrgsAsync();

            if (!allErpSalesOrgs.Any())
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                    ErpSyncLevel.GroupPrice,
                    $"No Sales Org found. Unable to run {ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName}.");

                return false;
            }

            var erpGroupPriceUpdateList = new List<ErpGroupPrice>();
            var erpGroupPriceCodeUpdateList = new List<ErpGroupPriceCode>();
            var erpGroupPriceInsertList = new List<ErpGroupPrice>();
            var erpGroupPriceCodeInsertList = new List<ErpGroupPriceCode>();

            //var syncStartTime = DateTime.UtcNow.AddMinutes(-10);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                ErpSyncLevel.GroupPrice,
                "Erp Group Price Sync started.");

            foreach (var salesOrg in allErpSalesOrgs)
            {
                var start = "0";
                var isError = false;
                var totalSyncedSoFar = 0;
                var totalNotSyncedSoFar = 0;
                var lastErpGroupPriceCodeSynced = string.Empty;

                while (true)
                {
                    var erpGetRequestModel = new ErpGetRequestModel
                    {
                        Start = start,
                        Location = salesOrg.Code,
                        PriceCode = priceCode,
                        ProductSku = stockCode,
                        DateFrom = isIncrementalSync ? salesOrg.LastErpGroupPriceSyncTimeOnUtc : null,
                    };

                    var response = await erpIntegrationPlugin.GetProductGroupPricesFromErpAsync(erpGetRequestModel);

                    if (response.ErpResponseModel.IsError)
                    {
                        isError = true;

                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                            ErpSyncLevel.GroupPrice,
                            response.ErpResponseModel.ErrorShortMessage,
                            response.ErpResponseModel.ErrorFullMessage);

                        await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                            DateTime.UtcNow,
                            ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
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
                        .Where(x => !string.IsNullOrWhiteSpace(x.Sku.Trim()) && !string.IsNullOrWhiteSpace(x.GroupPriceCode.Trim()))
                        .GroupBy(x => new { StockCode = x.Sku.Trim(), PriceCode = x.GroupPriceCode.Trim() })
                        .Select(g => g.Last());

                    totalNotSyncedSoFar += response.Data.Count - responseData.Count();

                    var products = await _erpProductService.GetProductsBySkuAsync(
                        response.Data
                        .Where(x => !string.IsNullOrWhiteSpace(x.Sku.Trim().ToLower()))
                        .Select(x => x.Sku.Trim().ToLower()).ToArray(),
                        filterOutDeleted: true,
                        filterOutUnpublished: true);

                    if (products is null || products.Count == 0)
                    {
                        totalNotSyncedSoFar += responseData.Count();
                        continue;
                    }

                    // GroupPriceCode should be unique
                    // so we'll track these codes to avoid duplication in the erpGroupPriceCodeUpdateList
                    var processedErpGroupPriceCodes = new HashSet<string>();

                    foreach (var erpGroupPrice in responseData)
                    {
                        var product = products.FirstOrDefault(x => x.Sku.Trim().ToLower() == erpGroupPrice.Sku.Trim().ToLower());

                        if (product is null)
                        {
                            totalNotSyncedSoFar++;
                            continue;
                        }

                        if (!string.IsNullOrEmpty(erpGroupPrice.GroupPriceCode))
                        {
                            var oldErpGroupPriceCode = await _erpGroupPriceCodeService
                                .GetErpGroupPriceCodeByCodeAsync(erpGroupPrice.GroupPriceCode);

                            if (!processedErpGroupPriceCodes.Contains(oldErpGroupPriceCode?.Code ?? string.Empty))
                            {
                                if (oldErpGroupPriceCode == null)
                                {
                                    oldErpGroupPriceCode = new ErpGroupPriceCode();
                                    oldErpGroupPriceCode.Code = erpGroupPrice.GroupPriceCode;
                                    oldErpGroupPriceCode.LastUpdateTime = DateTime.UtcNow;
                                    oldErpGroupPriceCode.CreatedById = 1;
                                    oldErpGroupPriceCode.CreatedOnUtc = DateTime.UtcNow;
                                    oldErpGroupPriceCode.UpdatedById = 1;
                                    oldErpGroupPriceCode.UpdatedOnUtc = DateTime.UtcNow;
                                    oldErpGroupPriceCode.IsActive = true;
                                    oldErpGroupPriceCode.IsDeleted = false;

                                    await _erpGroupPriceCodeService.InsertErpGroupPriceCodeAsync(oldErpGroupPriceCode);
                                }
                                else
                                {
                                    oldErpGroupPriceCode.Code = erpGroupPrice.GroupPriceCode;
                                    oldErpGroupPriceCode.LastUpdateTime = DateTime.UtcNow;
                                    oldErpGroupPriceCode.UpdatedById = 1;
                                    oldErpGroupPriceCode.UpdatedOnUtc = DateTime.UtcNow;
                                    oldErpGroupPriceCode.IsActive = true;
                                    oldErpGroupPriceCode.IsDeleted = false;
                                }

                                if (await IsValidErpGroupPriceCode(oldErpGroupPriceCode))
                                {
                                    if (oldErpGroupPriceCode.Id <= 0)
                                    {
                                        await _erpGroupPriceCodeService.InsertErpGroupPriceCodeAsync(oldErpGroupPriceCode);
                                    }
                                    else
                                    {
                                        await _erpGroupPriceCodeService.UpdateErpGroupPriceCodeAsync(oldErpGroupPriceCode);
                                    }
                                    processedErpGroupPriceCodes.Add(oldErpGroupPriceCode.Code);
                                }
                            }

                            if (await IsValidErpGroupPriceCode(oldErpGroupPriceCode))
                            {
                                var oldErpGroupPrice = await _erpGroupPriceService.GetErpGroupPriceByErpPriceGroupCodeAndProductId
                                    (productId: product.Id, priceGroupCodeId: oldErpGroupPriceCode.Id);

                                if (oldErpGroupPrice == null)
                                {
                                    oldErpGroupPrice = new ErpGroupPrice();
                                    oldErpGroupPrice.ErpNopGroupPriceCodeId = oldErpGroupPriceCode.Id;
                                    oldErpGroupPrice.NopProductId = product.Id;
                                    oldErpGroupPrice.Price = erpGroupPrice.Price ?? 0;
                                    oldErpGroupPrice.CreatedById = oldErpGroupPriceCode.CreatedById;
                                    oldErpGroupPrice.CreatedOnUtc = DateTime.UtcNow;
                                    oldErpGroupPrice.UpdatedById = oldErpGroupPriceCode.UpdatedById;
                                    oldErpGroupPrice.UpdatedOnUtc = DateTime.UtcNow;
                                    oldErpGroupPrice.IsActive = true;
                                    oldErpGroupPrice.IsDeleted = false;

                                    if (await IsValidErpGroupPrice(oldErpGroupPrice))
                                    {
                                        erpGroupPriceInsertList.Add(oldErpGroupPrice);
                                    }
                                    else
                                        totalNotSyncedSoFar++;
                                }
                                else
                                {
                                    oldErpGroupPrice.Price = erpGroupPrice.Price ?? 0;
                                    oldErpGroupPrice.UpdatedById = oldErpGroupPriceCode.UpdatedById;
                                    oldErpGroupPrice.UpdatedOnUtc = DateTime.UtcNow;

                                    if (await IsValidErpGroupPrice(oldErpGroupPrice))
                                    {
                                        erpGroupPriceUpdateList.Add(oldErpGroupPrice);
                                    }
                                    else
                                        totalNotSyncedSoFar++;
                                }
                            }
                        }

                        if (erpGroupPrice.GroupPrices.Count != 0)
                        {
                            foreach (var price in erpGroupPrice.GroupPrices)
                            {
                                if (price.Value > 0)
                                {
                                    var oldErpGroupPriceCode = await _erpGroupPriceCodeService.GetErpGroupPriceCodeByCodeAsync(price.Key);

                                    if (!processedErpGroupPriceCodes.Contains(oldErpGroupPriceCode?.Code ?? string.Empty))
                                    {
                                        if (oldErpGroupPriceCode == null)
                                        {
                                            oldErpGroupPriceCode = new ErpGroupPriceCode();
                                            oldErpGroupPriceCode.Code = price.Key;
                                            oldErpGroupPriceCode.LastUpdateTime = DateTime.UtcNow;
                                            oldErpGroupPriceCode.CreatedById = 1;
                                            oldErpGroupPriceCode.CreatedOnUtc = DateTime.UtcNow;
                                            oldErpGroupPriceCode.UpdatedById = 1;
                                            oldErpGroupPriceCode.UpdatedOnUtc = DateTime.UtcNow;
                                            oldErpGroupPriceCode.IsActive = true;
                                            oldErpGroupPriceCode.IsDeleted = false;
                                        }
                                        else
                                        {
                                            oldErpGroupPriceCode.Code = price.Key;
                                            oldErpGroupPriceCode.LastUpdateTime = DateTime.UtcNow;
                                            oldErpGroupPriceCode.UpdatedById = 1;
                                            oldErpGroupPriceCode.UpdatedOnUtc = DateTime.UtcNow;
                                        }

                                        if (await IsValidErpGroupPriceCode(oldErpGroupPriceCode))
                                        {
                                            if (oldErpGroupPriceCode.Id <= 0)
                                            {
                                                await _erpGroupPriceCodeService.InsertErpGroupPriceCodeAsync(oldErpGroupPriceCode);
                                            }
                                            else
                                            {
                                                await _erpGroupPriceCodeService.UpdateErpGroupPriceCodeAsync(oldErpGroupPriceCode);
                                            }
                                            // mark this code as processed
                                            processedErpGroupPriceCodes.Add(oldErpGroupPriceCode.Code);
                                        }
                                    }

                                    if (await IsValidErpGroupPriceCode(oldErpGroupPriceCode))
                                    {
                                        var oldErpGroupPrice = await _erpGroupPriceService
                                        .GetErpGroupPriceByErpPriceGroupCodeAndProductId(productId: product.Id, priceGroupCodeId: oldErpGroupPriceCode.Id);

                                        if (oldErpGroupPrice == null)
                                        {
                                            oldErpGroupPrice = new ErpGroupPrice();
                                            oldErpGroupPrice.ErpNopGroupPriceCodeId = oldErpGroupPriceCode.Id;
                                            oldErpGroupPrice.NopProductId = product.Id;
                                            oldErpGroupPrice.Price = erpGroupPrice.Price ?? 0;
                                            oldErpGroupPrice.CreatedById = oldErpGroupPriceCode.CreatedById;
                                            oldErpGroupPrice.CreatedOnUtc = DateTime.UtcNow;
                                            oldErpGroupPrice.UpdatedById = oldErpGroupPriceCode.UpdatedById;
                                            oldErpGroupPrice.UpdatedOnUtc = DateTime.UtcNow;
                                            oldErpGroupPrice.IsActive = true;
                                            oldErpGroupPrice.IsDeleted = false;

                                            if (await IsValidErpGroupPrice(oldErpGroupPrice))
                                            {
                                                erpGroupPriceInsertList.Add(oldErpGroupPrice);
                                            }
                                            else
                                                totalNotSyncedSoFar++;
                                        }
                                        else
                                        {
                                            oldErpGroupPrice.Price = erpGroupPrice.Price ?? 0;
                                            oldErpGroupPrice.UpdatedById = oldErpGroupPriceCode.UpdatedById;
                                            oldErpGroupPrice.UpdatedOnUtc = DateTime.UtcNow;

                                            if (await IsValidErpGroupPrice(oldErpGroupPrice))
                                            {
                                                erpGroupPriceUpdateList.Add(oldErpGroupPrice);
                                            }
                                            else
                                                totalNotSyncedSoFar++;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    processedErpGroupPriceCodes.Clear();

                    if (erpGroupPriceCodeInsertList.Count != 0)
                    {
                        await _erpGroupPriceCodeService.InsertErpGroupPriceCodesAsync(erpGroupPriceCodeInsertList);
                        lastErpGroupPriceCodeSynced = erpGroupPriceCodeInsertList.LastOrDefault()?.Code;
                        erpGroupPriceCodeInsertList.Clear();
                    }

                    if (erpGroupPriceCodeUpdateList.Count != 0)
                    {
                        await _erpGroupPriceCodeService.UpdateErpGroupPriceCodesAsync(erpGroupPriceCodeUpdateList);
                        lastErpGroupPriceCodeSynced = erpGroupPriceCodeUpdateList.LastOrDefault()?.Code;
                        await _erpDataClearCacheService.ClearCacheOfEntities(erpGroupPriceCodeUpdateList);
                        erpGroupPriceCodeUpdateList.Clear();
                    }

                    if (erpGroupPriceInsertList.Count != 0)
                    {
                        await _erpGroupPriceService.InsertErpGroupPricesAsync(erpGroupPriceInsertList);
                        totalSyncedSoFar += erpGroupPriceInsertList.Count;
                        erpGroupPriceInsertList.Clear();
                    }

                    if (erpGroupPriceUpdateList.Count != 0)
                    {
                        await _erpGroupPriceService.UpdateErpGroupPricesAsync(erpGroupPriceUpdateList);
                        totalSyncedSoFar += erpGroupPriceUpdateList.Count;
                        await _erpDataClearCacheService.ClearCacheOfEntities(erpGroupPriceUpdateList);
                        erpGroupPriceUpdateList.Clear();
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                            ErpSyncLevel.GroupPrice,
                            "The Erp Group Price Sync run is cancelled. " +
                            (!string.IsNullOrWhiteSpace(lastErpGroupPriceCodeSynced) ?
                            $"The last synced Erp Group Price Code - {lastErpGroupPriceCodeSynced}, for Sales Org - ({salesOrg.Code}) {salesOrg.Name}. " : string.Empty) +
                            $"Total group prices synced in this session - {totalSyncedSoFar}, " +
                            $"And total not synced due to invalid data or product not found - {totalNotSyncedSoFar}");

                        return false;
                    }
                }

                if (!isError)
                {
                    //await _erpGroupPriceService.InActiveAllOldGroupPrice(syncStartTime);
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                        ErpSyncLevel.GroupPrice,
                        $"Erp Group Price sync successful for Sales Org - ({salesOrg.Code}) {salesOrg.Name}. " 
                        /*+ $"The group prices which were updated before {syncStartTime} are deactivated."*/);
                }
                else
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                        ErpSyncLevel.GroupPrice,
                        $"Erp Group Price sync is paritally or not successful for Sales Org - ({salesOrg.Code}) {salesOrg.Name}");
                }

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                    ErpSyncLevel.GroupPrice,
                    (!string.IsNullOrWhiteSpace(lastErpGroupPriceCodeSynced) ?
                    $"The last synced Erp Group Price Code - {lastErpGroupPriceCodeSynced}, for Sales Org - ({salesOrg.Code}) {salesOrg.Name}. " : string.Empty) +
                    $"Total group prices synced in this session - {totalSyncedSoFar}, " +
                    $"And total not synced due to invalid data or product not found - {totalNotSyncedSoFar}");

                salesOrg.LastErpGroupPriceSyncTimeOnUtc = DateTime.UtcNow;
                await _erpSalesOrgService.UpdateErpSalesOrgAsync(salesOrg);
            }

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                ErpSyncLevel.GroupPrice,
                "Erp Group Price Sync ended.");

            return true;
        }
        catch (Exception ex)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                ErpSyncLevel.GroupPrice,
                ex.Message,
                ex.StackTrace ?? string.Empty);

            await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                DateTime.UtcNow,
                ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                ex.Message + "\n\n" + ex.StackTrace);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                ErpSyncLevel.GroupPrice,
                "Erp Group Price Sync ended.");

            return false;
        }
    }

    #endregion
}