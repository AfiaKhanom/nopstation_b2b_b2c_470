using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Components;
using Nop.Web.Factories;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Widgets.AdvanceCart.Components;
using NopStation.Plugin.Widgets.AdvanceCart.Factories;
using NopStation.Plugin.Widgets.AdvanceCart.Models;
using Nop.Web.Framework.Mvc.Routing;

namespace NopStation.Plugin.Widgets.AdvanceCart.Controllers;

[AutoValidateAntiforgeryToken]
public class AdvanceCartController : NopStationPublicController
{
    #region Fields

    private readonly IPermissionService _permissionService;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IProductService _productService;
    private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
    private readonly IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeParser;
    private readonly IAttributeService<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IDiscountService _discountService;
    private readonly ICustomerService _customerService;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger _logger;
    private readonly IGiftCardService _giftCardService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IProductAttributeService _productAttributeService;
    private readonly IProductAttributeParser _productAttributeParser;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly ShoppingCartSettings _shoppingCartSettings;
    private readonly IDownloadService _downloadService;
    private readonly IWidgetPluginManager _widgetPluginManager;
    private readonly AdvanceCartSettings _advanceCartSettings;
    private readonly IAdvanceCartModelFactory _advanceCartModelFactory;

    #endregion

    #region Ctor

    public AdvanceCartController(IPermissionService permissionService,
        IWorkContext workContext,
        IStoreContext storeContext,
        IShoppingCartService shoppingCartService,
        IProductService productService,
        IShoppingCartModelFactory shoppingCartModelFactory,
        IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser,
        IAttributeService<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeService,
        IGenericAttributeService genericAttributeService,
        IDiscountService discountService,
        ICustomerService customerService,
        ILocalizationService localizationService,
        ILogger logger, 
        IGiftCardService giftCardService,
        IUrlRecordService urlRecordService,
        IProductAttributeService productAttributeService,
        IProductAttributeParser productAttributeParser,
        ICustomerActivityService customerActivityService,
        ShoppingCartSettings shoppingCartSettings,
        IDownloadService downloadService,
        IWidgetPluginManager widgetPluginManager,
        AdvanceCartSettings advanceCartSettings,
        IAdvanceCartModelFactory advanceCartModelFactory)
    {
        _permissionService = permissionService;
        _workContext = workContext;
        _storeContext = storeContext;
        _shoppingCartService = shoppingCartService;
        _productService = productService;
        _shoppingCartModelFactory = shoppingCartModelFactory;
        _checkoutAttributeService = checkoutAttributeService;
        _checkoutAttributeParser = checkoutAttributeParser;
        _genericAttributeService = genericAttributeService;
        _discountService = discountService;
        _customerService = customerService;
        _localizationService = localizationService;
        _logger = logger;
        _giftCardService = giftCardService;
        _urlRecordService = urlRecordService;
        _productAttributeService = productAttributeService;
        _productAttributeParser = productAttributeParser;
        _customerActivityService = customerActivityService;
        _shoppingCartSettings = shoppingCartSettings;
        _downloadService = downloadService;
        _widgetPluginManager = widgetPluginManager;
        _advanceCartSettings = advanceCartSettings;
        _advanceCartModelFactory = advanceCartModelFactory;
    }

    #endregion

    #region Utilities

    protected virtual async Task SaveItemAsync(ShoppingCartItem updatecartitem, List<string> addToCartWarnings, Product product,
       string attributes, decimal customerEnteredPriceConverted, DateTime? rentalStartDate,
       DateTime? rentalEndDate, int quantity)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        if (updatecartitem == null)
        {
            //add to the cart
            addToCartWarnings.AddRange(await _shoppingCartService.AddToCartAsync(customer,
                product, ShoppingCartType.ShoppingCart, store.Id,
                attributes, customerEnteredPriceConverted,
                rentalStartDate, rentalEndDate, quantity, true));
        }
        else
        {
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, updatecartitem.ShoppingCartType, store.Id);

