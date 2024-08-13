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

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices
{
    public class ErpAccountSyncService : IErpAccountSyncService
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
        private readonly IErpGroupPriceCodeService _erpGroupPriceCodeService;
        private readonly IErpDataClearCacheService _erpDataClearCacheService;
        private readonly IErpIntegrationPluginManager _erpIntegrationPluginManager;

        #endregion

        #region Ctor

        public ErpAccountSyncService(
            IStoreContext storeContext,
            ISettingService settingService,
            IAddressService addressService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            ISyncLogService erpSyncLogService,
            IErpAccountService erpAccountService,
            IErpSalesOrgService erpSalesOrgService,
            IErpGroupPriceCodeService erpGroupPriceCodeService,
            IErpDataClearCacheService erpDataClearCacheService,
            IErpIntegrationPluginManager erpIntegrationPluginService)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _addressService = addressService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _erpSyncLogService = erpSyncLogService;
            _erpAccountService = erpAccountService;
            _erpSalesOrgService = erpSalesOrgService;
            _erpGroupPriceCodeService = erpGroupPriceCodeService;
            _erpDataClearCacheService = erpDataClearCacheService;
            _erpIntegrationPluginManager = erpIntegrationPluginService;
        }

        #endregion

        #region Method

        public virtual async Task<bool> IsErpAccountSyncSuccessfulAsync()
        {
            var erpIntegrationPlugin = await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();

            if (erpIntegrationPlugin is null)
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                    ErpSyncLavel.Account,
                    "No integration method found.");

                return false;
            }

            try
            {
                #region Data collections

                var allCountries = (await _countryService.GetAllCountriesAsync()).ToList();
                var allStateProvinces = (await _stateProvinceService.GetStateProvincesAsync()).ToList();
                var listOfSalesOrgs = new List<ErpSalesOrg>();

                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var b2BB2CFeaturesSettings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(storeScope);

                var syncStartTime = DateTime.UtcNow.AddMinutes(-10);

                var salesOrgCode = await erpIntegrationPlugin.GetSalesOrgCodeFromIQIntegrationSettings();
                if (!string.IsNullOrWhiteSpace(salesOrgCode))
                {
                    var salesOrg = (await _erpSalesOrgService.GetAllErpSalesOrgAsync(code: salesOrgCode)).FirstOrDefault();

                    if (salesOrg == null)
                    {
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                            ErpSyncLavel.Account,
                            $"No Sales org found with Sales org code: {salesOrgCode}. Unable to run {ErpDataSchedulerDefaults.ErpAccountSyncTaskName}.");

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
                    ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                    ErpSyncLavel.Account,
                    "Erp Account Sync started.");

                foreach (var salesOrg in listOfSalesOrgs)
                {
                    var oldErpAccounts = (List<ErpAccount>)await _erpAccountService.GetAllErpAccountsAsync(salesOrgId: salesOrg.Id);
                    var isError = false;
                    var start = "0";
                    var lastErpAccountSynced = new ErpAccount();
                    var totalSyncedSoFar = 0;

                    while (true)
                    {
                        var erpGetRequestModel = new ErpGetRequestModel
                        {
                            Start = start,
                            Location = salesOrg.Code
                        };

                        var response = await erpIntegrationPlugin.GetAccountsFromErpAsync(erpGetRequestModel);

                        if (response.ErpResponseModel.IsError || response.Data is null)
                        {
                            isError = true;

                            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                                ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                                ErpSyncLavel.Account,
                                response.ErpResponseModel.ErrorShortMessage,
                                response.ErpResponseModel.ErrorFullMessage);
                            break;
                        }

                        start = response.ErpResponseModel.Next;

                        foreach (var erpAccount in response.Data)
                        {
                            var oldErpAccount = oldErpAccounts.Find(x => x.AccountNumber == erpAccount.AccNo) ?? new ErpAccount();

                            var address = await _addressService.GetAddressByIdAsync(oldErpAccount.BillingAddressId ?? 0) ?? new Address();
                            var countryId = allCountries.Find(country => country.Name.Contains(erpAccount.Province))?.Id ?? b2BB2CFeaturesSettings.DefaultCountryId;

                            if (address.Id <= 0)
                            {
                                address.Email = erpAccount.EMail;
                                address.Company = erpAccount.Name;
                                address.CountryId = countryId;
                                address.City = erpAccount.Address3 ?? string.Empty;
                                address.County = erpAccount.Address3 ?? string.Empty;
                                address.Address1 = erpAccount.Address1 ?? string.Empty;
                                address.Address2 = erpAccount.Address2 ?? string.Empty;
                                address.ZipPostalCode = erpAccount.PostalCode ?? string.Empty;
                                address.StateProvinceId = allStateProvinces.Find(state => state.CountryId == countryId)?.Id ?? 0;
                                address.PhoneNumber = erpAccount.TelNo;
                                address.FaxNumber = string.Empty;

                                address.CreatedOnUtc = DateTime.UtcNow;
                                await _addressService.InsertAddressAsync(address);
                            }
                            else
                            {
                                address.Email = erpAccount.EMail;
                                address.Company = erpAccount.Name;
                                address.CountryId = countryId;
                                address.City = erpAccount.Address3 ?? string.Empty;
                                address.County = erpAccount.Address3 ?? string.Empty;
                                address.Address1 = erpAccount.Address1 ?? string.Empty;
                                address.Address2 = erpAccount.Address2 ?? string.Empty;
                                address.ZipPostalCode = erpAccount.PostalCode ?? string.Empty;
                                address.StateProvinceId = allStateProvinces.Find(state => state.CountryId == countryId)?.Id ?? 0;
                                address.PhoneNumber = erpAccount.TelNo;
                                address.FaxNumber = string.Empty;

                                await _addressService.UpdateAddressAsync(address);
                            }

                            if (oldErpAccount.Id <= 0)
                            {
                                oldErpAccount.ErpSalesOrg = salesOrg;
                                oldErpAccount.ErpSalesOrgId = salesOrg.Id;

                                oldErpAccount.AccountNumber = erpAccount.AccNo;
                                oldErpAccount.AccountName = erpAccount.Name ?? string.Empty;
                                oldErpAccount.IsActive = true;
                                oldErpAccount.VatNumber = erpAccount.VatNumber ?? string.Empty;
                                oldErpAccount.PreFilterFacets = erpAccount.PrefilterFacets;
                                oldErpAccount.PaymentTypeCode = string.Empty;
                                oldErpAccount.BillingAddressId = address.Id;
                                oldErpAccount.BillingSuburb = address.Address1;

                                oldErpAccount.AllowOverspend = erpAccount.AllowOverspend ?? b2BB2CFeaturesSettings.AllowOverspend;
                                oldErpAccount.AllowAccountsAddressEditOnCheckout = b2BB2CFeaturesSettings.AllowAddressEditOnCheckoutForAll;
                                oldErpAccount.B2BPriceGroupCodeId = _erpGroupPriceCodeService.GetErpGroupPriceCodeByCodedAsync(erpAccount.PriceGroupCode).Id;

                                oldErpAccount.CreditLimitAvailable = erpAccount.CreditLimitAvailable ?? 0;
                                oldErpAccount.CreditLimit = erpAccount.CreditLimit;
                                if (erpAccount.CreditLimitUsed != null)
                                {
                                    oldErpAccount.CurrentBalance = erpAccount.CreditLimitUsed.Value;
                                }
                                else if (erpAccount.Balance != null)
                                {
                                    oldErpAccount.CurrentBalance = erpAccount.Balance.Value;
                                }

                                var hideStockValues = erpAccount.Attributes?.Exists(kvp =>
                                        "HideStockValues".Equals(kvp.Key, StringComparison.InvariantCultureIgnoreCase)
                                        && bool.TryParse(kvp.Value, out var value)
                                        && value) ?? false;

                                if (hideStockValues)
                                {
                                    oldErpAccount.OverrideStockDisplayFormatConfigSetting = true;
                                    oldErpAccount.StockDisplayFormatTypeId = (int)StockDisplayFormat.ShowInOrOutOfStockIndicators;
                                }
                                else
                                {
                                    oldErpAccount.OverrideStockDisplayFormatConfigSetting = true;
                                    oldErpAccount.StockDisplayFormatTypeId = (int)StockDisplayFormat.ShowStockQuantities;
                                }

                                oldErpAccount.ErpAccountStatusTypeId = (int)ErpAccountStatusType.Normal;
                                oldErpAccount.PercentageOfStockAllowed = erpAccount.PercentageOfStockAllowedForCustomer ?? 100;
                                if (oldErpAccount.PercentageOfStockAllowed <= 0)
                                {
                                    oldErpAccount.PercentageOfStockAllowed = 100;
                                }

                                oldErpAccount.IsDeleted = false;

                                oldErpAccount.CreatedById = 1;
                                oldErpAccount.CreatedOnUtc = DateTime.UtcNow;
                                oldErpAccount.UpdatedById = 1;
                                oldErpAccount.UpdatedOnUtc = DateTime.UtcNow;
                                oldErpAccount.LastPriceRefresh = DateTime.UtcNow;
                                oldErpAccount.LastErpAccountSyncDate = DateTime.UtcNow;

                                await _erpAccountService.InsertErpAccountAsync(oldErpAccount);
                            }
                            else
                            {
                                oldErpAccount.AccountName = erpAccount.Name ?? string.Empty;
                                oldErpAccount.VatNumber = erpAccount.VatNumber ?? string.Empty;
                                oldErpAccount.PreFilterFacets = erpAccount.PrefilterFacets;
                                oldErpAccount.PaymentTypeCode = string.Empty;
                                oldErpAccount.BillingAddressId = address.Id;
                                oldErpAccount.BillingSuburb = address.Address1;

                                oldErpAccount.AllowOverspend = erpAccount.AllowOverspend ?? b2BB2CFeaturesSettings.AllowOverspend;
                                oldErpAccount.AllowAccountsAddressEditOnCheckout = b2BB2CFeaturesSettings.AllowAddressEditOnCheckoutForAll;
                                oldErpAccount.B2BPriceGroupCodeId = _erpGroupPriceCodeService.GetErpGroupPriceCodeByCodedAsync(erpAccount.PriceGroupCode).Id;

                                oldErpAccount.CreditLimitAvailable = erpAccount.CreditLimitAvailable ?? 0;
                                oldErpAccount.CreditLimit = erpAccount.CreditLimit;
                                if (erpAccount.CreditLimitUsed != null)
                                {
                                    oldErpAccount.CurrentBalance = erpAccount.CreditLimitUsed.Value;
                                }
                                else if (erpAccount.Balance != null)
                                {
                                    oldErpAccount.CurrentBalance = erpAccount.Balance.Value;
                                }

                                var hideStockValues = erpAccount.Attributes?.Any(kvp =>
                                        "HideStockValues".Equals(kvp.Key, StringComparison.InvariantCultureIgnoreCase)
                                        && bool.TryParse(kvp.Value, out var value)
                                        && value) ?? false;

                                if (hideStockValues)
                                {
                                    oldErpAccount.OverrideStockDisplayFormatConfigSetting = true;
                                    oldErpAccount.StockDisplayFormatTypeId = (int)StockDisplayFormat.ShowInOrOutOfStockIndicators;
                                }
                                else
                                {
                                    oldErpAccount.OverrideStockDisplayFormatConfigSetting = true;
                                    oldErpAccount.StockDisplayFormatTypeId = (int)StockDisplayFormat.ShowStockQuantities;
                                }

                                oldErpAccount.ErpAccountStatusTypeId = (int)ErpAccountStatusType.Normal;
                                oldErpAccount.PercentageOfStockAllowed = erpAccount.PercentageOfStockAllowedForCustomer ?? 100;
                                if (oldErpAccount.PercentageOfStockAllowed <= 0)
                                {
                                    oldErpAccount.PercentageOfStockAllowed = 100;
                                }

                                oldErpAccount.UpdatedById = 1;
                                oldErpAccount.UpdatedOnUtc = DateTime.UtcNow;
                                oldErpAccount.LastPriceRefresh = DateTime.UtcNow;
                                oldErpAccount.LastErpAccountSyncDate = DateTime.UtcNow;

                                await _erpAccountService.UpdateErpAccountAsync(oldErpAccount);
                            }

                            lastErpAccountSynced = oldErpAccount;
                            totalSyncedSoFar++;

                            #region Cache clear for this erp account

                            await _erpDataClearCacheService.ClearCacheOfEntity(oldErpAccount, oldErpAccount.Id);

                            #endregion
                        }
                    }

                    if (!isError)
                    {
                        await _erpAccountService.InActiveAllOldAccount(syncStartTime);
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                                ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                                ErpSyncLavel.Account,
                                $"Erp Accounts sync is successful for Sales Org: {salesOrg.Name}. The accounts which were updated before {syncStartTime} are deactivated.");
                    }
                    else
                    {
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                                ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                                ErpSyncLavel.Account,
                                $"Erp Accounts sync is partially or not successful for Sales Org: {salesOrg.Name}");
                    }

                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                        ErpSyncLavel.Account,
                        (lastErpAccountSynced is not null ?
                        $"The last synced Erp Account: {lastErpAccountSynced.AccountNumber}, for Sales Org: {salesOrg.Name}. " : string.Empty) +
                        $"Total synced in this session: {totalSyncedSoFar}");
                }

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                    ErpSyncLavel.Account,
                    "Erp Account Sync ended.");

                return true;
            }
            catch (Exception ex)
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                    ErpSyncLavel.Account,
                    ex.Message,
                    ex.StackTrace);

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                    ErpSyncLavel.Account,
                    "Erp Account Sync ended.");

                return false;
            }
        }

        #endregion
    }
}