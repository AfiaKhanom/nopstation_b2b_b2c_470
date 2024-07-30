using LinqToDB.Common;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Orders;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices
{
    public class ErpOrderSyncService : IErpOrderSyncService
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IProductService _productService;
        private readonly IAddressService _addressService;
        private readonly ICustomerService _customerService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ISyncLogService _erpSyncLogService;
        private readonly IErpNopUserService _erpNopUserService;
        private readonly IErpAccountService _erpAccountService;
        private readonly IErpSalesOrgService _erpSalesOrgService;
        private readonly IErpShipToAddressService _erpShipToAddressService;
        private readonly IErpDataClearCacheService _erpDataClearCacheService;
        private readonly IErpIntegrationPluginManager _erpIntegrationPluginService;
        private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
        private readonly IErpOrderItemAdditionalDataService _erpOrderItemAdditionalDataService;

        #endregion

        #region Ctor

        public ErpOrderSyncService(
            IOrderService orderService,
            IStoreContext storeContext,
            ISettingService settingService,
            IProductService productService,
            IAddressService addressService,
            ICustomerService customerService,
            IStateProvinceService stateProvinceService,
            ISyncLogService erpSyncLogService,
            IErpNopUserService erpNopUserService,
            IErpAccountService erpAccountService,
            IErpSalesOrgService erpSalesOrgService,
            IErpShipToAddressService erpSShipToAddressService,
            IErpDataClearCacheService erpDataClearCacheService,
            IErpIntegrationPluginManager erpIntegrationPluginService,
            IErpOrderAdditionalDataService erpOrderAdditionalDataService,
            IErpOrderItemAdditionalDataService erpOrderItemAdditionalDataService)
        {
            _orderService = orderService;
            _storeContext = storeContext;
            _settingService = settingService;
            _productService = productService;
            _addressService = addressService;
            _customerService = customerService;
            _stateProvinceService = stateProvinceService;
            _erpSyncLogService = erpSyncLogService;
            _erpNopUserService = erpNopUserService;
            _erpAccountService = erpAccountService;
            _erpSalesOrgService = erpSalesOrgService;
            _erpShipToAddressService = erpSShipToAddressService;
            _erpDataClearCacheService = erpDataClearCacheService;
            _erpIntegrationPluginService = erpIntegrationPluginService;
            _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
            _erpOrderItemAdditionalDataService = erpOrderItemAdditionalDataService;
        }

        #endregion

        #region Method

        private async Task CreateNopUser(ErpNopUser erpNopUser, Customer customer, ErpAccount erpAccount, int erpShipToAddressId)
        {
            erpNopUser.NopCustomerId = customer.Id;
            erpNopUser.CreatedById = 1;
            erpNopUser.CreatedOnUtc = DateTime.UtcNow;
            erpNopUser.ErpShipToAddressId = erpShipToAddressId != 0 ? erpShipToAddressId : customer.ShippingAddressId ?? 0;
            erpNopUser.ShippingErpShipToAddressId = customer.ShippingAddressId ?? 0;
            erpNopUser.BillingErpShipToAddressId = customer.BillingAddressId ?? 0;
            erpNopUser.ErpAccountId = erpAccount.Id;
            erpNopUser.ErpAccount = erpAccount;
            erpNopUser.ErpUserType = ErpUserType.B2BUser;
            erpNopUser.IsActive = true;
            erpNopUser.UpdatedOnUtc = DateTime.UtcNow;

            await _erpNopUserService.InsertErpNopUserAsync(erpNopUser);
        }

        public virtual async Task<bool> IsErpOrderSyncSuccessfulAsync()
        {
            var erpIntegrationPlugin = await _erpIntegrationPluginService.LoadActiveERPIntegrationPlugin();

            if (erpIntegrationPlugin is null)
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                    ErpSyncLavel.Order,
                    "No integration method found.");

                return false;
            }

            try
            {
                #region Data collection

                var salesOrgCode = await erpIntegrationPlugin.GetSalesOrgCodeFromIQIntegrationSettings();
                if (salesOrgCode == null)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                        ErpSyncLavel.Order,
                        $"No Sales org was configured in the erp integration settings. Unable to run {ErpDataSchedulerDefaults.ErpOrderSyncTaskName}.");

                    return false;
                }

                var salesOrg = (await _erpSalesOrgService.GetAllErpSalesOrgAsync(code: salesOrgCode)).FirstOrDefault();
                if (salesOrg == null)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                        ErpSyncLavel.Order,
                        $"No Sales org found with Sales org code: {salesOrgCode}. Unable to run {ErpDataSchedulerDefaults.ErpOrderSyncTaskName}.");

                    return false;
                }

                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var erpDataSchedulerSettings = await _settingService.LoadSettingAsync<ErpDataSchedulerSettings>(storeScope);
                var b2BB2CFeaturesSettings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(storeScope);

                var allStateProvinces = (await _stateProvinceService.GetStateProvincesAsync()).ToList();

                var oldErpAccounts = (List<ErpAccount>)await _erpAccountService.GetAllErpAccountsAsync(salesOrgId: salesOrg.Id);
                if (!oldErpAccounts.Any())
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                        ErpSyncLavel.Order,
                        $"No Erp Accounts found with the Sales org : {salesOrg.Name}");

                    return false;
                }

                var lastErpOrderSynced = new ErpOrderAdditionalData();
                var lastErpOrderSyncedOfErpAccount = "";
                var totalSyncedSoFar = 0;
                var isError = false;
                var lastErrorMessage = "";

                #endregion

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                    ErpSyncLavel.Order,
                    "Erp Order Sync started.");

                foreach (var erpAccount in oldErpAccounts)
                {
                    var start = "0";
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

                        var response = await erpIntegrationPlugin.GetOrderByAccountFromErpAsync(erpGetRequestModel);

                        if (response.ErpResponseModel.IsError || response.Data is null)
                        {
                            isError = true;
                            lastErrorMessage = $"The last error: {response.ErpResponseModel.ErrorShortMessage}";
                            break;
                        }

                        start = response.ErpResponseModel.Next;

                        var erpOrders = response.Data;

                        (lastErpOrderSynced, lastErpOrderSyncedOfErpAccount, totalSyncedSoFar) = await MapOrderData(
                            erpOrders,
                            erpAccount,
                            lastErpOrderSynced,
                            lastErpOrderSyncedOfErpAccount,
                            totalSyncedSoFar,
                            allStateProvinces,
                            b2BB2CFeaturesSettings.DefaultCountryId);

                    }
                    if (erpDataSchedulerSettings.NeedQuoteOrderCall)
                    {
                        start = "0";
                        while (true)
                        {
                            var erpGetRequestModel = new ErpGetRequestModel
                            {
                                Start = start,
                                AccountNumber = erpAccount.AccountNumber,
                                Location = salesOrg.Code,
                                DateFrom = erpAccount.LastTimeOrderSyncOnUtc.HasValue ? erpAccount.LastTimeOrderSyncOnUtc : dateFrom
                            };

                            var response = await erpIntegrationPlugin.GetQuoteByAccountFromErpAsync(erpGetRequestModel);

                            if (response.ErpResponseModel.IsError || response.Data is null)
                            {
                                isError = true;
                                lastErrorMessage = $"The last error: {response.ErpResponseModel.ErrorShortMessage}";
                                break;
                            }

                            start = response.ErpResponseModel.Next;

                            var erpOrders = response.Data;

                            (lastErpOrderSynced, lastErpOrderSyncedOfErpAccount, totalSyncedSoFar) = await MapOrderData(
                                erpOrders,
                                erpAccount,
                                lastErpOrderSynced,
                                lastErpOrderSyncedOfErpAccount,
                                totalSyncedSoFar,
                                allStateProvinces,
                                b2BB2CFeaturesSettings.DefaultCountryId);
                        }
                    }
                }

                if (!isError)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                        ErpSyncLavel.Order,
                        $"Erp Order sync successful for Sales Org: {salesOrg.Name}");
                }
                else
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                        ErpSyncLavel.Order,
                        $"Erp Order sync is partially or not successful for Sales Org: {salesOrg.Name}",
                        lastErrorMessage);
                }

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                    ErpSyncLavel.Order,
                    (lastErpOrderSynced is not null ? $"The last synced Erp Order: {lastErpOrderSynced.ErpOrderNumber}, of Erp Account: {lastErpOrderSyncedOfErpAccount} for Sales Org: {salesOrg.Name}. " : string.Empty) + $"Total synced in this session: {totalSyncedSoFar}");



                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                    ErpSyncLavel.Order,
                    "Erp Order Sync ended.");

                return true;
            }
            catch (Exception ex)
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                    ErpSyncLavel.Order,
                    ex.Message,
                    ex.StackTrace);

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                    ErpSyncLavel.Order,
                    "Erp Order Sync ended.");

                return false;
            }
        }

        private async Task<(ErpOrderAdditionalData lastErpOrderSynced, string lastErpOrderSyncedOfErpAccount, int totalSyncedSoFar)> MapOrderData(
            IList<ErpPlaceOrderDataModel> erpOrders,
            ErpAccount erpAccount,
            ErpOrderAdditionalData lastErpOrderSynced,
            string lastErpOrderSyncedOfErpAccount,
            int totalSyncedSoFar,
            List<StateProvince> allStateProvinces,
            int defaultCountryId)
        {
            foreach (var erpOrder in erpOrders)
            {
                #region Nop Order

                var erpNopUser = new ErpNopUser();

                var oldNopOrder = await _orderService.GetOrderByCustomOrderNumberAsync(erpOrder.Reference) ?? new Order();

                if (oldNopOrder.Id <= 0)
                {
                    #region Address

                    var address = new Address();
                    address.Email = erpOrder.CustomerEmail ?? string.Empty;
                    address.PhoneNumber = erpOrder.CustomerPhoneNumber ?? string.Empty;
                    address.Address1 = erpOrder.ShippingAddress?.AddressLine1;
                    address.Address2 = erpOrder.ShippingAddress?.AddressLine2;
                    address.City = erpOrder.ShippingAddress?.AddressLine3;
                    address.ZipPostalCode = erpOrder.ShippingAddress?.PostalCode;
                    address.CountryId = defaultCountryId;
                    address.StateProvinceId = allStateProvinces.Find(state => state.CountryId == defaultCountryId)?.Id ?? 0;
                    address.CreatedOnUtc = DateTime.UtcNow;
                    await _addressService.InsertAddressAsync(address);

                    #endregion

                    var customer = await _customerService.GetCustomerByEmailAsync(erpOrder.CustomerEmail) ?? new Customer();
                    if (customer.Id <= 0)
                    {
                        customer.FirstName = erpOrder.CustomerFirstName ?? string.Empty;
                        customer.LastName = erpOrder.CustomerLastName ?? string.Empty;
                        customer.Email = erpOrder.CustomerEmail ?? string.Empty;
                        customer.City = address.City ?? string.Empty;
                        customer.CountryId = address.CountryId ?? 0;
                        customer.Company = address.Company ?? string.Empty;
                        customer.BillingAddressId = address.Id;
                        customer.ShippingAddressId = address.Id;
                        customer.Active = true;
                        customer.CreatedOnUtc = DateTime.UtcNow;
                        customer.CustomerGuid = Guid.NewGuid();

                        await _customerService.InsertCustomerAsync(customer);
                        var erpShiptoAddressforAccount = await _erpShipToAddressService.GetErpShipToAddressesByAccountIdAsync(showHidden: false, isActiveOnly: true, accountId: erpAccount.Id);
                        await CreateNopUser(erpNopUser, customer, erpAccount, erpShiptoAddressforAccount.FirstOrDefault()?.Id ?? 0);
                    }
                    else
                    {
                        var isCustomerHasAdminRole = await _customerService.IsAdminAsync(customer);

                        if (isCustomerHasAdminRole)
                        {
                            continue;
                        }

                        erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer.Id) ?? new ErpNopUser();
                        if (erpNopUser.Id <= 0)
                        {
                            var erpShiptoAddressforAccount = await _erpShipToAddressService.GetErpShipToAddressesByAccountIdAsync(showHidden: false, isActiveOnly: true, accountId: erpAccount.Id);
                            await CreateNopUser(erpNopUser, customer, erpAccount, erpShiptoAddressforAccount.FirstOrDefault()?.Id ?? 0);
                        }
                    }

                    oldNopOrder.OrderGuid = Guid.NewGuid();
                    oldNopOrder.CustomerId = customer.Id;
                    oldNopOrder.BillingAddressId = erpAccount.BillingAddressId ?? address.Id;
                    oldNopOrder.ShippingAddressId = address.Id;
                    oldNopOrder.StoreId = 1;
                    oldNopOrder.CustomOrderNumber = erpOrder.Reference;
                    oldNopOrder.PickupInStore = false;
                    if (erpOrder.OrderType == ErpDataSchedulerDefaults.ErpOrderType)
                    {
                        oldNopOrder.OrderStatusId = (int)OrderStatus.Processing;
                        oldNopOrder.ShippingStatusId = (int)ShippingStatus.Delivered;
                    }
                    else if (erpOrder.OrderType == ErpDataSchedulerDefaults.ErpQuoteType)
                    {
                        oldNopOrder.OrderStatusId = (int)OrderStatus.Pending;
                        oldNopOrder.ShippingStatusId = (int)ShippingStatus.NotYetShipped;
                    }
                    oldNopOrder.PaymentStatusId = (int)PaymentStatus.Paid;
                    oldNopOrder.PaymentMethodSystemName = string.Empty;
                    oldNopOrder.CustomerCurrencyCode = "ZAR";
                    oldNopOrder.CurrencyRate = 1;
                    oldNopOrder.CustomerTaxDisplayTypeId = (int)TaxDisplayType.ExcludingTax;
                    oldNopOrder.PaidDateUtc = DateTime.UtcNow;
                    oldNopOrder.CreatedOnUtc = DateTime.UtcNow;
                    await _orderService.InsertOrderAsync(oldNopOrder);
                }
                else
                {
                    oldNopOrder.CustomOrderNumber = erpOrder.Reference ?? string.Empty;
                    if (erpOrder.OrderType == ErpDataSchedulerDefaults.ErpOrderType)
                    {
                        oldNopOrder.OrderStatusId = (int)OrderStatus.Processing;
                        oldNopOrder.ShippingStatusId = (int)ShippingStatus.Delivered;
                    }
                    else if (erpOrder.OrderType == ErpDataSchedulerDefaults.ErpQuoteType)
                    {
                        oldNopOrder.OrderStatusId = (int)OrderStatus.Pending;
                        oldNopOrder.ShippingStatusId = (int)ShippingStatus.NotYetShipped;
                    }
                    await _orderService.UpdateOrderAsync(oldNopOrder);
                }

                #region Cache clear for this nop order

                await _erpDataClearCacheService.ClearCacheOfEntity(oldNopOrder, oldNopOrder.Id);
                await _erpDataClearCacheService.ClearCacheOfEntity(erpNopUser, erpNopUser.Id);

                #endregion

                #endregion

                #region Erp Order

                var oldErpOrder = (await _erpOrderAdditionalDataService
                    .GetAllErpOrderAdditionalDataAsync(accountId: erpAccount.Id, nopOrderNumber: erpOrder.Reference)).FirstOrDefault() ?? new ErpOrderAdditionalData();

                if (oldErpOrder.Id <= 0)
                {
                    oldErpOrder.NopOrderId = oldNopOrder.Id;
                    oldErpOrder.ErpOrderNumber = erpOrder.QuoteNumber ?? string.Empty;
                    oldErpOrder.ErpOrderOriginType = ErpOrderOriginType.ERPOrder;
                    oldErpOrder.ErpOrderType = (ErpOrderType)Enum.Parse(typeof(ErpOrderType), erpOrder.OrderType);
                    oldErpOrder.OrderPlacedByNopCustomerId = oldNopOrder.CustomerId;
                    oldErpOrder.ChangedOnUtc = DateTime.UtcNow;
                    oldErpOrder.LastERPUpdateUtc = DateTime.UtcNow;
                    oldErpOrder.QuoteExpiryDate = erpOrder.DeliveryDate;
                    oldErpOrder.QuoteSalesOrderId = 0;
                    oldErpOrder.ErpAccountId = erpAccount.Id;

                    var newErpShiptoAddress = new ErpShipToAddress
                    {
                        ShipToCode = erpAccount.AccountNumber,
                        ShipToName = erpAccount.AccountName,
                        AddressId = oldNopOrder.ShippingAddressId ?? 0,
                        CreatedOnUtc = DateTime.UtcNow,
                        IsActive = true,
                        DeliveryNotes = erpOrder.Notes
                    };

                    await _erpShipToAddressService.InsertErpShipToAddressAsync(newErpShiptoAddress);

                    oldErpOrder.ErpShipToAddressId = newErpShiptoAddress.Id;
                    oldErpOrder.SpecialInstructions = erpOrder.DelInstruction1 ?? string.Empty;
                    oldErpOrder.CustomerReference = erpOrder.CustomerReference ?? string.Empty;
                    oldErpOrder.ERPOrderStatus = OrderStatus.Processing.ToString(); // need to verify
                    oldErpOrder.DeliveryDate = erpOrder.DeliveryDate;
                    oldErpOrder.IntegrationStatusType = IntegrationStatusType.Confirmed;
                    oldErpOrder.IntegrationError = string.Empty;
                    oldErpOrder.ErpOrderItemAdditionalDatas = new List<ErpOrderItemAdditionalData>();

                    await _erpOrderAdditionalDataService.InsertErpOrderAdditionalDataAsync(oldErpOrder);
                }
                else
                {
                    oldErpOrder.NopOrderId = oldNopOrder.Id;
                    oldErpOrder.ErpOrderNumber = erpOrder.QuoteNumber ?? string.Empty;
                    oldErpOrder.ErpOrderOriginType = ErpOrderOriginType.ERPOrder;
                    oldErpOrder.ErpOrderType = (ErpOrderType)Enum.Parse(typeof(ErpOrderType), erpOrder.OrderType);
                    oldErpOrder.OrderPlacedByNopCustomerId = oldNopOrder.CustomerId;
                    oldErpOrder.ChangedOnUtc = DateTime.UtcNow;
                    oldErpOrder.LastERPUpdateUtc = DateTime.UtcNow;
                    oldErpOrder.QuoteExpiryDate = erpOrder.DeliveryDate;

                    oldErpOrder.QuoteSalesOrderId = 0;
                    oldErpOrder.ErpAccountId = erpAccount.Id;
                    oldErpOrder.SpecialInstructions = erpOrder.DelInstruction1 ?? string.Empty;
                    oldErpOrder.CustomerReference = erpOrder.CustomerReference ?? string.Empty;
                    oldErpOrder.ERPOrderStatus = OrderStatus.Processing.ToString(); // need to verify
                    oldErpOrder.DeliveryDate = erpOrder.DeliveryDate;

                    oldErpOrder.IntegrationStatusType = IntegrationStatusType.Confirmed;
                    oldErpOrder.IntegrationError = string.Empty;

                    await _erpOrderAdditionalDataService.UpdateErpOrderAdditionalDataAsync(oldErpOrder);
                }

                #region Cache clear for this erp order

                await _erpDataClearCacheService.ClearCacheOfEntity(oldErpOrder, oldErpOrder.Id);

                #endregion

                #endregion

                #region Nop Order Items and Erp Order Items

                var nopOrderItems = await _orderService.GetOrderItemsAsync(orderId: oldNopOrder.Id) ?? new List<OrderItem>();
                var erpOrderItems = await _erpOrderItemAdditionalDataService.GetAllErpOrderItemAdditionalDataByErpOrderIdAsync(oldErpOrder.Id);

                foreach (var item in erpOrder.ErpPlaceOrderItemDatas)
                {
                    var product = await _productService.GetProductBySkuAsync(item.ItemNo) ?? new Product();

                    if (product.Id == 0)
                    {
                        continue;
                    }

                    var nopOrderItem = nopOrderItems.FirstOrDefault(prd => prd.ProductId == product.Id) ?? new OrderItem();

                    if (nopOrderItem.Id <= 0)
                    {
                        nopOrderItem.OrderItemGuid = Guid.NewGuid();
                        nopOrderItem.OrderId = oldNopOrder.Id;
                        nopOrderItem.ProductId = product.Id;
                        nopOrderItem.Quantity = Convert.ToInt16(item.Quantity);
                        nopOrderItem.PriceInclTax = Convert.ToInt16(item.UnitPrice);
                        nopOrderItem.UnitPriceInclTax = item.LineTotalIncl;
                        nopOrderItem.UnitPriceExclTax = item.LineTotalExcl;
                        nopOrderItem.RentalStartDateUtc = DateTime.UtcNow;
                        nopOrderItem.RentalEndDateUtc = DateTime.UtcNow.AddMonths(1);

                        await _orderService.InsertOrderItemAsync(nopOrderItem);
                    }
                    else
                    {
                        nopOrderItem.UnitPriceInclTax = item.LineTotalIncl;
                        nopOrderItem.UnitPriceExclTax = item.LineTotalExcl;
                        nopOrderItem.Quantity = Convert.ToInt16(item.Quantity);

                        await _orderService.UpdateOrderItemAsync(nopOrderItem);
                    }

                    var erpOrderItem = erpOrderItems.FirstOrDefault(prd => prd.NopOrderItemId == nopOrderItem.Id) ?? new ErpOrderItemAdditionalData();

                    if (erpOrderItem.Id <= 0)
                    {
                        erpOrderItem.NopOrderItemId = nopOrderItem.Id;
                        erpOrderItem.ErpOrderId = oldErpOrder.Id;
                        erpOrderItem.ErpOrderLineNumber = string.Empty;
                        erpOrderItem.ErpSalesUoM = string.Empty;
                        erpOrderItem.ErpOrderLineStatus = string.Empty;
                        erpOrderItem.ErpOrderLineNotes = string.Empty;
                        erpOrderItem.ErpDeliveryMethod = erpOrder.DelMethod ?? string.Empty;
                        erpOrderItem.ErpInvoiceNumber = erpOrder.QuoteNumber ?? string.Empty;
                        erpOrderItem.ChangedBy = 1;
                        erpOrderItem.ErpDateRequired = erpOrder.DeliveryDate;
                        erpOrderItem.ErpDateExpected = erpOrder.DeliveryDate;
                        erpOrderItem.LastErpUpdateUtc = DateTime.UtcNow;
                        erpOrderItem.ChangedOnUtc = DateTime.UtcNow;

                        await _erpOrderItemAdditionalDataService.InsertErpOrderItemAdditionalDataAsync(erpOrderItem);
                    }
                    else
                    {
                        erpOrderItem.LastErpUpdateUtc = DateTime.UtcNow;
                        await _erpOrderItemAdditionalDataService.UpdateErpOrderItemAdditionalDataAsync(erpOrderItem);
                    }

                    #region Cache clear for this nop order items and erp order items

                    await _erpDataClearCacheService.ClearCacheOfEntity(nopOrderItem, nopOrderItem.Id);
                    await _erpDataClearCacheService.ClearCacheOfEntity(erpOrderItem, erpOrderItem.Id);

                    #endregion
                }

                #endregion

                lastErpOrderSynced = oldErpOrder;
                lastErpOrderSyncedOfErpAccount = erpAccount.AccountNumber;
                totalSyncedSoFar++;
            }

            return (lastErpOrderSynced, lastErpOrderSyncedOfErpAccount, totalSyncedSoFar);
        }

        #endregion
    }
}