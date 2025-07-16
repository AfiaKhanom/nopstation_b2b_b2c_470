using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Messages;
using Nop.Services.Orders;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.Overriden;
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services;

/// <summary>
/// Represents event consumer
/// </summary>
public class EventConsumer :
    IConsumer<EntityDeletedEvent<Order>>,
    IConsumer<OrderPaidEvent>,
    IConsumer<OrderPlacedEvent>,
    IConsumer<OrderStatusChangedEvent>,
    IConsumer<EntityTokensAddedEvent<Customer, Token>>,
    IConsumer<EntityTokensAddedEvent<Order, Token>>,
    IConsumer<EntityInsertedEvent<ErpNopUser>>
{
    #region Fields

    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
    private readonly IOverriddenOrderProcessingService _overriddenOrderProcessingService;
    private readonly ICustomerService _customerService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IOrderTotalCalculationService _orderTotalCalculationService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpNopUserService _erpNopUserService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IWorkContext _workContext;
    private readonly TaxSettings _taxSettings;

    #endregion

    #region Ctor

    public EventConsumer(B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        IOverriddenOrderProcessingService overriddenOrderProcessingService,
        ICustomerService customerService,
        IWorkContext workContext,
        TaxSettings taxSettings,
        IShoppingCartService shoppingCartService,
        IOrderTotalCalculationService orderTotalCalculationService,
        IErpAccountService erpAccountService,
        IErpNopUserService erpNopUserService,
        IStaticCacheManager staticCacheManager)
    {
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
        _overriddenOrderProcessingService = overriddenOrderProcessingService;
        _customerService = customerService;
        _workContext = workContext;
        _taxSettings = taxSettings;
        _shoppingCartService = shoppingCartService;
        _orderTotalCalculationService = orderTotalCalculationService;
        _erpAccountService = erpAccountService;
        _erpNopUserService = erpNopUserService;
        _staticCacheManager = staticCacheManager;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Handle the order paid event
    /// </summary>
    /// <param name="eventMessage">The event message.</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task HandleEventAsync(OrderPaidEvent eventMessage)
    {
        //handle event
        if (eventMessage.Order == null)
            return;

        var erpOrder = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(eventMessage.Order.Id);

        // if order per account is not null && IntegrationStatusType is WaitingForPayment the we will send from here
        if (erpOrder != null)
        {
            if (erpOrder.IntegrationStatusType == IntegrationStatusType.WaitingForPayment)
            {
                erpOrder.IntegrationStatusTypeId = (int)IntegrationStatusType.Queued;
                await _erpOrderAdditionalDataService.UpdateErpOrderAdditionalDataAsync(erpOrder);
            }

            if (erpOrder.IntegrationStatusType == IntegrationStatusType.Queued)
                await _overriddenOrderProcessingService.RetryPlaceErpOrderAtErpAsync(erpOrder);
        }

    }

    /// <summary>
    /// Handle the order placed event
    /// </summary>
    /// <param name="eventMessage">The event message.</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
    {
        //handle event
        await _erpCustomerFunctionalityService.ClearCurrentCustomerYearlySavingsCacheAsync(eventMessage.Order.CustomerId);
        await _erpCustomerFunctionalityService.ClearCurrentCustomerAllTimeSavingsCacheAsync(eventMessage.Order.CustomerId);
    }

    /// <summary>
    /// Handle the order status changed event
    /// </summary>
    /// <param name="eventMessage">The event message.</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task HandleEventAsync(OrderStatusChangedEvent eventMessage)
    {
        var order = eventMessage.Order;

        var erpOrder = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(order.Id);

        if (order.OrderStatusId == (int)OrderStatus.Cancelled && erpOrder != null)
        {
            erpOrder.IntegrationStatusTypeId = (int)IntegrationStatusType.Cancelled;
            await _erpOrderAdditionalDataService.UpdateErpOrderAdditionalDataAsync(erpOrder);
        }
    }

    public async Task HandleEventAsync(EntityDeletedEvent<Order> eventMessage)
    {
        if (eventMessage.Entity == null)
            return;

        var order = eventMessage.Entity;

        var erpOrder = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(order.Id);


        if (erpOrder != null)
            await _erpOrderAdditionalDataService.DeleteErpOrderAdditionalDataByIdAsync(erpOrder.Id);
    }

    public async Task HandleEventAsync(EntityTokensAddedEvent<Customer, Token> eventMessage)
    {
        var customer = eventMessage.Entity;
        var customerCompany = customer.Company;
        eventMessage.Tokens.Add(new Token("Customer.Company", customerCompany));
    }

    public async Task HandleEventAsync(EntityTokensAddedEvent<Order, Token> eventMessage)
    {
        var order = eventMessage.Entity;

        if (order is null)
        {
            return;
        }

        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
        var erpNopUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(customer);

        if (erpNopUser is not null)
        {
            var erpOrderAdditionalData = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(order.Id);

            if (erpOrderAdditionalData is null || string.IsNullOrEmpty(erpOrderAdditionalData.ErpOrderNumber))
            {
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ErpOrderNumber", "(Null order)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.NopOrderNumber", "(Null)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ErpOrderOriginType", "(Null)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ErpOrderType", "(Null)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.SpecialInstructions", "(Null)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.CustomerReference", "(Null)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ErpOrderStatus", "(Null)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.DeliveryDate", "(Null)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.IntegrationStatusType", "(Null)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.IntegrationError", "(Null)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.LastErpUpdateUtc", "(Null)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ErpOrderPlaceByCustomerType", "(Null)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ChangedOnUtc", "(Null)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ChangedById", "(Null)"));

                return;
            }

            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ErpOrderNumber", string.IsNullOrEmpty(erpOrderAdditionalData.ErpOrderNumber) ? order.CustomOrderNumber : erpOrderAdditionalData.ErpOrderNumber));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.NopOrderNumber", !string.IsNullOrEmpty(order.CustomOrderNumber) ? order.CustomOrderNumber : erpOrderAdditionalData.NopOrderId.ToString()));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ErpOrderOriginType", erpOrderAdditionalData.ErpOrderOriginType.ToString()));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ErpOrderType", erpOrderAdditionalData.ErpOrderType.ToString()));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.SpecialInstructions", erpOrderAdditionalData.SpecialInstructions));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.CustomerReference", erpOrderAdditionalData.CustomerReference));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ErpOrderStatus", erpOrderAdditionalData.ERPOrderStatus));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.DeliveryDate", erpOrderAdditionalData.DeliveryDate.HasValue ? erpOrderAdditionalData.DeliveryDate.Value.ToString("d") : ""));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.IntegrationStatusType", erpOrderAdditionalData.IntegrationStatusType.ToString()));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.IntegrationError", erpOrderAdditionalData.IntegrationError));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.LastErpUpdateUtc", erpOrderAdditionalData.LastERPUpdateUtc.HasValue ? erpOrderAdditionalData.LastERPUpdateUtc.Value.ToString("d") : ""));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ErpOrderPlaceByCustomerType", Enum.GetName(typeof(ErpUserType), erpOrderAdditionalData.ErpOrderPlaceByCustomerTypeId)));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ChangedOnUtc", erpOrderAdditionalData.ChangedOnUtc.HasValue ? erpOrderAdditionalData.ChangedOnUtc.Value.ToString("d") : ""));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ChangedById", erpOrderAdditionalData.ChangedById.ToString()));
        }
    }

    public async Task HandleEventAsync(EntityInsertedEvent<ErpNopUser> eventMessage)
    {
        await _staticCacheManager.RemoveAsync(_staticCacheManager.PrepareKeyForDefaultCache(ERPIntegrationCoreDefaults.ErpNopUserByCustomerCacheKey,
            eventMessage.Entity.NopCustomerId));
    }

    #endregion
}