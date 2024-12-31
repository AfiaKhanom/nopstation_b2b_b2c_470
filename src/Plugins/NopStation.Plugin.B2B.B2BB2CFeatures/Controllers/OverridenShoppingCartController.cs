using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Infrastructure;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Components;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Models.ShoppingCart;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.B2BB2CFeatures.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Infrastructure;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Controllers;

public class OverridenShoppingCartController : ShoppingCartController
{
    #region Fields

    private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly IOrderService _orderService;
    private readonly IErpProductModelFactory _erpProductModelFactory;
    private readonly ILogger _logger;
    private readonly IErpLogsService _erpLogsService;
    #endregion

    #region Ctor

    public OverridenShoppingCartController(CaptchaSettings captchaSettings,
        CustomerSettings customerSettings,
        IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser,
        IAttributeService<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeService,
        ICurrencyService currencyService,
        ICustomerActivityService customerActivityService,
        ICustomerService customerService,
        IDiscountService discountService,
        IDownloadService downloadService,
        IGenericAttributeService genericAttributeService,
        IGiftCardService giftCardService,
        IHtmlFormatter htmlFormatter,
        ILocalizationService localizationService,
        INopFileProvider fileProvider,
        INopUrlHelper nopUrlHelper,
        INotificationService notificationService,
        IPermissionService permissionService,
        IPictureService pictureService,
        IPriceFormatter priceFormatter,
        IProductAttributeParser productAttributeParser,
        IProductAttributeService productAttributeService,
        IProductService productService,
        IShippingService shippingService,
        IShoppingCartModelFactory shoppingCartModelFactory,
        IShoppingCartService shoppingCartService,
        IStaticCacheManager staticCacheManager,
        IStoreContext storeContext,
        ITaxService taxService,
        IUrlRecordService urlRecordService,
        IWebHelper webHelper,
        IWorkContext workContext,
        IWorkflowMessageService workflowMessageService,
        MediaSettings mediaSettings,
        OrderSettings orderSettings,
        ShoppingCartSettings shoppingCartSettings,
        ShippingSettings shippingSettings,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        IOrderService orderService,
        IPriceCalculationService priceCalculationService,
        IErpProductModelFactory erpProductModelFactory,
        ILogger logger,
        IErpLogsService erpLogsService,
        IStoreMappingService storeMappingService) : base(captchaSettings,
            customerSettings,
            checkoutAttributeParser,
            checkoutAttributeService,
            currencyService,
            customerActivityService,
            customerService,
            discountService,
            downloadService,
            genericAttributeService,
            giftCardService,
            htmlFormatter,
            localizationService,
            fileProvider,
            nopUrlHelper,
            notificationService,
            permissionService,
            pictureService,
            priceFormatter,
            productAttributeParser,
            productAttributeService,
            productService,
            shippingService,
            shoppingCartModelFactory,
            shoppingCartService,
            staticCacheManager,
            storeContext,
            storeMappingService,
            taxService,
            urlRecordService,
            webHelper,
            workContext,
            workflowMessageService,
            mediaSettings,
            orderSettings,
            shoppingCartSettings,
            shippingSettings)
    {
        _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _orderService = orderService;
        _erpProductModelFactory = erpProductModelFactory;
        _logger = logger;
        _erpLogsService = erpLogsService;
    }

    #endregion

    #region Utilities

