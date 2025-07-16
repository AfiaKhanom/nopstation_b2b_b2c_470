using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using Nop.Services.Payments;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.Overriden;

public interface IOverriddenOrderProcessingService
{
    Task<PlaceOrderResult> PlaceQuoteOrderAsync(ProcessPaymentRequest processPaymentRequest);

    Task PlaceErpOrderAtNopAsync(Order order, ErpOrderType erpOrderType);

    Task<(bool, string)> RetryPlaceErpOrderAtErpAsync(ErpOrderAdditionalData erpOrderAdditionalData);
}