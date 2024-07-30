
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Controllers;
using NopStation.Plugin.B2B.B2BB2CFeatures.Infrastructure;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Infrastructure;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Controllers
{
    public class QuoteOrderController : BasePublicController
    {
        #region Fields

        private readonly CustomerSettings _customerSettings;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly OrderSettings _orderSettings;
        private readonly IPermissionService _permissionService;
        private readonly IErpCustomerFunctionalityService _b2BCustomerFunctionality;
        private readonly IProductService _productService;

        #endregion

        #region Ctor

        public QuoteOrderController(CustomerSettings customerSettings,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IPaymentPluginManager paymentPluginManager,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IWorkContext workContext,
            OrderSettings orderSettings,
            IPermissionService permissionService,
            IErpCustomerFunctionalityService b2BCustomerFunctionality,
            IProductService productService)
        {
            _customerSettings = customerSettings;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _paymentPluginManager = paymentPluginManager;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _workContext = workContext;
            _orderSettings = orderSettings;
            _permissionService = permissionService;
            _b2BCustomerFunctionality = b2BCustomerFunctionality;
            _productService = productService;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> Index()
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var currCustomer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(currCustomer, ShoppingCartType.ShoppingCart, store.Id);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            var hasDownloadableProduct = false;

            foreach (var sci in cart)
            {
                var pro = await _productService.GetProductByIdAsync(sci.ProductId);
                hasDownloadableProduct = pro?.IsDownload ?? false;

                if (hasDownloadableProduct)
                    break;
            }
            var downloadableProductsRequireRegistration =
                _customerSettings.RequireRegistrationForDownloadableProducts && hasDownloadableProduct;

            if (await _customerService.IsGuestAsync(currCustomer) && (!_orderSettings.AnonymousCheckoutAllowed || downloadableProductsRequireRegistration))
                return Challenge();

            //if we have only "button" payment methods available (displayed onthe shopping cart page, not during checkout),
            //then we should allow standard checkout
            //all payment methods (do not filter by country here as it could be not specified yet)
            var paymentMethods = await (await _paymentPluginManager
                .LoadActivePluginsAsync(currCustomer, store.Id))
                .WhereAwait(async pm => !await pm.HidePaymentMethodAsync(cart)).ToListAsync();
            //payment methods displayed during checkout (not with "Button" type)
            var nonButtonPaymentMethods = paymentMethods
                .Where(pm => pm.PaymentMethodType != PaymentMethodType.Button)
                .ToList();
            //"button" payment methods(*displayed on the shopping cart page)
            var buttonPaymentMethods = paymentMethods
                .Where(pm => pm.PaymentMethodType == PaymentMethodType.Button)
                .ToList();
            if (!nonButtonPaymentMethods.Any() && buttonPaymentMethods.Any())
                return RedirectToRoute("ShoppingCart");

            //reset checkout data
            await _customerService.ResetCheckoutDataAsync(currCustomer, store.Id);

            //validation (cart)
            var checkoutAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(currCustomer,
                NopCustomerDefaults.CheckoutAttributes, store.Id);
            var scWarnings = await _shoppingCartService.GetShoppingCartWarningsAsync(cart, checkoutAttributesXml, true);
            if (scWarnings.Any())
                return RedirectToRoute("ShoppingCart");
            //validation (each shopping cart item)
            foreach (var sci in cart)
            {

                var sciWarnings = await _shoppingCartService.GetShoppingCartItemWarningsAsync(currCustomer,
                    sci.ShoppingCartType,
                    await _productService.GetProductByIdAsync(sci.ProductId),
                    sci.StoreId,
                    sci.AttributesXml,
                    sci.CustomerEnteredPrice,
                    sci.RentalStartDateUtc,
                    sci.RentalEndDateUtc,
                    sci.Quantity,
                    false,
                    sci.Id);
                if (sciWarnings.Any())
                    return RedirectToRoute("ShoppingCart");
            }

            var erpNopUser = await _b2BCustomerFunctionality.GetActiveErpNopUserByCustomerAsync(currCustomer);
            var b2BUser = erpNopUser.ErpUserType == ErpUserType.B2BUser ? erpNopUser : null;
            var b2CUser = erpNopUser.ErpUserType == ErpUserType.B2CUser ? erpNopUser : null;

            if (b2BUser != null)
            {
                // if b2b user is active, only then place qoute order otherwise go for normal order
                if (await _b2BCustomerFunctionality.IsConsideredAsB2BOrderByB2BUserInformation(b2BUser))
                {
                    if (!await _permissionService.AuthorizeAsync(ErpPermissionProvider.PlaceB2BQuote))
                        return RedirectToRoute("ShoppingCart");

                    //save generic Attribute for Qoute Order
                    await _genericAttributeService.SaveAttributeAsync(currCustomer, B2BB2CFeaturesDefaults.B2BQouteOrderAttribute, true, store.Id);
                }
            }

            else if (b2CUser != null)
            {
                // if b2c user is active, only then place qoute order otherwise go for normal order
                if (await _b2BCustomerFunctionality.IsConsideredAsB2COrderByB2CUser(b2CUser))
                {
                    if (!await _permissionService.AuthorizeAsync(ErpPermissionProvider.PlaceB2BQuote))
                        return RedirectToRoute("ShoppingCart");

                    //save generic Attribute for Qoute Order
                    await _genericAttributeService.SaveAttributeAsync(currCustomer, B2BB2CFeaturesDefaults.B2CQouteOrderAttribute, true, store.Id);
                }
            }

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            return RedirectToRoute("CheckoutBillingAddress");
        }

        public virtual async Task<IActionResult> MoveToRegular()
        {
            var currCustomer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var b2bOrderId = await _genericAttributeService.GetAttributeAsync<int>(currCustomer, B2BB2CFeaturesDefaults.B2BConvertedQuoteB2BOrderId, store.Id);
            if (b2bOrderId > 0)
            {
                _b2BCustomerFunctionality.ClearGenericAttributeOfB2BQuoteOrder();
            }
            var b2COrderId = await _genericAttributeService.GetAttributeAsync<int>(currCustomer, B2BB2CFeaturesDefaults.B2CConvertedQuoteB2COrderId, store.Id);
            if (b2COrderId > 0)
            {
                _b2BCustomerFunctionality.ClearGenericAttributeOfB2CQuoteOrder();
            }
            return Json("success");
        }

        #endregion
    }
}