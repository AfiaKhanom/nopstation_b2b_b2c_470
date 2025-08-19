using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Components;
using Nop.Web.Factories;
using Nop.Web.Models.ShoppingCart;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.Widgets.AjaxCart.Controllers;

[AutoValidateAntiforgeryToken]
public class AjaxCartController : NopStationPublicController
{
    #region Fields

    private readonly IPermissionService _permissionService;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
    private readonly IDiscountService _discountService;
    private readonly ICustomerService _customerService;
    private readonly IOrderTotalCalculationService _orderTotalCalculationService;
    private readonly ICurrencyService _currencyService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly ILocalizationService _localizationService;
    private readonly IGiftCardService _giftCardService;
    private readonly IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeParser;
    private readonly IAttributeService<CheckoutAttribute, CheckoutAttributeValue> _checkoutAttributeService;
    private readonly IDownloadService _downloadService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IProductService _productService;
    private readonly TaxSettings _taxSettings;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IStoreMappingService _storeMappingService;

    #endregion

    #region Ctor

    public AjaxCartController(IPermissionService permissionService,
        IWorkContext workContext,
        IStoreContext storeContext,
        IShoppingCartService shoppingCartService,
        IShoppingCartModelFactory shoppingCartModelFactory,
        IDiscountService discountService,
        ICustomerService customerService,
        IOrderTotalCalculationService orderTotalCalculationService,
        ICurrencyService currencyService,
        IPriceFormatter priceFormatter,
        ILocalizationService localizationService,
        IGiftCardService giftCardService,
        IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser,
        IAttributeService<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeService,
        IDownloadService downloadService,
        IGenericAttributeService genericAttributeService,
        IProductService productService,
        TaxSettings taxSettings,
        IStaticCacheManager staticCacheManager,
        IStoreMappingService storeMappingService)
    {
        _permissionService = permissionService;
        _workContext = workContext;
        _storeContext = storeContext;
        _shoppingCartService = shoppingCartService;
        _shoppingCartModelFactory = shoppingCartModelFactory;
        _discountService = discountService;
        _customerService = customerService;
        _orderTotalCalculationService = orderTotalCalculationService;
        _currencyService = currencyService;
        _priceFormatter = priceFormatter;
        _localizationService = localizationService;
        _giftCardService = giftCardService;
        _checkoutAttributeParser = checkoutAttributeParser;
        _checkoutAttributeService = checkoutAttributeService;
        _downloadService = downloadService;
        _genericAttributeService = genericAttributeService;
        _productService = productService;
        _taxSettings = taxSettings;
        _staticCacheManager = staticCacheManager;
        _storeMappingService = storeMappingService;
    }

    #endregion

    #region Utilities

    protected virtual async Task ParseAndSaveCheckoutAttributesAsync(IList<ShoppingCartItem> cart, IFormCollection form)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        if (form == null)
            throw new ArgumentNullException(nameof(form));

