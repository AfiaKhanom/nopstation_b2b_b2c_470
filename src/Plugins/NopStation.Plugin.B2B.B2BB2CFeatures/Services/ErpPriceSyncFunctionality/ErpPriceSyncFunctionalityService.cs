using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpPriceSyncFunctionality;

public class ErpPriceSyncFunctionalityService : IErpPriceSyncFunctionalityService
{
    #region Fields

    private readonly ICustomerService _customerService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IWorkContext _workContext;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IStoreContext _storeContext;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IErpGroupPriceCodeService _erpGroupPriceCodeService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IErpLogsService _erpLogsService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginManager;
    private readonly IProductService _productService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpSpecialPriceService _erpSpecialPriceService;
    private readonly IErpGroupPriceService _erpGroupPriceService;

    #endregion Fields

    #region Ctor

    public ErpPriceSyncFunctionalityService(ICustomerService customerService,
        IErpAccountService erpAccountService,
        IWorkContext workContext,
        IGenericAttributeService genericAttributeService,
        IStoreContext storeContext,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IErpGroupPriceCodeService erpGroupPriceCodeService,
        IDateTimeHelper dateTimeHelper,
        IErpLogsService erpLogsService,
        IErpIntegrationPluginManager erpIntegrationPluginManager,
        IProductService productService,
        IErpSalesOrgService erpSalesOrgService,
        IErpSpecialPriceService erpSpecialPriceService,
        IErpGroupPriceService erpGroupPriceService)
    {
        _customerService = customerService;
        _erpAccountService = erpAccountService;
        _workContext = workContext;
        _genericAttributeService = genericAttributeService;
        _storeContext = storeContext;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpGroupPriceCodeService = erpGroupPriceCodeService;
        _dateTimeHelper = dateTimeHelper;
        _erpLogsService = erpLogsService;
        _erpIntegrationPluginManager = erpIntegrationPluginManager;
        _productService = productService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpSpecialPriceService = erpSpecialPriceService;
        _erpGroupPriceService = erpGroupPriceService;
    }

    #endregion Ctor

    #region Utilities

    private async Task<bool> CheckPriceSyncRequired(Customer customer, Store store)
    {
        if (customer == null || store == null)
            return false;

        var lastDateOfDisplayB2BPriceSyncInfo = await _genericAttributeService.GetAttributeAsync<DateTime?>(customer,
            B2BB2CFeaturesDefaults.CustomerLastDateOfDisplayB2BPriceSyncInfo, store.Id, defaultValue: null);

        // we will display price update notification only once in a day
        if (lastDateOfDisplayB2BPriceSyncInfo.HasValue && lastDateOfDisplayB2BPriceSyncInfo.Value.Date == DateTime.Now.Date)
            return false;

        var b2BAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);
        if (b2BAccount == null)
            return false;

        //Checking for B2BPriceGroupProduct Pricing applicable for the B2BAccount
        if (_b2BB2CFeaturesSettings.UseProductGroupPrice)
        {
            if (!b2BAccount.B2BPriceGroupCodeId.HasValue)
                return false;

            var b2BPriceGroupCode = await _erpGroupPriceCodeService.GetErpGroupPriceCodeByIdAsync(b2BAccount.B2BPriceGroupCodeId.Value);

            if (b2BPriceGroupCode == null)
                return false;

            var lastDateOfDisplayB2BPriceGroupPriceSyncInfo = await _genericAttributeService.GetAttributeAsync<DateTime?>(b2BPriceGroupCode,
            B2BB2CFeaturesDefaults.CustomerLastDateOfDisplayB2BPriceGroupPriceSyncInfo, store.Id, defaultValue: null);

            if (lastDateOfDisplayB2BPriceGroupPriceSyncInfo.HasValue && lastDateOfDisplayB2BPriceGroupPriceSyncInfo.Value.Date == DateTime.Now.Date)
            {
                return false;
            }

            var b2BPriceGroupLastPriceRefreshDate = await _dateTimeHelper.ConvertToUserTimeAsync(b2BPriceGroupCode.LastUpdateTime, DateTimeKind.Utc);

            if (b2BPriceGroupLastPriceRefreshDate < DateTime.Now.Date)
            {
                await _genericAttributeService.SaveAttributeAsync(b2BPriceGroupCode, B2BB2CFeaturesDefaults.CustomerLastDateOfDisplayB2BPriceGroupPriceSyncInfo,
                    DateTime.Now.Date, store.Id);
                return true;
            }

            return false;
        }
        else
        {
            if (!b2BAccount.LastPriceRefresh.HasValue)
            {
                return true;
            }

            //convert dates to the user time
            var lastPriceRefreshDate = await _dateTimeHelper.ConvertToUserTimeAsync(b2BAccount.LastPriceRefresh.Value, DateTimeKind.Utc);
            if (lastPriceRefreshDate < DateTime.Now.Date)
            {
                return true;
            }
        }

