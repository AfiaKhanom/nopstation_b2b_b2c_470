using DocumentFormat.OpenXml.Bibliography;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public class ErpShipToAddressSyncService : IErpShipToAddressSyncService
{
    #region Fields

    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;
    private readonly IAddressService _addressService;
    private readonly ICountryService _countryService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly ISyncLogService _erpSyncLogService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly IErpDataClearCacheService _erpDataClearCacheService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginService;

    #endregion

    #region Ctor

    public ErpShipToAddressSyncService(IStoreContext storeContext,
        ISettingService settingService,
        IAddressService addressService,
        ICountryService countryService,
        IStateProvinceService stateProvinceService,
        ISyncLogService erpSyncLogService,
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService,
        IErpShipToAddressService erpShipToAddressService,
        IErpDataClearCacheService erpDataClearCacheService,
        IErpIntegrationPluginManager erpIntegrationPluginService)
    {
        _storeContext = storeContext;
        _addressService = addressService;
        _countryService = countryService;
        _settingService = settingService;
        _stateProvinceService = stateProvinceService;
        _erpSyncLogService = erpSyncLogService;
        _erpAccountService = erpAccountService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpShipToAddressService = erpShipToAddressService;
        _erpDataClearCacheService = erpDataClearCacheService;
        _erpIntegrationPluginService = erpIntegrationPluginService;
    }

    #endregion

    #region Method

    public async virtual Task<bool> IsErpShipToAddressSyncSuccessfulAsync()
    {
        var erpIntegrationPlugin = await _erpIntegrationPluginService.LoadActiveERPIntegrationPlugin();

        if (erpIntegrationPlugin is null)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                ErpSyncLavel.ShipToAddress,
                "No integration method found.");

            return false;
        }

        try
        {
            #region Data collection

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var b2BB2CFeaturesSettings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(storeScope);
            var allStateProvinces = (await _stateProvinceService.GetStateProvincesAsync()).ToList();
            var allCountries = (await _countryService.GetAllCountriesAsync()).ToList();
            var listOfSalesOrgs = new List<ErpSalesOrg>();
            var salesOrgCode = await erpIntegrationPlugin.GetSalesOrgCodeFromIntegrationSettings();

            if (!string.IsNullOrWhiteSpace(salesOrgCode))
            {
                var salesOrg = (await _erpSalesOrgService.GetAllErpSalesOrgAsync(code: salesOrgCode)).FirstOrDefault();

                if (salesOrg == null)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                    ErpSyncLavel.ShipToAddress,
                    $"No Sales org found with Sales org code: {salesOrgCode}. Unable to run {ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName}.");

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
                ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                ErpSyncLavel.ShipToAddress,
                "Erp ShipToAddress Sync started.");

            foreach (var salesOrg in listOfSalesOrgs)
            {
                var oldErpAccounts = (List<ErpAccount>)await _erpAccountService.GetAllErpAccountsAsync(salesOrgId: salesOrg.Id);

                if (oldErpAccounts.Count == 0)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                        ErpSyncLavel.ShipToAddress,
                        $"No Erp Accounts found with the Sales org : {salesOrg.Name}");

                    return false;
                }

                var lastErpShipToAddressSynced = new ErpShipToAddress();
                var lastErpShipToAddressSyncedOfErpAccount = "";
                var totalSyncedSoFar = 0;
                var isError = false;
                var lastErrorMessage = "";

                foreach (var erpAccount in oldErpAccounts)
                {
                    var start = "0";

                    while (true)
                    {
                        var erpGetRequestModel = new ErpGetRequestModel
                        {
                            Start = start,
                            AccountNumber = erpAccount.AccountNumber,
                            Location = salesOrg.Code
                        };

                        var response = await erpIntegrationPlugin.GetShipToAddressByAccountNumberFromErpAsync(erpGetRequestModel);

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

                        foreach (var erpShipToAddress in response.Data)
                        {
                            var oldShipToAddressByThisAccount = (await _erpShipToAddressService.GetAllErpShipToAddressesAsync(shipToCode: erpShipToAddress.ShipToCode, erpAccountId: erpAccount.Id)).FirstOrDefault() ?? new ErpShipToAddress();

                            var address = await _addressService.GetAddressByIdAsync(erpAccount.BillingAddressId ?? 0) ?? new Address();

                            var countryId = allCountries.Find(x => x.Name.Equals(erpShipToAddress.Country)
                            || x.TwoLetterIsoCode.Equals(erpShipToAddress.Country)
                            || x.ThreeLetterIsoCode.Equals(erpShipToAddress.Country)
                            || x.NumericIsoCode.Equals(erpShipToAddress.Country))?.Id ?? b2BB2CFeaturesSettings.DefaultCountryId;

                            var stateProvinceId = allStateProvinces.Find(x => x.CountryId == countryId
                            && (x.Name.Equals(erpShipToAddress.StateProvince) || x.Abbreviation.Equals(erpShipToAddress.StateProvince) ))?.Id ?? 0;

                            if (address.Id <= 0)
                            {
                                address.Email = erpShipToAddress.EmailAddress;
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
                                address.Email = erpShipToAddress.EmailAddress;
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


                            if (oldShipToAddressByThisAccount.Id > 0)
                            {
                                oldShipToAddressByThisAccount.ShipToCode = erpShipToAddress.ShipToCode;
                                oldShipToAddressByThisAccount.ShipToName = erpShipToAddress.ShipToName;
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

                                await _erpShipToAddressService.UpdateErpShipToAddressAsync(oldShipToAddressByThisAccount);

                                if (await _erpShipToAddressService.GetErpShipToAddressErpAccountMapByErpShipToAddressIdAsync(oldShipToAddressByThisAccount.Id) == null)
                                {
                                    await _erpShipToAddressService.InsertErpShipToAddressErpAccountMapAsync(erpAccount, oldShipToAddressByThisAccount);
                                }
                            }
                            else
                            {
                                oldShipToAddressByThisAccount.ShipToCode = erpShipToAddress.ShipToCode;
                                oldShipToAddressByThisAccount.ShipToName = erpShipToAddress.ShipToName;
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

                                await _erpShipToAddressService.InsertErpShipToAddressAsync(oldShipToAddressByThisAccount);
                                await _erpShipToAddressService.InsertErpShipToAddressErpAccountMapAsync(erpAccount, oldShipToAddressByThisAccount);
                            }
                            lastErpShipToAddressSynced = oldShipToAddressByThisAccount;
                            lastErpShipToAddressSyncedOfErpAccount = erpAccount.AccountNumber;
                            totalSyncedSoFar++;

                            #region Cache clear for this erp ship to address

                            await _erpDataClearCacheService.ClearCacheOfEntity(oldShipToAddressByThisAccount, oldShipToAddressByThisAccount.Id);

                            #endregion
                        }
                    }


                }
                if (!isError)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                        ErpSyncLavel.ShipToAddress,
                        $"Erp Ship to address sync successful for Sales Org: {salesOrg.Name}");
                }
                else
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                        ErpSyncLavel.ShipToAddress,
                        $"Erp Ship to address sync is partially or not successful for Sales Org: {salesOrg.Name}",
                        lastErrorMessage);
                }

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                    ErpSyncLavel.ShipToAddress,
                    (lastErpShipToAddressSynced is not null ? $"The last synced Erp Ship To Address: {lastErpShipToAddressSynced.ShipToCode}, of Erp Account: {lastErpShipToAddressSyncedOfErpAccount} for Sales Org: {salesOrg.Name}. " : string.Empty) + $"Total synced in this session: {totalSyncedSoFar}");

            }

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                ErpSyncLavel.ShipToAddress,
                "Erp ShipToAddress Sync ended.");

            return true;
        }
        catch (Exception ex)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                ErpSyncLavel.ShipToAddress,
                ex.Message,
                ex.StackTrace);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                ErpSyncLavel.ShipToAddress,
                "Erp ShipToAddress Sync ended.");

            return false;
        }
    }

    #endregion
}