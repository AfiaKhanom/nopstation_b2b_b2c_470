using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Factories.ErpOrderDetails;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Infrastructure;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Controllers;

public class OverridenOrderController : OrderController
{
    #region Fields

    private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IStoreContext _storeContext;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IPermissionService _permissionService;
    private readonly ILogger _logger;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpNopUserService _erpNopUserService;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly IErpLogsService _erpLogsService;
    private readonly IErpOrderDetailsModelFactory _erpOrderDetailsModelFactory;
    private readonly IErpActivityLogsService _erpActivityLogsService;
    private readonly IErpProductModelFactory _erpProductModelFactory;

    #endregion

    #region Ctor

    public OverridenOrderController(ICustomerService customerService,
        IOrderModelFactory orderModelFactory,
        IOrderProcessingService orderProcessingService,
        IOrderService orderService,
        IPaymentService paymentService,
        IPdfService pdfService,
        IShipmentService shipmentService,
        IWebHelper webHelper,
        IWorkContext workContext,
        RewardPointsSettings rewardPointsSettings,
        INotificationService notificationService,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        IGenericAttributeService genericAttributeService,
        ISettingService settingService,
        IErpSpecialPriceService erpSpecialPriceService,
        IStoreContext storeContext,
        IShoppingCartService shoppingCartService,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        ILogger logger,
        IErpAccountService erpAccountService,
        IErpNopUserService erpNopUserService,
        IErpOrderModelFactory erpOrderModelFactory,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        IErpLogsService erpLogsService,
        IErpOrderDetailsModelFactory erpOrderDetailsModelFactory,
        IErpActivityLogsService erpActivityLogsService,
        IErpProductModelFactory erpProductModelFactory) : base(customerService,
            localizationService,
            notificationService,
            orderModelFactory,
            orderProcessingService,
            orderService,
            paymentService,
            pdfService,
            shipmentService,
            webHelper,
            workContext,
            rewardPointsSettings)
    {
        _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
        _genericAttributeService = genericAttributeService;
        _storeContext = storeContext;
        _shoppingCartService = shoppingCartService;
        _permissionService = permissionService;
        _logger = logger;
        _erpAccountService = erpAccountService;
        _erpNopUserService = erpNopUserService;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _erpLogsService = erpLogsService;
        _erpOrderDetailsModelFactory = erpOrderDetailsModelFactory;
        _erpActivityLogsService = erpActivityLogsService;
        _erpProductModelFactory = erpProductModelFactory;
    }

    #endregion

    #region Utilities