            var otherCartItemWithSameParameters = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(
                cart, updatecartitem.ShoppingCartType, product, attributes, customerEnteredPriceConverted,
                rentalStartDate, rentalEndDate);
            if (otherCartItemWithSameParameters != null &&
                otherCartItemWithSameParameters.Id == updatecartitem.Id)
            {
                //ensure it's some other shopping cart item
                otherCartItemWithSameParameters = null;
            }
            //update existing item
            addToCartWarnings.AddRange(await _shoppingCartService.UpdateShoppingCartItemAsync(customer,
                updatecartitem.Id, attributes, customerEnteredPriceConverted,
                rentalStartDate, rentalEndDate, quantity + (otherCartItemWithSameParameters?.Quantity ?? 0), true));
            if (otherCartItemWithSameParameters != null && !addToCartWarnings.Any())
            {
                //delete the same shopping cart item (the other one)
                await _shoppingCartService.DeleteShoppingCartItemAsync(otherCartItemWithSameParameters);
            }
        }
    }

    protected virtual async Task<IActionResult> GetProductToCartDetailsAsync(List<string> addToCartWarnings, Product product, string attributes, bool buynow = false)
    {
        if (addToCartWarnings.Any())
        {
            //cannot be added to the cart/wishlist
            //let's display warnings
            return Json(new
            {
                success = false,
                message = addToCartWarnings.ToArray()
            });
        }

        //added to the cart/wishlist
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        //activity log
        await _customerActivityService.InsertActivityAsync("PublicStore.AddToShoppingCart",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToShoppingCart"), product.Name), product);

        if (_advanceCartSettings.EnableBuyNowButton && buynow)
        {
            //redirect to the checkout page
            return Json(new
            {
                redirect = Url.RouteUrl("Checkout")
            });
        }

        if (_shoppingCartSettings.DisplayCartAfterAddingProduct)
        {
            //redirect to the shopping cart page
            return Json(new
            {
                redirect = Url.RouteUrl("ShoppingCart")
            });
        }

        //display notification message and update appropriate blocks
        var shoppingCarts = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

        var updateTopCartSectionHtml = string.Format(
            await _localizationService.GetResourceAsync("ShoppingCart.HeaderQuantity"),
            shoppingCarts.Sum(item => item.Quantity));

        var updateFlyoutCartSectionHtml = _shoppingCartSettings.MiniShoppingCartEnabled ?  await RenderViewComponentToStringAsync(typeof(FlyoutShoppingCartViewComponent))
            : string.Empty;

        var popupnotificationhtml = _advanceCartSettings.EnableAddedToCartNotificationPopup
            ? await RenderViewComponentToStringAsync(typeof(AdvanceCartAddedToCartViewComponent), await _advanceCartModelFactory.PrepareAdvanceCartAddedToCartModelAsync(product, customer, ShoppingCartType.ShoppingCart, store.Id, attributes, 0))
            : string.Empty;

        return Json(new
        {
            success = true,
            message = string.Format(await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheCart.Link"), Url.RouteUrl("ShoppingCart")),
            popupnotificationhtml,
            updatetopcartsectionhtml = updateTopCartSectionHtml,
            updateflyoutcartsectionhtml = updateFlyoutCartSectionHtml
        });
    }

    #endregion

    #region Methods

    [HttpPost]
    public virtual async Task<IActionResult> AddProductToCart_Catalog(int productId, int quantity, bool buynow = false)
    {
        var cartType = ShoppingCartType.ShoppingCart;

        var quickViewEnabled = await _widgetPluginManager.IsPluginActiveAsync("NopStation.Plugin.Widgets.QuickView");

        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
            //no product found
            return Json(new
            {
                success = false,
                message = "No product found with the specified ID"
            });

        var productRedirectUrl = Url.RouteUrl<Product>(new { SeName = await _urlRecordService.GetSeNameAsync(product) });

        //we can add only simple products
        if (product.ProductType != ProductType.SimpleProduct)
        {
            return Json(new
            {
                openquickview = quickViewEnabled && _advanceCartSettings.OpenQuickViewIfRedirectToDetailsPageRequired,
                redirect = productRedirectUrl,
                productid = productId
            });
        }

        //products with "minimum order quantity" more than a specified qty
        if (product.OrderMinimumQuantity > quantity)
        {
            //we cannot add to the cart such products from category pages
            //it can confuse customers. That's why we redirect customers to the product details page
            return Json(new
            {
                openquickview = quickViewEnabled && _advanceCartSettings.OpenQuickViewIfRedirectToDetailsPageRequired,
                redirect = productRedirectUrl,
                productid = productId
            });
        }

        if (product.CustomerEntersPrice)
        {
            //cannot be added to the cart (requires a customer to enter price)
            return Json(new
            {
                openquickview = quickViewEnabled && _advanceCartSettings.OpenQuickViewIfRedirectToDetailsPageRequired,
                redirect = productRedirectUrl,
                productid = productId
            });
        }

        if (product.IsRental)
        {
            //rental products require start/end dates to be entered
            return Json(new
            {
                openquickview = quickViewEnabled && _advanceCartSettings.OpenQuickViewIfRedirectToDetailsPageRequired,
                redirect = productRedirectUrl,
                productid = productId
            });
        }

        var allowedQuantities = _productService.ParseAllowedQuantities(product);
        if (allowedQuantities.Length > 0 && !allowedQuantities.Contains(quantity))
        {
            //cannot be added to the cart (requires a customer to select a quantity from dropdownlist)
            return Json(new
            {
                openquickview = quickViewEnabled && _advanceCartSettings.OpenQuickViewIfRedirectToDetailsPageRequired,
                redirect = productRedirectUrl,
                productid = productId
            });
        }

        //allow a product to be added to the cart when all attributes are with "read-only checkboxes" type
        var productAttributes = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
        if (productAttributes.Any(pam => pam.AttributeControlType != AttributeControlType.ReadonlyCheckboxes))
        {

            //product has some attributes. let a customer see them
            return Json(new
            {
                openquickview = quickViewEnabled && _advanceCartSettings.OpenQuickViewIfRedirectToDetailsPageRequired,
                redirect = productRedirectUrl,
                productid = productId
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
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, cartType, store.Id);
        var shoppingCartItem = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(cart, cartType, product);
        //if we already have the same product in the cart, then use the total quantity to validate
        var quantityToValidate = shoppingCartItem != null ? shoppingCartItem.Quantity + quantity : quantity;
        var addToCartWarnings = await _shoppingCartService
            .GetShoppingCartItemWarningsAsync(customer, cartType,
            product, store.Id, string.Empty,
            decimal.Zero, null, null, quantityToValidate, false, shoppingCartItem?.Id ?? 0, true, false, false, false);
        if (addToCartWarnings.Any())
        {
            //cannot be added to the cart
            //let's display standard warnings
            return Json(new
            {
                success = false,
                message = addToCartWarnings.ToArray()
            });
        }

        //now let's try adding product to the cart (now including product attribute validation, etc)
        addToCartWarnings = await _shoppingCartService.AddToCartAsync(customer: customer,
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
                openquickview = quickViewEnabled && _advanceCartSettings.OpenQuickViewIfRedirectToDetailsPageRequired,
                redirect = productRedirectUrl,
                productid = productId
            });
        }

        //activity log
        await _customerActivityService.InsertActivityAsync("PublicStore.AddToShoppingCart",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToShoppingCart"), product.Name), product);

        if (_advanceCartSettings.EnableBuyNowButton && buynow)
        {
            //redirect to the checkout page
            return Json(new
            {
                redirect = Url.RouteUrl("Checkout")
            });
        }

        if (_shoppingCartSettings.DisplayCartAfterAddingProduct)
        {
            //redirect to the shopping cart page
            return Json(new
            {
                redirect = Url.RouteUrl("ShoppingCart"),
                productid = productId
            });
        }

        //display notification message and update appropriate blocks
        var shoppingCarts = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

        var updatetopcartsectionhtml = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.HeaderQuantity"),
            shoppingCarts.Sum(item => item.Quantity));

        var updateflyoutcartsectionhtml = _shoppingCartSettings.MiniShoppingCartEnabled
            ? (_advanceCartSettings.EnableAdvanceFlyoutCart ? await RenderViewComponentToStringAsync(typeof(AdvanceCartFlyoutShoppingCartViewComponent)) : await RenderViewComponentToStringAsync(typeof(FlyoutShoppingCartViewComponent)))
            : string.Empty;

        var popupnotificationhtml = _advanceCartSettings.EnableAddedToCartNotificationPopup
            ? await RenderViewComponentToStringAsync(typeof(AdvanceCartAddedToCartViewComponent), await _advanceCartModelFactory.PrepareAdvanceCartAddedToCartModelAsync(product, customer, ShoppingCartType.ShoppingCart, store.Id))
            : string.Empty;

        return Json(new
        {
            success = true,
            message = string.Format(await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheCart.Link"), Url.RouteUrl("ShoppingCart")),
            popupnotificationhtml,
            updatetopcartsectionhtml,
            updateflyoutcartsectionhtml,
            productid = productId
        });
    }

    //add product to cart using AJAX
    //currently we use this method on the product details pages
    [HttpPost]
    public virtual async Task<IActionResult> AddProductToCart_Details(int productId, IFormCollection form, bool buynow = false)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
        {
            return Json(new
            {
                redirect = Url.RouteUrl("Homepage")
            });
        }

        //we can add only simple products
        if (product.ProductType != ProductType.SimpleProduct)
        {
            return Json(new
            {
                success = false,
                message = "Only simple products could be added to the cart"
            });
        }

        //update existing shopping cart item
        var updatecartitemid = 0;
        foreach (var formKey in form.Keys)
            if (formKey.Equals($"addtocart_{productId}.UpdatedShoppingCartItemId", StringComparison.InvariantCultureIgnoreCase))
            {
                _ = int.TryParse(form[formKey], out updatecartitemid);
                break;
            }

        ShoppingCartItem updatecartitem = null;
        if (_shoppingCartSettings.AllowCartItemEditing && updatecartitemid > 0)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            //search with the same cart type as specified
            var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, store.Id);

            updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
            //not found? let's ignore it. in this case we'll add a new item
            //if (updatecartitem == null)
            //{
            //    return Json(new
            //    {
            //        success = false,
            //        message = "No shopping cart item found to update"
            //    });
            //}
            //is it this product?
            if (updatecartitem != null && product.Id != updatecartitem.ProductId)
            {
                return Json(new
                {
                    success = false,
                    message = "This product does not match a passed shopping cart item identifier"
                });
            }
        }

        var addToCartWarnings = new List<string>();

        //customer entered price
        var customerEnteredPriceConverted = await _productAttributeParser.ParseCustomerEnteredPriceAsync(product, form);

        //entered quantity
        var quantity = _productAttributeParser.ParseEnteredQuantity(product, form);

        //product and gift card attributes
        var attributes = await _productAttributeParser.ParseProductAttributesAsync(product, form, addToCartWarnings);

        //rental attributes
        _productAttributeParser.ParseRentalDates(product, form, out var rentalStartDate, out var rentalEndDate);

        await SaveItemAsync(updatecartitem, addToCartWarnings, product, attributes, customerEnteredPriceConverted, rentalStartDate, rentalEndDate, quantity);

        //return result
        return await GetProductToCartDetailsAsync(addToCartWarnings, product, attributes, buynow);
    }

    public virtual async Task<IActionResult> GetFlyoutCart()
    {
        var html = _advanceCartSettings.EnableAdvanceFlyoutCart ? await RenderViewComponentToStringAsync(typeof(AdvanceCartFlyoutShoppingCartViewComponent)) : "";
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, await _storeContext.GetActiveStoreScopeConfigurationAsync());

        var updatetopcartsectionhtml = string.Format(
            await _localizationService.GetResourceAsync("ShoppingCart.HeaderQuantity"),
            cart.Sum(item => item.Quantity));

        //return result
        return Json(new
        {
            updateflyoutcartsectionhtml = html,
            updatetopcartsectionhtml

        });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteFlyoutCartItemAsync(MiniShoppingCartRequestModel miniShoppingCartRequestModel)
    {

        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
        var products = (await _productService.GetProductsByIdsAsync(cart.Select(item => item.ProductId).Distinct().ToArray()))
            .ToDictionary(item => item.Id, item => item);

        var itemIdToRemove = miniShoppingCartRequestModel.ShoppingCartItemId;
        //await _shoppingCartService.DeleteShoppingCartItemAsync(itemIdToRemove);
        var itemsWithNewQuantity = cart.Select(item => new
        {
            //try to get a new quantity for the item, set 0 for items to remove
            NewQuantity = item.Id == itemIdToRemove ? 0 : item.Quantity,
            Item = item,
            Product = products.ContainsKey(item.ProductId) ? products[item.ProductId] : null
        }).Where(item => item.NewQuantity != item.Item.Quantity);

        var orderedCart = itemsWithNewQuantity
                         .OrderByDescending(async cartItem =>
                         (cartItem.NewQuantity < cartItem.Item.Quantity &&
                         (cartItem.Product?.RequireOtherProducts ?? false)) ||
                         (cartItem.NewQuantity > cartItem.Item.Quantity && cartItem.Product != null && (await _shoppingCartService
                         .GetProductsRequiringProductAsync(cart, cartItem.Product)).Any())).ToList();

        var warnings = await orderedCart.SelectAwait(async cartItem => new
        {
            ItemId = cartItem.Item.Id,
            Warnings = await _shoppingCartService.UpdateShoppingCartItemAsync(await _workContext.GetCurrentCustomerAsync(),
                 cartItem.Item.Id, cartItem.Item.AttributesXml, cartItem.Item.CustomerEnteredPrice,
                 cartItem.Item.RentalStartDateUtc, cartItem.Item.RentalEndDateUtc, cartItem.NewQuantity, true)
        }).ToListAsync();
        var miniShoppingCartModel = await _shoppingCartModelFactory.PrepareMiniShoppingCartModelAsync();
        MiniShoppingCartResponseModel miniShoppingCartResponseModel = new MiniShoppingCartResponseModel();
        bool isSuccess = true;
        string errorOrWarningMsg = string.Empty;

        if (warnings.Any(x => x.Warnings.Count > 0))
        {
            isSuccess = false;
            var errorOrWarningMessage = warnings.SelectMany(x => x.Warnings);
            errorOrWarningMsg = string.Join(",", errorOrWarningMessage);
        }

        else
        {
            miniShoppingCartResponseModel.ShoppingCartItemId = miniShoppingCartRequestModel.ShoppingCartItemId;
            miniShoppingCartResponseModel.ShoppingCartItemUpdatedQuantity = miniShoppingCartRequestModel.ShoppingCartItemNewQuantity;
            miniShoppingCartResponseModel.TotalQuantity = miniShoppingCartModel.TotalProducts;
            miniShoppingCartResponseModel.Subtotal = miniShoppingCartModel.SubTotal;
            miniShoppingCartResponseModel.UnitPrice = miniShoppingCartModel?.Items?.Where(x => x.Id == miniShoppingCartRequestModel.ShoppingCartItemId)?.FirstOrDefault()?.UnitPrice;
        }
        if (isSuccess)
            return Json(new { Success = isSuccess, data = miniShoppingCartResponseModel });
        else
            return Json(new { Success = isSuccess, message = errorOrWarningMsg });
    }

    [HttpPost]
    public async Task<IActionResult> GetFlyoutCartQuantityChangeAsync(MiniShoppingCartRequestModel miniShoppingCartRequestModel)
    {
        if (miniShoppingCartRequestModel.ShoppingCartItemNewQuantity == 0)
        {
            var msg = await _localizationService.GetResourceAsync("ShoppingCart.QuantityShouldPositive");
            return Json(new { Success = false, message = msg });
        }
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        var products = (await _productService.GetProductsByIdsAsync(cart.Select(item => item.ProductId).Distinct().ToArray()))
            .ToDictionary(item => item.Id, item => item);

        var itemsWithNewQuantity = cart.Select(item => new
        {
            //try to get a new quantity for the item, set 0 for items to remove
            NewQuantity = miniShoppingCartRequestModel.ShoppingCartItemNewQuantity,
            Item = item,
            Product = products.ContainsKey(item.ProductId) ? products[item.ProductId] : null
        }).Where(item => item.Item.Id == miniShoppingCartRequestModel.ShoppingCartItemId
                        && item.NewQuantity != item.Item.Quantity);

        var orderedCart = itemsWithNewQuantity
            .OrderByDescending(async cartItem =>
                (cartItem.NewQuantity < cartItem.Item.Quantity &&
                 (cartItem.Product?.RequireOtherProducts ?? false)) ||
                (cartItem.NewQuantity > cartItem.Item.Quantity && cartItem.Product != null && (await _shoppingCartService
                     .GetProductsRequiringProductAsync(cart, cartItem.Product)).Any()))
            .ToList();


        var warnings = await orderedCart.SelectAwait(async cartItem => new
        {
            ItemId = cartItem.Item.Id,
            Warnings = await _shoppingCartService.UpdateShoppingCartItemAsync(await _workContext.GetCurrentCustomerAsync(),
                cartItem.Item.Id, cartItem.Item.AttributesXml, cartItem.Item.CustomerEnteredPrice,
                cartItem.Item.RentalStartDateUtc, cartItem.Item.RentalEndDateUtc, cartItem.NewQuantity, true)
        }).ToListAsync();

        cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        var miniShoppingCartModel = await _shoppingCartModelFactory.PrepareMiniShoppingCartModelAsync();
        MiniShoppingCartResponseModel miniShoppingCartResponseModel = new MiniShoppingCartResponseModel();
        bool isSuccess = true;
        string errorOrWarningMsg = string.Empty;
        if (warnings.Any(x => x.Warnings.Count > 0))
        {
            isSuccess = false;
            var errorOrWarningMessage = warnings.SelectMany(x => x.Warnings);
            errorOrWarningMsg = string.Join(",", errorOrWarningMessage);
        }
        else
        {
            miniShoppingCartResponseModel.ShoppingCartItemId = miniShoppingCartRequestModel.ShoppingCartItemId;
            miniShoppingCartResponseModel.ShoppingCartItemUpdatedQuantity = miniShoppingCartRequestModel.ShoppingCartItemNewQuantity;
            miniShoppingCartResponseModel.TotalQuantity = miniShoppingCartModel.TotalProducts;
            miniShoppingCartResponseModel.Subtotal = miniShoppingCartModel.SubTotal;
            miniShoppingCartResponseModel.UnitPrice = miniShoppingCartModel?.Items?.Where(x => x.Id == miniShoppingCartRequestModel.ShoppingCartItemId)?.FirstOrDefault()?.UnitPrice;
        }
        if (isSuccess)
            return Json(new { Success = isSuccess, data = miniShoppingCartResponseModel });
        else
            return Json(new { Success = isSuccess, message = errorOrWarningMsg });

    }
    #endregion
}