        var attributesXml = string.Empty;
        var excludeShippableAttributes = !await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);
        var store = await _storeContext.GetCurrentStoreAsync();
        var checkoutAttributes = await _checkoutAttributeService.GetAllAttributesAsync(_staticCacheManager, _storeMappingService, store.Id, excludeShippableAttributes);
        foreach (var attribute in checkoutAttributes)
        {
            var controlId = $"checkout_attribute_{attribute.Id}";
            switch (attribute.AttributeControlType)
            {
                case AttributeControlType.DropdownList:
                case AttributeControlType.RadioList:
                case AttributeControlType.ColorSquares:
                case AttributeControlType.ImageSquares:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var selectedAttributeId = int.Parse(ctrlAttributes);
                            if (selectedAttributeId > 0)
                                attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }
                    }

                    break;
                case AttributeControlType.Checkboxes:
                    {
                        var cblAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(cblAttributes))
                        {
                            foreach (var item in cblAttributes.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                var selectedAttributeId = int.Parse(item);
                                if (selectedAttributeId > 0)
                                    attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                    }

                    break;
                case AttributeControlType.ReadonlyCheckboxes:
                    {
                        //load read-only (already server-side selected) values
                        var attributeValues = await _checkoutAttributeService.GetAttributeValuesAsync(attribute.Id);
                        foreach (var selectedAttributeId in attributeValues
                            .Where(v => v.IsPreSelected)
                            .Select(v => v.Id)
                            .ToList())
                        {
                            attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                        attribute, selectedAttributeId.ToString());
                        }
                    }

                    break;
                case AttributeControlType.TextBox:
                case AttributeControlType.MultilineTextbox:
                    {
                        var ctrlAttributes = form[controlId];
                        if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                        {
                            var enteredText = ctrlAttributes.ToString().Trim();
                            attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                attribute, enteredText);
                        }
                    }

                    break;
                case AttributeControlType.Datepicker:
                    {
                        var date = form[controlId + "_day"];
                        var month = form[controlId + "_month"];
                        var year = form[controlId + "_year"];
                        DateTime? selectedDate = null;
                        try
                        {
                            selectedDate = new DateTime(int.Parse(year), int.Parse(month), int.Parse(date));
                        }
                        catch
                        {
                            // ignored
                        }

                        if (selectedDate.HasValue)
                            attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                attribute, selectedDate.Value.ToString("D"));
                    }

                    break;
                case AttributeControlType.FileUpload:
                    {
                        _ = Guid.TryParse(form[controlId], out var downloadGuid);
                        var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
                        if (download != null)
                        {
                            attributesXml = _checkoutAttributeParser.AddAttribute(attributesXml,
                                       attribute, download.DownloadGuid.ToString());
                        }
                    }

                    break;
                default:
                    break;
            }
        }

        //validate conditional attributes (if specified)
        foreach (var attribute in checkoutAttributes)
        {
            var conditionMet = await _checkoutAttributeParser.IsConditionMetAsync(attribute.ConditionAttributeXml, attributesXml);
            if (conditionMet.HasValue && !conditionMet.Value)
                attributesXml = _checkoutAttributeParser.RemoveAttribute(attributesXml, attribute.Id);
        }

        //save checkout attributes
        await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.CheckoutAttributes, attributesXml, store.Id);
    }

    #endregion

    #region Methods

    [HttpPost]
    public async Task<IActionResult> UpdateCart(IFormCollection form)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart))
            return RedirectToRoute("Homepage");

        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

        //get identifiers of items to remove
        var itemIdsToRemove = form["removefromcart"]
            .SelectMany(value => value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            .Select(idString => int.TryParse(idString, out var id) ? id : 0)
            .Distinct().ToList();

        var products = (await _productService.GetProductsByIdsAsync(cart.Select(item => item.ProductId).Distinct().ToArray()))
            .ToDictionary(item => item.Id, item => item);

        //get order items with changed quantity
        var itemsWithNewQuantity = cart.Select(item => new
        {
            //try to get a new quantity for the item, set 0 for items to remove
            NewQuantity = itemIdsToRemove.Contains(item.Id) ? 0 : int.TryParse(form[$"itemquantity{item.Id}"], out var quantity) ? quantity : item.Quantity,
            Item = item,
            Product = products.ContainsKey(item.ProductId) ? products[item.ProductId] : null
        }).Where(item => item.NewQuantity != item.Item.Quantity);

        //order cart items
        //first should be items with a reduced quantity and that require other products; or items with an increased quantity and are required for other products
        var orderedCart = await itemsWithNewQuantity
            .OrderByDescendingAwait(async cartItem =>
                (cartItem.NewQuantity < cartItem.Item.Quantity &&
                 (cartItem.Product?.RequireOtherProducts ?? false)) ||
                (cartItem.NewQuantity > cartItem.Item.Quantity && cartItem.Product != null && (await _shoppingCartService
                     .GetProductsRequiringProductAsync(cart, cartItem.Product)).Any()))
            .ToListAsync();

        //try to update cart items with new quantities and get warnings
        var warnings = await orderedCart.SelectAwait(async cartItem => new
        {
            ItemId = cartItem.Item.Id,
            Warnings = await _shoppingCartService.UpdateShoppingCartItemAsync(customer,
                cartItem.Item.Id, cartItem.Item.AttributesXml, cartItem.Item.CustomerEnteredPrice,
                cartItem.Item.RentalStartDateUtc, cartItem.Item.RentalEndDateUtc, cartItem.NewQuantity, true)
        }).ToListAsync();

        //updated cart
        cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

        //parse and save checkout attributes
        await ParseAndSaveCheckoutAttributesAsync(cart, form);

        //prepare model
        var model = new ShoppingCartModel();
        model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);

        //update current warnings
        foreach (var warningItem in warnings.Where(warningItem => warningItem.Warnings.Any()))
        {
            //find shopping cart item model to display appropriate warnings
            var itemModel = model.Items.FirstOrDefault(item => item.Id == warningItem.ItemId);
            if (itemModel != null)
                itemModel.Warnings = warningItem.Warnings.Concat(itemModel.Warnings).Distinct().ToList();
        }

        var html = await RenderViewComponentToStringAsync(typeof(OrderSummaryViewComponent), new { overriddenModel = model });

        //var subTotalIncludingTax = await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;

        //var (_, _, _, subTotalWithoutDiscountBase, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, subTotalIncludingTax);
        //var subtotalBase = subTotalWithoutDiscountBase;
        //var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
        //var subTotalValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(subtotalBase, currentCurrency);
        //var subTotal = await _priceFormatter.FormatPriceAsync(subTotalValue, false, currentCurrency, (await _workContext.GetWorkingLanguageAsync()).Id, subTotalIncludingTax);

        var quantity = cart.Sum(x => x.Quantity);

        return Json(new { html });
    }

    [HttpPost]
    public virtual async Task<IActionResult> ApplyDiscountCoupon(string discountcouponcode)
    {
        //trim
        if (discountcouponcode != null)
            discountcouponcode = discountcouponcode.Trim();

        //cart
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

        var model = new ShoppingCartModel();
        if (!string.IsNullOrWhiteSpace(discountcouponcode))
        {
            //we find even hidden records here. this way we can display a user-friendly message if it's expired
            var discounts = (await _discountService.GetAllDiscountsAsync(couponCode: discountcouponcode, showHidden: true))
                .Where(d => d.RequiresCouponCode)
                .ToList();
            if (discounts.Any())
            {
                var userErrors = new List<string>();
                var anyValidDiscount = await discounts.AnyAwaitAsync(async discount =>
                {
                    var validationResult = await _discountService.ValidateDiscountAsync(discount, customer, new[] { discountcouponcode });
                    userErrors.AddRange(validationResult.Errors);

                    return validationResult.IsValid;
                });

                if (anyValidDiscount)
                {
                    //valid
                    await _customerService.ApplyDiscountCouponCodeAsync(customer, discountcouponcode);
                    model.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.Applied"));
                    model.DiscountBox.IsApplied = true;
                }
                else
                {
                    if (userErrors.Any())
                        //some user errors
                        model.DiscountBox.Messages = userErrors;
                    else
                        //general error text
                        model.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.WrongDiscount"));
                }
            }
            else
                //discount cannot be found
                model.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.CannotBeFound"));
        }
        else
            //empty coupon code
            model.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.Empty"));

        model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);

        var html = await RenderViewComponentToStringAsync(typeof(OrderSummaryViewComponent), new { overriddenModel = model });

        return Json(new { html = html });
    }

    [HttpPost]
    public virtual async Task<IActionResult> ApplyGiftCard(string giftcardcouponcode)
    {
        //trim
        if (giftcardcouponcode != null)
            giftcardcouponcode = giftcardcouponcode.Trim();

        //cart
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

        var model = new ShoppingCartModel();
        if (!await _shoppingCartService.ShoppingCartIsRecurringAsync(cart))
        {
            if (!string.IsNullOrWhiteSpace(giftcardcouponcode))
            {
                var giftCard = (await _giftCardService.GetAllGiftCardsAsync(giftCardCouponCode: giftcardcouponcode)).FirstOrDefault();
                var isGiftCardValid = giftCard != null && await _giftCardService.IsGiftCardValidAsync(giftCard);
                if (isGiftCardValid)
                {
                    await _customerService.ApplyGiftCardCouponCodeAsync(customer, giftcardcouponcode);
                    model.GiftCardBox.Message = await _localizationService.GetResourceAsync("ShoppingCart.GiftCardCouponCode.Applied");
                    model.GiftCardBox.IsApplied = true;
                }
                else
                {
                    model.GiftCardBox.Message = await _localizationService.GetResourceAsync("ShoppingCart.GiftCardCouponCode.WrongGiftCard");
                    model.GiftCardBox.IsApplied = false;
                }
            }
            else
            {
                model.GiftCardBox.Message = await _localizationService.GetResourceAsync("ShoppingCart.GiftCardCouponCode.WrongGiftCard");
                model.GiftCardBox.IsApplied = false;
            }
        }
        else
        {
            model.GiftCardBox.Message = await _localizationService.GetResourceAsync("ShoppingCart.GiftCardCouponCode.DontWorkWithAutoshipProducts");
            model.GiftCardBox.IsApplied = false;
        }

        model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);

        var html = await RenderViewComponentToStringAsync(typeof(OrderSummaryViewComponent), new { overriddenModel = model });

        return Json(new { html = html });
    }

    [HttpPost]
    public virtual async Task<IActionResult> UpdateWishlist(IFormCollection form)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist))
            return RedirectToRoute("Homepage");

        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.Wishlist, store.Id);

        var allIdsToRemove = form.ContainsKey("removefromcart")
            ? form["removefromcart"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToList()
            : new List<int>();

        //current warnings <cart item identifier, warnings>
        var innerWarnings = new Dictionary<int, IList<string>>();
        foreach (var sci in cart)
        {
            var remove = allIdsToRemove.Contains(sci.Id);
            if (remove)
                await _shoppingCartService.DeleteShoppingCartItemAsync(sci);
            else
            {
                foreach (var formKey in form.Keys)
                    if (formKey.Equals($"itemquantity{sci.Id}", StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (int.TryParse(form[formKey], out var newQuantity))
                        {
                            var currSciWarnings = await _shoppingCartService.UpdateShoppingCartItemAsync(customer,
                                sci.Id, sci.AttributesXml, sci.CustomerEnteredPrice,
                                sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                                newQuantity, true);
                            innerWarnings.Add(sci.Id, currSciWarnings);
                        }

                        break;
                    }
            }
        }

        //updated wishlist
        cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.Wishlist, store.Id);
        var model = new WishlistModel();
        model = await _shoppingCartModelFactory.PrepareWishlistModelAsync(model, cart);
        //update current warnings
        foreach (var kvp in innerWarnings)
        {
            //kvp = <cart item identifier, warnings>
            var sciId = kvp.Key;
            var warnings = kvp.Value;
            //find model
            var sciModel = model.Items.FirstOrDefault(x => x.Id == sciId);
            if (sciModel != null)
                foreach (var w in warnings)
                    if (!sciModel.Warnings.Contains(w))
                        sciModel.Warnings.Add(w);
        }

        var html = await RenderPartialViewToStringAsync("Wishlist", model);

        var updatetopwishlistsectionhtml = string.Format(await _localizationService.GetResourceAsync("Wishlist.HeaderQuantity"),
         cart.Sum(item => item.Quantity));

        return Json(new { updatetopwishlistsectionhtml, html, warnings = innerWarnings });
    }

    [HttpPost]
    public virtual async Task<IActionResult> RemoveDiscountCoupon(string key)
    {
        var model = new ShoppingCartModel();

        //get discount identifier
        var discountId = 0;
        if (!string.IsNullOrWhiteSpace(key) && key.StartsWith("removediscount-", StringComparison.InvariantCultureIgnoreCase))
            discountId = Convert.ToInt32(key["removediscount-".Length..]);
        var discount = await _discountService.GetDiscountByIdAsync(discountId);
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (discount != null)
            await _customerService.RemoveDiscountCouponCodeAsync(customer, discount.CouponCode);

        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

        model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);
        var html = await RenderViewComponentToStringAsync(typeof(OrderSummaryViewComponent), new { overriddenModel = model });

        return Json(new { html = html });
    }

    [HttpPost]
    public virtual async Task<IActionResult> RemoveGiftCardCode(string key)
    {
        var model = new ShoppingCartModel();

        //get gift card identifier
        var giftCardId = 0;
        if (!string.IsNullOrWhiteSpace(key) && key.StartsWith("removegiftcard-", StringComparison.InvariantCultureIgnoreCase))
            giftCardId = Convert.ToInt32(key["removegiftcard-".Length..]);
        var gc = await _giftCardService.GetGiftCardByIdAsync(giftCardId);
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (gc != null)
            await _customerService.RemoveGiftCardCouponCodeAsync(customer, gc.GiftCardCouponCode);

        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

        model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);
        var html = await RenderViewComponentToStringAsync(typeof(OrderSummaryViewComponent), new { overriddenModel = model });

        return Json(new { html = html });
    }

    #endregion
}
