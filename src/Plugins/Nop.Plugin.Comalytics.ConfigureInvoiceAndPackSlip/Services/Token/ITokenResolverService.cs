using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Services.Models;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Services.Token
{
    /// <summary>
    /// Token resolver service interface for resolving order tokens to render models
    /// </summary>
    public interface ITokenResolverService
    {
        /// <summary>
        /// Resolves tokens for an invoice render model from an order
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Invoice render model</returns>
        Task<InvoiceRenderModel> ResolveInvoiceTokensAsync(Order order);

        /// <summary>
        /// Resolves tokens for a packing slip render model from a shipment
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <returns>Packing slip render model</returns>
        Task<PackingSlipRenderModel> ResolvePackingSlipTokensAsync(Shipment shipment);
    }
}
