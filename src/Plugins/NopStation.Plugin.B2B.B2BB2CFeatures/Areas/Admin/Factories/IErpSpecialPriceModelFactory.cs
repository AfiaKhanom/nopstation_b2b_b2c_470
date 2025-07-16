using System.Threading.Tasks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public interface IErpSpecialPriceModelFactory
{
    Task<ErpSpecialPriceSearchModel> PrepareErpProductSpecialPriceSearchModel(ErpSpecialPriceSearchModel searchModel, int productId);

    Task<ErpSpecialPriceListModel> PrepareErpProductSpecialPriceListModel(ErpSpecialPriceSearchModel searchModel);

    Task<ErpSpecialPriceModel> PrepareErpProductSpecialPriceModel(ErpSpecialPriceModel model, ErpSpecialPrice erpProductPricing);
}