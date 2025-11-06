using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpSpecificationAttributeService;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpWorkflowMessage;
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.Overriden;

public class OverriddenOrderProcessingService : OrderProcessingService, IOverriddenOrderProcessingService
{
    #region Fields

    private readonly IStoreContext _storeContext;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly IErpOrderItemAdditionalDataService _erpOrderItemAdditionalDataService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpNopUserService _erpNopUserService;
    private readonly IErpLogsService _erpLogsService;
    private readonly IErpSpecificationAttributeService _erpSpecificationAttributeService;
    private readonly IErpWorkflowMessageService _erpWorkflowMessageService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IErpSpecialPriceService _erpSpecialPriceService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginManager;
    private const string DELIVERY_METHOD_COLLECT = "COLLECT";
    private const string DELIVERY_METHOD_DELIVERY = "DELIVERY";

    #endregion

    #region Ctor

    public OverriddenOrderProcessingService(CurrencySettings currencySettings,
        IAddressService addressService,
        IAffiliateService affiliateService,
        ICheckoutAttributeFormatter checkoutAttributeFormatter,
        ICountryService countryService,
        ICurrencyService currencyService,
        ICustomerActivityService customerActivityService,
        ICustomerService customerService,
        ICustomNumberFormatter customNumberFormatter,
        IDiscountService discountService,
        IEncryptionService encryptionService,
        IEventPublisher eventPublisher,
        IGenericAttributeService genericAttributeService,
        IGiftCardService giftCardService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        ILogger logger,
        IOrderService orderService,
        IOrderTotalCalculationService orderTotalCalculationService,
        IPaymentPluginManager paymentPluginManager,
        IPaymentService paymentService,
        IPdfService pdfService,
        IPriceCalculationService priceCalculationService,
        IPriceFormatter priceFormatter,
        IProductAttributeFormatter productAttributeFormatter,
        IProductAttributeParser productAttributeParser,
        IProductService productService,
        IRewardPointService rewardPointService,
        IShipmentService shipmentService,
        IShippingService shippingService,
        IShoppingCartService shoppingCartService,
        IStateProvinceService stateProvinceService,
        IStoreContext storeContext,
        ITaxService taxService,
        IVendorService vendorService,
        IWebHelper webHelper,
        IWorkContext workContext,
        IWorkflowMessageService workflowMessageService,
        LocalizationSettings localizationSettings,
        OrderSettings orderSettings,
        PaymentSettings paymentSettings,
        RewardPointsSettings rewardPointsSettings,
        ShippingSettings shippingSettings,
        TaxSettings taxSettings,
        IReturnRequestService returnRequestService,
        IStoreService storeService,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        IErpSalesOrgService erpSalesOrgService,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        IErpShipToAddressService erpShipToAddressService,
        IErpOrderItemAdditionalDataService erpOrderItemAdditionalDataService,
        IErpAccountService erpAccountService,
        IErpNopUserService erpNopUserService,
        IErpLogsService erpLogsService,
        IErpIntegrationPluginManager erpIntegrationPluginManager,
        IErpSpecificationAttributeService erpSpecificationAttributeService,
        IErpWorkflowMessageService erpWorkflowMessageService,
        IStoreMappingService storeMappingService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IErpSpecialPriceService erpSpecialPriceService) : base(currencySettings,
            addressService,
            affiliateService,
            checkoutAttributeFormatter,
            countryService,
            currencyService,
            customerActivityService,
            customerService,
            customNumberFormatter,
            discountService,
            encryptionService,
            eventPublisher,
            genericAttributeService,
            giftCardService,
            languageService,
            localizationService,
            logger,
            orderService,
            orderTotalCalculationService,
            paymentPluginManager,
            paymentService,
            pdfService,
            priceCalculationService,
            priceFormatter,
            productAttributeFormatter,
            productAttributeParser,
            productService,
            returnRequestService,
            rewardPointService,
            shipmentService,
            shippingService,
            shoppingCartService,
            stateProvinceService,
            storeMappingService,
            storeService,
            taxService,
            vendorService,
            webHelper,
            workContext,
            workflowMessageService,
            localizationSettings,
            orderSettings,
            paymentSettings,
            rewardPointsSettings,
            shippingSettings,
            taxSettings)
    {
        _storeContext = storeContext;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
        _erpShipToAddressService = erpShipToAddressService;
        _erpOrderItemAdditionalDataService = erpOrderItemAdditionalDataService;
        _erpAccountService = erpAccountService;
        _erpNopUserService = erpNopUserService;
        _erpLogsService = erpLogsService;
        _erpIntegrationPluginManager = erpIntegrationPluginManager;
        _erpSpecificationAttributeService = erpSpecificationAttributeService;
        _erpWorkflowMessageService = erpWorkflowMessageService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpSpecialPriceService = erpSpecialPriceService;
    }

    #endregion

    #region Methods

    #region Common

    public override async Task<PlaceOrderResult> PlaceOrderAsync(ProcessPaymentRequest processPaymentRequest)
    {
        ArgumentNullException.ThrowIfNull(processPaymentRequest);

        var result = new PlaceOrderResult();
        try
        {
            if (processPaymentRequest.OrderGuid == Guid.Empty)
                throw new Exception("Order GUID is not generated");

            var details = await PreparePlaceOrderDetailsAsync(processPaymentRequest);

            var processPaymentResult = await GetProcessPaymentResultAsync(
                processPaymentRequest,
                details
            ) ?? throw new NopException("processPaymentResult is not available");

            if (processPaymentResult.Success)
            {
                var order = await SaveOrderDetailsAsync(processPaymentRequest, processPaymentResult, details);
                result.PlacedOrder = order;

                var user = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(await _customerService.GetCustomerByIdAsync(order.CustomerId));

                var shoppingCartItems = details.Cart.ToList();

                await MoveShoppingCartItemsToOrderItemsAsync(details, order);

                await SaveDiscountUsageHistoryAsync(details, order);

                await SaveGiftCardUsageHistoryAsync(details, order);

                if (details.IsRecurringShoppingCart)
                {
                    await CreateFirstRecurringPaymentAsync(processPaymentRequest, order);
                }

                await SendNotificationsAndSaveNotesAsync(order);

                await _customerService.ResetCheckoutDataAsync(details.Customer, processPaymentRequest.StoreId, clearCouponCodes: true, clearCheckoutAttributes: true);
                await _customerActivityService.InsertActivityAsync("PublicStore.PlaceOrder",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.PlaceOrder"), order.Id), order);

                await CheckOrderStatusAsync(order);

                await _eventPublisher.PublishAsync(new OrderPlacedEvent(order));

                if (order.PaymentStatus == PaymentStatus.Paid)
                    await ProcessOrderPaidAsync(order);
            }
            else
                foreach (var paymentError in processPaymentResult.Errors)
                    result.AddError(string.Format(await _localizationService.GetResourceAsync("Checkout.PaymentError"), paymentError));

        }
        catch (Exception exc)
        {
            _logger.Error(exc.Message, exc);
            result.AddError(exc.Message);
        }

        if (result.Success)
        {
            return result;
        }

        var logError = result.Errors.Aggregate("Error while placing order. ",
            (current, next) => $"{current}Error {result.Errors.IndexOf(next) + 1}: {next}. ");
        var customer = await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId);
        _logger.Error(logError, customer: customer);

        return result;
    }

