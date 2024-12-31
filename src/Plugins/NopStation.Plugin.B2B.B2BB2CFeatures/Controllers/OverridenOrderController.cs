using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
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
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.B2BB2CFeatures.Factories.ErpOrderDetails;
using NopStation.Plugin.B2B.B2BB2CFeatures.Infrastructure;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Infrastructure;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Controllers
{
    public class OverridenOrderController : OrderController
    {

        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IOrderModelFactory _orderModelFactory;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IPdfService _pdfService;
        private readonly IShipmentService _shipmentService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly INotificationService _notificationService;
        private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ILogger _logger;
        private readonly IErpAccountService _erpAccountService;
        private readonly IErpNopUserService _erpNopUserService;
        private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
        private readonly IErpLogsService _erpLogsService;
        private readonly IErpOrderDetailsModelFactory _erpOrderDetailsModelFactory;
        private readonly IProductService _productService;
        private readonly IErpActivityLogsService _erpActivityLogsService;

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
            IErpOrderItemAdditionalDataService erpOrderItemAdditionalDataService,
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
            IProductService productService,
            IErpActivityLogsService erpActivityLogsService) : 

            base(customerService,
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
            _customerService = customerService;
            _orderModelFactory = orderModelFactory;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _paymentService = paymentService;
            _pdfService = pdfService;
            _shipmentService = shipmentService;
            _webHelper = webHelper;
            _workContext = workContext;
            _rewardPointsSettings = rewardPointsSettings;
            _notificationService = notificationService;
            _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
            _settingService = settingService;
            _shoppingCartService = shoppingCartService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _logger = logger;
            _erpAccountService = erpAccountService;
            _erpNopUserService = erpNopUserService;
            _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
            _erpLogsService = erpLogsService;
            _erpOrderDetailsModelFactory = erpOrderDetailsModelFactory;
            _productService = productService;
            _erpActivityLogsService = erpActivityLogsService;
        }

        #endregion

        #region Utilities

        private async Task<(ErpAccount erpAccount, ErpNopUser erpNopUser)> GetErpAccountAndUserOfCurrentCustomerAsync(int customerId)
        {
            var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customerId);
            var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customerId);

            return (erpAccount, erpNopUser);
        }

        #endregion

        #region Methods

        //My account / Order details page
        public async Task<IActionResult> QuoteDetails(int orderId)
        {

            var order = await _orderService.GetOrderByIdAsync(orderId);
            var customer = await _workContext.GetCurrentCustomerAsync();

            (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(customer.Id);


            if (erpAccount == null)
                return Challenge();
            if (erpNopUser == null)
                return Challenge();

            var erpOrderPerAccount = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(orderId);
            if (order == null || order.Deleted || erpOrderPerAccount == null || erpOrderPerAccount.ErpAccountId != erpAccount.Id)
                return Challenge();

            var model = await _erpOrderDetailsModelFactory.PrepareErpOrderDetailsModelFactoryAsync(erpOrderPerAccount);

            return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/Order/ErpB2bOrderDetails.cshtml", model);
        }

        public override async Task<IActionResult> Details(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            var customer = await _workContext.GetCurrentCustomerAsync();

            #region Erp

            (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(customer.Id);

            if (erpAccount != null && erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2BUser)
            {
                var erpOrderPerAccount = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(orderId);
                if (order == null || order.Deleted || erpOrderPerAccount == null || erpOrderPerAccount.ErpAccountId != erpAccount.Id)
                    return Challenge();
            }
            else
            {
                if (order == null || order.Deleted || customer.Id != order.CustomerId)
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
            var customer = await _workContext.GetCurrentCustomerAsync();

            #region Erp

            (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(customer.Id);

            if (erpAccount != null && erpNopUser != null)
            {
                var erpOrderPerAccount = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(orderId);
                if (order == null || order.Deleted || erpOrderPerAccount == null || erpOrderPerAccount.ErpAccountId != erpAccount.Id)
                    return Challenge();
            }
            else
            {
                if (order == null || order.Deleted || customer.Id != order.CustomerId)
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
            var customer = await _workContext.GetCurrentCustomerAsync();

            #region Erp

            (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(customer.Id);

            if (erpAccount != null && erpNopUser != null)
            {
                var erpOrderPerAccount = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(orderId);
                if (order == null || order.Deleted || erpOrderPerAccount == null || erpOrderPerAccount.ErpAccountId != erpAccount.Id)
                    return Challenge();
            }
            else
            {
                if (order == null || order.Deleted || customer.Id != order.CustomerId)
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

            #region Erp

            (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(customer.Id);

            if (erpAccount != null && erpNopUser != null)
            {
                var erpOrderPerAccount = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(order?.Id ?? 0);
                if (order == null || order.Deleted || erpOrderPerAccount == null || erpOrderPerAccount.ErpAccountId != erpAccount.Id)
                    return Challenge();
            }
            else
            {
                if (order == null || order.Deleted || customer.Id != order.CustomerId)
                    return Challenge();
            }

            #endregion

            var model = await _orderModelFactory.PrepareShipmentDetailsModelAsync(shipment);
            return View(model);
        }

        #endregion         

        #region Utilities

        // Customer have Quote Assistant role are allowed to see all orders and quotes,
        // but they are not allowed to see the Accounts screen with the invoices and available credit.
        private async Task<bool> HasB2BQuoteAssistantRole()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoles = await _customerService.GetCustomerRolesAsync(customer);
            return customerRoles.Any(x => x.SystemName.Equals(B2BB2CFeaturesDefaults.ErpQuoteAssistantRoleSystemName));
        }

        #endregion

        #region B2B Custom

        public virtual async Task<IActionResult> IsItemsInCart()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var shoppingCartItem = await _shoppingCartService.GetShoppingCartAsync(customer);
            var hasItemOnCart = shoppingCartItem.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart).Any();

            return hasItemOnCart ? Json(new { success = true }) : Json(new { success = false });
        }

        public override async Task<IActionResult> ReOrder(int orderId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted || customer.Id != order.CustomerId)
                return Challenge();

            #region B2B

            (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(customer.Id);
            if (erpAccount != null)
            {
                //-->ToDo: After completing custom permission service for b2b b2c feature this permission check will be reopen
                if (!await _permissionService.AuthorizeAsync(ErpPermissionProvider.PlaceB2BOrder) && !await _permissionService.AuthorizeAsync(ErpPermissionProvider.PlaceB2BQuote))
                    return RedirectToRoute("ShoppingCart");

                // if cart is in any process, let the process finish first and then add new product to the cart
                var isCartActivityOn = await _genericAttributeService.GetAttributeAsync<bool>(customer, B2BB2CFeaturesDefaults.IsCartActivityOn, store.Id);

                if (isCartActivityOn)
                {
                    _notificationService.WarningNotification(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.ShoppingCart.CartActivityOn"));
                    return RedirectToRoute("ShoppingCart");
                }

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
                        await _genericAttributeService.SaveAttributeAsync(customer, B2BB2CFeaturesDefaults.B2BConvertedQuoteB2BOrderId, erpOrderPerAccount.Id, _storeContext.GetCurrentStore().Id);
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

                    foreach (var orderItem in await _orderService.GetOrderItemsAsync(order.Id))
                    {
                        var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                        var warnnings = await _shoppingCartService.GetShoppingCartItemWarningsAsync(customer,
                            ShoppingCartType.ShoppingCart, product, orderItem.Quantity, orderItem.AttributesXml, orderItem.UnitPriceExclTax,
                    orderItem.RentalStartDateUtc, orderItem.RentalEndDateUtc,
                    orderItem.Quantity, false);

                        if (warnnings.Any())
                        {
                            await _genericAttributeService.SaveAttributeAsync(customer, B2BB2CFeaturesDefaults.IsCartActivityOn, false, store.Id);
                            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.Reorder.Error"));
                            return RedirectToRoute("ShoppingCart");
                        }
                    }

                    await _orderProcessingService.ReOrderAsync(order);
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

        #endregion
    }
}