using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.ErpInterface;

public interface IErpIntegrationAccountService
{
    Task<ErpResponseModel> CreateAccountNoErpAsync(ErpCreateAccountModel erpCreateAccountModel);

    Task<ErpResponseData<ErpAccountDataModel>> GetAccountFromErpAsync(ErpGetRequestModel erpRequest);

    Task<ErpResponseData<IList<ErpAccountDataModel>>> GetAccountsFromErpAsync(ErpGetRequestModel erpRequest);

    Task<ErpResponseData<IList<ErpInvoiceDataModel>>> GetInvoiceByAccountNoFromErpAsync(ErpGetRequestModel erpRequest);

    Task<ErpResponseData<string>> GetInvoicePdfByteCodeByDocumentNoFromErpAsync(ErpGetRequestModel erpRequest);

    Task<ErpResponseData<string>> GetStatementPdfByteCodeFromErpAsync(ErpGetRequestModel erpRequest);

    Task<ErpResponseData<IList<ErpShipToAddressDataModel>>> GetShipToAddressesFromErpAsync(ErpGetRequestModel erpRequest);

    Task<ErpResponseData<IList<ErpAccountDataModel>>> GetAllAccountCreditFromErpAsync(ErpGetRequestModel erpRequest);
}