    public async Task<PlaceOrderResult> PlaceQuoteOrderAsync(ProcessPaymentRequest processPaymentRequest)
    {
        ArgumentNullException.ThrowIfNull(processPaymentRequest);

        (var b2BAccount, var b2BUser, var b2CUser) = await GetB2BAccountAndUserOfCurrentCustomerAsync();

        ArgumentNullException.ThrowIfNull(b2BAccount);        

        if (b2BUser != null)
        {
            if (!await _erpCustomerFunctionalityService.IsConsideredAsB2BOrderByB2BUser(b2BUser))
                return await base.PlaceOrderAsync(processPaymentRequest);
        }
        else if (b2CUser != null)
        {
            if (!await _erpCustomerFunctionalityService.IsConsideredAsB2COrderByB2CUser(b2CUser))
                return await base.PlaceOrderAsync(processPaymentRequest);
        }
        else
            return await base.PlaceOrderAsync(processPaymentRequest);

        var result = new PlaceOrderResult();
        try
        {
            if (processPaymentRequest.OrderGuid == Guid.Empty)
                throw new Exception("Order GUID is not generated");

            var details = await PreparePlaceOrderDetailsAsync(processPaymentRequest);

            var processPaymentResult = new ProcessPaymentResult();
            if (processPaymentResult.Success)
            {
                var order = await SaveOrderDetailsAsync(processPaymentRequest, processPaymentResult, details);
                result.PlacedOrder = order;

                await MoveShoppingCartItemsToOrderItemsAsync(details, order);

                await SaveDiscountUsageHistoryAsync(details, order);

                await SaveGiftCardUsageHistoryAsync(details, order);

                if (details.IsRecurringShoppingCart)
                    await CreateFirstRecurringPaymentAsync(processPaymentRequest, order);

                await SendNotificationsAndSaveNotesAsync(order);

                await _customerService.ResetCheckoutDataAsync(details.Customer, processPaymentRequest.StoreId, clearCouponCodes: true, clearCheckoutAttributes: true);
                await _customerActivityService.InsertActivityAsync("PublicStore.PlaceOrder",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.PlaceOrder"), order.Id), order);

                order.OrderStatus = OrderStatus.Complete;
                await _orderService.UpdateOrderAsync(order);

                if (order.PaymentStatus == PaymentStatus.Paid)
                    await ProcessOrderPaidAsync(order);
            }
            else
                foreach (var paymentError in processPaymentResult.Errors)
                    result.AddError(string.Format(await _localizationService.GetResourceAsync("Checkout.PaymentError"), paymentError));
        }
        catch (Exception exc)
        {
            _logger.Error(exc.Message, exc);
            result.AddError(exc.Message);
        }

        if (result.Success)
        {
            return result;
        }

        var logError = result.Errors.Aggregate("Error while placing order. ",
            (current, next) => $"{current}Error {result.Errors.IndexOf(next) + 1}: {next}. ");
        var customer = await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId);
        _logger.Error(logError, customer: customer);

        return result;
    }

