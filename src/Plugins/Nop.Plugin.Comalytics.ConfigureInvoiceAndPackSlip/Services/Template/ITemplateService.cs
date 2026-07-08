using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Domain;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Services.Template
{
    /// <summary>
    /// Template service interface
    /// </summary>
    public interface ITemplateService
    {
        /// <summary>
        /// Gets an invoice template by identifier
        /// </summary>
        /// <param name="id">Template identifier</param>
        /// <returns>Invoice template</returns>
        Task<InvoiceTemplate> GetInvoiceTemplateByIdAsync(int id);

        /// <summary>
        /// Gets all invoice templates
        /// </summary>
        /// <param name="storeId">Store identifier (0 for all)</param>
        /// <param name="languageId">Language identifier (0 for all)</param>
        /// <returns>Invoice templates</returns>
        Task<IList<InvoiceTemplate>> GetAllInvoiceTemplatesAsync(int storeId = 0, int languageId = 0);

        /// <summary>
        /// Inserts an invoice template
        /// </summary>
        /// <param name="template">Invoice template</param>
        Task InsertInvoiceTemplateAsync(InvoiceTemplate template);

        /// <summary>
        /// Updates an invoice template
        /// </summary>
        /// <param name="template">Invoice template</param>
        Task UpdateInvoiceTemplateAsync(InvoiceTemplate template);

        /// <summary>
        /// Deletes an invoice template
        /// </summary>
        /// <param name="template">Invoice template</param>
        Task DeleteInvoiceTemplateAsync(InvoiceTemplate template);

        /// <summary>
        /// Gets a packing slip template by identifier
        /// </summary>
        /// <param name="id">Template identifier</param>
        /// <returns>Packing slip template</returns>
        Task<PackingSlipTemplate> GetPackingSlipTemplateByIdAsync(int id);

        /// <summary>
        /// Gets all packing slip templates
        /// </summary>
        /// <param name="storeId">Store identifier (0 for all)</param>
        /// <param name="languageId">Language identifier (0 for all)</param>
        /// <returns>Packing slip templates</returns>
        Task<IList<PackingSlipTemplate>> GetAllPackingSlipTemplatesAsync(int storeId = 0, int languageId = 0);

        /// <summary>
        /// Inserts a packing slip template
        /// </summary>
        /// <param name="template">Packing slip template</param>
        Task InsertPackingSlipTemplateAsync(PackingSlipTemplate template);

        /// <summary>
        /// Updates a packing slip template
        /// </summary>
        /// <param name="template">Packing slip template</param>
        Task UpdatePackingSlipTemplateAsync(PackingSlipTemplate template);

        /// <summary>
        /// Deletes a packing slip template
        /// </summary>
        /// <param name="template">Packing slip template</param>
        Task DeletePackingSlipTemplateAsync(PackingSlipTemplate template);
    }
}
