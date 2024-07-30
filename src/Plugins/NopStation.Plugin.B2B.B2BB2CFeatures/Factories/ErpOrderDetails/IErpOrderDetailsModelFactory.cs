using System.Threading.Tasks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.OrderSummary;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories.ErpOrderDetails
{
    public interface IErpOrderDetailsModelFactory
    {
        Task<ErpOrderDetailsModel> PrepareErpOrderDetailsModelFactoryAsync(ErpOrderAdditionalData erpOrderPerAccount);
    }
}