using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Web.Factories;
using Nop.Web.Models.ShoppingCart;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Widgets.AdvanceCart.Components;

public class AdvanceCartFlyoutShoppingCartViewComponent : NopStationViewComponent
{
    private readonly AdvanceCartSettings _advanceCartSettings;
    private readonly TaxSettings _taxSettings;
    private readonly ICurrencyService _currencyService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IWorkContext _workContext;
    private readonly IOrderTotalCalculationService _orderTotalCalculationService;
    private readonly IStoreContext _storeContext;

    public AdvanceCartFlyoutShoppingCartViewComponent(
        AdvanceCartSettings advanceCartSettings,
        TaxSettings taxSettings,
        ICurrencyService currencyService,
        IPriceFormatter priceFormatter,
        IShoppingCartModelFactory shoppingCartModelFactory,
        IShoppingCartService shoppingCartService,
        IWorkContext workContext,
        IOrderTotalCalculationService orderTotalCalculationService,
        IStoreContext storeContext)
    {
        _advanceCartSettings = advanceCartSettings;
        _taxSettings = taxSettings;
        _currencyService = currencyService;
        _priceFormatter = priceFormatter;
        _shoppingCartModelFactory = shoppingCartModelFactory;
        _shoppingCartService = shoppingCartService;
        _workContext = workContext;
        _orderTotalCalculationService = orderTotalCalculationService;
        _storeContext = storeContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        if (!_advanceCartSettings.EnableAdvanceCartPlugin)
            return Content("");

        if (!_advanceCartSettings.EnableAdvanceFlyoutCart)
            return Content("");

        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, store.Id);

        var model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(new ShoppingCartModel(), cart);
        var subTotalIncludingTax = await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;

        var (_, _, _, subTotalWithoutDiscountBase, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, subTotalIncludingTax);
        var subtotalBase = subTotalWithoutDiscountBase;
        var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
        var subTotalValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(subtotalBase, currentCurrency);
        var subTotal = await _priceFormatter.FormatPriceAsync(subTotalValue, false, currentCurrency, (await _workContext.GetWorkingLanguageAsync()).Id, subTotalIncludingTax);

        model.CustomProperties.Add("SubTotal", subTotal);
        model.CustomProperties.Add("SubTotalValue", subTotalValue.ToString());


        return View(model);
    }
}
