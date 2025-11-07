using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.ErpInterface;

public interface IErpIntegrationSalesOrgService
{
    Task<ErpResponseData< Dictionary<string,string>>> GetSalesOrgsFromErpAsync(ErpGetRequestModel erpRequest);

    Task<ErpResponseData<Dictionary<string,string>>> GetSalesWarehouseFromErpAsync(ErpGetRequestModel erpRequest);
}