    protected override async Task<IActionResult> GetProductToCartDetailsAsync(List<string> addToCartWarnings, ShoppingCartType cartType,
        Product product)
    {
        if (addToCartWarnings.Any())
        {
            //cannot be added to the cart/wishlist
            //let's display warnings
            return Json(new
            {
                success = false,
                productId = product.Id,
                message = addToCartWarnings.ToArray()
            });
        }

        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var currStore = await _storeContext.GetCurrentStoreAsync();

        //added to the cart/wishlist
        switch (cartType)
        {
            case ShoppingCartType.Wishlist:
                {
                    //activity log
                    await _customerActivityService.InsertActivityAsync("PublicStore.AddToWishlist",
                        string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToWishlist"), product.Name), product);

                    if (_shoppingCartSettings.DisplayWishlistAfterAddingProduct)
                    {
                        //redirect to the wishlist page
                        return Json(new
                        {
                            redirect = Url.RouteUrl("Wishlist")
                        });
                    }

                    //display notification message and update appropriate blocks
                    var shoppingCarts = await _shoppingCartService.GetShoppingCartAsync(currCustomer, ShoppingCartType.Wishlist, currStore.Id);

                    var updatetopwishlistsectionhtml = string.Format(
                        await _localizationService.GetResourceAsync("Wishlist.HeaderQuantity"),
                        shoppingCarts.Sum(item => item.Quantity));

                    return Json(new
                    {
                        success = true,
                        message = string.Format(
                            await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheWishlist.Link"),
                            Url.RouteUrl("Wishlist")),
                        updatetopwishlistsectionhtml
                    });
                }
            case ShoppingCartType.ShoppingCart:
            default:
                {
                    //activity log
                    await _customerActivityService.InsertActivityAsync("PublicStore.AddToShoppingCart",
                        string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToShoppingCart"), product.Name), product);

                    if (_shoppingCartSettings.DisplayCartAfterAddingProduct)
                    {
                        //redirect to the shopping cart page
                        return Json(new
                        {
                            redirect = Url.RouteUrl("ShoppingCart")
                        });
                    }

                    //display notification message and update appropriate blocks
                    var shoppingCarts = await _shoppingCartService.GetShoppingCartAsync(currCustomer, ShoppingCartType.ShoppingCart, currStore.Id);

                    var updatetopcartsectionhtml = string.Format(
                        await _localizationService.GetResourceAsync("ShoppingCart.HeaderQuantity"),
                        shoppingCarts.Sum(item => item.Quantity));

                    var updateflyoutcartsectionhtml = _shoppingCartSettings.MiniShoppingCartEnabled
                        ? await RenderViewComponentToStringAsync(typeof(FlyoutShoppingCartViewComponent))
                        : string.Empty;

                    // B2B Custom (backorder Checking)
                    var shoppingCartItem = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(shoppingCarts, cartType, product);
                    var shoppingCartItemQuantity = shoppingCartItem?.Quantity ?? 0;
                    var productQuantity = await _productService.GetTotalStockQuantityAsync(product);
                    var message = productQuantity < shoppingCartItemQuantity ? await _localizationService.GetResourceAsync("B2B.BackOrderProducts.ProductHasBeenAddedToTheCart.Link")
                        : await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheCart.Link");

                    return Json(new
                    {
                        success = true,
                        isBackOrder = productQuantity < shoppingCartItemQuantity,
                        productId = product.Id,
                        message = string.Format(message, Url.RouteUrl("ShoppingCart")),
                        updatetopcartsectionhtml,
                        updateflyoutcartsectionhtml
                    });
                }
        }
    }
    #endregion

    #region Shopping cart

    public override async Task<IActionResult> Cart()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart))
            return RedirectToRoute("Homepage");

        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, store.Id);
        var model = new ShoppingCartModel();
        model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);

        return View(model);
    }

    [HttpPost, ActionName("Cart")]
    [FormValueRequired("checkout")]
    public override async Task<IActionResult> StartCheckout(IFormCollection form)
    {
        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountOfCurrentCustomer();
        if (erpAccount == null)
        {
            _notificationService.WarningNotification("Erp Accounts Need for Checkout");
            return RedirectToAction(nameof(Cart));
        }

        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

        //parse and save checkout attributes
        await ParseAndSaveCheckoutAttributesAsync(cart, form);

        //validate attributes
        var checkoutAttributes = await _genericAttributeService.GetAttributeAsync<string>(customer,
            NopCustomerDefaults.CheckoutAttributes, store.Id);
        var checkoutAttributeWarnings = await _shoppingCartService.GetShoppingCartWarningsAsync(cart, checkoutAttributes, true);
        if (checkoutAttributeWarnings.Any())
        {
            //something wrong, redisplay the page with warnings
            var model = new ShoppingCartModel();
            model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart, validateCheckoutAttributes: true);
            return View(model);
        }

        var anonymousPermissed = _orderSettings.AnonymousCheckoutAllowed
                                 && _customerSettings.UserRegistrationType == UserRegistrationType.Disabled;

        if (anonymousPermissed || !await _customerService.IsGuestAsync(customer))
            return RedirectToRoute("Checkout");

        var cartProductIds = cart.Select(ci => ci.ProductId).ToArray();
        var downloadableProductsRequireRegistration =
            _customerSettings.RequireRegistrationForDownloadableProducts && await _productService.HasAnyDownloadableProductAsync(cartProductIds);

        if (!_orderSettings.AnonymousCheckoutAllowed || downloadableProductsRequireRegistration)
        {
            //verify user identity (it may be facebook login page, or google, or local)
            return Challenge();
        }

        return RedirectToRoute("LoginCheckoutAsGuest", new { returnUrl = Url.RouteUrl("ShoppingCart") });
    }

    [HttpPost, ActionName("Cart")]
    [FormValueRequired("quote")]
    public virtual async Task<IActionResult> StartQuoteCheckout(IFormCollection form)
    {
        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountOfCurrentCustomer();
        if (erpAccount == null)
        {
            _notificationService.WarningNotification("Erp Accounts Need for Checkout");
            return RedirectToAction(nameof(Cart));
        }

        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(currCustomer, ShoppingCartType.ShoppingCart, store.Id);

        //parse and save checkout attributes
        await ParseAndSaveCheckoutAttributesAsync(cart, form);

        //validate attributes
        var checkoutAttributes = await _genericAttributeService.GetAttributeAsync<string>(currCustomer,
            NopCustomerDefaults.CheckoutAttributes, store.Id);
        var checkoutAttributeWarnings = await _shoppingCartService.GetShoppingCartWarningsAsync(cart, checkoutAttributes, true);
        if (checkoutAttributeWarnings.Any())
        {
            //something wrong, redisplay the page with warnings
            var model = new ShoppingCartModel();
            model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart, validateCheckoutAttributes: true);
            return View("~/Views/ShoppingCart/Cart.cshtml", model);
        }

        var anonymousPermissed = _orderSettings.AnonymousCheckoutAllowed
                                 && _customerSettings.UserRegistrationType == UserRegistrationType.Disabled;

        if (anonymousPermissed || !await _customerService.IsGuestAsync(currCustomer))
            return RedirectToRoute("QuoteOrder");

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

        if (!_orderSettings.AnonymousCheckoutAllowed || downloadableProductsRequireRegistration)
        {
            //verify user identity (it may be facebook login page, or google, or local)
            return Challenge();
        }

        return RedirectToRoute("LoginCheckoutAsGuest", new { returnUrl = Url.RouteUrl("ShoppingCart") });
    }

    //add product to cart using AJAX
    //currently we use this method on catalog pages (category/manufacturer/etc)
    [HttpPost]
    public override async Task<IActionResult> AddProductToCart_Catalog(int productId, int shoppingCartTypeId,
        int quantity, bool forceredirection = false)
    {
        var cartType = (ShoppingCartType)shoppingCartTypeId;

        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
            //no product found
            return Json(new
            {
                success = false,
                message = "No product found with the specified ID"
            });

        //we can add only simple products
        if (product.ProductType != ProductType.SimpleProduct)
        {
            return Json(new
            {
                redirect = Url.RouteUrl("ProductDetails", new { SeName = await _urlRecordService.GetSeNameAsync(product) })
            });
        }

        //products with "minimum order quantity" more than a specified qty
        if (product.OrderMinimumQuantity > quantity)
        {
            //we cannot add to the cart such products from category pages
            //it can confuse customers. That's why we redirect customers to the product details page
            return Json(new
            {
                redirect = Url.RouteUrl("ProductDetails", new { SeName = await _urlRecordService.GetSeNameAsync(product) })
            });
        }

        if (product.CustomerEntersPrice)
        {
            //cannot be added to the cart (requires a customer to enter price)
            return Json(new
            {
                redirect = Url.RouteUrl("ProductDetails", new { SeName = await _urlRecordService.GetSeNameAsync(product) })
            });
        }

        if (product.IsRental)
        {
            //rental products require start/end dates to be entered
            return Json(new
            {
                redirect = Url.RouteUrl("ProductDetails", new { SeName = await _urlRecordService.GetSeNameAsync(product) })
            });
        }

        var allowedQuantities = _productService.ParseAllowedQuantities(product);
        if (allowedQuantities.Length > 0)
        {
            //cannot be added to the cart (requires a customer to select a quantity from dropdownlist)
            return Json(new
            {
                redirect = Url.RouteUrl("ProductDetails", new { SeName = await _urlRecordService.GetSeNameAsync(product) })
            });
        }

        //allow a product to be added to the cart when all attributes are with "read-only checkboxes" type
        var productAttributes = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
        if (productAttributes.Any(pam => pam.AttributeControlType != AttributeControlType.ReadonlyCheckboxes))
        {
            //product has some attributes. let a customer see them
            return Json(new
            {
                redirect = Url.RouteUrl("ProductDetails", new { SeName = await _urlRecordService.GetSeNameAsync(product) })
            });
        }

        //creating XML for "read-only checkboxes" attributes
        var attXml = await productAttributes.AggregateAwaitAsync(string.Empty, async (attributesXml, attribute) =>
        {
            var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
            foreach (var selectedAttributeId in attributeValues
                .Where(v => v.IsPreSelected)
                .Select(v => v.Id)
                .ToList())
            {
                attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                    attribute, selectedAttributeId.ToString());
            }
            return attributesXml;
        });

        //get standard warnings without attribute validations
        //first, try to find existing shopping cart item
        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(currCustomer, cartType, store.Id);
        var shoppingCartItem = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(cart, cartType, product);
        //if we already have the same product in the cart, then use the total quantity to validate
        var quantityToValidate = shoppingCartItem != null ? shoppingCartItem.Quantity + quantity : quantity;
        var addToCartWarnings = await _shoppingCartService
            .GetShoppingCartItemWarningsAsync(currCustomer, cartType,
            product, store.Id, string.Empty,
            decimal.Zero, null, null, quantityToValidate, false, shoppingCartItem?.Id ?? 0, true, false, false, false);
        if (addToCartWarnings.Any())
        {
            //cannot be added to the cart
            //let's display standard warnings
            return Json(new
            {
                success = false,
                productId = productId,
                message = addToCartWarnings.ToArray()
            });
        }

        //now let's try adding product to the cart (now including product attribute validation, etc)
        addToCartWarnings = await _shoppingCartService.AddToCartAsync(customer: currCustomer,
            product: product,
            shoppingCartType: cartType,
            storeId: store.Id,
            attributesXml: attXml,
            quantity: quantity);
        if (addToCartWarnings.Any())
        {
            //cannot be added to the cart
            //but we do not display attribute and gift card warnings here. let's do it on the product details page
            return Json(new
            {
                redirect = Url.RouteUrl("ProductDetails", new { SeName = await _urlRecordService.GetSeNameAsync(product) })
            });
        }

        //added to the cart/wishlist
        switch (cartType)
        {
            case ShoppingCartType.Wishlist:
                {
                    //activity log
                    await _customerActivityService.InsertActivityAsync("PublicStore.AddToWishlist",
                        string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToWishlist"), product.Name), product);

                    if (_shoppingCartSettings.DisplayWishlistAfterAddingProduct || forceredirection)
                    {
                        //redirect to the wishlist page
                        return Json(new
                        {
                            redirect = Url.RouteUrl("Wishlist")
                        });
                    }

                    //display notification message and update appropriate blocks
                    var shoppingCarts = await _shoppingCartService.GetShoppingCartAsync(currCustomer, ShoppingCartType.Wishlist, store.Id);

                    var updatetopwishlistsectionhtml = string.Format(await _localizationService.GetResourceAsync("Wishlist.HeaderQuantity"),
                        shoppingCarts.Sum(item => item.Quantity));
                    return Json(new
                    {
                        success = true,
                        message = string.Format(await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheWishlist.Link"), Url.RouteUrl("Wishlist")),
                        updatetopwishlistsectionhtml
                    });
                }
            case ShoppingCartType.ShoppingCart:
            default:
                {
                    //activity log
                    await _customerActivityService.InsertActivityAsync("PublicStore.AddToShoppingCart",
                        string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToShoppingCart"), product.Name), product);

                    if (_shoppingCartSettings.DisplayCartAfterAddingProduct || forceredirection)
                    {
                        //redirect to the shopping cart page
                        return Json(new
                        {
                            redirect = Url.RouteUrl("ShoppingCart")
                        });
                    }

                    //display notification message and update appropriate blocks
                    var shoppingCarts = await _shoppingCartService.GetShoppingCartAsync(currCustomer, ShoppingCartType.ShoppingCart, store.Id);

                    var updatetopcartsectionhtml = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.HeaderQuantity"),
                        shoppingCarts.Sum(item => item.Quantity));

                    var updateflyoutcartsectionhtml = _shoppingCartSettings.MiniShoppingCartEnabled
                        ? await RenderViewComponentToStringAsync(typeof(FlyoutShoppingCartViewComponent))
                        : "";

                    // B2B Custom (backorder Checking)
                    var productQuantity = await _productService.GetTotalStockQuantityAsync(product);
                    var message = productQuantity < quantityToValidate ? await _localizationService.GetResourceAsync("B2B.BackOrderProducts.ProductHasBeenAddedToTheCart.Link")
                        : await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheCart.Link");
                    return Json(new
                    {
                        success = true,
                        isBackOrder = productQuantity < quantityToValidate,
                        productId = productId,
                        message = string.Format(message, Url.RouteUrl("ShoppingCart")),
                        updatetopcartsectionhtml,
                        updateflyoutcartsectionhtml
                    });
                }
        }
    }

    #endregion

    #region B2B Extra

    // for cart page
    public async Task<IActionResult> LoadB2BCartItemData()
    {
        var model = await _erpProductModelFactory.PrepareErpOrderSummaryModelAsync();
        return Json(new
        {
            Data = model
        });
    }

    public virtual async Task<IActionResult> ClearCart(bool deleteWishlistAndShoppingCartItems)
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        // if cart is undergoing any activity, don't clear it. wait for it to finish.
        var isCartActivityOn = await _genericAttributeService.GetAttributeAsync<bool>(currentCustomer, B2BB2CFeaturesDefaults.IsCartActivityOn, store.Id);
        if (isCartActivityOn)
        {
            _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ShoppingCart.Warning.CartActivityOn"));
            return RedirectToRoute("ShoppingCart");
        }

        try
        {
            // set cart activity attribute and begin clearing cart
            await _genericAttributeService.SaveAttributeAsync(currentCustomer, B2BB2CFeaturesDefaults.IsCartActivityOn, true, store.Id);

            var carts = deleteWishlistAndShoppingCartItems ?
                await _shoppingCartService.GetShoppingCartAsync(currentCustomer, storeId: store.Id) :
                await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart, store.Id);

            if (carts != null)
            {
                //clear shopping cart
                carts.ToList().ForEach(async sci => await _shoppingCartService.DeleteShoppingCartItemAsync(sci, false));
            }
        }
        catch (Exception ex)
        {
            _logger.Error(_localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Wishlist.ClearCart.Error") + " " + ex.Message, ex);
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Wishlist.ClearCart.Error"));

            //ERP activity log
            await _erpLogsService.ErrorAsync(_localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Wishlist.ClearCart.Error") + " " + ex.Message, ErpSyncLevel.Order, customer: currentCustomer);

            return Json(new
            {
                success = false,
                message = _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Wishlist.ClearCart.Error")
            });
        }
        finally
        {
            // reset cart activity attribute 
            await _genericAttributeService.SaveAttributeAsync(currentCustomer, B2BB2CFeaturesDefaults.IsCartActivityOn, false, store.Id);
        }

        return Json(new { success = true });
    }


    // for cart page
    public async Task<IActionResult> CheckCartItemQuotePriceChangeWarning()
    {
        var showWarning = false;
        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var currStore = await _storeContext.GetCurrentStoreAsync();
        var b2bOrderId = await _genericAttributeService.GetAttributeAsync<int>(currCustomer, B2BB2CFeaturesDefaults.B2BConvertedQuoteB2BOrderId, currStore.Id);

        if (b2bOrderId > 0)
        {
            var b2BOrderPerAccount = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByIdAsync(b2bOrderId);

            if (await _erpCustomerFunctionalityService.CheckQuoteOrderStatusAsync(b2BOrderPerAccount))
            {
                var quoteOrder = await _orderService.GetOrderByIdAsync(b2BOrderPerAccount.NopOrderId);
                var quoteOrderItems = await _orderService.GetOrderItemsAsync(quoteOrder?.Id ?? 0);
                var cartItems = await _shoppingCartService.GetShoppingCartAsync(currCustomer, ShoppingCartType.ShoppingCart, currStore.Id);

                if (quoteOrderItems != null && cartItems.Any())
                {
                    foreach (var cartItem in cartItems)
                    {
                        var quoteOrderItemsForCartItem = quoteOrderItems?.Where(x => x.ProductId == cartItem?.ProductId)?.OrderByDescending(x => x.UnitPriceExclTax)?.ToList();
                        var relatedQuoteOrderItem = quoteOrderItemsForCartItem?.FirstOrDefault();

                        var lowestLineQuantity = quoteOrderItemsForCartItem != null && quoteOrderItemsForCartItem?.Count > 0 ? quoteOrderItemsForCartItem?.Min(x => x.Quantity) : 0;

                        if (relatedQuoteOrderItem != null && cartItem.Quantity < lowestLineQuantity)
                        {
                            //unit price
                            var (productUnitPriceWithDiscount, discountAmount, appliedDiscounts) = await _shoppingCartService.GetUnitPriceAsync(cartItem, false);
                            if (relatedQuoteOrderItem.UnitPriceExclTax < productUnitPriceWithDiscount)
                                showWarning = true;
                        }
                    }
                }

            }
        }
        return Json(new
        {
            Data = showWarning
        });
    }

    #endregion
}
