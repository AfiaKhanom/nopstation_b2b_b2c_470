using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using Nop.Web.Framework.Components;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Components;

public class ErpSpecialPriceViewComponent : NopViewComponent
{
    private readonly IProductService _productService;

    public ErpSpecialPriceViewComponent(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        try
        {
            var productId = additionalData is int pId ? pId : 0;
            var searchModel = new ErpSpecialPriceSearchModel();
            
            if (productId > 0)
            {
                if (await _productService.GetProductByIdAsync(productId) is null)
                {
                    throw new ArgumentException("No product found with the specified id");
                }

                searchModel.ProductId = productId;
            }

            searchModel.SetGridPageSize();
            return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Areas/Admin/Views/ErpProductPricing/_SpecialPriceComponentView.cshtml", searchModel);
        }
        catch
        {
            return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Areas/Admin/Views/ErpProductPricing/_SpecialPriceComponentView.cshtml", new ErpSpecialPriceSearchModel());
        }
    }

}
