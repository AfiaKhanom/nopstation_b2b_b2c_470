using System;
using System.Threading.Tasks;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public interface IErpProductService
    {
        Task UnpublishAllOldProduct(DateTime syncStartTime);
    }
}