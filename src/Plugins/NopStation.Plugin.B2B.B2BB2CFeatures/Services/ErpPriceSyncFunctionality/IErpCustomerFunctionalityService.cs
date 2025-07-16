using System.Threading.Tasks;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpPriceSyncFunctionality
{
    public interface IErpPriceSyncFunctionalityService
    {
        Task<bool> IsB2BPriceSyncRequiredAsync();

        Task<bool> IsCartProductB2BPriceSyncRequiredAsync();

        Task ExecuteAllProductsLivePriceSync();
    }
}