    private async Task<(ErpAccount erpAccount, ErpNopUser erpNopUser)> GetErpAccountAndUserOfCurrentCustomerAsync(int customerId)
    {
        var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customerId);
        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpNopUser?.ErpAccountId ?? 0);

        return (erpAccount, erpNopUser);
    }

    #endregion

    #region Methods

    //My account / Order details page
    public async Task<IActionResult> QuoteDetails(int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null || order.Deleted)
            return Challenge();

        var customer = await _workContext.GetCurrentCustomerAsync();

        (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(customer.Id);

        if (erpAccount == null)
            return Challenge();

        if (erpNopUser == null)
            return Challenge();

        var erpOrderPerAccount = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(orderId);
        if (erpOrderPerAccount == null || erpOrderPerAccount.ErpAccountId != erpAccount.Id)
            return Challenge();

        var model = await _erpOrderDetailsModelFactory.PrepareErpOrderDetailsModelFactoryAsync(erpOrderPerAccount);

        return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/Order/ErpB2bOrderDetails.cshtml", model);
    }

    public override async Task<IActionResult> Details(int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null || order.Deleted)
            return Challenge();

        var customer = await _workContext.GetCurrentCustomerAsync();

        #region B2B

        (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(customer.Id);

        if (erpAccount != null && erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2BUser)
        {
            var erpOrderPerAccount = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(orderId);
            if (erpOrderPerAccount == null || erpOrderPerAccount.ErpAccountId != erpAccount.Id)
                return Challenge();
        }

        #endregion

        var model = await _orderModelFactory.PrepareOrderDetailsModelAsync(order);
        return View(model);
    }

    //My account / Order details page / Print
    public override async Task<IActionResult> PrintOrderDetails(int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null || order.Deleted)
            return Challenge();

        var customer = await _workContext.GetCurrentCustomerAsync();

        #region B2B

        (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(customer.Id);

        if (erpAccount != null && erpNopUser != null)
        {
            var erpOrderPerAccount = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(orderId);
            if (erpOrderPerAccount == null || erpOrderPerAccount.ErpAccountId != erpAccount.Id)
                return Challenge();
        }
        else
        {
            if (customer.Id != order.CustomerId)
                return Challenge();
        }

        #endregion

        var model = await _orderModelFactory.PrepareOrderDetailsModelAsync(order);
        model.PrintMode = true;

        return View("Details", model);
    }

    [HttpPost, ActionName("Details")]
    [FormValueRequired("repost-payment")]
    public override async Task<IActionResult> RePostPayment(int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null || order.Deleted)
            return Challenge();

        var customer = await _workContext.GetCurrentCustomerAsync();

        #region B2B

        (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(customer.Id);

        if (erpAccount != null && erpNopUser != null)
        {
            var erpOrderPerAccount = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(orderId);
            if (erpOrderPerAccount == null || erpOrderPerAccount.ErpAccountId != erpAccount.Id)
                return Challenge();
        }
        else
        {
            if (customer.Id != order.CustomerId)
                return Challenge();
        }

        #endregion

        if (!await _paymentService.CanRePostProcessPaymentAsync(order))
            return RedirectToRoute("OrderDetails", new { orderId = orderId });

        var postProcessPaymentRequest = new PostProcessPaymentRequest
        {
            Order = order
        };
        await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);

        if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
        {
            //redirection or POST has been done in PostProcessPayment
            return Content("Redirected");
        }

        //if no redirection has been done (to a third-party payment page)
        //theoretically it's not possible
        return RedirectToRoute("OrderDetails", new { orderId = orderId });
    }

    //My account / Order details page / Shipment details page
    public override async Task<IActionResult> ShipmentDetails(int shipmentId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);
        if (shipment == null)
            return Challenge();

        var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);
        if (order == null || order.Deleted)
            return Challenge();

        #region B2B

        (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(customer.Id);

        if (erpAccount != null && erpNopUser != null)
        {
            var erpOrderPerAccount = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(order.Id);
            if (erpOrderPerAccount == null || erpOrderPerAccount.ErpAccountId != erpAccount.Id)
                return Challenge();
        }
        else
        {
            if (customer.Id != order.CustomerId)
                return Challenge();
        }

        #endregion

        var model = await _orderModelFactory.PrepareShipmentDetailsModelAsync(shipment);
        return View(model);
    }

    #endregion         

    #region B2B Custom

    public virtual async Task<IActionResult> IsItemsInCart()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var shoppingCartItem = await _shoppingCartService.GetShoppingCartAsync(customer);
        var hasItemOnCart = shoppingCartItem.Any(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart);

        return hasItemOnCart ? Json(new { success = true }) : Json(new { success = false });
    }

    public override async Task<IActionResult> ReOrder(int orderId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null || order.Deleted)
            return Challenge();

        #region B2B

        (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(customer.Id);
        if (erpAccount != null)
        {
            if (!await _permissionService.AuthorizeAsync(ErpPermissionProvider.PlaceB2BOrder) && 
                !await _permissionService.AuthorizeAsync(ErpPermissionProvider.PlaceB2BQuote))
                return RedirectToRoute("ShoppingCart");

            if (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2BUser)
            {
                var erpOrderPerAccount = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(orderId);
                if (erpOrderPerAccount == null || erpOrderPerAccount.ErpAccountId != erpAccount.Id)
                {
                    await _genericAttributeService.SaveAttributeAsync(customer, B2BB2CFeaturesDefaults.IsCartActivityOn, false, store.Id);
                    return Challenge();
                }

                // If the original order of new reorder is a quote order then we directly delete previously quote order Id from 
                // the genericAttribute. If the reorder method the call we will make value of B2BConvertedQuoteB2BOrderId null
                // each time. No considaration will be done for quote or order in this process.
                _erpCustomerFunctionalityService.ClearGenericAttributeOfB2BQuoteOrder();

                //erp activity log
                await _erpActivityLogsService.InsertErpActivityAsync("Erp_QuoteToOrderConvert",
                    string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.QuoteToOrderConvert"),
                    erpOrderPerAccount.Id, customer.Id),
                    erpOrderPerAccount);

                var quoteValidate = await _erpCustomerFunctionalityService.CheckQuoteOrderStatusAsync(erpOrderPerAccount);
                if (quoteValidate)
                    await _genericAttributeService.SaveAttributeAsync(customer, B2BB2CFeaturesDefaults.B2BConvertedQuoteB2BOrderId, erpOrderPerAccount.Id, store.Id);
            }
            else if (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2CUser)
            {
                var b2COrderPerUser = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(orderId);
                if (b2COrderPerUser == null || b2COrderPerUser.OrderPlacedByNopCustomerId != erpNopUser.NopCustomerId)
                {
                    await _genericAttributeService.SaveAttributeAsync(customer, B2BB2CFeaturesDefaults.IsCartActivityOn, false, store.Id);
                    return Challenge();
                }

                // If the original order of new reorder is a quote order then we directly delete previously quote order Id from 
                // the genericAttribute. If the reorder method the call we will make value of B2BConvertedQuoteB2BOrderId null
                // each time. No considaration will be done for quote or order in this process.
                _erpCustomerFunctionalityService.ClearGenericAttributeOfB2CQuoteOrder();

                //erp activity log
                await _erpActivityLogsService.InsertErpActivityAsync("Erp_QuoteToOrderConvert",
                    string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.QuoteToOrderConvert"),
                    b2COrderPerUser.Id, customer.Id),
                    b2COrderPerUser);

                var quoteValidate = await _erpCustomerFunctionalityService.CheckQuoteOrderStatusAsync(b2COrderPerUser);
                if (quoteValidate)
                    await _genericAttributeService.SaveAttributeAsync(customer, B2BB2CFeaturesDefaults.B2CConvertedQuoteB2COrderId, b2COrderPerUser.Id, store.Id);
            }

            try
            {
                // set cart activity attribute and begin clearing cart
                await _genericAttributeService.SaveAttributeAsync(customer, B2BB2CFeaturesDefaults.IsCartActivityOn, true, store.Id);

                var reorderWarning = await _orderProcessingService.ReOrderAsync(order);
                if (reorderWarning.Any())
                {
                    await _genericAttributeService.SaveAttributeAsync(customer, B2BB2CFeaturesDefaults.IsCartActivityOn, false, store.Id);
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("ShoppingCart.ReorderWarning"));
                    await _logger.ErrorAsync(string.Join(", ", reorderWarning));
                    return RedirectToRoute("ShoppingCart");
                }

                await _erpLogsService.InformationAsync($"Reordered! Order id: {order.Id}", ErpSyncLevel.Order, customer: customer);

                //erp activity log
                await _erpActivityLogsService.InsertErpActivityAsync(customer, "Erp_ReOrder",
                    string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.ErpReOrder"),
                    order.Id, customer.Id), customer);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.Reorder.Succeed"));
            }
            catch (Exception ex)
            {
                var msg = await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.Reorder.Error");
                _logger.Error(msg + " " + ex.Message, ex);
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.Reorder.Error"));
                await _erpLogsService.ErrorAsync(msg, ErpSyncLevel.Order, ex, customer: customer);
            }
            finally
            {
                // reset cart activity attribute 
                await _genericAttributeService.SaveAttributeAsync(customer, B2BB2CFeaturesDefaults.IsCartActivityOn, false, store.Id);
            }
        }
        else
        {
            await base.ReOrder(orderId);
        }

        #endregion

        return RedirectToRoute("ShoppingCart");
    }

    // for cart page
    public async Task<IActionResult> LoadOrderItemsUOMData(int id)
    {
        var data = await _erpProductModelFactory.GetOrderItemsUOMData(id);
        return Json(new
        {
            Data = data.Select(x => new { Uom = x.Item1, ItemId = x.Item2 }).ToList()
        });
    }

    #endregion
}