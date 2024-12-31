using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using Nop.Web.Framework.Components;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Components;

public class ErpPriceGroupViewComponent : NopViewComponent
{
    #region Fields

    private readonly IProductService _productService;
    private readonly IErpPriceGroupProductPricingModelFactory _erpPriceGroupProductPricingModelFactory;

    #endregion

    #region Ctor

    public ErpPriceGroupViewComponent(IProductService productService,
        IErpPriceGroupProductPricingModelFactory erpPriceGroupProductPricingModelFactory)
    {
        _productService = productService;
        _erpPriceGroupProductPricingModelFactory = erpPriceGroupProductPricingModelFactory;
    }

    #endregion

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        try
        {
            var productId = additionalData is int pId ? pId : 0;
            var searchModel = await _erpPriceGroupProductPricingModelFactory.PrepareErpProductPricingSearchModel(new ErpPriceGroupProductPricingSearchModel(), productId);

            if (productId > 0)
            {
                if (await _productService.GetProductByIdAsync(productId) is null)
                {
                    throw new ArgumentException("No product found with the specified id");
                }

                searchModel.ProductId = productId;
            }
            return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Areas/Admin/Views/ErpProductPricing/_PriceGroupComponentView.cshtml", searchModel);
        }
        catch
        {
            return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Areas/Admin/Views/ErpProductPricing/_PriceGroupComponentView.cshtml", new ErpPriceGroupProductPricingSearchModel());
        }
    }

    #endregion
}