        return false;
    }

    private async Task InsertOrUpdateSpecialPricesBatchAsync(List<ErpSpecialPrice> insertList, List<ErpSpecialPrice> updateList)
    {
        if (insertList is not null && insertList.Any())
        {
            await _erpSpecialPriceService.InsertErpSpecialPricesAsync(insertList);
            insertList.Clear();
        }

        if (updateList is not null && updateList.Any())
        {
            await _erpSpecialPriceService.UpdateErpSpecialPricesAsync(updateList);
            updateList.Clear();
        }
    }

    private async Task<(int totalSyncedSoFarForThisAccount, bool isError, string lastErrorMessage)> ProcessSpecialPriceSyncFromErpAsync(IErpIntegrationPlugin erpIntegrationPlugin, ErpSalesOrg salesOrg, ErpAccount erpAccount)
    {
        if (erpIntegrationPlugin == null || salesOrg == null || erpAccount == null)
        {
            return (0, true, "Price sync failed, erpIntegrationPlugin or salesOrg or erpAccount is null.");
        }

        var erpSpecialPriceInsertList = new List<ErpSpecialPrice>();
        var erpSpecialPriceUpdateList = new List<ErpSpecialPrice>();
        /*var lastErpSpecialPriceSynced = decimal.Zero;
        var lastErpSpecialPriceSyncedOfErpAccount = "";
        var lastErpSpecialPriceSyncedOfProduct = "";*/
        var isError = false;
        var lastErrorMessage = "";

        var totalSyncedSoFarForThisAccount = 0;
        var start = "0";
        while (true)
        {
            var erpGetRequestModel = new ErpGetRequestModel
            {
                Start = start,
                Location = salesOrg.Code,
                DateFrom = erpAccount.LastPriceRefresh,
                AccountNumber = erpAccount.AccountNumber
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

            var responseData = response.Data
                .Where(x => !string.IsNullOrWhiteSpace(x.Sku))
                .GroupBy(x => x.Sku)
                .Select(g => g.Last());

            foreach (var erpSpecialPrice in responseData)
            {
                var product = await _productService.GetProductBySkuAsync(erpSpecialPrice.Sku);
                if (product is null)
                {
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
                    oldSpecialPrice.DiscountPerc = erpSpecialPrice.DiscountPercentage ?? 0;
                    oldSpecialPrice.PricingNote = erpSpecialPrice.PricingNotes;
                    erpSpecialPriceInsertList.Add(oldSpecialPrice);
                }
                else
                {
                    oldSpecialPrice.Price = erpSpecialPrice.SpecialPrice ?? 0;
                    oldSpecialPrice.ListPrice = erpSpecialPrice.ListPrice ?? 0;
                    oldSpecialPrice.DiscountPerc = erpSpecialPrice.DiscountPercentage ?? 0;
                    oldSpecialPrice.PercentageOfAllocatedStock = 0;
                    oldSpecialPrice.PercentageOfAllocatedStockResetTimeUtc = DateTime.MinValue;
                    oldSpecialPrice.VolumeDiscount = true;
                    oldSpecialPrice.DiscountPerc = erpSpecialPrice.DiscountPercentage ?? 0;
                    oldSpecialPrice.PricingNote = erpSpecialPrice.PricingNotes;
                    erpSpecialPriceUpdateList.Add(oldSpecialPrice);
                }

                /*lastErpSpecialPriceSynced = oldSpecialPrice.Price;
                lastErpSpecialPriceSyncedOfErpAccount = erpAccount.AccountNumber;
                lastErpSpecialPriceSyncedOfProduct = product.Sku;*/
                totalSyncedSoFarForThisAccount++;
            }

            await InsertOrUpdateSpecialPricesBatchAsync(erpSpecialPriceInsertList, erpSpecialPriceUpdateList);
        }

        return (totalSyncedSoFarForThisAccount, isError, lastErrorMessage);
    }

    private async Task SyncSpecialPricesAsync(IErpIntegrationPlugin erpIntegrationPlugin, ErpAccount erpAccount, ErpSalesOrg salesOrg)
    {
        if (erpIntegrationPlugin == null || erpAccount == null || salesOrg == null)
        {
            await _erpLogsService.ErrorAsync("PriceSync failed. ErpAccount or salesOrg or integrationPlugin is null.", ErpSyncLevel.Account);
            return;
        }

        try
        {
            await _erpLogsService.InformationAsync($"Erp Special Price Sync started for: ErpAccount Number = {erpAccount.AccountNumber}", ErpSyncLevel.SpecialPrice);

            var syncResult = await ProcessSpecialPriceSyncFromErpAsync(erpIntegrationPlugin, salesOrg, erpAccount);

            erpAccount.LastPriceRefresh = DateTime.UtcNow;
            await _erpAccountService.UpdateErpAccountAsync(erpAccount);

            await _erpLogsService.InformationAsync(
                $"Total {syncResult.totalSyncedSoFarForThisAccount} Erp Special Prices synced. " +
                $"For Erp Account: {erpAccount.AccountNumber} - ({erpAccount.AccountName}), " +
                $"Sales Org: {salesOrg.Name}",
                ErpSyncLevel.SpecialPrice);

            if (!syncResult.isError)
            {
                await _erpLogsService.InformationAsync($"Erp Special Price sync successful for Sales Org: {salesOrg.Name}", ErpSyncLevel.SpecialPrice);
            }
            else
            {
                await _erpLogsService.ErrorAsync($"Erp Special Price sync is partially or not successful for Sales Org: {salesOrg.Name}, for erp account: {erpAccount.Id}. Due to this error - {syncResult.lastErrorMessage}", ErpSyncLevel.SpecialPrice);
            }
            await _erpLogsService.InformationAsync($"Erp Special price sync ended for - sales org: {salesOrg.Name}, erp account number: {erpAccount.AccountNumber}.", ErpSyncLevel.SpecialPrice);
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Error, ErpSyncLevel.SpecialPrice, "Error - for Erp Special price sync", $"Due to - {ex.Message}. StackTrace - {ex.StackTrace}");
            await _erpLogsService.InformationAsync($"Erp Special price sync ended for - sales org: {salesOrg.Name}, erp account number: {erpAccount.AccountNumber}.", ErpSyncLevel.SpecialPrice);
        }
    }

    private async Task InsertOrUpdateGroupPricesBatchAsync(
        IList<ErpGroupPriceCode> erpGroupPriceCodeInsertList,
        IList<ErpGroupPriceCode> erpGroupPriceCodeUpdateList,
        IList<ErpGroupPrice> erpGroupPriceInsertList,
        IList<ErpGroupPrice> erpGroupPriceUpdateList)
    {
        // Insert or update ErpGroupPriceCodes
        if (erpGroupPriceCodeInsertList is not null && erpGroupPriceCodeInsertList.Any())
        {
            await _erpGroupPriceCodeService.InsertErpGroupPriceCodesAsync(erpGroupPriceCodeInsertList);
            erpGroupPriceCodeInsertList.Clear();
        }

        if (erpGroupPriceCodeUpdateList is not null && erpGroupPriceCodeUpdateList.Any())
        {
            await _erpGroupPriceCodeService.UpdateErpGroupPriceCodesAsync(erpGroupPriceCodeUpdateList);
            erpGroupPriceCodeUpdateList.Clear();
        }

        // Insert or update ErpGroupPrices
        if (erpGroupPriceInsertList is not null && erpGroupPriceInsertList.Any())
        {
            await _erpGroupPriceService.InsertErpGroupPricesAsync(erpGroupPriceInsertList);
            erpGroupPriceInsertList.Clear();
        }

        if (erpGroupPriceUpdateList is not null && erpGroupPriceUpdateList.Any())
        {
            await _erpGroupPriceService.UpdateErpGroupPricesAsync(erpGroupPriceUpdateList);
            erpGroupPriceUpdateList.Clear();
        }
    }

    private async Task SyncGroupPricesAsync(IErpIntegrationPlugin erpIntegrationPlugin, ErpAccount erpAccount, ErpSalesOrg salesOrg)
    {
        try
        {
            var erpGroupPriceUpdateList = new List<ErpGroupPrice>();
            var erpGroupPriceCodeUpdateList = new List<ErpGroupPriceCode>();
            var erpGroupPriceInsertList = new List<ErpGroupPrice>();
            var erpGroupPriceCodeInsertList = new List<ErpGroupPriceCode>();

            var syncStartTime = DateTime.UtcNow.AddMinutes(-10);

            await _erpLogsService.InformationAsync($"Erp Group Price Sync started for: ErpAccount Number = {erpAccount.AccountNumber}", ErpSyncLevel.GroupPrice);

            var start = "0";
            var isError = false;
            var totalSyncedSoFar = 0;
            var lastErpGroupPriceCodeSynced = string.Empty;

            while (true)
            {
                var erpGetRequestModel = new ErpGetRequestModel
                {
                    Start = start,
                    Location = salesOrg.Code
                };

                var response = await erpIntegrationPlugin.GetProductGroupPricesFromErpAsync(erpGetRequestModel);

                if (response.ErpResponseModel.IsError)
                {
                    isError = true;
                    await _erpLogsService.ErrorAsync($"Error for Group price sync. ErpAccount Number = {erpAccount.AccountNumber}, SalesOrg id = {salesOrg.Id}. Due to - {response.ErpResponseModel.ErrorShortMessage}", ErpSyncLevel.GroupPrice);
                    break;
                }
                else if (response.Data is null)
                {
                    isError = false;
                    break;
                }

                var products = await _productService.GetProductsBySkuAsync(response.Data.Where(x => !string.IsNullOrWhiteSpace(x.Sku)).Select(x => x.Sku).ToArray());
                if (products is null || products.Count == 0)
                {
                    isError = false;
                    break;
                }

                // GroupPriceCode should be unique
                // so we'll track these codes to avoid duplication in the erpGroupPriceCodeUpdateList
                var processedErpGroupPriceCodes = new HashSet<string>();

                foreach (var erpGroupPrice in response.Data)
                {
                    var product = products.FirstOrDefault(x => x.Sku == erpGroupPrice.Sku);

                    if (product is null)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(erpGroupPrice.GroupPriceCode))
                    {
                        var oldErpGroupPriceCode = await _erpGroupPriceCodeService.GetErpGroupPriceCodeByCodeAsync(erpGroupPrice.GroupPriceCode);
                        if (!processedErpGroupPriceCodes.Contains(oldErpGroupPriceCode.Code))
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
                                erpGroupPriceCodeInsertList.Add(oldErpGroupPriceCode);
                            }
                            else
                            {
                                oldErpGroupPriceCode.Code = erpGroupPrice.GroupPriceCode;
                                oldErpGroupPriceCode.LastUpdateTime = DateTime.UtcNow;
                                oldErpGroupPriceCode.UpdatedById = 1;
                                oldErpGroupPriceCode.UpdatedOnUtc = DateTime.UtcNow;
                                oldErpGroupPriceCode.IsActive = true;
                                oldErpGroupPriceCode.IsDeleted = false;
                                erpGroupPriceCodeUpdateList.Add(oldErpGroupPriceCode);
                            }

                            // mark this code as processed
                            processedErpGroupPriceCodes.Add(oldErpGroupPriceCode.Code);
                        }

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
                            erpGroupPriceInsertList.Add(oldErpGroupPrice);
                        }
                        else
                        {
                            oldErpGroupPrice.Price = erpGroupPrice.Price ?? 0;
                            oldErpGroupPrice.UpdatedById = oldErpGroupPriceCode.UpdatedById;
                            oldErpGroupPrice.UpdatedOnUtc = DateTime.UtcNow;
                            erpGroupPriceUpdateList.Add(oldErpGroupPrice);
                        }

                        totalSyncedSoFar++;
                        lastErpGroupPriceCodeSynced = oldErpGroupPriceCode.Code;
                    }

                    if (erpGroupPrice.GroupPrices.Count != 0)
                    {
                        foreach (var price in erpGroupPrice.GroupPrices)
                        {
                            if (price.Value > 0)
                            {
                                var oldErpGroupPriceCode = await _erpGroupPriceCodeService.GetErpGroupPriceCodeByCodeAsync(price.Key);

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
                                    erpGroupPriceCodeInsertList.Add(oldErpGroupPriceCode);
                                }
                                else
                                {
                                    oldErpGroupPriceCode.Code = price.Key;
                                    oldErpGroupPriceCode.LastUpdateTime = DateTime.UtcNow;
                                    oldErpGroupPriceCode.UpdatedById = 1;
                                    oldErpGroupPriceCode.UpdatedOnUtc = DateTime.UtcNow;
                                    erpGroupPriceCodeUpdateList.Add(oldErpGroupPriceCode);
                                }

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
                                    erpGroupPriceInsertList.Add(oldErpGroupPrice);
                                }
                                else
                                {
                                    oldErpGroupPrice.Price = erpGroupPrice.Price ?? 0;
                                    oldErpGroupPrice.UpdatedById = oldErpGroupPriceCode.UpdatedById;
                                    oldErpGroupPrice.UpdatedOnUtc = DateTime.UtcNow;
                                    erpGroupPriceUpdateList.Add(oldErpGroupPrice);
                                }

                                totalSyncedSoFar++;
                                lastErpGroupPriceCodeSynced = oldErpGroupPriceCode.Code;
                            }
                        }
                    }
                }

                processedErpGroupPriceCodes.Clear();
                await InsertOrUpdateGroupPricesBatchAsync(erpGroupPriceCodeInsertList, erpGroupPriceCodeUpdateList, erpGroupPriceInsertList, erpGroupPriceUpdateList);
            }

            if (!isError)
            {
                await _erpGroupPriceService.InActiveAllOldGroupPrice(syncStartTime);
                await _erpLogsService.InformationAsync($"Erp Group Price sync successful for Sales Org: {salesOrg.Name}. The group prices which were updated before {syncStartTime} are deactivated.", ErpSyncLevel.GroupPrice);
            }
            else
            {
                await _erpLogsService.ErrorAsync($"Erp Group Price sync is partially or not successful for Sales Org: {salesOrg.Name}, for erp account: {erpAccount.Id}.", ErpSyncLevel.GroupPrice);
            }

            await _erpLogsService.InformationAsync((!string.IsNullOrWhiteSpace(lastErpGroupPriceCodeSynced) ?
                    $"The last synced Erp Group Price Code: {lastErpGroupPriceCodeSynced}, for Sales Org: {salesOrg.Name}. " : string.Empty) +
                    $"Total synced in this session: {totalSyncedSoFar}",
              ErpSyncLevel.GroupPrice);

            await _erpLogsService.InformationAsync($"Erp Group price sync ended for - sales org: {salesOrg.Name}, erp account number: {erpAccount.AccountNumber}.", ErpSyncLevel.GroupPrice);
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Error, ErpSyncLevel.GroupPrice, "Error - for Erp Group price sync", $"Due to - {ex.Message}. StackTrace - {ex.StackTrace}");

            await _erpLogsService.InformationAsync($"Erp Group price sync ended for - sales org: {salesOrg.Name}, erp account number: {erpAccount.AccountNumber}.", ErpSyncLevel.GroupPrice);
        }
    }

    #endregion Utilities

    #region Methods

    public async Task ExecuteAllProductsLivePriceSync()
    {
        var erpIntegrationPlugin = await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin(ErpSyncLevel.Invoice);
        if (erpIntegrationPlugin is null)
        {
            await _erpLogsService.ErrorAsync("No integration plugin found to sync price.", ErpSyncLevel.Account);
            return;
        }

        var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id);
        if (erpAccount is null)
        {
            await _erpLogsService.ErrorAsync("No erp account found to sync price.", ErpSyncLevel.Account);
            return;
        }

        var salesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdWithActiveAsync(erpAccount.ErpSalesOrgId);
        if (salesOrg is null)
        {
            await _erpLogsService.ErrorAsync("No sales org found to sync price.", ErpSyncLevel.SalesRep);
            return;
        }

        if (_b2BB2CFeaturesSettings.UseProductSpecialPrice)
        {
            await SyncSpecialPricesAsync(erpIntegrationPlugin, erpAccount, salesOrg);
        }
        else if (_b2BB2CFeaturesSettings.UseProductGroupPrice)
        {
            await SyncGroupPricesAsync(erpIntegrationPlugin, erpAccount, salesOrg);
        }
    }

    public async Task<bool> IsB2BPriceSyncRequiredAsync()
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (!await _customerService.IsRegisteredAsync(customer))
            return false;

        var isRequired = await CheckPriceSyncRequired(customer, store);
        if (isRequired)
        {
            // we will display price update notification only once in a day
            await _genericAttributeService.SaveAttributeAsync(customer, B2BB2CFeaturesDefaults.CustomerLastDateOfDisplayB2BPriceSyncInfo,
                    DateTime.Now.Date, store.Id);
        }

        return isRequired;
    }

    public async Task<bool> IsCartProductB2BPriceSyncRequiredAsync()
    {
        throw new NotImplementedException();
    }

    #endregion Methods
}