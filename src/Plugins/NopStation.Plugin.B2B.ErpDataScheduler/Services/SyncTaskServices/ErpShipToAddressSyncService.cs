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

public class ErpShipToAddressSyncService : IErpShipToAddressSyncService
{
    #region Fields

    private readonly IAddressService _addressService;
    private readonly ICountryService _countryService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly ISyncLogService _erpSyncLogService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginService;
    private readonly IValidator<ErpShipToAddress> _erpShipToAddressValidator;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly ISyncWorkflowMessageService _syncWorkflowMessageService;

    #endregion

    #region Ctor

    public ErpShipToAddressSyncService(IAddressService addressService,
        ICountryService countryService,
        IStateProvinceService stateProvinceService,
        ISyncLogService erpSyncLogService,
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService,
        IErpShipToAddressService erpShipToAddressService,
        IErpIntegrationPluginManager erpIntegrationPluginService,
        IValidator<ErpShipToAddress> erpShipToAddressValidator,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        ISyncWorkflowMessageService syncWorkflowMessageService)
    {
        _addressService = addressService;
        _countryService = countryService;
        _stateProvinceService = stateProvinceService;
        _erpSyncLogService = erpSyncLogService;
        _erpAccountService = erpAccountService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpShipToAddressService = erpShipToAddressService;
        _erpIntegrationPluginService = erpIntegrationPluginService;
        _erpShipToAddressValidator = erpShipToAddressValidator;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _syncWorkflowMessageService = syncWorkflowMessageService;
    }

    #endregion

    #region Utilities

    private async Task<bool> IsvalidErpShipToAddressAsync(ErpShipToAddress erpShipToAddress)
    {
        if (erpShipToAddress is null)
            return false;

        var validationResult = await _erpShipToAddressValidator.ValidateAsync(erpShipToAddress);

        if (!validationResult.IsValid)
        {
            var errorMessages = ErpDataValidationHelper.PrepareValidationLog(validationResult);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                ErpSyncLevel.ShipToAddress,
                $"Data mapping skipped for {nameof(ErpShipToAddress)}, {nameof(ErpShipToAddress.ShipToCode)}: {erpShipToAddress.ShipToCode}.\r\n {errorMessages}");
        }

