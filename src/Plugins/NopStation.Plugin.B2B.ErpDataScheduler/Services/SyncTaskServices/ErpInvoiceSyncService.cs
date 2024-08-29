using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public class ErpInvoiceSyncService : IErpInvoiceSyncService
{
    #region Fields

    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;
    private readonly ICurrencyService _currencyService;
    private readonly CurrencySettings _currencySettings;
    private readonly ISyncLogService _erpSyncLogService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpInvoiceService _erpInvoiceService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpDataClearCacheService _erpDataClearCacheService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginService;

    #endregion

    #region Ctor

    public ErpInvoiceSyncService(
        IStoreContext storeContext,
        ISettingService settingService,
        ICurrencyService currencyService,
        CurrencySettings currencySettings,
        ISyncLogService erpSyncLogService,
        IErpAccountService erpAccountService,
        IErpInvoiceService erpInvoiceService,
        IErpSalesOrgService erpSalesOrgService,
        IErpDataClearCacheService erpDataClearCacheService,
        IErpIntegrationPluginManager erpIntegrationPluginService)
    {
        _storeContext = storeContext;
        _settingService = settingService;
        _currencyService = currencyService;
        _currencySettings = currencySettings;
        _erpSyncLogService = erpSyncLogService;
        _erpAccountService = erpAccountService;
        _erpInvoiceService = erpInvoiceService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpDataClearCacheService = erpDataClearCacheService;
        _erpIntegrationPluginService = erpIntegrationPluginService;
    }

    #endregion

    #region Method

    public async virtual Task<bool> IsErpInvoiceSyncSuccessfulAsync()
    {
        var erpIntegrationPlugin = await _erpIntegrationPluginService.LoadActiveERPIntegrationPlugin();

        if (erpIntegrationPlugin is null)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
                ErpSyncLevel.Invoice,
                "No integration method found.");

            return false;
        }

        try
        {
            #region Data collections

            var listOfSalesOrgs = new List<ErpSalesOrg>();
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var erpDataSchedulerSettings = await _settingService.LoadSettingAsync<ErpDataSchedulerSettings>(storeScope);
            var salesOrgCode = await erpIntegrationPlugin.GetSalesOrgCodeFromIntegrationSettings();
            var currency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);

            if (!string.IsNullOrWhiteSpace(salesOrgCode))
            {
                var salesOrg = (await _erpSalesOrgService.GetAllErpSalesOrgAsync(code: salesOrgCode)).FirstOrDefault();

                if (salesOrg == null)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
                    ErpSyncLevel.Invoice,
                    $"No Sales org found with Sales org code: {salesOrgCode}. Unable to run {ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName}.");

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
                ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
                ErpSyncLevel.Invoice,
                "Erp Invoice Sync started.");

            foreach (var salesOrg in listOfSalesOrgs)
            {
                var oldErpAccounts = (List<ErpAccount>)await _erpAccountService.GetAllErpAccountsAsync(salesOrgId: salesOrg.Id);
                if (oldErpAccounts.Count == 0)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
                        ErpSyncLevel.Invoice,
                        $"No Erp Accounts found with the Sales org : {salesOrg.Name}");

                    continue;
                }

                var lastErpInvoiceSynced = new ErpInvoice();
                var lastErpInvoiceSyncedOfErpAccount = "";
                var totalSyncedSoFar = 0;
                var isError = false;
                var lastErrorMessage = "";

                foreach (var erpAccount in oldErpAccounts)
                {
                    var start = "0";
                    var erpInvoices = (List<ErpInvoice>)await _erpInvoiceService.GetErpInvoicesByErpAccountIdAsync(erpAccount.Id);
                    var dateFrom = erpDataSchedulerSettings.SyncFromDate.HasValue ? erpDataSchedulerSettings.SyncFromDate.Value : DateTime.MinValue;

                    while (true)
                    {
                        var erpGetRequestModel = new ErpGetRequestModel
                        {
                            Start = start,
                            AccountNumber = erpAccount.AccountNumber,
                            Location = salesOrg.Code,
                            DateFrom = erpAccount.LastTimeOrderSyncOnUtc.HasValue ? erpAccount.LastTimeOrderSyncOnUtc : dateFrom
                        };

                        var response = await erpIntegrationPlugin.GetInvoiceByAccountNoFromErpAsync(erpGetRequestModel);

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

                        foreach (var erpInvoice in response.Data)
                        {
                            var oldErpInvoiceByThisAccount = erpInvoices.Find(inv => inv.ErpDocumentNumber == erpInvoice.ErpDocumentNumber) ?? new ErpInvoice();

                            if (oldErpInvoiceByThisAccount.Id <= 0)
                            {
                                oldErpInvoiceByThisAccount.ShipmentDateUtc = erpInvoice.ShipmentDateUtc;
                                oldErpInvoiceByThisAccount.PostingDateUtc = erpInvoice.PostingDateUtc ?? DateTime.UtcNow;
                                oldErpInvoiceByThisAccount.DocumentDateUtc = erpInvoice.DocumentDateUtc;
                                oldErpInvoiceByThisAccount.ErpDocumentNumber = erpInvoice.ErpDocumentNumber;
                                oldErpInvoiceByThisAccount.ErpOrderNumber = erpInvoice.ErpOrderNumber;
                                oldErpInvoiceByThisAccount.Description = erpInvoice.Description;
                                oldErpInvoiceByThisAccount.ErpAccountId = erpAccount.Id;
                                oldErpInvoiceByThisAccount.CurrencyCode = currency.CurrencyCode;
                                oldErpInvoiceByThisAccount.PODSignedById = erpInvoice.PODSignedById;
                                oldErpInvoiceByThisAccount.PODSignedOnUtc = erpInvoice.PODSignedOnUtc;
                                oldErpInvoiceByThisAccount.RelatedDocumentNo = erpInvoice.RelatedDocumentNo;
                                oldErpInvoiceByThisAccount.ItemCount = erpInvoice.Items?.Count ?? 0;

                                if (Enum.TryParse(erpInvoice.DocumentType, out ErpDocumentType parsedDocumentType))
                                {
                                    oldErpInvoiceByThisAccount.DocumentType = parsedDocumentType;
                                    oldErpInvoiceByThisAccount.DocumentDisplayName = parsedDocumentType.ToString();
                                }

                                await _erpInvoiceService.InsertErpInvoiceAsync(oldErpInvoiceByThisAccount);
                            }
                            else
                            {
                                oldErpInvoiceByThisAccount.ErpDocumentNumber = erpInvoice.ErpDocumentNumber;
                                oldErpInvoiceByThisAccount.ErpOrderNumber = erpInvoice.ErpOrderNumber;
                                oldErpInvoiceByThisAccount.Description = erpInvoice.Description;
                                oldErpInvoiceByThisAccount.ErpAccountId = erpAccount.Id;
                                oldErpInvoiceByThisAccount.CurrencyCode = currency.CurrencyCode;
                                oldErpInvoiceByThisAccount.RelatedDocumentNo = erpInvoice.RelatedDocumentNo;
                                oldErpInvoiceByThisAccount.ItemCount = erpInvoice.Items?.Count ?? 0;

                                if (Enum.TryParse(erpInvoice.DocumentType, out ErpDocumentType parsedDocumentType))
                                {
                                    oldErpInvoiceByThisAccount.DocumentType = parsedDocumentType;
                                    oldErpInvoiceByThisAccount.DocumentDisplayName = parsedDocumentType.ToString();
                                }

                                await _erpInvoiceService.UpdateErpInvoiceAsync(oldErpInvoiceByThisAccount);
                            }

                            lastErpInvoiceSynced = oldErpInvoiceByThisAccount;
                            lastErpInvoiceSyncedOfErpAccount = erpAccount.AccountNumber;
                            totalSyncedSoFar++;

                            #region Cache clear for this erp invoice

                            await _erpDataClearCacheService.ClearCacheOfEntity(oldErpInvoiceByThisAccount, oldErpInvoiceByThisAccount.Id);

                            #endregion
                        }
                    }
                }

                if (!isError)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
                        ErpSyncLevel.Invoice,
                        $"Erp Invoice sync successful for Sales Org: {salesOrg.Name}");
                }
                else
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
                        ErpSyncLevel.Invoice,
                        $"Erp Invoice sync is partially or not successful for Sales Org: {salesOrg.Name}",
                        lastErrorMessage);
                }

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
                    ErpSyncLevel.Invoice,
                    (lastErpInvoiceSynced is not null ?
                    $"The last synced Erp Invoice: {lastErpInvoiceSynced.ErpDocumentNumber}, of Erp Account: {lastErpInvoiceSyncedOfErpAccount} for Sales Org: {salesOrg.Name}. " : string.Empty) + $"Total synced in this session: {totalSyncedSoFar}");
            }

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
                ErpSyncLevel.Invoice,
                "Erp Invoice Sync ended.");

            return true;
        }
        catch (Exception ex)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
                ErpSyncLevel.Invoice,
                ex.Message,
                ex.StackTrace);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
                ErpSyncLevel.Invoice,
                "Erp Invoice Sync ended.");

            return false;
        }
    }

    #endregion
}