    private async Task PlaceOrderOrQuoteOnERPAsync(ErpPlaceOrderDataModel erpPlaceOrderDataModel, ErpOrderAdditionalData erpOrderAdditionalData, Order order)
    {
        var erpIntegrationPlugin = await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin(ErpSyncLevel.Order);

        if (erpIntegrationPlugin == null)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.Order,
                $"Integration method not found. Unable to place Nop Order (ID: {erpOrderAdditionalData.NopOrderId}) at Erp."
            );
            return;
        }

        if (erpPlaceOrderDataModel.OrderType == ERPIntegrationCoreDefaults.ErpB2BOrderFromQuoteType || 
            erpPlaceOrderDataModel.OrderType == ERPIntegrationCoreDefaults.ErpB2COrderFromQuoteType ||
            erpPlaceOrderDataModel.OrderType == ERPIntegrationCoreDefaults.ErpB2BOrderType || 
            erpPlaceOrderDataModel.OrderType == ERPIntegrationCoreDefaults.ErpB2COrderType)
        {
            erpPlaceOrderDataModel.OrderType = ERPIntegrationCoreDefaults.ErpB2BOrderType;
        }
        else if (erpPlaceOrderDataModel.OrderType == ERPIntegrationCoreDefaults.ErpB2BQuoteType || 
            erpPlaceOrderDataModel.OrderType == ERPIntegrationCoreDefaults.ErpB2CQuoteType)
        {
            erpPlaceOrderDataModel.OrderType = ERPIntegrationCoreDefaults.ErpB2BQuoteType;
        }

        var response = await erpIntegrationPlugin.CreateOrderOnErpAsync(erpPlaceOrderDataModel);

        var message = "";

        if (!response.IsError && !string.IsNullOrWhiteSpace(response.OrderNumber))
        {
            erpOrderAdditionalData.ErpOrderNumber = response.OrderNumber;
            erpOrderAdditionalData.ERPOrderStatus = B2BB2CFeaturesDefaults.ErpOrderStatusApproved;
            erpOrderAdditionalData.IntegrationStatusType = IntegrationStatusType.Confirmed;
            erpOrderAdditionalData.LastERPUpdateUtc = DateTime.UtcNow;
            erpOrderAdditionalData.ChangedOnUtc = DateTime.UtcNow;
            await _erpOrderAdditionalDataService.UpdateErpOrderAdditionalDataAsync(erpOrderAdditionalData);

            message = $"Erp {erpPlaceOrderDataModel.OrderType} placed successfully with Erp {erpPlaceOrderDataModel.OrderType} number: {erpOrderAdditionalData.ErpOrderNumber}";
        }
        else
        {
            erpOrderAdditionalData.ERPOrderStatus = B2BB2CFeaturesDefaults.ErpOrderStatusPendingApproval;
            erpOrderAdditionalData.IntegrationStatusType = IntegrationStatusType.Failed;
            erpOrderAdditionalData.LastERPUpdateUtc = DateTime.UtcNow;
            erpOrderAdditionalData.ChangedOnUtc = DateTime.UtcNow;
            await _erpOrderAdditionalDataService.UpdateErpOrderAdditionalDataAsync(erpOrderAdditionalData);

            message = $"Epr {erpPlaceOrderDataModel.OrderType} placement failed.";
        }

        erpOrderAdditionalData.LastERPUpdateUtc = DateTime.UtcNow;
        erpOrderAdditionalData.ChangedOnUtc = DateTime.UtcNow;
        await _erpOrderAdditionalDataService.UpdateErpOrderAdditionalDataAsync(erpOrderAdditionalData);
        await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLevel.Order, $"{message}. {response.ErrorShortMessage}", response.ErrorFullMessage);
    }

    #endregion

    #endregion

    #region B2B

    public override async Task<IList<string>> ReOrderAsync(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        var customer = await _workContext.GetCurrentCustomerAsync();

        var warnings = new List<string>();

        //move shopping cart items (if possible)
        foreach (var orderItem in await _orderService.GetOrderItemsAsync(order.Id))
        {
            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

            warnings.AddRange(await _shoppingCartService.AddToCartAsync(customer, product,
                ShoppingCartType.ShoppingCart, order.StoreId,
                orderItem.AttributesXml, orderItem.UnitPriceExclTax,
                orderItem.RentalStartDateUtc, orderItem.RentalEndDateUtc,
                orderItem.Quantity, false));
        }

        //set checkout attributes
        //comment the code below if you want to disable this functionality
        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.CheckoutAttributes, order.CheckoutAttributesXml, order.StoreId);

        return warnings;
    }

    public async Task PlaceErpOrderAtNopAsync(Order order, ErpOrderType erpOrderType)
    {
        var currentStore = await _storeContext.GetCurrentStoreAsync();
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        var erpNopUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(
            await _customerService.GetCustomerByIdAsync(order.CustomerId)
        );

        if (erpNopUser == null)
        {
            await _erpLogsService.ErrorAsync("Erp Nop User not found", ErpSyncLevel.Order);
            return;
        }

        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(
            erpNopUser.ErpAccountId
        );

        if (erpAccount == null)
        {
            await _erpLogsService.ErrorAsync("Erp Account not found", ErpSyncLevel.Order);
            return;
        }

        var b2BOrderPlaceByCustomerType = erpNopUser.ErpUserType;

        ErpOrderAdditionalData originalQuoteOrder = null;
        if (erpOrderType == ErpOrderType.B2BSalesOrder || erpOrderType == ErpOrderType.B2CSalesOrder)
        {
            var originalQuoteNopOrderIdReference = await _genericAttributeService.GetAttributeAsync<int>(currentCustomer,
                    B2BB2CFeaturesDefaults.B2BOriginalB2BQuoteOrderIdReference,
                    currentStore.Id);

            if (originalQuoteNopOrderIdReference == 0)
            {
                originalQuoteNopOrderIdReference = await _genericAttributeService.GetAttributeAsync<int>(
                    currentCustomer,
                    B2BB2CFeaturesDefaults.B2COriginalB2CQuoteOrderIdReference,
                    currentStore.Id
                );
            }

            if (originalQuoteNopOrderIdReference > 0)
            {
                originalQuoteOrder = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByIdAsync(originalQuoteNopOrderIdReference);

                if (originalQuoteOrder != null)
                {
                    var orderNote = await _localizationService.GetResourceAsync("B2B.B2BOrder.QuoteReferenceNote.QuoteReferenceNumber") + " " + originalQuoteOrder.ErpOrderNumber;
                    await AddOrderNoteAsync(order, orderNote);
                }
            }
        }

        var erpOrderAdditionalData = new ErpOrderAdditionalData
        {
            NopOrderId = order.Id,
            ErpOrderOriginType = ErpOrderOriginType.OnlineOrder,
            ErpAccountId = erpAccount.Id,
            ErpOrderType = erpOrderType,
            QuoteSalesOrderId = originalQuoteOrder?.Id ?? 0,
            IsOrderPlaceNotificationSent = false,
            SpecialInstructions = "",
            CustomerReference = "",
            ERPOrderStatus = order.OrderStatus.ToString(),
            IntegrationStatusTypeId = (int)(order.PaymentStatus == PaymentStatus.Paid ? IntegrationStatusType.Queued : IntegrationStatusType.WaitingForPayment),
            IntegrationError = "",
            IsShippingAddressModified = false,
            ErpShipToAddressId = erpNopUser?.ShippingErpShipToAddressId,
            ErpOrderPlaceByCustomerTypeId = (int)b2BOrderPlaceByCustomerType,
            QuoteExpiryDate =
                erpOrderType == ErpOrderType.B2BQuote || erpOrderType == ErpOrderType.B2CQuote
                    ? DateTime.Now.AddDays(1)
                    : null,
            ChangedById = currentCustomer.Id,
            ChangedOnUtc = DateTime.UtcNow,
            OrderPlacedByNopCustomerId = currentCustomer.Id,
            ErpOrderNumber = string.Empty
        };

        var deliveryDate = DateTime.Now.AddDays(1);

        if (b2BOrderPlaceByCustomerType == ErpUserType.B2BUser)
        {
            deliveryDate = 
                await _genericAttributeService.GetAttributeAsync<DateTime>(currentCustomer, B2BB2CFeaturesDefaults.SelectedB2BDeliveryDateAttribute, currentStore.Id);
        }

        var isShippingAddressModifiedInCheckout = await _genericAttributeService.GetAttributeAsync<bool>(currentCustomer, B2BB2CFeaturesDefaults.IsShippingAddressModifiedInCheckoutAttribute, currentStore.Id);
        var modifiedShipToAddressIdOnCheckout = await _genericAttributeService.GetAttributeAsync<int>(currentCustomer, B2BB2CFeaturesDefaults.ShippingAddressModifiedIdInCheckoutAttribute, currentStore.Id);
        var specialInstructions = "";
        var customerReference = "";
        if (erpNopUser.ErpUserType == ErpUserType.B2BUser)
        {
            specialInstructions = await _genericAttributeService.GetAttributeAsync<string>(currentCustomer, B2BB2CFeaturesDefaults.ProvidedB2BSpecialInstructions, currentStore.Id);
            customerReference = await _genericAttributeService.GetAttributeAsync<string>(currentCustomer, B2BB2CFeaturesDefaults.ProvidedB2BCustomerReferenceAsPO, currentStore.Id);
        }
        else
        {
            specialInstructions = await _genericAttributeService.GetAttributeAsync<String>(currentCustomer, B2BB2CFeaturesDefaults.B2CSpecialInstructions, currentStore.Id);
        }

        erpOrderAdditionalData.CustomerReference = $"{customerReference}";
        erpOrderAdditionalData.DeliveryDate = deliveryDate;
        erpOrderAdditionalData.IsShippingAddressModified = isShippingAddressModifiedInCheckout;

        if (order.PickupInStore)
        {
            erpOrderAdditionalData.ErpShipToAddressId = null;
            erpOrderAdditionalData.SpecialInstructions = specialInstructions;
        }
        else
        {
            ErpShipToAddress erpShipToAddress = null;
            if (modifiedShipToAddressIdOnCheckout > 0)
                erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(modifiedShipToAddressIdOnCheckout);
            else
                erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(
                    erpNopUser?.ShippingErpShipToAddressId ?? erpNopUser?.ErpShipToAddressId ?? 0
                );

            erpShipToAddress ??= order.ShippingAddressId.HasValue
                    ? await _erpShipToAddressService.GetErpShipToAddressByShippingAddressIdAsync(
                        order.ShippingAddressId.Value
                    )
                    : null;

            erpShipToAddress ??= order.ShippingAddressId.HasValue ? 
                await _erpShipToAddressService.GetErpShipToAddressByShippingAddressIdAsync(order.ShippingAddressId.Value) : 
                null;

            if (erpShipToAddress != null)
            {
                await _erpShipToAddressService.InsertErpShipToAddressAsync(erpShipToAddress);
                await _erpShipToAddressService.InsertErpShipToAddressErpAccountMapAsync(
                    erpAccount,
                    erpShipToAddress,
                    ErpShipToAddressCreatedByType.User
                );
                erpOrderAdditionalData.ErpShipToAddressId = erpShipToAddress.Id;

                erpOrderAdditionalData.SpecialInstructions = erpShipToAddress.DeliveryNotes; 
                order.ShippingAddressId = erpShipToAddress.AddressId;
            }

            erpOrderAdditionalData.SpecialInstructions += specialInstructions;
        }

        if (!string.IsNullOrEmpty(erpOrderAdditionalData.SpecialInstructions))
        {
            await AddOrderNoteAsync(order, $"Special Instructions: {erpOrderAdditionalData.SpecialInstructions}");
        }

        if (!string.IsNullOrEmpty(customerReference))
        {
            if (erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2BSalesOrder || erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2CSalesOrder)
            {
                await AddOrderNoteAsync(order, $"{await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeaturesPlugins.CustomerReference.PurchaseOrderReference")}: {customerReference}");
            }

            else if (erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2BQuote || erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2CQuote)
            {
                await AddOrderNoteAsync(order, $"{await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeaturesPlugins.CustomerReference.QuoteOrderReference")}: {customerReference}");
            }
        }

        await _orderService.UpdateOrderAsync(order);

        if (erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2BQuote || 
            erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2CQuote || 
            order.PaymentStatus == PaymentStatus.Paid)
        {
            erpOrderAdditionalData.IntegrationStatusTypeId = (int)IntegrationStatusType.Queued;
        }
        else
        {
            erpOrderAdditionalData.IntegrationStatusTypeId = (int)IntegrationStatusType.WaitingForPayment;
        }

        await _erpOrderAdditionalDataService.InsertErpOrderAdditionalDataAsync(erpOrderAdditionalData);

        var erpPlaceOrderItemList = new List<ErpPlaceOrderItemDataModel>();

        var orderItemCount = 0;
        var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
        foreach (var nopOrderItem in orderItems)
        {
            orderItemCount++;

            var uom = await _erpSpecificationAttributeService.GetProductUOMByProductIdAndSpecificationAttributeId(nopOrderItem.ProductId, _b2BB2CFeaturesSettings.UnitOfMeasureSpecificationAttributeId) ?? string.Empty;

            var erpSpecialPrice = await _erpSpecialPriceService.GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(erpAccount.Id, nopOrderItem.ProductId);
            var discountPercentage = erpSpecialPrice?.DiscountPerc ?? 0;
            var listPrice = erpSpecialPrice?.ListPrice ?? decimal.Zero;
            var priceWithoutDiscount = listPrice * nopOrderItem.Quantity;

            var orderItemAdditionalData = new ErpOrderItemAdditionalData
            {
                NopOrderItemId = nopOrderItem.Id,
                ErpOrderId = erpOrderAdditionalData.Id,
                ErpOrderLineNumber = $"{(orderItemCount * 10): 000000}",
                ErpSalesUoM = uom,
                ErpOrderLineStatus = "",
                ErpDeliveryMethod = "",
                ErpInvoiceNumber = "",
                ErpOrderLineNotes = "",
                ChangedBy = currentCustomer.Id,
                ErpDateRequired = deliveryDate,
                ErpDateExpected = deliveryDate,
            };

            await _erpOrderItemAdditionalDataService.InsertErpOrderItemAdditionalDataAsync(orderItemAdditionalData);
            var unitPriceWithDiscount = nopOrderItem.UnitPriceExclTax;
            if (nopOrderItem.DiscountAmountExclTax > 0)
            {
                unitPriceWithDiscount = unitPriceWithDiscount + (nopOrderItem.DiscountAmountExclTax / nopOrderItem.Quantity);
            }

            try
            {
                var product = await _productService.GetProductByIdAsync(nopOrderItem.ProductId);
                var priceInclTax = Math.Round(_currencyService.ConvertCurrency(nopOrderItem.PriceInclTax, order.CurrencyRate), 2);
                var priceExclTax = Math.Round(_currencyService.ConvertCurrency(nopOrderItem.PriceExclTax, order.CurrencyRate), 2);
                var unitPriceExclTax = Math.Round(_currencyService.ConvertCurrency(unitPriceWithDiscount, order.CurrencyRate), 2);

                var convertedTotalPrice = _currencyService.ConvertCurrency(priceWithoutDiscount, order.CurrencyRate);
                if (discountPercentage > 0)
                {
                    priceExclTax = convertedTotalPrice;
                }
                erpPlaceOrderItemList.Add(new ErpPlaceOrderItemDataModel
                {
                    Sku = product.Sku,
                    BatchCode = product.ManufacturerPartNumber,
                    Description = product.Name,
                    Quantity = nopOrderItem.Quantity,
                    UnitOfMeasure = uom,
                    SpecialInstruction = erpOrderAdditionalData.SpecialInstructions,
                    UnitPriceExclTax = unitPriceExclTax,
                    DiscountPercentage = discountPercentage,
                    PriceExclTax = priceExclTax,
                    PriceInclTax = priceInclTax,
                });
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"B2B Order Additional Item data prepare error for Account Number: {erpAccount.AccountNumber}", ex);
            }
        }

        if (originalQuoteOrder != null)
        {
            originalQuoteOrder.QuoteSalesOrderId = erpOrderAdditionalData.Id;
            await _erpOrderAdditionalDataService.UpdateErpOrderAdditionalDataAsync(originalQuoteOrder);
        }

        await _genericAttributeService.SaveAttributeAsync(currentCustomer, B2BB2CFeaturesDefaults.IsShippingAddressModifiedInCheckoutAttribute, false, currentStore.Id);
        await _genericAttributeService.SaveAttributeAsync<int?>(currentCustomer, B2BB2CFeaturesDefaults.ShippingAddressModifiedIdInCheckoutAttribute, 0, currentStore.Id);
        await _genericAttributeService.SaveAttributeAsync<string>(currentCustomer, B2BB2CFeaturesDefaults.B2CSpecialInstructions, null, currentStore.Id);

        if (b2BOrderPlaceByCustomerType == ErpUserType.B2BUser)
        {
            DateTime? date = null;
            await _genericAttributeService.SaveAttributeAsync(currentCustomer, B2BB2CFeaturesDefaults.SelectedB2BDeliveryDateAttribute, date, currentStore.Id);
            await _genericAttributeService.SaveAttributeAsync<string>(currentCustomer, B2BB2CFeaturesDefaults.ProvidedB2BCustomerReferenceAsPO, null, currentStore.Id);

            if (_b2BB2CFeaturesSettings.UseERPIntegration && erpOrderAdditionalData.IntegrationStatusType == IntegrationStatusType.Queued)
                await PlaceERPOrderAtERPAsync(order, erpOrderAdditionalData, erpNopUser, erpPlaceOrderItemList, _b2BB2CFeaturesSettings.MaxErpIntegrationOrderPlaceRetries);
        }
    }

    public async Task<(bool, string)> RetryPlaceErpOrderAtErpAsync(ErpOrderAdditionalData erpOrderAdditionalData)
    {
        if (!_b2BB2CFeaturesSettings.UseERPIntegration)
            return (false, "Use of Erp Integration is disabled!");

        if (erpOrderAdditionalData == null)
            return (false, "Erp order details not found");

        if (erpOrderAdditionalData.IntegrationStatusType == IntegrationStatusType.Confirmed)
            return (false, "Order already placed at Erp");

        if (erpOrderAdditionalData.ErpOrderType != ErpOrderType.B2BQuote
            && erpOrderAdditionalData.ErpOrderType != ErpOrderType.B2CQuote
            && erpOrderAdditionalData.IntegrationStatusType
                == IntegrationStatusType.WaitingForPayment)
            return (false, "This Order can't be placed at Erp since this order is not paid");

        var nopOrder = await _orderService.GetOrderByIdAsync(erpOrderAdditionalData.NopOrderId);
        if (nopOrder == null)
            return (false, "Nop Order not found");

        var customer = await _customerService.GetCustomerByIdAsync(nopOrder.CustomerId);
        if (customer is null)
            return (false, "Customer not found");

        var erpNopUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(
            customer
        );
        if (erpNopUser == null)
            return (false, "Erp user not found");

        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(
            erpOrderAdditionalData.ErpAccountId
        );
        if (erpAccount is null)
            return (false, "Erp account not found");

        var erpPlaceOrderItemList = new List<ErpPlaceOrderItemDataModel>();

        var orderItems = await _orderService.GetOrderItemsAsync(nopOrder.Id);
        foreach (var nopOrderItem in orderItems)
        {
            var unitPriceWithDiscount = nopOrderItem.UnitPriceExclTax;
            if (nopOrderItem.DiscountAmountExclTax > 0)
            {
                unitPriceWithDiscount = unitPriceWithDiscount + (nopOrderItem.DiscountAmountExclTax / nopOrderItem.Quantity);
            }

            try
            {
                var uom = await _erpSpecificationAttributeService.GetProductUOMByProductIdAndSpecificationAttributeId(nopOrderItem.ProductId, _b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId) ?? string.Empty;

                var totalQuantityDiscount = _currencyService.ConvertCurrency(nopOrderItem.DiscountAmountExclTax, nopOrder.CurrencyRate);
                var unitDiscount = Math.Round(totalQuantityDiscount / nopOrderItem.Quantity, 2);

                var product = await _productService.GetProductByIdAsync(nopOrderItem.ProductId);

                erpPlaceOrderItemList.Add(new ErpPlaceOrderItemDataModel
                {
                    Sku = product.Sku,
                    BatchCode = product.ManufacturerPartNumber,
                    Description = product.Name,
                    Quantity = nopOrderItem.Quantity,
                    UnitOfMeasure = uom,
                    SpecialInstruction = "",
                    UnitPriceExclTax = Math.Round(_currencyService.ConvertCurrency(unitPriceWithDiscount, nopOrder.CurrencyRate), 2),
                    DiscountPercentage = unitDiscount,
                    PriceExclTax = Math.Round(_currencyService.ConvertCurrency(nopOrderItem.PriceExclTax, nopOrder.CurrencyRate), 2),
                    PriceInclTax = Math.Round(_currencyService.ConvertCurrency(nopOrderItem.PriceInclTax, nopOrder.CurrencyRate), 2),
                });
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"B2B B2BOrderPlaceModel.DetailLine Prepare error (while retry erp place) for Nop Order Id: {nopOrder.Id}", ex);
            }
        }

        if (erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2BSalesOrder || erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2CSalesOrder)
        {
            if (erpOrderAdditionalData.IntegrationStatusType != IntegrationStatusType.WaitingForPayment)
            {
                erpOrderAdditionalData.IntegrationRetries ??= 0;
                erpOrderAdditionalData.IntegrationRetries++;
                await _erpOrderAdditionalDataService.UpdateErpOrderAdditionalDataAsync(erpOrderAdditionalData);

                await PlaceERPOrderAtERPAsync(
                    nopOrder,
                    erpOrderAdditionalData,
                    erpNopUser,
                    erpPlaceOrderItemList,
                    maxRetries: _b2BB2CFeaturesSettings.MaxErpIntegrationOrderPlaceRetries
                );
            }
        }
        else
        {
            erpOrderAdditionalData.IntegrationRetries ??= 0;
            erpOrderAdditionalData.IntegrationRetries++;
            await _erpOrderAdditionalDataService.UpdateErpOrderAdditionalDataAsync(erpOrderAdditionalData);

            await PlaceERPOrderAtERPAsync(
                nopOrder,
                erpOrderAdditionalData,
                erpNopUser,
                erpPlaceOrderItemList,
                maxRetries: _b2BB2CFeaturesSettings.MaxErpIntegrationOrderPlaceRetries
            );
        }

        return (true, string.Empty);
    }

    public async Task PlaceERPOrderAtERPAsync(Order order, ErpOrderAdditionalData erpOrderAdditionalData, ErpNopUser erpNopUser, IList<ErpPlaceOrderItemDataModel> erpPlaceOrderItemData, int maxRetries = 0)
    {
        if (order == null)
        {
            await _erpLogsService.ErrorAsync("Order not found", ErpSyncLevel.Order);
            return;
        }
        if (erpOrderAdditionalData == null)
        {
            await _erpLogsService.ErrorAsync("Erp Order Additional Data not found", ErpSyncLevel.Order);
            return;
        }
        if (erpNopUser == null)
        {
            await _erpLogsService.ErrorAsync("Erp Nop User not found", ErpSyncLevel.Order);
            return;
        }
        if (erpPlaceOrderItemData == null)
        {
            await _erpLogsService.ErrorAsync("Erp Place Order Item Data not found", ErpSyncLevel.Order);
            return;
        }
        try
        {
            var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(
                erpOrderAdditionalData.ErpAccountId
            );
            if (erpAccount == null)
            {
                await _erpLogsService.ErrorAsync("Erp Account not found", ErpSyncLevel.Order);
                return;
            }
            var accountSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(
                erpAccount.ErpSalesOrgId
            );
            if (accountSalesOrg == null)
            {
                await _erpLogsService.ErrorAsync("Erp Account Sales Org not found", ErpSyncLevel.Order);
                return;
            }

            var orderTypeString = GetOrderTypeString(erpOrderAdditionalData) ?? string.Empty;

            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var currentCustomer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            if (currentCustomer == null)
            {
                await _erpLogsService.ErrorAsync("Customer not found", ErpSyncLevel.Order);
                return;
            }

            var shippingAddress = await _addressService.GetAddressByIdAsync(
                order.ShippingAddressId ?? 0
            );
            if (shippingAddress == null)
            {
                await _erpLogsService.ErrorAsync("Shipping Address not found", ErpSyncLevel.Order);
                return;
            }

            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
            if (billingAddress == null)
            {
                await _erpLogsService.ErrorAsync("Billing Address not found", ErpSyncLevel.Order);
                return;
            }

            var erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(
                erpOrderAdditionalData.ErpShipToAddressId ?? 0
            );
            var erpShipToAddressOfErpNopUser =
                await _erpShipToAddressService.GetErpShipToAddressByIdAsync(
                    erpNopUser.ErpShipToAddressId
                );

            #region Erp Order Data Model Prepare

            var erpPlaceOrderDataModel = new ErpPlaceOrderDataModel()
            {
                AccountNumber = erpAccount.AccountNumber,
                AccountName = erpAccount.AccountName,
                Location = accountSalesOrg.Code,
                CustomOrderNumber = string.IsNullOrWhiteSpace(order.CustomOrderNumber) ? $"{order.Id}" : order.CustomOrderNumber,
                DateRequired =
                    order.PickupInStore || erpOrderAdditionalData.DeliveryDate == null
                        ? DateTime.Now
                        : erpOrderAdditionalData.DeliveryDate.Value,
                RepCode =
                    erpShipToAddress == null
                        ? erpShipToAddressOfErpNopUser?.RepNumber ?? string.Empty
                        : erpShipToAddress?.RepNumber ?? string.Empty,
                AddressCode =
                    erpShipToAddress == null
                        ? erpShipToAddressOfErpNopUser?.ShipToCode ?? string.Empty
                        : erpShipToAddress?.ShipToCode ?? string.Empty,
                ShippingAddress = order.PickupInStore
                    ? new ErpAddressModel
                    {
                        Address1 =
                            (await GetStateProvinceAsync(order.PickupAddressId))?.Abbreviation
                            ?? string.Empty,
                        Region =
                            (await GetStateProvinceAsync(order.PickupAddressId))?.Abbreviation
                            ?? string.Empty,
                        Country = await GetCountryNameAsync(order.PickupAddressId) ?? string.Empty,
                    }
                    : new ErpAddressModel
                    {
                        Name = erpShipToAddress?.ShipToName ?? string.Empty,
                        Address1 = shippingAddress?.Address1 ?? string.Empty,
                        Address2 = shippingAddress?.Address2 ?? string.Empty,
                        Address3 =
                            (await GetStateProvinceAsync(order.ShippingAddressId))?.Abbreviation
                            ?? string.Empty,
                        City = shippingAddress?.City ?? string.Empty,
                        StateProvince =
                            (await GetStateProvinceAsync(order.ShippingAddressId))?.Name
                            ?? string.Empty,
                        Region =
                            (await GetStateProvinceAsync(order.ShippingAddressId))?.Abbreviation
                            ?? string.Empty,
                        ZipPostalCode = shippingAddress?.ZipPostalCode ?? string.Empty,
                        Country =
                            await GetCountryNameAsync(order.ShippingAddressId) ?? string.Empty,
                        PhoneNumber = shippingAddress?.PhoneNumber ?? string.Empty,
                        Suburb = erpShipToAddress?.Suburb?.ToUpper() ?? string.Empty,
                    },
                BillingAddress = new ErpAddressModel
                {
                    Name = $"{billingAddress.FirstName ?? string.Empty} {billingAddress.LastName ?? string.Empty}",
                    Address1 = billingAddress.Address1 ?? string.Empty,
                    Address2 = billingAddress.Address2 ?? string.Empty,
                    Address3 = "",
                    City = billingAddress.City ?? string.Empty,
                    StateProvince = (await GetStateProvinceAsync(order.BillingAddressId))?.Name ?? "",
                    Region = (await GetStateProvinceAsync(order.BillingAddressId))?.Abbreviation ?? string.Empty,
                    ZipPostalCode = billingAddress.ZipPostalCode ?? string.Empty,
                    Country = (await GetCountryNameAsync(order.BillingAddressId)) ?? string.Empty,
                    PhoneNumber = billingAddress.PhoneNumber ?? string.Empty
                },
                CustomerName = erpAccount.AccountName,
                CustomerReference = $"{erpOrderAdditionalData.CustomerReference}",
                DeliveryInstruction = erpOrderAdditionalData.SpecialInstructions ?? string.Empty,
                OrderCategory = "",
                DeliveryMethod = order.PickupInStore ? DELIVERY_METHOD_COLLECT : DELIVERY_METHOD_DELIVERY,
                CustomerFirstName = currentCustomer.FirstName,
                CustomerLastName = currentCustomer.LastName,
                CustomerPhoneNumber = currentCustomer.Phone ?? string.Empty,
                CustomerMobileNumber = currentCustomer.Phone ?? string.Empty,
                CustomerEmail = currentCustomer.Email,
                VatNumber = erpAccount.VatNumber ?? string.Empty,
                OrderType = orderTypeString.ToUpper(),
                QuoteNumber = await GetQuoteNumberAsync(erpOrderAdditionalData),
                OrderTax = Math.Round(_currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate), 2),
                OrderSubtotalExclTax = Math.Round(_currencyService.ConvertCurrency((order.OrderTotal - order.OrderTax), order.CurrencyRate), 2),
                OrderDate = order.CreatedOnUtc,
                CustomerCurrencyCode = order.CustomerCurrencyCode ?? string.Empty,
                ErpPlaceOrderItemDatas = erpPlaceOrderItemData,
                DeliveryDate = erpOrderAdditionalData.DeliveryDate == null ? DateTime.Now : erpOrderAdditionalData.DeliveryDate.Value
            };

            #endregion

            try
            {
                await PlaceOrderOrQuoteOnERPAsync(
                    erpPlaceOrderDataModel,
                    erpOrderAdditionalData,
                    order
                );
            }
            catch (Exception ex)
            {
                await _erpLogsService.ErrorAsync(
                    ex.Message,
                    ErpSyncLevel.Order,
                    ex,
                    currentCustomer
                );
            }

            #region Email Notification

            if (erpOrderAdditionalData.IntegrationRetries != null && 
                erpOrderAdditionalData.IntegrationRetries >= maxRetries && 
                erpOrderAdditionalData.IntegrationStatusType != IntegrationStatusType.Confirmed)
            {
                erpOrderAdditionalData.ERPOrderStatus = await _localizationService.GetLocalizedEnumAsync(IntegrationStatusType.Failed);
                erpOrderAdditionalData.IntegrationStatusType = IntegrationStatusType.Failed;

                if (!string.IsNullOrEmpty(erpShipToAddress?.RepEmail))
                {
                    //sent email notification
                    var queuedEmailIds =
                        await _erpWorkflowMessageService.SendERPOrderPlaceFailedSalesRepNotificationAsync(
                            order,
                            order.CustomerLanguageId,
                            erpShipToAddress
                        );
                    if (queuedEmailIds.Any())
                        await AddOrderNoteAsync(
                            order,
                            $"\"Erp {orderTypeString} place failed\" email (to sales rep) has been queued. queued email identifier: {string.Join(", ", queuedEmailIds)}."
                        );
                }
            }

            #endregion

            var note = $"{orderTypeString} place in ERP call complete. Try no: {erpOrderAdditionalData.IntegrationRetries ?? 0}, Integration status: {await _localizationService.GetLocalizedEnumAsync(erpOrderAdditionalData.IntegrationStatusType)}";
            if (!string.IsNullOrEmpty(erpOrderAdditionalData.IntegrationError))
            {
                note = note + $", Integration error: {erpOrderAdditionalData.IntegrationError}";
            }
            await AddOrderNoteAsync(order, note);

            if (erpOrderAdditionalData.IntegrationRetries != null && erpOrderAdditionalData.IntegrationRetries == 1
                && erpOrderAdditionalData.IntegrationStatusType != IntegrationStatusType.Confirmed)
            {
                var errorNote = $"{erpOrderAdditionalData.IntegrationError}, for {erpOrderAdditionalData.ErpOrderType}";
                await AddOrderNoteAsync(order, errorNote);
            }

            if (erpOrderAdditionalData.IntegrationStatusType == IntegrationStatusType.Confirmed)
            {
                await AddOrderNoteAsync(order, erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2BSalesOrder ? await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.B2BOrder.SuccessOrderNote.OrderPlacedInERP")
                    : await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.B2BOrder.SuccessOrderNote.QuotePlacedInERP"));
            }

            await _orderService.UpdateOrderAsync(order);

            if (erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2BSalesOrder || 
                erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2CSalesOrder && 
                erpOrderAdditionalData.IntegrationStatusType == IntegrationStatusType.Confirmed)
            {
                await _genericAttributeService.SaveAttributeAsync<int?>(currentCustomer, B2BB2CFeaturesDefaults.B2BOriginalB2BQuoteOrderIdReference, null, currentStore.Id);
                await _genericAttributeService.SaveAttributeAsync<int?>(currentCustomer, B2BB2CFeaturesDefaults.ProvidedB2BSpecialInstructions, null, currentStore.Id);
                await _genericAttributeService.SaveAttributeAsync<int?>(currentCustomer, B2BB2CFeaturesDefaults.B2CSpecialInstructions, null, currentStore.Id);

                erpOrderAdditionalData.IsOrderPlaceNotificationSent = true;
            }

            erpOrderAdditionalData.LastERPUpdateUtc = DateTime.UtcNow;
            erpOrderAdditionalData.ChangedOnUtc = DateTime.UtcNow;
            await _erpOrderAdditionalDataService.UpdateErpOrderAdditionalDataAsync(
                erpOrderAdditionalData
            );
        }
        catch (Exception ex)
        {
            await _erpLogsService.WarningAsync(ex.Message, ErpSyncLevel.Order, ex, await _workContext.GetCurrentCustomerAsync());
        }
    }

    #endregion

    #region Utilities

    protected override async Task<Order> SaveOrderDetailsAsync(ProcessPaymentRequest processPaymentRequest,
        ProcessPaymentResult processPaymentResult, PlaceOrderContainer details)
    {
        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var currStore = await _storeContext.GetCurrentStoreAsync();
        var adjustmentValue = await _genericAttributeService.GetAttributeAsync<decimal>(currCustomer, B2BB2CFeaturesDefaults.B2COrderOnlineSavingsAdjustmentValue, currStore.Id);

        if (adjustmentValue > 0)
        {
            details.OrderSubTotalInclTax -= adjustmentValue;
            details.OrderSubTotalExclTax -= adjustmentValue;
        }

        var order = new Order
        {
            StoreId = processPaymentRequest.StoreId,
            OrderGuid = processPaymentRequest.OrderGuid,
            CustomerId = details.Customer.Id,
            CustomerLanguageId = details.CustomerLanguage.Id,
            CustomerTaxDisplayType = details.CustomerTaxDisplayType,
            CustomerIp = _webHelper.GetCurrentIpAddress(),
            OrderSubtotalInclTax = details.OrderSubTotalInclTax,
            OrderSubtotalExclTax = details.OrderSubTotalExclTax,
            OrderSubTotalDiscountInclTax = details.OrderSubTotalDiscountInclTax,
            OrderSubTotalDiscountExclTax = details.OrderSubTotalDiscountExclTax,
            OrderShippingInclTax = details.OrderShippingTotalInclTax,
            OrderShippingExclTax = details.OrderShippingTotalExclTax,
            PaymentMethodAdditionalFeeInclTax = details.PaymentAdditionalFeeInclTax,
            PaymentMethodAdditionalFeeExclTax = details.PaymentAdditionalFeeExclTax,
            TaxRates = details.TaxRates,
            OrderTax = details.OrderTaxTotal,
            OrderTotal = details.OrderTotal,
            RefundedAmount = decimal.Zero,
            OrderDiscount = details.OrderDiscountAmount,
            CheckoutAttributeDescription = details.CheckoutAttributeDescription,
            CheckoutAttributesXml = details.CheckoutAttributesXml,
            CustomerCurrencyCode = details.CustomerCurrencyCode,
            CurrencyRate = details.CustomerCurrencyRate,
            AffiliateId = details.AffiliateId,
            OrderStatus = OrderStatus.Pending,
            AllowStoringCreditCardNumber = processPaymentResult.AllowStoringCreditCardNumber,
            CardType = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardType) : string.Empty,
            CardName = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardName) : string.Empty,
            CardNumber = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardNumber) : string.Empty,
            MaskedCreditCardNumber = _encryptionService.EncryptText(_paymentService.GetMaskedCreditCardNumber(processPaymentRequest.CreditCardNumber)),
            CardCvv2 = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardCvv2) : string.Empty,
            CardExpirationMonth = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireMonth.ToString()) : string.Empty,
            CardExpirationYear = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireYear.ToString()) : string.Empty,
            PaymentMethodSystemName = processPaymentRequest.PaymentMethodSystemName,
            AuthorizationTransactionId = processPaymentResult.AuthorizationTransactionId,
            AuthorizationTransactionCode = processPaymentResult.AuthorizationTransactionCode,
            AuthorizationTransactionResult = processPaymentResult.AuthorizationTransactionResult,
            CaptureTransactionId = processPaymentResult.CaptureTransactionId,
            CaptureTransactionResult = processPaymentResult.CaptureTransactionResult,
            SubscriptionTransactionId = processPaymentResult.SubscriptionTransactionId,
            PaymentStatus = processPaymentResult.NewPaymentStatus,
            PaidDateUtc = null,
            PickupInStore = details.PickupInStore,
            ShippingStatus = details.ShippingStatus,
            ShippingMethod = details.ShippingMethodName,
            ShippingRateComputationMethodSystemName = details.ShippingRateComputationMethodSystemName,
            CustomValuesXml = _paymentService.SerializeCustomValues(processPaymentRequest),
            VatNumber = details.VatNumber,
            CreatedOnUtc = DateTime.UtcNow,
            CustomOrderNumber = string.Empty
        };

        if (details.BillingAddress is null)
            throw new NopException("Billing address is not provided");

        await _addressService.InsertAddressAsync(details.BillingAddress);
        order.BillingAddressId = details.BillingAddress.Id;

        if (details.PickupAddress != null)
        {
            await _addressService.InsertAddressAsync(details.PickupAddress);
            order.PickupAddressId = details.PickupAddress.Id;
        }

        if (details.ShippingAddress != null)
        {
            await _addressService.InsertAddressAsync(details.ShippingAddress);
            order.ShippingAddressId = details.ShippingAddress.Id;
        }

        await _orderService.InsertOrderAsync(order);

        //generate and set custom order number
        order.CustomOrderNumber = _customNumberFormatter.GenerateOrderCustomNumber(order);
        await _orderService.UpdateOrderAsync(order);

        #region B2B/B2C Customer

        var orderCustomer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
        var erpNopUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(orderCustomer);
        (var _, var b2BUser, var b2CUser) = await GetB2BAccountAndUserOfCurrentCustomerAsync();

        if (b2BUser != null && erpNopUser.ShippingErpShipToAddressId > 0)
        {
            var checkoutShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(erpNopUser.ErpShipToAddressId);

            if (checkoutShipToAddress != null)
            {
                checkoutShipToAddress.AddressId = details.ShippingAddress?.Id ?? checkoutShipToAddress.AddressId;
                checkoutShipToAddress.UpdatedById = details.Customer.Id;
                checkoutShipToAddress.UpdatedOnUtc = DateTime.UtcNow;
                await _erpShipToAddressService.UpdateErpShipToAddressAsync(checkoutShipToAddress);
            }
        }

        //Quote reference Id
        var isQuoteOrder = false;

        if (erpNopUser != null && b2BUser != null)
        {
            // Update allocated stock at B2BPerAccountProductPricing for each item only for sales order
            isQuoteOrder = await _genericAttributeService.GetAttributeAsync<bool>(currCustomer, B2BB2CFeaturesDefaults.B2BQouteOrderAttribute, currStore.Id);
            if (!isQuoteOrder)
            {
                var b2BConvertedQuoteOrderId = await _genericAttributeService.GetAttributeAsync<int>(currCustomer, B2BB2CFeaturesDefaults.B2BConvertedQuoteB2BOrderId, currStore.Id);
                if (b2BConvertedQuoteOrderId > 0)
                {
                    var b2BOrderPerAccount = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByIdAsync(b2BConvertedQuoteOrderId);
                    if (await _erpCustomerFunctionalityService.CheckQuoteOrderStatusAsync(b2BOrderPerAccount))
                    {
                        //save generic Attribute for original B2B Quote Order Id as reference
                        await _genericAttributeService.SaveAttributeAsync(currCustomer, B2BB2CFeaturesDefaults.B2BOriginalB2BQuoteOrderIdReference, b2BOrderPerAccount.Id, currStore.Id);
                    }
                }
            }
        }
        else if (erpNopUser != null && b2CUser != null)
        {
            // Update allocated stock at B2BPerAccountProductPricing for each item only for sales order
            isQuoteOrder = await _genericAttributeService.GetAttributeAsync<bool>(currCustomer, B2BB2CFeaturesDefaults.B2CQouteOrderAttribute, currStore.Id);
            if (!isQuoteOrder)
            {
                var b2CConvertedQuoteOrderId = await _genericAttributeService.GetAttributeAsync<int>(currCustomer, B2BB2CFeaturesDefaults.B2CConvertedQuoteB2COrderId, currStore.Id);
                if (b2CConvertedQuoteOrderId > 0)
                {
                    var b2COrder = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByIdAsync(b2CConvertedQuoteOrderId);
                    if (await _erpCustomerFunctionalityService.CheckQuoteOrderStatusAsync(b2COrder))
                    {
                        //save generic Attribute for original B2C Quote Order Id as reference
                        await _genericAttributeService.SaveAttributeAsync(currCustomer, B2BB2CFeaturesDefaults.B2COriginalB2CQuoteOrderIdReference, b2COrder.Id, currStore.Id);
                    }
                }
            }
        }

        #endregion

        //reward points history
        if (details.RedeemedRewardPointsAmount <= decimal.Zero)
            return order;

        order.RedeemedRewardPointsEntryId = await _rewardPointService.AddRewardPointsHistoryEntryAsync(details.Customer, -details.RedeemedRewardPoints, order.StoreId,
            string.Format(await _localizationService.GetResourceAsync("RewardPoints.Message.RedeemedForOrder", order.CustomerLanguageId), order.CustomOrderNumber),
            order, details.RedeemedRewardPointsAmount);
        await _customerService.UpdateCustomerAsync(details.Customer);
        await _orderService.UpdateOrderAsync(order);

        return order;
    }

    protected async Task<string> GetQuoteNumberAsync(ErpOrderAdditionalData erpOrderAdditionalData)
    {
        if (erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2BQuote)
            return string.Empty;

        if (!erpOrderAdditionalData.QuoteSalesOrderId.HasValue || erpOrderAdditionalData.QuoteSalesOrderId.Value < 1)
        {
            return string.Empty;
        }
        return (await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByIdAsync(erpOrderAdditionalData.QuoteSalesOrderId.Value))?.ErpOrderNumber ?? string.Empty;
    }

    protected string GetOrderTypeString(ErpOrderAdditionalData erpOrderAdditionalData)
    {
        if (erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2BSalesOrder && erpOrderAdditionalData.QuoteSalesOrderId.HasValue)
        {
            return "Order from Quote";
        }
        else
        {
            return erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2BSalesOrder ? "Order" : "Quote";
        }
    }

    protected async Task<StateProvince> GetStateProvinceAsync(int? addressId)
    {
        if (addressId.HasValue)
        {
            var address = await _addressService.GetAddressByIdAsync(addressId.Value);
            if (address != null)
            {
                return await _stateProvinceService.GetStateProvinceByAddressAsync(address);
            }
            return null;
        }
        return null;
    }

    protected async Task<string> GetCountryNameAsync(int? addressId)
    {
        if (addressId.HasValue)
        {
            var address = await _addressService.GetAddressByIdAsync(addressId.Value);
            if (address != null)
            {
                var country = await _countryService.GetCountryByAddressAsync(address);
                return (country == null) ? string.Empty : country.Name;
            }
            return string.Empty;
        }
        return string.Empty;
    }

    private async Task<(ErpAccount b2BAccount, ErpNopUser b2BUser, ErpNopUser b2CUser)> GetB2BAccountAndUserOfCurrentCustomerAsync()
    {
        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var b2BAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(currCustomer.Id);
        var erpUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(currCustomer);

        var b2BUser = erpUser?.ErpUserType == ErpUserType.B2BUser ? erpUser : null;
        var b2CUser = erpUser?.ErpUserType == ErpUserType.B2CUser ? erpUser : null;

        return (b2BAccount, b2BUser, b2CUser);
    }

    #endregion
}