        return validationResult.IsValid;
    }

    #endregion

    #region Method

    public virtual async Task<bool> IsErpShipToAddressSyncSuccessfulAsync(string? erpAccountNumber, bool isManualTrigger = false, bool isIncrementalSync = true, CancellationToken cancellationToken = default)
    {
        var erpIntegrationPlugin = await _erpIntegrationPluginService.LoadActiveERPIntegrationPlugin();

        if (erpIntegrationPlugin is null)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                ErpSyncLevel.ShipToAddress,
                $"No integration method found. Unable to run {ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName}.");

            return false;
        }

        try
        {
            #region Data collection

            var salesOrgs = await _erpSalesOrgService.GetAllErpSalesOrgsAsync();
            if (!salesOrgs.Any())
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                    ErpSyncLevel.ShipToAddress,
                    $"No Sales org found. Unable to run {ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName}.");

                return false;
            }

            IList<ErpAccount> specificErpAccounts = null;
            var specificErpAccountSalesOrgFound = false;
            if (!string.IsNullOrWhiteSpace(erpAccountNumber))
            {
                specificErpAccounts = await _erpAccountService.GetErpAccountListAsync(accountNumber: erpAccountNumber);

                if (specificErpAccounts == null || specificErpAccounts != null && specificErpAccounts.Count == 0)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                        ErpSyncLevel.ShipToAddress,
                        $"No Erp Account found with Account Number: {erpAccountNumber}");

                    return false;
                }
            }

            var allCountries = await _countryService.GetAllCountriesAsync();
            var allStateProvinces = await _stateProvinceService.GetStateProvincesAsync();

            var erpShipToAddressUpdateList = new Dictionary<int, List<ErpShipToAddress>>();
            var erpShipToAddressInsertList = new Dictionary<int, List<ErpShipToAddress>>();
            var erpShiptoAddressErpAccountMapInsertList = new List<ErpShiptoAddressErpAccountMap>();

            #endregion

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                ErpSyncLevel.ShipToAddress,
                "Erp ShipToAddress Sync started.");

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
                    oldErpAccounts = await _erpAccountService.GetErpAccountListAsync(salesOrgId: salesOrg.Id, showHidden: false);               
                }

                if (oldErpAccounts.Count == 0)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                        ErpSyncLevel.ShipToAddress,
                        $"No Erp Accounts found with the Sales org : {salesOrg.Name}");

                    if (specificErpAccounts != null)
                        return false;

                    continue;
                }

                var erpAccountShipToAddressesMap = await _erpShipToAddressService
                    .GetErpAccountShipToAddressMappingAsync(oldErpAccounts.Select(x => x.Id).ToArray());

                var isError = false;
                var start = "0";
                var countryId = 0;
                var stateProvinceId = 0;
                var totalSyncedSoFar = 0;
                var totalNotSyncedSoFar = 0;
                var lastErrorMessage = "";
                var lastSyncedErpShipToAddressShipToCode = string.Empty;

                while (true)
                {
                    var erpGetRequestModel = new ErpGetRequestModel
                    {
                        Start = start,
                        Location = salesOrg.Code,
                        AccountNumber = erpAccountNumber,
                        DateFrom = isIncrementalSync ? salesOrg.LastErpShipToAddressSyncTimeOnUtc : null,
                        
                    };

                    var response = await erpIntegrationPlugin.GetShipToAddressesFromErpAsync(erpGetRequestModel);

                    if (response.ErpResponseModel.IsError)
                    {
                        isError = true;
                        lastErrorMessage = $"The last error: {response.ErpResponseModel.ErrorShortMessage}";

                        await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                            DateTime.UtcNow,
                            ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
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
                        .Where(x => !string.IsNullOrWhiteSpace(x.ShipToCode.Trim()) && !string.IsNullOrWhiteSpace(x.AccountNumber.Trim()))
                        .GroupBy(x => new { ShipToCode = x.ShipToCode.Trim(), ErpAccountNumber = x.AccountNumber.Trim() })
                        .Select(g => g.Last())
                        .DistinctBy(x => new { ShipToCode = x.ShipToCode.Trim(), ErpAccountNumber = x.AccountNumber.Trim() });

                    totalNotSyncedSoFar += response.Data.Count - responseData.Count();

                    foreach (var erpShipToAddress in responseData)
                    {
                        ErpShipToAddress? oldShipToAddressByThisAccount = null;
                        Address address = null;

                        var erpAccount = oldErpAccounts.FirstOrDefault(x => x.AccountNumber == erpShipToAddress.AccountNumber.Trim());

                        if (erpAccount == null)
                        {
                            totalNotSyncedSoFar++;
                            continue;
                        }

                        var matchingErpAccountShipToAddress = erpAccountShipToAddressesMap
                            .FirstOrDefault(map => map.Key.Trim() == erpAccount.AccountNumber);

                        if (matchingErpAccountShipToAddress.Key != null && 
                            matchingErpAccountShipToAddress.Value.Exists(addr => addr.ShipToCode.Trim() == erpShipToAddress.ShipToCode.Trim()))
                        {
                            oldShipToAddressByThisAccount = matchingErpAccountShipToAddress.Value
                                .Find(addr => addr.ShipToCode.Trim() == erpShipToAddress.ShipToCode.Trim());

                            address = await _addressService.GetAddressByIdAsync(oldShipToAddressByThisAccount.AddressId);
                        }

                        countryId = allCountries.FirstOrDefault(x =>
                            !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(erpShipToAddress.Country)
                            || !string.IsNullOrWhiteSpace(x.TwoLetterIsoCode) && x.TwoLetterIsoCode.Equals(erpShipToAddress.Country)
                            || !string.IsNullOrWhiteSpace(x.ThreeLetterIsoCode) && x.ThreeLetterIsoCode.Equals(erpShipToAddress.Country))?.Id
                            ?? _b2BB2CFeaturesSettings.DefaultCountryId;

                        stateProvinceId = allStateProvinces.FirstOrDefault(x => x.CountryId == countryId
                            && (!string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(erpShipToAddress.StateProvince) ||
                            !string.IsNullOrWhiteSpace(x.Abbreviation) && x.Abbreviation.Equals(erpShipToAddress.StateProvince)))?.Id ?? 0;

                        if (address is null)
                        {
                            address = new Address();
                            address.FirstName = erpShipToAddress.ShipToName;
                            address.Email = erpShipToAddress.EmailAddress.Split(";").FirstOrDefault()?.Trim();
                            address.Company = erpShipToAddress.Company;
                            address.CountryId = countryId;
                            address.City = erpShipToAddress.City;
                            address.County = erpShipToAddress.County;
                            address.Address1 = erpShipToAddress.Address1;
                            address.Address2 = erpShipToAddress.Address2;
                            address.ZipPostalCode = erpShipToAddress.ZipPostalCode;
                            address.StateProvinceId = stateProvinceId;
                            address.PhoneNumber = erpShipToAddress.PhoneNumber;
                            address.FaxNumber = string.Empty;

                            address.CreatedOnUtc = DateTime.UtcNow;
                            await _addressService.InsertAddressAsync(address);
                        }
                        else
                        {
                            address.FirstName = erpShipToAddress.ShipToName;
                            address.Email = erpShipToAddress.EmailAddress.Split(";").FirstOrDefault()?.Trim();
                            address.Company = erpShipToAddress.Company;
                            address.CountryId = countryId;
                            address.City = erpShipToAddress.City;
                            address.County = erpShipToAddress.County;
                            address.Address1 = erpShipToAddress.Address1;
                            address.Address2 = erpShipToAddress.Address2;
                            address.ZipPostalCode = erpShipToAddress.ZipPostalCode;
                            address.StateProvinceId = stateProvinceId;
                            address.PhoneNumber = erpShipToAddress.PhoneNumber;
                            address.FaxNumber = string.Empty;

                            await _addressService.UpdateAddressAsync(address);
                        }

                            if (oldShipToAddressByThisAccount is null)
                            {
                                oldShipToAddressByThisAccount = new ErpShipToAddress();
                                oldShipToAddressByThisAccount.ShipToCode = erpShipToAddress.ShipToCode.Trim();
                                oldShipToAddressByThisAccount.ShipToName = erpShipToAddress.ShipToName.Trim();
                                oldShipToAddressByThisAccount.Suburb = erpShipToAddress.Suburb;
                                oldShipToAddressByThisAccount.ProvinceCode = erpShipToAddress.StateProvince;
                                oldShipToAddressByThisAccount.DeliveryNotes = erpShipToAddress.DeliveryNotes;
                                oldShipToAddressByThisAccount.EmailAddresses = erpShipToAddress.EmailAddress;
                                oldShipToAddressByThisAccount.RepNumber = erpShipToAddress.RepNumber;
                                oldShipToAddressByThisAccount.RepPhoneNumber = erpShipToAddress.RepPhoneNumber;
                                oldShipToAddressByThisAccount.RepEmail = erpShipToAddress.RepEmail;
                                oldShipToAddressByThisAccount.RepFullName = erpShipToAddress.RepFullName;
                                oldShipToAddressByThisAccount.AddressId = address.Id;
                                oldShipToAddressByThisAccount.IsActive = erpAccount.IsActive;
                                oldShipToAddressByThisAccount.CreatedOnUtc = DateTime.UtcNow;
                                oldShipToAddressByThisAccount.CreatedById = 1;
                                oldShipToAddressByThisAccount.UpdatedOnUtc = DateTime.UtcNow;
                                oldShipToAddressByThisAccount.UpdatedById = 1;
                                oldShipToAddressByThisAccount.LastShipToAddressSyncDate = DateTime.UtcNow;

                            if (await IsvalidErpShipToAddressAsync(oldShipToAddressByThisAccount))
                            {
                                if (!erpShipToAddressInsertList.ContainsKey(erpAccount.Id))
                                    erpShipToAddressInsertList.Add(erpAccount.Id, [oldShipToAddressByThisAccount]);
                                else
                                    erpShipToAddressInsertList[erpAccount.Id].Add(oldShipToAddressByThisAccount);

                                lastSyncedErpShipToAddressShipToCode = oldShipToAddressByThisAccount.ShipToCode;
                            }
                            else
                                totalNotSyncedSoFar++;
                        }
                        else
                        {
                            oldShipToAddressByThisAccount.ShipToCode = erpShipToAddress.ShipToCode.Trim();
                            oldShipToAddressByThisAccount.ShipToName = erpShipToAddress.ShipToName.Trim();
                            oldShipToAddressByThisAccount.Suburb = erpShipToAddress.Suburb;
                            oldShipToAddressByThisAccount.ProvinceCode = erpShipToAddress.StateProvince;
                            oldShipToAddressByThisAccount.DeliveryNotes = erpShipToAddress.DeliveryNotes;
                            oldShipToAddressByThisAccount.EmailAddresses = erpShipToAddress.EmailAddress;
                            oldShipToAddressByThisAccount.RepNumber = erpShipToAddress.RepNumber;
                            oldShipToAddressByThisAccount.RepPhoneNumber = erpShipToAddress.RepPhoneNumber;
                            oldShipToAddressByThisAccount.RepEmail = erpShipToAddress.RepEmail;
                            oldShipToAddressByThisAccount.RepFullName = erpShipToAddress.RepFullName;
                            oldShipToAddressByThisAccount.AddressId = address.Id;
                            oldShipToAddressByThisAccount.IsActive = erpAccount.IsActive;
                            oldShipToAddressByThisAccount.UpdatedOnUtc = DateTime.UtcNow;
                            oldShipToAddressByThisAccount.UpdatedById = 1;
                            oldShipToAddressByThisAccount.LastShipToAddressSyncDate = DateTime.UtcNow;

                            if (await IsvalidErpShipToAddressAsync(oldShipToAddressByThisAccount))
                            {
                                if (!erpShipToAddressUpdateList.ContainsKey(erpAccount.Id))
                                    erpShipToAddressUpdateList.Add(erpAccount.Id, [oldShipToAddressByThisAccount]);
                                else
                                    erpShipToAddressUpdateList[erpAccount.Id].Add(oldShipToAddressByThisAccount);

                                if (await _erpShipToAddressService.GetErpShipToAddressErpAccountMapByErpShipToAddressIdAsync(oldShipToAddressByThisAccount.Id) == null)
                                {
                                    erpShiptoAddressErpAccountMapInsertList.Add(new ErpShiptoAddressErpAccountMap
                                    {
                                        ErpAccountId = erpAccount.Id,
                                        ErpShiptoAddressId = oldShipToAddressByThisAccount.Id,
                                        ErpShipToAddressCreatedByTypeId = (int)ErpShipToAddressCreatedByType.Admin,
                                    });
                                }
                                lastSyncedErpShipToAddressShipToCode = oldShipToAddressByThisAccount.ShipToCode;
                            }
                            else
                                totalNotSyncedSoFar++;
                        }                        
                    }

                    if (erpShipToAddressInsertList.Count != 0)
                    {
                        await _erpShipToAddressService.InsertErpShipToAddressesAsync(
                            erpShipToAddressInsertList
                            .SelectMany(kvp => kvp.Value)
                            .ToList());

                        totalSyncedSoFar += erpShipToAddressInsertList.SelectMany(kvp => kvp.Value).Count();

                        foreach (var map in erpShipToAddressInsertList)
                        {
                            foreach (var shipToAddress in map.Value)
                            {
                                erpShiptoAddressErpAccountMapInsertList.Add(new ErpShiptoAddressErpAccountMap
                                {
                                    ErpAccountId = map.Key,
                                    ErpShiptoAddressId = shipToAddress.Id,
                                    ErpShipToAddressCreatedByTypeId = (int)ErpShipToAddressCreatedByType.Admin
                                });
                            }
                        }
                        erpShipToAddressInsertList.Clear();
                    }

                    if (erpShipToAddressUpdateList.Count != 0)
                    {
                        await _erpShipToAddressService.UpdateErpShipToAddressesAsync(
                            erpShipToAddressUpdateList
                            .SelectMany(kvp => kvp.Value)
                            .ToList());

                        totalSyncedSoFar += erpShipToAddressUpdateList.SelectMany(kvp => kvp.Value).Count();

                        erpShipToAddressUpdateList.Clear();
                    }

                    if (erpShiptoAddressErpAccountMapInsertList.Count != 0)
                    {
                        await _erpShipToAddressService.InsertErpShipToAddressErpAccountMapsAsync(erpShiptoAddressErpAccountMapInsertList);
                        erpShiptoAddressErpAccountMapInsertList.Clear();
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                            ErpSyncLevel.ShipToAddress,
                            "The Erp Ship To Address Sync run is cancelled. " +
                            (!string.IsNullOrWhiteSpace(lastSyncedErpShipToAddressShipToCode) ?
                            $"The last synced Erp Ship To Address: {lastSyncedErpShipToAddressShipToCode}, for Sales Org: ({salesOrg.Code}) {salesOrg.Name}. " : string.Empty) +
                            $"Total synced in this session: {totalSyncedSoFar}" +
                            $"And total not synced due to invalid data: {totalNotSyncedSoFar}");

                        return false;
                    }
                }
                
                if (!isError)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                        ErpSyncLevel.ShipToAddress,
                        $"Erp Ship to address sync successful for Sales Org: ({salesOrg.Code}) {salesOrg.Name}");
                }
                else
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                        ErpSyncLevel.ShipToAddress,
                        $"Erp Ship to address sync is partially or not successful for Sales Org: ({salesOrg.Code}) {salesOrg.Name}",
                        lastErrorMessage);
                }

                oldErpAccounts.Clear();

                salesOrg.LastErpShipToAddressSyncTimeOnUtc = DateTime.UtcNow;
                await _erpSalesOrgService.UpdateErpSalesOrgAsync(salesOrg);

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                    ErpSyncLevel.ShipToAddress,
                    (!string.IsNullOrWhiteSpace(lastSyncedErpShipToAddressShipToCode) ?
                    $"The last synced Erp Ship To Address: {lastSyncedErpShipToAddressShipToCode}, for Sales Org: ({salesOrg.Code}) {salesOrg.Name}. " : string.Empty) +
                    $"Total synced in this session: {totalSyncedSoFar} " +
                    $"And total not synced due to invalid data: {totalNotSyncedSoFar}");
            }

            if (!string.IsNullOrWhiteSpace(erpAccountNumber) && !specificErpAccountSalesOrgFound)
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                    ErpSyncLevel.ShipToAddress,
                    $"No Sales org found for the Erp Account : {erpAccountNumber} to sync Ship To Address.");
            }

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                ErpSyncLevel.ShipToAddress,
                "Erp ShipToAddress Sync ended.");

            return true;
        }
        catch (Exception ex)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                ErpSyncLevel.ShipToAddress,
                ex.Message,
                ex.StackTrace);

            await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                DateTime.UtcNow,
                ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                ex.Message + "\n\n" + ex.StackTrace);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                ErpSyncLevel.ShipToAddress,
                "Erp ShipToAddress Sync ended.");

            return false;
        }
    }

    #endregion
}