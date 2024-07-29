using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.ErpInterface
{
    public interface IErpIntegrationProductService
    {
        Task<ErpResponseData<ErpProductDataModel>> GetProductByItemNoFromErpAsync(ErpGetRequestModel erpRequest);

        Task<ErpResponseData<IList<ErpProductDataModel>>> GetProductsFromErpAsync(ErpGetRequestModel erpRequest);

        Task<ErpResponseData<ErpProductDataModel>> GetStockByItemNoFromErpAsync(ErpGetRequestModel erpRequest);

        Task<ErpResponseData<IList<ErpProductDataModel>>> GetStocksFromErpAsync(ErpGetRequestModel erpRequest);

        Task<ErpResponseData<ErpPriceGroupPricingDataModel>> GetProductGroupPriceFromErpAsync(ErpGetRequestModel erpRequest);

        Task<ErpResponseData<IList<ErpPriceGroupPricingDataModel>>> GetProductGroupPricesFromErpAsync(ErpGetRequestModel erpRequest);

        Task<ErpResponseData<ErpPriceSpecialPricingDataModel>> GetProductSpecialPriceFromErpAsync(ErpGetRequestModel erpRequest);

        Task<ErpResponseData<IList<ErpPriceSpecialPricingDataModel>>> GetProductSpecialPricesFromErpAsync(ErpGetRequestModel erpRequest);
    }
}
