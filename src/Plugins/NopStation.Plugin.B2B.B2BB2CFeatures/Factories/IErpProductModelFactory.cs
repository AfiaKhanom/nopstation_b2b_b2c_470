using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.ErpProductList;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.OrderSummary;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories;

public interface IErpProductModelFactory
{
    Task<ErpProductListSearchModel> PrepareErpProductListSearchModelAsync(ErpProductListSearchModel searchModel);

    Task<ErpProductListModel> PrepareErpProductListModelAsync(ErpProductListSearchModel searchModel);

    Task<IList<ErpProductDataModel>> PrepareErpProductDataListModelAsync(List<string> productIds, ErpAccount erpAccount, B2BB2CFeaturesSettings b2BB2CFeaturesSettings);

    Task<ProductInCartQuantityModel> PrepareProductInCartQuantityModelAsync(int productId);

    Task<IList<ProductInCartQuantityModel>> PrepareProductInCartQuantityModelAsync(List<string> productIds);

    Task<string> UpdateLiveStockAndGetProductAvailabilityAsync(int productId, ErpAccount erpAccount);

    Task<ErpOrderSummaryModel> PrepareErpOrderSummaryModelAsync();

    Task<decimal> GetErpProductPriceAsync(int productId);

    Task<IList<(string, int)>> GetOrderItemsUOMData(int orderId);
}