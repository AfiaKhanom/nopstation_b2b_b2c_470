using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;

public class ErpProductPricingModel
{
    public ErpProductPricingModel()
    {
        ErpPriceGroupProductPricingSearchModel = new ErpPriceGroupProductPricingSearchModel();
        ErpSpecialPriceSearchModel = new ErpSpecialPriceSearchModel();
    }

    public int ProductId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPrice.Fields.ProductName")]
    public string ProductName { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPrice.Fields.ProductSku")]
    public string ProductSku { get; set; }

    public decimal ProductPrice { get; set; }

    public ErpSpecialPriceSearchModel ErpSpecialPriceSearchModel { get; set; }
    public ErpPriceGroupProductPricingSearchModel ErpPriceGroupProductPricingSearchModel { get; set; }
}
