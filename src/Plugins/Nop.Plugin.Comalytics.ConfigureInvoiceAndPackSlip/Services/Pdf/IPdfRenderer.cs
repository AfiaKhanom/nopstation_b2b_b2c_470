using System.Threading;
using System.Threading.Tasks;
using Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Services.Models;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Services.Pdf
{
    /// <summary>
    /// PDF renderer interface for generating PDFs from render models
    /// </summary>
    public interface IPdfRenderer
    {
        /// <summary>
        /// Renders an invoice to PDF
        /// </summary>
        /// <param name="model">Invoice render model</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>PDF as byte array</returns>
        Task<byte[]> RenderInvoiceToPdfAsync(InvoiceRenderModel model, CancellationToken cancellationToken = default);

        /// <summary>
        /// Renders a packing slip to PDF
        /// </summary>
        /// <param name="model">Packing slip render model</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>PDF as byte array</returns>
        Task<byte[]> RenderPackingSlipToPdfAsync(PackingSlipRenderModel model, CancellationToken cancellationToken = default);
    }
}
