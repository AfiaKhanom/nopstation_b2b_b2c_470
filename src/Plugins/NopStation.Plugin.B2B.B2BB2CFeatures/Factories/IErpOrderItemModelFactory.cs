using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Checkout;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.OrderSummary;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories
{
    public interface IErpOrderItemModelFactory
    {
        Task<ErpOrderDetailsModel> PrepareB2BOrderItemDataModelListModelAsync(int nopOrderId, List<string> itemIds);

        Task<ErpCheckoutCompletedModel> PrepareB2BCheckoutCompletedModelAsync(int nopOrderId);
    }
}