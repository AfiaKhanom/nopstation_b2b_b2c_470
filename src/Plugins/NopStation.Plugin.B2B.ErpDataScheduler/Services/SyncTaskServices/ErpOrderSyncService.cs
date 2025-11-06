using System.Text.RegularExpressions;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Orders;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncWorkflowMessage;
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public class ErpOrderSyncService : IErpOrderSyncService
{
    #region Fields

    private readonly IOrderService _orderService;
    private readonly IStoreContext _storeContext;
    private readonly ICountryService _countryService;
    private readonly IProductService _productService;
    private readonly IAddressService _addressService;
    private readonly ICustomerService _customerService;
    private readonly ICurrencyService _currencyService;
    private readonly CurrencySettings _currencySettings;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly ISyncLogService _erpSyncLogService;
    private readonly IErpNopUserService _erpNopUserService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly ErpDataSchedulerSettings _erpDataSchedulerSettings;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly IErpDataClearCacheService _erpDataClearCacheService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginService;
    private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
    private readonly IErpOrderItemAdditionalDataService _erpOrderItemAdditionalDataService;
    private readonly ISyncWorkflowMessageService _syncWorkflowMessageService;

    #endregion

    #region Ctor

    public ErpOrderSyncService(IOrderService orderService,
        IStoreContext storeContext,
        ICountryService countryService,
        IProductService productService,
        IAddressService addressService,
        ICustomerService customerService,
        ICurrencyService currencyService,
        CurrencySettings currencySettings,
        IGenericAttributeService genericAttributeService,
        IStateProvinceService stateProvinceService,
        ISyncLogService erpSyncLogService,
        IErpNopUserService erpNopUserService,
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        ErpDataSchedulerSettings erpDataSchedulerSettings,
        IErpShipToAddressService erpSShipToAddressService,
        IErpDataClearCacheService erpDataClearCacheService,
        IErpIntegrationPluginManager erpIntegrationPluginService,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        IErpOrderItemAdditionalDataService erpOrderItemAdditionalDataService,
        ISyncWorkflowMessageService syncWorkflowMessageService)
    {
        _orderService = orderService;
        _storeContext = storeContext;
        _countryService = countryService;
        _productService = productService;
        _addressService = addressService;
        _customerService = customerService;
        _currencyService = currencyService;
        _currencySettings = currencySettings;
        _genericAttributeService = genericAttributeService;
        _stateProvinceService = stateProvinceService;
        _erpSyncLogService = erpSyncLogService;
        _erpNopUserService = erpNopUserService;
        _erpAccountService = erpAccountService;
        _erpSalesOrgService = erpSalesOrgService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpDataSchedulerSettings = erpDataSchedulerSettings;
        _erpShipToAddressService = erpSShipToAddressService;
        _erpDataClearCacheService = erpDataClearCacheService;
        _erpIntegrationPluginService = erpIntegrationPluginService;
        _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
        _erpOrderItemAdditionalDataService = erpOrderItemAdditionalDataService;
        _syncWorkflowMessageService = syncWorkflowMessageService;
    }

    #endregion

    #region Method

    private async Task CreateNopUser(ErpNopUser? erpNopUser, Customer customer, ErpAccount erpAccount, int erpShipToAddressId)
    {
        erpNopUser ??= new ErpNopUser();
        erpNopUser.NopCustomerId = customer.Id;
        erpNopUser.CreatedById = 1;
        erpNopUser.CreatedOnUtc = DateTime.UtcNow;
        erpNopUser.ErpShipToAddressId = erpShipToAddressId != 0 ? erpShipToAddressId : customer.ShippingAddressId ?? 0;
        erpNopUser.ShippingErpShipToAddressId = customer.ShippingAddressId ?? 0;
        erpNopUser.BillingErpShipToAddressId = customer.BillingAddressId ?? 0;
        erpNopUser.ErpAccountId = erpAccount.Id;
        erpNopUser.ErpUserType = ErpUserType.B2BUser;
        erpNopUser.IsActive = true;
        erpNopUser.UpdatedOnUtc = DateTime.UtcNow;

        await _erpNopUserService.InsertErpNopUserAsync(erpNopUser);
        
        //add to 'B2B Customer' role
        var b2bCustomerRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BCustomerRole)
            ?? throw new NopException($"'{ERPIntegrationCoreDefaults.B2BCustomerRole}' role could not be loaded");
        await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = b2bCustomerRole.Id });
    }

    public virtual async Task<bool> IsErpOrderSyncSuccessfulAsync(string? erpAccountNumber = null, string? orderNumber = null, bool isManualTrigger = false, bool isIncrementalSync = true, CancellationToken cancellationToken = default)
    {
        var erpIntegrationPlugin = await _erpIntegrationPluginService.LoadActiveERPIntegrationPlugin(ErpSyncLevel.Order);

        if (erpIntegrationPlugin is null)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                ErpSyncLevel.Order,
                $"No integration method found. Unable to run {ErpDataSchedulerDefaults.ErpOrderSyncTaskName}.");

            return false;
        }

        try
        {
            #region Data collection

            var salesOrgs = await _erpSalesOrgService.GetAllErpSalesOrgsAsync();
            if (!salesOrgs.Any())
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                    ErpSyncLevel.Order,
                    $"No Sales org found. Unable to run {ErpDataSchedulerDefaults.ErpOrderSyncTaskName}.");

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
                        ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                        ErpSyncLevel.Order,
                        $"No Active Erp Account found with Active Erp Nop User with Account Number: {erpAccountNumber}");

                    return false;
                }
            }

            var currency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
            var allCountries = (await _countryService.GetAllCountriesAsync()).ToList();
            var allStateProvinces = (await _stateProvinceService.GetStateProvincesAsync()).ToList();

            #endregion

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                ErpSyncLevel.Order,
                "Erp Order Sync started.");

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
                        ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                        ErpSyncLevel.Order,
                        $"No Erp Accounts found with Active Nop Users for Sales org : {salesOrg.Name}");

                    if (specificErpAccounts != null)
                        return false;

                    continue;
                }                

                var isError = false;
                var totalSyncedSoFar = 0;
                var totalNotSyncedSoFar = 0;
                var lastErrorMessage = "";
                var lastErpOrderSynced = "";
                var lastErpOrderSyncedOfErpAccount = "";

                var erpAccountNumbersWithOrders = await _erpOrderAdditionalDataService
                    .CheckAccountHasOrders(salesOrg.Code, oldErpAccounts.Select(x => x.AccountNumber).ToArray());

                foreach (var erpAccount in oldErpAccounts)
                {
                    var start = "0";
                    DateTime? dateFrom = DateTime.Today.AddMonths(-3);

                    if (erpAccountNumbersWithOrders[erpAccount.AccountNumber])
                    {
                        if (erpAccount.LastTimeOrderSyncOnUtc.HasValue)
                        {
                            dateFrom = isIncrementalSync ? erpAccount.LastTimeOrderSyncOnUtc.Value.AddHours(-2) : null;
                        }
                        else
                        {
                            dateFrom = isIncrementalSync ? DateTime.Today.AddDays(-1) : null;
                        }
                    }

                    while (true)
                    {
                        var erpGetRequestModel = new ErpGetRequestModel
                        {
                            Start = start,
                            AccountNumber = erpAccount.AccountNumber,
                            Location = salesOrg.Code,
                            DateFrom = dateFrom,
                            OrderNumber = orderNumber
                        };

                        var response = await erpIntegrationPlugin.GetOrderByAccountFromErpAsync(erpGetRequestModel);

                        if (response.ErpResponseModel.IsError)
                        {
                            isError = true;
                            lastErrorMessage = $"The last error: {response.ErpResponseModel.ErrorShortMessage}";

                            await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                                DateTime.UtcNow,
                                ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                                response.ErpResponseModel.ErrorShortMessage + "\n\n" + response.ErpResponseModel.ErrorFullMessage);

                            break;
                        }
                        else if (response.Data is null)
                        {
                            isError = false;
                            break;
                        }

                        start = response.ErpResponseModel.Next;

                        var erpOrders = await response.Data
                            .Where(x => !string.IsNullOrWhiteSpace(x.OrderType) && !string.IsNullOrWhiteSpace(x.CustomOrderNumber.Trim()))
                            .GroupBy(x => x.CustomOrderNumber.Trim())
                            .Select(g => g.Last())
                            .ToListAsync();

                        totalNotSyncedSoFar += response.Data.Count - erpOrders.Count;

                        if (!string.IsNullOrWhiteSpace(orderNumber))
                        {
                            erpOrders = erpOrders.Where(x => x.CustomOrderNumber == orderNumber).ToList();
                        }

                        (lastErpOrderSynced, lastErpOrderSyncedOfErpAccount, totalSyncedSoFar, totalNotSyncedSoFar) = await MapOrderData(
                            erpOrders,
                            erpAccount,
                            lastErpOrderSynced,
                            lastErpOrderSyncedOfErpAccount,
                            totalSyncedSoFar,
                            totalNotSyncedSoFar,
                            allStateProvinces,
                            allCountries,
                            currency);

                        // If specific order was found and synced, break the loop
                        if (!string.IsNullOrWhiteSpace(orderNumber) && erpOrders.Count != 0)
                        {
                            break;
                        }

                        if (cancellationToken.IsCancellationRequested)
                        {
                            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                                ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                                ErpSyncLevel.Order,
                                "The Erp Order Sync run is cancelled. " +
                                (!string.IsNullOrWhiteSpace(lastErpOrderSynced) ?
                                $"The last synced Erp Order: {lastErpOrderSynced}, of Erp Account: {lastErpOrderSyncedOfErpAccount} for Sales Org: ({salesOrg.Code}) {salesOrg.Name}. " : string.Empty) +
                                $"Total orders synced in this session: {totalSyncedSoFar} and " + 
                                $"Total orders not synced due to invalid data in this session: {totalNotSyncedSoFar}");

                            return false;
                        }
                    }

                    // Skip quote order call if specific order number is provided
                    if (string.IsNullOrWhiteSpace(orderNumber) && _erpDataSchedulerSettings.NeedQuoteOrderCall)
                    {
                        start = "0";
                        while (true)
                        {
                            var erpGetRequestModel = new ErpGetRequestModel
                            {
                                Start = start,
                                AccountNumber = erpAccount.AccountNumber,
                                Location = salesOrg.Code,
                                DateFrom = isIncrementalSync ? erpAccount.LastTimeOrderSyncOnUtc : null,

                            };

                            var response = await erpIntegrationPlugin.GetQuoteByAccountFromErpAsync(erpGetRequestModel);

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

                            var erpOrders = await response.Data.Where(x => !string.IsNullOrWhiteSpace(x.OrderType)).ToListAsync();

                            (lastErpOrderSynced, lastErpOrderSyncedOfErpAccount, totalSyncedSoFar, totalNotSyncedSoFar) = await MapOrderData(
                                erpOrders,
                                erpAccount,
                                lastErpOrderSynced,
                                lastErpOrderSyncedOfErpAccount,
                                totalSyncedSoFar,
                                totalNotSyncedSoFar,
                                allStateProvinces,
                                allCountries,
                                currency);

                            if (cancellationToken.IsCancellationRequested)
                            {
                                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                                    ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                                    ErpSyncLevel.Order,
                                    "The Erp Order Sync run is cancelled. " +
                                    (!string.IsNullOrWhiteSpace(lastErpOrderSynced) ?
                                    $"The last synced Erp Order: {lastErpOrderSynced}, of Erp Account: {lastErpOrderSyncedOfErpAccount} for Sales Org: ({salesOrg.Code}) {salesOrg.Name}. " : string.Empty) +
                                    $"Total orders synced in this session: {totalSyncedSoFar} and " +
                                    $"Total orders not synced due to invalid data in this session: {totalNotSyncedSoFar}");

                                return false;
                            }
                        }
                    }

                    erpAccount.LastTimeOrderSyncOnUtc = DateTime.UtcNow;
                    await _erpAccountService.UpdateErpAccountAsync(erpAccount);
                }

                if (!isError)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                        ErpSyncLevel.Order,
                        $"Erp Order sync successful for Sales Org: ({salesOrg.Code}) {salesOrg.Name}");
                }
                else
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                        ErpSyncLevel.Order,
                        $"Erp Order sync is partially or not successful for Sales Org: ({salesOrg.Code}) {salesOrg.Name}",
                        lastErrorMessage);
                }

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                    ErpSyncLevel.Order,
                    (!string.IsNullOrWhiteSpace(lastErpOrderSynced) ?
                    $"The last synced Erp Order: {lastErpOrderSynced}, of Erp Account: {lastErpOrderSyncedOfErpAccount} for Sales Org: ({salesOrg.Code}) {salesOrg.Name}. " : string.Empty) +
                    $"Total synced in this session: {totalSyncedSoFar} and " +
                    $"Total orders not synced due to invalid data in this session: {totalNotSyncedSoFar}");
            }

            if (!string.IsNullOrWhiteSpace(erpAccountNumber) && !specificErpAccountSalesOrgFound)
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                    ErpSyncLevel.Order,
                    $"No Sales org found for the Erp Account : {erpAccountNumber} to sync Orders.");
            }

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                ErpSyncLevel.Order,
                "Erp Order Sync ended.");

            return true;
        }
        catch (Exception ex)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                ErpSyncLevel.Order,
                ex.Message,
                ex.StackTrace ?? string.Empty);

            await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                DateTime.UtcNow,
                ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                ex.Message + "\n\n" + ex.StackTrace);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                ErpSyncLevel.Order,
                "Erp Order Sync ended.");

            return false;
        }
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task<(string lastErpOrderSynced, string lastErpOrderSyncedOfErpAccount, int totalSyncedSoFar, int totalNotSyncedSoFar)> MapOrderData(
        IList<ErpPlaceOrderDataModel> erpOrders,
        ErpAccount erpAccount,
        string lastErpOrderSynced,
        string lastErpOrderSyncedOfErpAccount,
        int totalSyncedSoFar,
        int totalNotSyncedSoFar,
        List<StateProvince> allStateProvinces,
        List<Country> allCountries,
        Currency currency)
    {
        foreach (var erpOrder in erpOrders)
        {
            #region Nop Order

            ErpNopUser? erpNopUser = null;

            if (!IsValidEmail(erpOrder.CustomerEmail))
            {
                totalNotSyncedSoFar++;
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                    ErpSyncLevel.Order,
                    $"Data mapping skipped for {nameof(Order.CustomOrderNumber)}: {erpOrder.CustomOrderNumber}. \nThe Customer email '{erpOrder.CustomerEmail}' is empty or invalid.");
                continue;
            }

            var oldNopOrder = await _orderService.GetOrderByCustomOrderNumberAsync(erpOrder.CustomOrderNumber);

            var oldErpOrder = await _erpOrderAdditionalDataService
                .GetErpOrderAdditionalDataByErpAccountIdAndErpOrderNumberAsync(accountId: erpAccount.Id, erpOrderNumber: erpOrder.ErpOrderNumber);

            if (oldErpOrder != null && oldNopOrder == null)
            {
                oldNopOrder = await _orderService.GetOrderByIdAsync(oldErpOrder.NopOrderId);
            }

            if (oldNopOrder == null)
            {
                #region Address

                var countryId = allCountries.Find(x =>
                    !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(erpOrder.ShippingAddress?.Country)
                    || !string.IsNullOrWhiteSpace(x.TwoLetterIsoCode) && x.TwoLetterIsoCode.Equals(erpOrder.ShippingAddress?.Country)
                    || !string.IsNullOrWhiteSpace(x.ThreeLetterIsoCode) && x.ThreeLetterIsoCode.Equals(erpOrder.ShippingAddress?.Country))?.Id
                    ?? _b2BB2CFeaturesSettings.DefaultCountryId;

                var stateProvinceId = allStateProvinces.Find(x => x.CountryId == countryId
                    && (!string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals(erpOrder.ShippingAddress?.StateProvince) ||
                    !string.IsNullOrWhiteSpace(x.Abbreviation) && x.Abbreviation.Equals(erpOrder.ShippingAddress?.StateProvince)))?.Id ?? 0;

                var address = new Address();
                address.Email = erpOrder.CustomerEmail;
                address.PhoneNumber = erpOrder.CustomerPhoneNumber;
                address.Address1 = erpOrder.ShippingAddress?.Address1;
                address.Address2 = erpOrder.ShippingAddress?.Address2;
                address.City = erpOrder.ShippingAddress?.City;
                address.ZipPostalCode = erpOrder.ShippingAddress?.ZipPostalCode;
                address.CountryId = countryId;
                address.StateProvinceId = stateProvinceId;
                address.CreatedOnUtc = DateTime.UtcNow;
                await _addressService.InsertAddressAsync(address);

                #endregion

                #region Customer

                var customer = await _customerService.GetCustomerByEmailAsync(erpOrder.CustomerEmail);
                if (customer == null)
                {
                    customer = new Customer();
                    customer.FirstName = erpOrder.CustomerFirstName;
                    customer.LastName = erpOrder.CustomerLastName;
                    customer.Email = erpOrder.CustomerEmail;
                    customer.City = address.City;
                    customer.CountryId = countryId;
                    customer.Company = address.Company;
                    customer.BillingAddressId = address.Id;
                    customer.ShippingAddressId = address.Id;
                    customer.Active = true;
                    customer.CreatedOnUtc = DateTime.UtcNow;
                    customer.CustomerGuid = Guid.NewGuid();

                    await _customerService.InsertCustomerAsync(customer);

                    //add to 'Registered' role
                    var registeredRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName) ?? throw new NopException("'Registered' role could not be loaded");

                    await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = registeredRole.Id });

                    //remove from 'Guests' role            
                    if (await _customerService.IsGuestAsync(customer))
                    {
                        var guestRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.GuestsRoleName);
                        await _customerService.RemoveCustomerRoleMappingAsync(customer, guestRole);
                    }

                    var erpShiptoAddressforAccount = await _erpShipToAddressService.GetErpShipToAddressesByAccountIdAsync(showHidden: false, isActiveOnly: true, accountId: erpAccount.Id);
                    await CreateNopUser(erpNopUser, customer, erpAccount, erpShiptoAddressforAccount.FirstOrDefault()?.Id ?? 0);
                }
                else
                {
                    var isCustomerHasAdminRole = await _customerService.IsAdminAsync(customer);
                    var isRegisteredCustomer = await _customerService.IsRegisteredAsync(customer);

                    if (!isRegisteredCustomer)
                    {
                        //add to 'Registered' role
                        var registeredRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName) ?? throw new NopException("'Registered' role could not be loaded");

                        await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = registeredRole.Id });

                        //remove from 'Guests' role            
                        if (await _customerService.IsGuestAsync(customer))
                        {
                            var guestRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.GuestsRoleName);
                            await _customerService.RemoveCustomerRoleMappingAsync(customer, guestRole);
                        }
                    }

                    if (isCustomerHasAdminRole)
                    {
                        totalNotSyncedSoFar++;
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                            ErpSyncLevel.Order,
                            $"Data mapping skipped for {nameof(Order.CustomOrderNumber)}: {erpOrder.CustomOrderNumber}. \nThe Customer  {erpOrder.CustomerEmail} has admin role.");
                        continue;
                    }

                    erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer.Id);
                    if (erpNopUser == null)
                    {
                        var erpShiptoAddressforAccount = await _erpShipToAddressService.GetErpShipToAddressesByAccountIdAsync(
                            showHidden: false,
                            isActiveOnly: true,
                            accountId: erpAccount.Id);
                        await CreateNopUser(erpNopUser, customer, erpAccount, erpShiptoAddressforAccount.FirstOrDefault()?.Id ?? 0);
                    }
                }

                #endregion

                oldNopOrder = new Order();
                oldNopOrder.OrderGuid = Guid.NewGuid();
                oldNopOrder.CustomerId = customer.Id;
                oldNopOrder.BillingAddressId = erpAccount.BillingAddressId ?? address.Id;
                oldNopOrder.ShippingAddressId = address.Id;
                oldNopOrder.CustomOrderNumber = erpOrder.CustomOrderNumber;
                oldNopOrder.ShippingStatusId = (int)ShippingStatus.NotYetShipped;
                oldNopOrder.OrderTotal = erpOrder.OrderSubtotalInclTax ?? decimal.Zero;
                oldNopOrder.OrderSubtotalExclTax = erpOrder.OrderSubtotalExclTax ?? decimal.Zero;
                oldNopOrder.OrderSubtotalInclTax = erpOrder.OrderSubtotalInclTax ?? decimal.Zero;
                oldNopOrder.OrderTax = erpOrder.OrderTax ?? decimal.Zero;
                oldNopOrder.OrderShippingInclTax = erpOrder.ShippingAmount;
                oldNopOrder.StoreId = (await _storeContext.GetCurrentStoreAsync()).Id;
                if (erpOrder.OrderType == ((int)ErpOrderType.B2BSalesOrder).ToString())
                {
                    oldNopOrder.OrderStatusId = (int)OrderStatus.Processing;
                }
                else if (erpOrder.OrderType == ((int)ErpOrderType.B2BQuote).ToString())
                {
                    oldNopOrder.OrderStatusId = (int)OrderStatus.Pending;
                }
                oldNopOrder.PaymentStatusId = (int)PaymentStatus.Paid;
                oldNopOrder.PaymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.SelectedPaymentMethodAttribute) ?? string.Empty;
                oldNopOrder.CustomerCurrencyCode = currency.CurrencyCode;
                oldNopOrder.CurrencyRate = currency.Rate;
                oldNopOrder.CustomerTaxDisplayTypeId = (int)TaxDisplayType.ExcludingTax;
                oldNopOrder.CustomerTaxDisplayType = TaxDisplayType.ExcludingTax;
                oldNopOrder.PaidDateUtc = DateTime.UtcNow;
                oldNopOrder.CreatedOnUtc = DateTime.UtcNow;

                await _orderService.InsertOrderAsync(oldNopOrder);
            }
            else
            {
                oldNopOrder.ShippingStatusId = (int)ShippingStatus.NotYetShipped;
                oldNopOrder.OrderTotal = erpOrder.OrderSubtotalInclTax ?? decimal.Zero;
                oldNopOrder.OrderSubtotalExclTax = erpOrder.OrderSubtotalExclTax ?? decimal.Zero;
                oldNopOrder.OrderSubtotalInclTax = erpOrder.OrderSubtotalInclTax ?? decimal.Zero;
                oldNopOrder.OrderTax = erpOrder.OrderTax ?? decimal.Zero;
                oldNopOrder.OrderShippingInclTax = erpOrder.ShippingAmount;
                oldNopOrder.StoreId = (await _storeContext.GetCurrentStoreAsync()).Id;
                if (erpOrder.OrderType == ((int)ErpOrderType.B2BSalesOrder).ToString())
                {
                    oldNopOrder.OrderStatusId = (int)OrderStatus.Processing;
                }
                else if (erpOrder.OrderType == ((int)ErpOrderType.B2BQuote).ToString())
                {
                    oldNopOrder.OrderStatusId = (int)OrderStatus.Pending;
                }
                oldNopOrder.CustomerCurrencyCode = currency.CurrencyCode;
                oldNopOrder.CurrencyRate = currency.Rate;
                oldNopOrder.CustomerTaxDisplayTypeId = (int)TaxDisplayType.ExcludingTax;
                await _orderService.UpdateOrderAsync(oldNopOrder);
            }

            #endregion

            #region Erp Order

            if (oldErpOrder == null)
            {
                var newErpShiptoAddress = new ErpShipToAddress
                {
                    ShipToCode = erpAccount.AccountNumber,
                    ShipToName = erpAccount.AccountName,
                    AddressId = oldNopOrder.ShippingAddressId ?? 0,
                    CreatedOnUtc = DateTime.UtcNow,
                    IsActive = true,
                    DeliveryNotes = erpOrder.Notes,
                    RepNumber = string.Empty
                };

                await _erpShipToAddressService.InsertErpShipToAddressAsync(newErpShiptoAddress);
                await _erpShipToAddressService.InsertErpShipToAddressErpAccountMapAsync(
                    erpAccount,
                    newErpShiptoAddress,
                    ErpShipToAddressCreatedByType.User
                );

                oldErpOrder = new ErpOrderAdditionalData();
                oldErpOrder.NopOrderId = oldNopOrder.Id;
                oldErpOrder.ErpOrderNumber = erpOrder.ErpOrderNumber;
                oldErpOrder.ErpOrderOriginType = ErpOrderOriginType.ERPOrder;
                oldErpOrder.ErpOrderType = (ErpOrderType)Enum.Parse(typeof(ErpOrderType), erpOrder.OrderType);
                oldErpOrder.OrderPlacedByNopCustomerId = oldNopOrder.CustomerId;
                oldErpOrder.ChangedOnUtc = DateTime.UtcNow;
                oldErpOrder.LastERPUpdateUtc = DateTime.UtcNow;
                oldErpOrder.QuoteExpiryDate = erpOrder.DeliveryDate;
                oldErpOrder.ErpAccountId = erpAccount.Id;
                oldErpOrder.ErpShipToAddressId = newErpShiptoAddress.Id;
                oldErpOrder.SpecialInstructions = erpOrder.DeliveryInstruction;
                oldErpOrder.CustomerReference = erpOrder.CustomerReference;
                oldErpOrder.ERPOrderStatus = nameof(OrderStatus.Processing);
                oldErpOrder.DeliveryDate = erpOrder.DeliveryDate;
                oldErpOrder.IntegrationStatusType = IntegrationStatusType.Confirmed;
                oldErpOrder.IntegrationError = string.Empty;
                oldErpOrder.QuoteSalesOrderId = 0;
                oldErpOrder.IntegrationRetries = 0;
                oldErpOrder.IntegrationErrorDateTimeUtc = null;
                oldErpOrder.IsShippingAddressModified = false;
                oldErpOrder.IsOrderPlaceNotificationSent = true;

                erpNopUser = erpNopUser == null ? await _erpNopUserService.GetErpNopUserByCustomerIdAsync(oldNopOrder.CustomerId) : null;

                oldErpOrder.ErpOrderPlaceByCustomerTypeId = 0;
                oldErpOrder.ChangedById = erpNopUser?.NopCustomerId ?? 0;

                await _erpOrderAdditionalDataService.InsertErpOrderAdditionalDataAsync(oldErpOrder);
            }
            else
            {
                oldErpOrder.NopOrderId = oldNopOrder.Id;
                oldErpOrder.ErpOrderNumber = erpOrder.ErpOrderNumber;
                oldErpOrder.ErpOrderType = (ErpOrderType)Enum.Parse(typeof(ErpOrderType), erpOrder.OrderType);
                oldErpOrder.ChangedOnUtc = DateTime.UtcNow;
                oldErpOrder.LastERPUpdateUtc = DateTime.UtcNow;
                oldErpOrder.QuoteExpiryDate = erpOrder.DeliveryDate;
                oldErpOrder.QuoteSalesOrderId = 0;
                oldErpOrder.ErpAccountId = erpAccount.Id;
                oldErpOrder.SpecialInstructions = erpOrder.DeliveryInstruction;
                oldErpOrder.CustomerReference = erpOrder.CustomerReference;
                oldErpOrder.ERPOrderStatus = nameof(OrderStatus.Processing);
                oldErpOrder.DeliveryDate = erpOrder.DeliveryDate;
                oldErpOrder.IntegrationStatusType = IntegrationStatusType.Confirmed;
                oldErpOrder.IntegrationError = !string.IsNullOrWhiteSpace(oldErpOrder.IntegrationError) ? oldErpOrder.IntegrationError : string.Empty;

                await _erpOrderAdditionalDataService.UpdateErpOrderAdditionalDataAsync(oldErpOrder);
            }

            #region Cache clear for this erp order

            await _erpDataClearCacheService.ClearCacheOfEntity(oldErpOrder);

            #endregion

            #endregion

            #region Nop Order Items and Erp Order Items

            var nopOrderItems = await _orderService.GetOrderItemsAsync(orderId: oldNopOrder.Id);
            var erpOrderItems = await _erpOrderItemAdditionalDataService.
                GetAllErpOrderItemAdditionalDataByErpOrderIdAsync(oldErpOrder.Id);

            foreach (var item in erpOrder.ErpPlaceOrderItemDatas)
            {
                var product = await _productService.GetProductBySkuAsync(item.Sku);

                if (product == null)
                {
                    continue;
                }

                var nopOrderItem = nopOrderItems?.FirstOrDefault(prd => prd.ProductId == product.Id);

                if (nopOrderItem == null)
                {
                    nopOrderItem = new OrderItem();
                    nopOrderItem.OrderItemGuid = Guid.NewGuid();
                    nopOrderItem.OrderId = oldNopOrder.Id;
                    nopOrderItem.ProductId = product.Id;
                    nopOrderItem.Quantity = Convert.ToInt16(item.Quantity);
                    nopOrderItem.PriceInclTax = item.PriceInclTax ?? 0;
                    nopOrderItem.PriceExclTax = item.PriceExclTax ?? 0;
                    nopOrderItem.UnitPriceInclTax = item.UnitPriceInclTax ?? 0;
                    nopOrderItem.UnitPriceExclTax = item.UnitPriceExclTax ?? 0;
                    nopOrderItem.DiscountAmountInclTax = item.DiscountAmountInclTax ?? 0;
                    nopOrderItem.DiscountAmountExclTax = item.DiscountAmountExclTax ?? 0;

                    await _orderService.InsertOrderItemAsync(nopOrderItem);
                }
                else
                {
                    nopOrderItem.Quantity = Convert.ToInt16(item.Quantity);
                    nopOrderItem.PriceInclTax = item.PriceInclTax ?? 0;
                    nopOrderItem.PriceExclTax = item.PriceExclTax ?? 0;
                    nopOrderItem.UnitPriceInclTax = item.UnitPriceInclTax ?? 0;
                    nopOrderItem.UnitPriceExclTax = item.UnitPriceExclTax ?? 0;
                    nopOrderItem.DiscountAmountInclTax = item.DiscountAmountInclTax ?? 0;
                    nopOrderItem.DiscountAmountExclTax = item.DiscountAmountExclTax ?? 0;

                    await _orderService.UpdateOrderItemAsync(nopOrderItem);
                }

                var erpOrderItem = erpOrderItems?.FirstOrDefault(prd => prd.NopOrderItemId == nopOrderItem.Id);

                if (erpOrderItem == null)
                {
                    erpOrderItem = new ErpOrderItemAdditionalData();
                    erpOrderItem.NopOrderItemId = nopOrderItem.Id;
                    erpOrderItem.ErpOrderId = oldErpOrder.Id;
                    erpOrderItem.ErpOrderLineNumber = string.Empty;
                    erpOrderItem.ErpSalesUoM = item.UnitOfMeasure;
                    erpOrderItem.ErpOrderLineStatus = string.Empty;
                    erpOrderItem.ErpOrderLineNotes = string.Empty;
                    erpOrderItem.ErpDeliveryMethod = erpOrder.DeliveryMethod;
                    erpOrderItem.ErpInvoiceNumber = string.Empty;
                    erpOrderItem.ChangedBy = 1;
                    erpOrderItem.ErpDateRequired = erpOrder.DateRequired;
                    erpOrderItem.ErpDateExpected = erpOrder.DeliveryDate;
                    erpOrderItem.LastErpUpdateUtc = DateTime.UtcNow;
                    erpOrderItem.ChangedOnUtc = DateTime.UtcNow;

                    await _erpOrderItemAdditionalDataService.InsertErpOrderItemAdditionalDataAsync(erpOrderItem);
                }
                else
                {
                    erpOrderItem.ErpOrderLineNumber = !string.IsNullOrWhiteSpace(erpOrderItem.ErpOrderLineNumber) ? erpOrderItem.ErpOrderLineNumber : string.Empty;
                    erpOrderItem.ErpSalesUoM = item.UnitOfMeasure;
                    erpOrderItem.ErpOrderLineStatus = !string.IsNullOrWhiteSpace(erpOrderItem.ErpOrderLineStatus) ? erpOrderItem.ErpOrderLineStatus : string.Empty;
                    erpOrderItem.ErpOrderLineNotes = !string.IsNullOrWhiteSpace(erpOrderItem.ErpOrderLineNotes) ? erpOrderItem.ErpOrderLineNotes : string.Empty;
                    erpOrderItem.ErpDeliveryMethod = erpOrder.DeliveryMethod;
                    erpOrderItem.ErpInvoiceNumber = !string.IsNullOrWhiteSpace(erpOrderItem.ErpInvoiceNumber) ? erpOrderItem.ErpInvoiceNumber : string.Empty;
                    erpOrderItem.ChangedBy = 1;
                    erpOrderItem.ErpDateRequired = erpOrder.DateRequired;
                    erpOrderItem.ErpDateExpected = erpOrder.DeliveryDate;
                    erpOrderItem.LastErpUpdateUtc = DateTime.UtcNow;

                    await _erpOrderItemAdditionalDataService.UpdateErpOrderItemAdditionalDataAsync(erpOrderItem);
                }

                #region Cache clear for this nop order items and erp order items

                await _erpDataClearCacheService.ClearCacheOfEntity(erpOrderItem);

                #endregion
            }

            #endregion

            lastErpOrderSynced = oldErpOrder?.ErpOrderNumber ?? string.Empty;
            lastErpOrderSyncedOfErpAccount = erpAccount.AccountNumber;
            totalSyncedSoFar++;
        }

        return (lastErpOrderSynced, lastErpOrderSyncedOfErpAccount, totalSyncedSoFar, totalNotSyncedSoFar);
    }

    #endregion
}