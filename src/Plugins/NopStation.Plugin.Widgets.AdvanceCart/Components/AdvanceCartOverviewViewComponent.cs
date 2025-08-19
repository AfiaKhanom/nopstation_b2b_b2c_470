using System.Threading.Tasks;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.AdvanceCart.Factories;

namespace NopStation.Plugin.Widgets.AdvanceCart.Components;

public class AdvanceCartOverviewViewComponent : NopStationViewComponent
{
    private readonly AdvanceCartSettings _advanceCartSettings;
    private readonly IAdvanceCartModelFactory _advanceCartModelFactory;
    private readonly IProductService _productService;
    private readonly ILocalizationService _localizationService;

    public AdvanceCartOverviewViewComponent(AdvanceCartSettings advanceCartSettings,
        IAdvanceCartModelFactory advanceCartModelFactory,
        IProductService productService, ILocalizationService localizationService)
    {
        _advanceCartSettings = advanceCartSettings;
        _advanceCartModelFactory = advanceCartModelFactory;
        _productService = productService;
        _localizationService = localizationService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        if (!_advanceCartSettings.EnableAdvanceCartPlugin)
            return Content("");

        if (!_advanceCartSettings.EnableBuyNowButton && !_advanceCartSettings.AllowCustomersToSelectQuantityFromProductBox)
            return Content("");

        if (additionalData.GetType() != typeof(ProductOverviewModel))
            return Content("");

        var productOverviewModel = (ProductOverviewModel)additionalData;
        var product = await _productService.GetProductByIdAsync(productOverviewModel.Id);
        var model = await _advanceCartModelFactory.PrepareProductOverviewQuantityModelAsync(product);

        if (!_advanceCartSettings.EnableBuyNowButton)
        {
            var addtocartlink = "";
            var shoppingCartTypeId = (int)ShoppingCartType.ShoppingCart;
            var quantity = 1;
            var addToCartText = await _localizationService.GetResourceAsync("ShoppingCart.AddToCart");
            if (productOverviewModel.ProductPrice.ForceRedirectionAfterAddingToCart)
            {
                addtocartlink = Url.RouteUrl("AddProductToCart-Catalog", new { productId = productOverviewModel.Id, shoppingCartTypeId = shoppingCartTypeId, quantity = quantity, forceredirection = productOverviewModel.ProductPrice.ForceRedirectionAfterAddingToCart });
            }
            else
            {
                addtocartlink = Url.RouteUrl("AddProductToCart-Catalog", new { productId = productOverviewModel.Id, shoppingCartTypeId = shoppingCartTypeId, quantity = quantity });
            }

            if (!productOverviewModel.ProductPrice.DisableBuyButton)
            {
                if (productOverviewModel.ProductPrice.IsRental)
                {
                    addToCartText = await _localizationService.GetResourceAsync("ShoppingCart.Rent");
                }
                if (productOverviewModel.ProductPrice.AvailableForPreOrder)
                {
                    addToCartText = await _localizationService.GetResourceAsync("ShoppingCart.PreOrder");
                }
            }

            ViewBag.addtocartlink = addtocartlink;
            ViewBag.addToCartText = addToCartText;
        }


        return View(model);
    }
}
