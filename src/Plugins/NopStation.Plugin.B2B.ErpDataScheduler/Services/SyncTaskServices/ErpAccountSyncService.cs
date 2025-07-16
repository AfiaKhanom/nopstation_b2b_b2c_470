using FluentValidation;
using Nop.Core.Domain.Common;
using Nop.Services.Common;
using Nop.Services.Directory;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncWorkflowMessage;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.ERPIntegrationCore.Validators.Helpers;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public class ErpAccountSyncService : IErpAccountSyncService
{
    #region Fields

    private readonly IAddressService _addressService;
    private readonly ICountryService _countryService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly ISyncLogService _erpSyncLogService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpGroupPriceCodeService _erpGroupPriceCodeService;
    private readonly IErpDataClearCacheService _erpDataClearCacheService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginManager;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private const string HIDE_STOCK_VALUES = "HideStockValues";
    private const int ALLOWED_STOCK_PERCENTAGE = 100;
    private readonly IValidator<ErpAccount> _validator;
    private readonly ISyncWorkflowMessageService _syncWorkflowMessageService;

    #endregion

    #region Ctor

    public ErpAccountSyncService(IAddressService addressService,
        ICountryService countryService,
        IStateProvinceService stateProvinceService,
        ISyncLogService erpSyncLogService,
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService,
        IErpGroupPriceCodeService erpGroupPriceCodeService,
        IErpDataClearCacheService erpDataClearCacheService,
        IErpIntegrationPluginManager erpIntegrationPluginService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IValidator<ErpAccount> validator,
        ISyncWorkflowMessageService syncWorkflowMessageService)
    {
        _addressService = addressService;
        _countryService = countryService;
        _stateProvinceService = stateProvinceService;
        _erpSyncLogService = erpSyncLogService;
        _erpAccountService = erpAccountService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpGroupPriceCodeService = erpGroupPriceCodeService;
        _erpDataClearCacheService = erpDataClearCacheService;
        _erpIntegrationPluginManager = erpIntegrationPluginService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _validator = validator;
        _syncWorkflowMessageService = syncWorkflowMessageService;
    }

    #endregion

    #region Utilities

    private async Task<bool> IsValidErpAccountAsync(ErpAccount erpAccount)
    {
        if (erpAccount is null)
            return false;

        var validationResult = await _validator.ValidateAsync(erpAccount);

        if (!validationResult.IsValid)
        {
            var errorMessages = ErpDataValidationHelper.PrepareValidationLog(validationResult);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                ErpSyncLevel.Account,
                $"Data mapping skipped for {nameof(ErpAccount)}, {nameof(ErpAccount.AccountNumber)}: {erpAccount.AccountNumber}. \r\n {errorMessages}");
        }

        return validationResult.IsValid;
    }

    #endregion

    #region Method

    public virtual async Task<bool> IsErpAccountSyncSuccessfulAsync(string? erpAccountNumber, bool isManualTrigger = false, bool isIncrementalSync = true, CancellationToken cancellationToken = default)
    {
        var erpIntegrationPlugin = await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();

        if (erpIntegrationPlugin is null)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                ErpSyncLevel.Account,
                $"No integration method found. Unable to run {ErpDataSchedulerDefaults.ErpAccountSyncTaskName}.");

            return false;
        }

        try
        {
            #region Data collections

            var erpAccountUpdateList = new List<ErpAccount>();
            var erpAccountInsertList = new List<ErpAccount>();

            var salesOrgs = await _erpSalesOrgService.GetAllErpSalesOrgsAsync();
            if (!salesOrgs.Any())
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                    ErpSyncLevel.Account,
                    $"No Sales org found. Unable to run {ErpDataSchedulerDefaults.ErpAccountSyncTaskName}.");

                return false;
            }

            var allCountries = (await _countryService.GetAllCountriesAsync()).ToList();
            var allStateProvinces = (await _stateProvinceService.GetStateProvincesAsync()).ToList();
            var countryId = 0;
            var stateProvinceId = 0;
            //var syncStartTime = DateTime.UtcNow.AddMinutes(-10);

            #endregion

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                ErpSyncLevel.Account,
                "Erp Account Sync started.");

            foreach (var salesOrg in salesOrgs)
            {
                var oldErpAccounts = await _erpAccountService.GetErpAccountListAsync(accountNumber: erpAccountNumber, salesOrgId: salesOrg.Id, filterDeleted: false);
                var isError = false;
                var start = "0";
                var lastSyncedErpAccountNumber = string.Empty;
                var totalSyncedSoFar = 0;
                var totalNotSyncedSoFar = 0;

                while (true)
                {
                    var erpGetRequestModel = new ErpGetRequestModel
                    {
                        Start = start,
                        DateFrom = isIncrementalSync ? salesOrg.LastErpAccountSyncTimeOnUtc : null,
                        AccountNumber = erpAccountNumber,
                        Location = salesOrg.Code
                    };

                    var response = await erpIntegrationPlugin.GetAccountsFromErpAsync(erpGetRequestModel);

                    if (response.ErpResponseModel.IsError)
                    {
                        isError = true;

                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                            ErpSyncLevel.Account,
                            response.ErpResponseModel.ErrorShortMessage,
                            response.ErpResponseModel.ErrorFullMessage);

                        await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                            DateTime.UtcNow,
                            ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
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
                        .Where(x => !string.IsNullOrWhiteSpace(x.AccountNumber.Trim()))
                        .GroupBy(x => x.AccountNumber.Trim())
                        .Select(g => g.Last());

                    totalNotSyncedSoFar += response.Data.Count - responseData.Count();

                    foreach (var erpAccount in responseData)
                    {
                        var oldErpAccount = oldErpAccounts.FirstOrDefault(x => x.AccountNumber == erpAccount.AccountNumber);

                        var address = await _addressService.GetAddressByIdAsync(oldErpAccount?.BillingAddressId ?? 0);

                        countryId = allCountries.FirstOrDefault(x =>
                                !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(erpAccount.Country)
                                || !string.IsNullOrWhiteSpace(x.TwoLetterIsoCode) && x.TwoLetterIsoCode.Equals(erpAccount.Country)
                                || !string.IsNullOrWhiteSpace(x.ThreeLetterIsoCode) && x.ThreeLetterIsoCode.Equals(erpAccount.Country))?.Id
                                ?? _b2BB2CFeaturesSettings.DefaultCountryId;

                        stateProvinceId = allStateProvinces.FirstOrDefault(x => x.CountryId == countryId
                            && (!string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(erpAccount.StateProvince) ||
                            !string.IsNullOrWhiteSpace(x.Abbreviation) && x.Abbreviation.Equals(erpAccount.StateProvince)))?.Id ?? 0;

                        if (address is null)
                        {
                            address = new Address();
                            address.FirstName = erpAccount.BillingName;
                            address.Email = erpAccount.Email;
                            address.Company = erpAccount.CompanyNo;
                            address.CountryId = countryId;
                            address.City = erpAccount.City;
                            address.County = erpAccount.Address3;
                            address.Address1 = erpAccount.Address1;
                            address.Address2 = erpAccount.Address2;
                            address.ZipPostalCode = erpAccount.ZipPostalCode;
                            address.StateProvinceId = stateProvinceId;
                            address.PhoneNumber = erpAccount.PhoneNumber;
                            address.FaxNumber = string.Empty;
                            address.CreatedOnUtc = DateTime.UtcNow;

                            await _addressService.InsertAddressAsync(address);
                        }
                        else
                        {
                            address.FirstName = erpAccount.BillingName;
                            address.Email = erpAccount.Email;
                            address.Company = erpAccount.CompanyNo;
                            address.CountryId = countryId;
                            address.City = erpAccount.City;
                            address.County = erpAccount.Address3;
                            address.Address1 = erpAccount.Address1;
                            address.Address2 = erpAccount.Address2;
                            address.ZipPostalCode = erpAccount.ZipPostalCode;
                            address.StateProvinceId = stateProvinceId;
                            address.PhoneNumber = erpAccount.PhoneNumber;
                            address.FaxNumber = string.Empty;

                            await _addressService.UpdateAddressAsync(address);
                        }

                        if (oldErpAccount is null)
                        {
                            oldErpAccount = new ErpAccount();
                            oldErpAccount.ErpSalesOrgId = salesOrg.Id;

                            oldErpAccount.AccountNumber = erpAccount.AccountNumber;
                            oldErpAccount.AccountName = erpAccount.AccountName;
                            oldErpAccount.IsActive = erpAccount.IsActive;
                            oldErpAccount.VatNumber = erpAccount.VatNumber;
                            oldErpAccount.PreFilterFacets = erpAccount.PreFilterFacets;
                            oldErpAccount.PaymentTypeCode = erpAccount.PaymentTypeCode;
                            oldErpAccount.BillingAddressId = address.Id;
                            oldErpAccount.BillingSuburb = address.Address1;

                            oldErpAccount.AllowOverspend = erpAccount.AllowOverspend ? erpAccount.AllowOverspend : _b2BB2CFeaturesSettings.AllowOverspend;
                            oldErpAccount.AllowAccountsAddressEditOnCheckout = _b2BB2CFeaturesSettings.AllowAddressEditOnCheckoutForAll;
                            oldErpAccount.B2BPriceGroupCodeId = (await _erpGroupPriceCodeService.GetErpGroupPriceCodeByCodeAsync(erpAccount.PriceGroupCode))?.Id ?? 0;

                            oldErpAccount.CreditLimitAvailable = erpAccount.CreditLimitAvailable ?? 0;
                            oldErpAccount.CreditLimit = erpAccount.CreditLimit ?? 0;
                            oldErpAccount.CurrentBalance = erpAccount.CurrentBalance ?? 0;

                            var hideStockValues = erpAccount.ErpAccountAttributes?.Exists(kvp =>
                                    HIDE_STOCK_VALUES.Equals(kvp.Key, StringComparison.InvariantCultureIgnoreCase)
                                    && bool.TryParse(kvp.Value, out var value)
                                    && value) ?? false;

                            oldErpAccount.OverrideStockDisplayFormatConfigSetting = false;
                            if (hideStockValues)
                            {
                                oldErpAccount.StockDisplayFormatTypeId = (int)StockDisplayFormat.ShowInOrOutOfStockIndicators;
                            }
                            else
                            {
                                oldErpAccount.StockDisplayFormatTypeId = (int)StockDisplayFormat.ShowStockQuantities;
                            }

                            oldErpAccount.ErpAccountStatusTypeId = (int)ErpAccountStatusType.Normal;
                            oldErpAccount.PercentageOfStockAllowed = erpAccount.PercentageOfStockAllowed ?? ALLOWED_STOCK_PERCENTAGE;

                            if (oldErpAccount.PercentageOfStockAllowed <= 0)
                            {
                                oldErpAccount.PercentageOfStockAllowed = ALLOWED_STOCK_PERCENTAGE;
                            }

                            oldErpAccount.IsDeleted = erpAccount.IsDeleted;

                            oldErpAccount.CreatedById = 1;
                            oldErpAccount.CreatedOnUtc = DateTime.UtcNow;
                            oldErpAccount.UpdatedById = 1;
                            oldErpAccount.UpdatedOnUtc = DateTime.UtcNow;
                            oldErpAccount.LastErpAccountSyncDate = DateTime.UtcNow;

                            if (!await IsValidErpAccountAsync(oldErpAccount))
                            {
                                totalNotSyncedSoFar++;
                                continue;
                            }

                            erpAccountInsertList.Add(oldErpAccount);
                        }
                        else
                        {
                            oldErpAccount.AccountName = erpAccount.AccountName;
                            oldErpAccount.IsActive = erpAccount.IsActive;
                            oldErpAccount.VatNumber = erpAccount.VatNumber;
                            oldErpAccount.PreFilterFacets = erpAccount.PreFilterFacets;
                            oldErpAccount.PaymentTypeCode = erpAccount.PaymentTypeCode;
                            oldErpAccount.BillingAddressId = address.Id;
                            oldErpAccount.BillingSuburb = address.Address1;

                            oldErpAccount.AllowOverspend = erpAccount.AllowOverspend ? erpAccount.AllowOverspend : _b2BB2CFeaturesSettings.AllowOverspend;
                            oldErpAccount.AllowAccountsAddressEditOnCheckout = _b2BB2CFeaturesSettings.AllowAddressEditOnCheckoutForAll;
                            oldErpAccount.B2BPriceGroupCodeId = (await _erpGroupPriceCodeService.GetErpGroupPriceCodeByCodeAsync(erpAccount.PriceGroupCode))?.Id ?? 0;

                            oldErpAccount.CreditLimitAvailable = erpAccount.CreditLimitAvailable ?? 0;
                            oldErpAccount.CreditLimit = erpAccount.CreditLimit ?? 0;
                            oldErpAccount.CurrentBalance = erpAccount.CurrentBalance ?? 0;

                            var hideStockValues = erpAccount.ErpAccountAttributes?.Exists(kvp =>
                                    HIDE_STOCK_VALUES.Equals(kvp.Key, StringComparison.InvariantCultureIgnoreCase)
                                    && bool.TryParse(kvp.Value, out var value)
                                    && value) ?? false;

                            oldErpAccount.OverrideStockDisplayFormatConfigSetting = false;
                            if (hideStockValues)
                            {
                                oldErpAccount.StockDisplayFormatTypeId = (int)StockDisplayFormat.ShowInOrOutOfStockIndicators;
                            }
                            else
                            {
                                oldErpAccount.StockDisplayFormatTypeId = (int)StockDisplayFormat.ShowStockQuantities;
                            }

                            oldErpAccount.ErpAccountStatusTypeId = (int)ErpAccountStatusType.Normal;
                            oldErpAccount.PercentageOfStockAllowed = erpAccount.PercentageOfStockAllowed ?? ALLOWED_STOCK_PERCENTAGE;

                            if (oldErpAccount.PercentageOfStockAllowed <= 0)
                            {
                                oldErpAccount.PercentageOfStockAllowed = ALLOWED_STOCK_PERCENTAGE;
                            }

                            oldErpAccount.IsDeleted = erpAccount.IsDeleted;

                            oldErpAccount.UpdatedById = 1;
                            oldErpAccount.UpdatedOnUtc = DateTime.UtcNow;
                            oldErpAccount.LastErpAccountSyncDate = DateTime.UtcNow;

                            if (!await IsValidErpAccountAsync(oldErpAccount))
                            {
                                totalNotSyncedSoFar++;
                                continue;
                            }

                            erpAccountUpdateList.Add(oldErpAccount);
                        }
                    }

                    if (erpAccountInsertList.Count != 0)
                    {
                        await _erpAccountService.InsertErpAccountsAsync(erpAccountInsertList);
                        totalSyncedSoFar += erpAccountInsertList.Count;
                        lastSyncedErpAccountNumber = erpAccountInsertList.LastOrDefault()?.AccountNumber;
                        erpAccountInsertList.Clear();
                    }

                    if (erpAccountUpdateList.Count != 0)
                    {
                        await _erpAccountService.UpdateErpAccountsAsync(erpAccountUpdateList);
                        totalSyncedSoFar += erpAccountUpdateList.Count;
                        lastSyncedErpAccountNumber = erpAccountUpdateList.LastOrDefault()?.AccountNumber;
                        await _erpDataClearCacheService.ClearCacheOfEntities(erpAccountUpdateList);
                        erpAccountUpdateList.Clear();
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                            ErpSyncLevel.Account,
                            "The Erp Account Sync run is cancelled. " +
                            (!string.IsNullOrWhiteSpace(lastSyncedErpAccountNumber) ?
                            $"The last synced Erp Account: {lastSyncedErpAccountNumber}, for Sales Org: ({salesOrg.Code}) {salesOrg.Name}. " : string.Empty) +
                            $"Total erp accounts synced in this session: {totalSyncedSoFar} " +
                            $"And total not synced due to invalid data: {totalNotSyncedSoFar}");

                        return false;
                    }
                }

                if (!isError)
                {
                    //await _erpAccountService.InActiveAllOldAccount(syncStartTime);
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                        ErpSyncLevel.Account,
                        $"Erp Accounts sync is successful for Sales Org: ({salesOrg.Code}) {salesOrg.Name}. "
                        /*+ $"The accounts which were updated before {syncStartTime} are deactivated."*/);
                }
                else
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                        ErpSyncLevel.Account,
                        $"Erp Accounts sync is partially or not successful for Sales Org: ({salesOrg.Code}) {salesOrg.Name}");
                }

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                    ErpSyncLevel.Account,
                    (!string.IsNullOrWhiteSpace(lastSyncedErpAccountNumber) ?
                    $"The last synced Erp Account: {lastSyncedErpAccountNumber}, for Sales Org: ({salesOrg.Code}) {salesOrg.Name}. " : string.Empty) +
                    $"Total synced in this session: {totalSyncedSoFar} " +
                    $"And total not synced due to invalid data: {totalNotSyncedSoFar}");

                salesOrg.LastErpAccountSyncTimeOnUtc = DateTime.UtcNow;
                await _erpSalesOrgService.UpdateErpSalesOrgAsync(salesOrg);
            }

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                ErpSyncLevel.Account,
                "Erp Account Sync ended.");

            return true;
        }
        catch (Exception ex)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                ErpSyncLevel.Account,
                ex.Message,
                ex.StackTrace ?? string.Empty);

            await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                DateTime.UtcNow,
                ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                ex.Message + "\n\n" + ex.StackTrace);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                ErpSyncLevel.Account,
                "Erp Account Sync ended.");

            return false;
        }
    }

    #endregion
}