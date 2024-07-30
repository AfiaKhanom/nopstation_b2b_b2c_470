using System.Threading.Tasks;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpPriceSyncFunctionality
{
    public interface IErpPriceSyncFunctionalityService
    {
        Task<bool> IsB2BPriceSyncRequiredAsync();

        Task<bool> IsCartProductB2BPriceSyncRequiredAsync();

        void ExecuteAllProductsLivePriceSync();
    }
}