using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;

public record ErpPriceGroupProductPricingSearchModel : BaseSearchModel
{
    public ErpPriceGroupProductPricingSearchModel()
    {
        AddErpPriceGroupProductPricing = new ErpPriceGroupProductPricingModel();
    }

    public int ProductId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPrice.Fields.SearchErpPriceGroupCode")]
    public string SearchErpPriceGroupCode { get; set; }

    public ErpPriceGroupProductPricingModel AddErpPriceGroupProductPricing { get; set; }
}
