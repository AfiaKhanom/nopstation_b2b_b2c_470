using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Debtor;
using NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Invoice;
using NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Stock;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Service
{
    public interface IErpNopMapperService
    {
        Task<IList<ErpAccountDataModel>> ErpAccountMapNop(IList<DebtorsMasterModel> debtorsMasters);
        Task<IList<ErpInvoiceDataModel>> ErpInvoiceMapNop(IList<ProcessingDocumentModel> erpInvoicesResponse);
        Task<IList<ErpProductDataModel>> ErpProductMapNop(IList<ErpStockRecordModel> erpStockResponses);
        Task<IList<ErpStockDataModel>> ErpStockMapNop(IList<ErpStockRecordModel> erpStockResponses);
        Task<IList<ErpShipToAddressDataModel>> ErpShipToAddressMapNop(IList<DebtorsMasterModel> erpShipToAddressResponse);
        Task<IList<ErpPriceGroupPricingDataModel>> ErpGroupPriceMapNop(IList<ErpStockRecordModel> erpGroupPriceResponses);
        Task<IList<ErpPriceSpecialPricingDataModel>> ErpSpecialPriceMapNop(IList<ErpStockRecordModel> erpSpecialPriceResponses);
        Task<IList<ErpPlaceOrderDataModel>> ErpOrderMapNop(IList<ProcessingDocumentModel> erpOrdersResponse);
    }
}