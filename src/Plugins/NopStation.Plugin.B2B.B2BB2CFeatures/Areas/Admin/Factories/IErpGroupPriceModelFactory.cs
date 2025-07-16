using System.Threading.Tasks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public interface IErpGroupPriceModelFactory
{
    Task<ErpPriceGroupProductPricingSearchModel> PrepareErpProductPricingSearchModel(ErpPriceGroupProductPricingSearchModel searchModel, int productId);

    Task<ErpPriceGroupProductPricingListModel> PrepareErpProductPricingListModel(ErpPriceGroupProductPricingSearchModel searchModel);

    Task<ErpPriceGroupProductPricingModel> PrepareErpProductPricingModel(ErpPriceGroupProductPricingModel model, ErpGroupPrice erpProductPricing);
}