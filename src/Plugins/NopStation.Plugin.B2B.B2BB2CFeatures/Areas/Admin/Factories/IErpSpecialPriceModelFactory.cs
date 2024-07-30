using System.Threading.Tasks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories
{
    public interface IErpSpecialPriceModelFactory
    {
        Task<ErpSpecialPriceSearchModel> PrepareErpProductPricingSearchModel(ErpSpecialPriceSearchModel searchModel, int productId);

        Task<ErpSpecialPriceListModel> PrepareErpProductPricingListModel(ErpSpecialPriceSearchModel searchModel);

        Task<ErpSpecialPriceModel> PrepareErpProductPricingModel(ErpSpecialPriceModel model, ErpSpecialPrice erpProductPricing);

        //byte[] ExportB2BPriceGroupProductPricingToXlsx(List<int> ids);

        //byte[] ExportB2BPriceGroupProductPricingToXlsxAll(ProductSearchModel searchModel);

        //void ImportB2BPriceGroupProductPricingFromXlsx(Stream stream);
    }
}