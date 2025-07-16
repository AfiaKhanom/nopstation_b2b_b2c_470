using FluentValidation;
using Nop.Core.Domain.Directory;
using Nop.Services.Directory;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncWorkflowMessage;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.ERPIntegrationCore.Validators.Helpers;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public class ErpInvoiceSyncService : IErpInvoiceSyncService
{
    #region Fields

    private readonly ICurrencyService _currencyService;
    private readonly CurrencySettings _currencySettings;
    private readonly ISyncLogService _erpSyncLogService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpInvoiceService _erpInvoiceService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpDataClearCacheService _erpDataClearCacheService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginService;
    private readonly IValidator<ErpInvoice> _erpInvoiceValidator;
    private readonly ISyncWorkflowMessageService _syncWorkflowMessageService;

    #endregion

    #region Ctor

    public ErpInvoiceSyncService(ICurrencyService currencyService,
        CurrencySettings currencySettings,
        ISyncLogService erpSyncLogService,
        IErpAccountService erpAccountService,
        IErpInvoiceService erpInvoiceService,
        IErpSalesOrgService erpSalesOrgService,
        IErpDataClearCacheService erpDataClearCacheService,
        IErpIntegrationPluginManager erpIntegrationPluginService,
        IValidator<ErpInvoice> erpInvoiceValidator,
        ISyncWorkflowMessageService syncWorkflowMessageService)
    {
        _currencyService = currencyService;
        _currencySettings = currencySettings;
        _erpSyncLogService = erpSyncLogService;
        _erpAccountService = erpAccountService;
        _erpInvoiceService = erpInvoiceService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpDataClearCacheService = erpDataClearCacheService;
        _erpIntegrationPluginService = erpIntegrationPluginService;
        _erpInvoiceValidator = erpInvoiceValidator;
        _syncWorkflowMessageService = syncWorkflowMessageService;
    }

    #endregion

    #region Utilities

    private async Task<bool> IsvalidErpInvoiceAsync(ErpInvoice erpInvoice)
    {
        if (erpInvoice is null)
            return false;

        var validationResult = await _erpInvoiceValidator.ValidateAsync(erpInvoice);

        if (!validationResult.IsValid)
        {
            var errorMessages = ErpDataValidationHelper.PrepareValidationLog(validationResult);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
                ErpSyncLevel.Invoice,
                $"Data mapping skipped for {nameof(ErpInvoice)}, {nameof(ErpInvoice.ErpAccountId)}: {erpInvoice.Id}. \r\n {errorMessages}");
        }

        return validationResult.IsValid;
    }

    #endregion

    #region Method

    public virtual async Task<bool> IsErpInvoiceSyncSuccessfulAsync(string? erpAccountNumber, bool isManualTrigger = false, bool isIncrementalSync = true, CancellationToken cancellationToken = default)
    {
        var erpIntegrationPlugin = await _erpIntegrationPluginService.LoadActiveERPIntegrationPlugin();

        if (erpIntegrationPlugin is null)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
                ErpSyncLevel.Invoice,
                $"No integration method found. Unable to run {ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName}.");

            return false;
        }

        try
        {
            #region Data collections

            var salesOrgs = await _erpSalesOrgService.GetAllErpSalesOrgsAsync();
            if (!salesOrgs.Any())
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
                    ErpSyncLevel.Invoice,
                    $"No Sales org found. Unable to run {ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName}.");

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
                        ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
                        ErpSyncLevel.Invoice,
                        $"No Erp Account found with Account Number: {erpAccountNumber}");

                    return false;
                }
            }

            var currency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);

            var erpInvoiceUpdateList = new List<ErpInvoice>();
            var erpInvoiceInsertList = new List<ErpInvoice>();

            #endregion

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
                ErpSyncLevel.Invoice,
                "Erp Invoice Sync started.");

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
                    oldErpAccounts = await _erpAccountService.GetErpAccountsOfOnlyActiveErpNopUsersAsync(salesOrgId: salesOrg.Id);
                }

                if (oldErpAccounts.Count == 0)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
                        ErpSyncLevel.Invoice,
                        $"No Erp Accounts found with Active Nop Users for Sales org : {salesOrg.Name}");

                    if (specificErpAccounts != null)
                        return false;

                    continue;
                }
                

                var lastErpInvoiceSynced = string.Empty;
                var lastErpInvoiceSyncedOfErpAccount = string.Empty;
                var totalSyncedSoFar = 0;
                var totalNotSyncedSoFar = 0;
                var isError = false;
                var lastErrorMessage = "";

                foreach (var erpAccount in oldErpAccounts)
                {
                    var start = "0";
                    var erpInvoices = await _erpInvoiceService.GetErpInvoicesByErpAccountIdAsync(erpAccount.Id);
                    lastErpInvoiceSyncedOfErpAccount = erpAccount.AccountNumber;

                    while (true)
                    {
                        var erpGetRequestModel = new ErpGetRequestModel
                        {
                            Start = start,
                            AccountNumber = erpAccount.AccountNumber,
                            Location = salesOrg.Code,
                            DateFrom = isIncrementalSync ? erpAccount.LastTimeOrderSyncOnUtc : null
                        };

                        var response = await erpIntegrationPlugin.GetInvoiceByAccountNoFromErpAsync(erpGetRequestModel);

                        if (response.ErpResponseModel.IsError)
                        {
                            isError = true;
                            lastErrorMessage = $"The last error: {response.ErpResponseModel.ErrorShortMessage}";

                            await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                                DateTime.UtcNow,
                                ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
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
                            .Where(x => !string.IsNullOrWhiteSpace(x.ErpDocumentNumber.Trim()))
                            .GroupBy(x => x.ErpDocumentNumber.Trim())
                            .Select(g => g.Last());

                        totalNotSyncedSoFar += response.Data.Count - responseData.Count();

                        foreach (var erpInvoice in responseData)
                        {
                            var oldErpInvoiceByThisAccount = erpInvoices.FirstOrDefault(inv => inv.ErpDocumentNumber == erpInvoice.ErpDocumentNumber);

                            if (oldErpInvoiceByThisAccount == null)
                            {
                                oldErpInvoiceByThisAccount = new ErpInvoice();
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
                                oldErpInvoiceByThisAccount.ItemCount = erpInvoice?.Items?.Count ?? 0;
                                oldErpInvoiceByThisAccount.DueDateUtc = erpInvoice?.DueDateUtc ?? DateTime.UtcNow;

                                if (Enum.TryParse(erpInvoice?.DocumentType, out ErpDocumentType parsedDocumentType))
                                {
                                    oldErpInvoiceByThisAccount.DocumentType = parsedDocumentType;
                                }
                                oldErpInvoiceByThisAccount.DocumentDisplayName = parsedDocumentType.ToString();

                                if (await IsvalidErpInvoiceAsync(oldErpInvoiceByThisAccount))
                                {
                                    erpInvoiceInsertList.Add(oldErpInvoiceByThisAccount);
                                }
                                else
                                    totalNotSyncedSoFar++;
                            }
                            else
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
                                oldErpInvoiceByThisAccount.ItemCount = erpInvoice?.Items?.Count ?? 0;
                                oldErpInvoiceByThisAccount.DueDateUtc = erpInvoice?.DueDateUtc ?? DateTime.UtcNow;

                                if (Enum.TryParse(erpInvoice?.DocumentType, out ErpDocumentType parsedDocumentType))
                                {
                                    oldErpInvoiceByThisAccount.DocumentType = parsedDocumentType;
                                }
                                oldErpInvoiceByThisAccount.DocumentDisplayName = parsedDocumentType.ToString();

                                if (await IsvalidErpInvoiceAsync(oldErpInvoiceByThisAccount))
                                {
                                    erpInvoiceUpdateList.Add(oldErpInvoiceByThisAccount);
                                }
                                else
                                    totalNotSyncedSoFar++;
                            }
                        }

                        if (erpInvoiceInsertList.Count != 0)
                        {
                            await _erpInvoiceService.InsertErpInvoicesAsync(erpInvoiceInsertList);
                            lastErpInvoiceSynced = erpInvoiceInsertList.LastOrDefault()?.ErpDocumentNumber;
                            totalSyncedSoFar += erpInvoiceInsertList.Count;
                            erpInvoiceInsertList.Clear();
                        }

                        if (erpInvoiceUpdateList.Count != 0)
                        {
                            await _erpInvoiceService.UpdateErpInvoicesAsync(erpInvoiceUpdateList);
                            lastErpInvoiceSynced = erpInvoiceUpdateList.LastOrDefault()?.ErpDocumentNumber;
                            totalSyncedSoFar += erpInvoiceUpdateList.Count;
                            await _erpDataClearCacheService.ClearCacheOfEntities(erpInvoiceUpdateList);
                            erpInvoiceUpdateList.Clear();
                        }

                        if (cancellationToken.IsCancellationRequested)
                        {
                            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                                ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
                                ErpSyncLevel.Invoice,
                                "The Erp Invoice Sync run is cancelled. " +
                                (!string.IsNullOrWhiteSpace(lastErpInvoiceSynced) ?
                                $"The last synced Erp Invoice: {lastErpInvoiceSynced}, of Erp Account: {lastErpInvoiceSyncedOfErpAccount} for Sales Org: ({salesOrg.Code}) {salesOrg.Name}. " : string.Empty) +
                                $"Total invoices synced in this session: {totalSyncedSoFar} " +
                                $"And total not synced due to invalid data: {totalNotSyncedSoFar}");

                            return false;
                        }
                    }

                    erpAccount.LastTimeOrderSyncOnUtc = DateTime.UtcNow;
                    await _erpAccountService.UpdateErpAccountAsync(erpAccount);
                }

                if (!isError)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
                        ErpSyncLevel.Invoice,
                        $"Erp Invoice sync successful for Sales Org: ({salesOrg.Code}) {salesOrg.Name}");
                }
                else
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
                        ErpSyncLevel.Invoice,
                        $"Erp Invoice sync is partially or not successful for Sales Org: ({salesOrg.Code}) {salesOrg.Name}",
                        lastErrorMessage);
                }

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
                    ErpSyncLevel.Invoice,
                    (!string.IsNullOrWhiteSpace(lastErpInvoiceSynced) ?
                    $"The last synced Erp Invoice: {lastErpInvoiceSynced}, of Erp Account: {lastErpInvoiceSyncedOfErpAccount} for Sales Org: ({salesOrg.Code}) {salesOrg.Name}. " : string.Empty) +
                    $"Total synced in this session: {totalSyncedSoFar} " +
                    $"And total not synced due to invalid data: {totalNotSyncedSoFar}");
            }

            if (!string.IsNullOrWhiteSpace(erpAccountNumber) && !specificErpAccountSalesOrgFound)
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
                    ErpSyncLevel.Invoice,
                    $"No Sales org found for the Erp Account : {erpAccountNumber} to sync Invoices.");
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
                ex.StackTrace ?? string.Empty);

            await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                DateTime.UtcNow,
                ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
                ex.Message + "\n\n" + ex.StackTrace);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
                ErpSyncLevel.Invoice,
                "Erp Invoice Sync ended.");

            return false;
        }
    }

    #endregion
}