using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.D365BCIntegration.Models;

namespace NopStation.Plugin.B2B.D365BCIntegration.Services;

public interface IErpNopMapperService
{
    /// <summary>
    /// Maps D365 customer data to ERP account data model
    /// </summary>
    Task<IList<ErpAccountDataModel>> ErpAccountMapNop(IList<D365Customer> customers);

    /// <summary>
    /// Maps D365 customer shipping addresses to ERP shipping address data model
    /// </summary>
    Task<IList<ErpShipToAddressDataModel>> ErpShipToAddressMapNop(IList<D365Customer> erpShipToAddressResponse);

    /// <summary>
    /// Maps D365 invoice data to ERP invoice data model
    /// </summary>
    Task<IList<ErpInvoiceDataModel>> ErpInvoiceMapNop(IList<object> erpInvoicesResponse);

    /// <summary>
    /// Maps D365 order data to ERP order data model
    /// </summary>
    Task<IList<ErpPlaceOrderDataModel>> ErpOrderMapNop(IList<object> erpOrdersResponse);

    /// <summary>
    /// Maps D365 product data to ERP product data model
    /// </summary>
    Task<IList<ErpProductDataModel>> ErpProductMapNop(IList<object> erpStockResponses);

    /// <summary>
    /// Maps D365 stock data to ERP stock data model
    /// </summary>
    Task<IList<ErpStockDataModel>> ErpStockMapNop(IList<object> erpStockResponses);

    /// <summary>
    /// Maps D365 special pricing data to ERP special pricing data model
    /// </summary>
    Task<IList<ErpPriceSpecialPricingDataModel>> ErpSpecialPriceMapNop(IList<object> erpSpecialPriceResponses);

    /// <summary>
    /// Maps D365 group pricing data to ERP group pricing data model
    /// </summary>
    Task<IList<ErpPriceGroupPricingDataModel>> ErpGroupPriceMapNop(IList<object> erpGroupPriceResponses);
}