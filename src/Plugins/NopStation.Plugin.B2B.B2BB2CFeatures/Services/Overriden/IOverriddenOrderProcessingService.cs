using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using Nop.Services.Payments;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.Overriden
{
    public interface IOverriddenOrderProcessingService
    {
        Task<PlaceOrderResult> PlaceQuoteOrderAsync(ProcessPaymentRequest processPaymentRequest);

        Task PlaceERPOrderAtNopAsync(Order order, ErpOrderType erpOrderType);

        Task<(bool, string)> RetryPlaceERPOrderAtERPAsync(ErpOrderAdditionalData orderAdditionalData, B2BB2CFeaturesSettings b2BB2CFeaturesSettings);

        //Task PlaceB2COrderAtNopAsync(Order order, ErpOrderType erpOrderType);

        //Task<(bool, string)> RetryPlaceB2COrderAtERPAsync(ErpOrderAdditionalData orderPerUser);
    }
}