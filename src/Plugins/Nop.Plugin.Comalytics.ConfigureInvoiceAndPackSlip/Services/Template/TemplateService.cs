using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using Nop.Data;
using Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Domain;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Services.Template
{
    /// <summary>
    /// Template service implementation
    /// </summary>
    public class TemplateService : ITemplateService
    {
        private readonly IRepository<InvoiceTemplate> _invoiceTemplateRepository;
        private readonly IRepository<PackingSlipTemplate> _packingSlipTemplateRepository;

        public TemplateService(
            IRepository<InvoiceTemplate> invoiceTemplateRepository,
            IRepository<PackingSlipTemplate> packingSlipTemplateRepository)
        {
            _invoiceTemplateRepository = invoiceTemplateRepository;
            _packingSlipTemplateRepository = packingSlipTemplateRepository;
        }

        public virtual async Task<InvoiceTemplate> GetInvoiceTemplateByIdAsync(int id)
        {
            return await _invoiceTemplateRepository.GetByIdAsync(id);
        }

        public virtual async Task<IList<InvoiceTemplate>> GetAllInvoiceTemplatesAsync(int storeId = 0, int languageId = 0)
        {
            var query = _invoiceTemplateRepository.Table;

            if (storeId > 0)
                query = query.Where(t => t.StoreId == storeId || t.StoreId == 0);

            if (languageId > 0)
                query = query.Where(t => t.LanguageId == languageId || t.LanguageId == 0);

            query = query.OrderBy(t => t.StoreId).ThenBy(t => t.LanguageId).ThenBy(t => t.Name);

            return await query.ToListAsync();
        }

        public virtual async Task InsertInvoiceTemplateAsync(InvoiceTemplate template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            template.CreatedOnUtc = DateTime.UtcNow;
            template.UpdatedOnUtc = DateTime.UtcNow;

            await _invoiceTemplateRepository.InsertAsync(template);
        }

        public virtual async Task UpdateInvoiceTemplateAsync(InvoiceTemplate template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            template.UpdatedOnUtc = DateTime.UtcNow;

            await _invoiceTemplateRepository.UpdateAsync(template);
        }

        public virtual async Task DeleteInvoiceTemplateAsync(InvoiceTemplate template)
        {
            await _invoiceTemplateRepository.DeleteAsync(template);
        }

        public virtual async Task<PackingSlipTemplate> GetPackingSlipTemplateByIdAsync(int id)
        {
            return await _packingSlipTemplateRepository.GetByIdAsync(id);
        }

        public virtual async Task<IList<PackingSlipTemplate>> GetAllPackingSlipTemplatesAsync(int storeId = 0, int languageId = 0)
        {
            var query = _packingSlipTemplateRepository.Table;

            if (storeId > 0)
                query = query.Where(t => t.StoreId == storeId || t.StoreId == 0);

            if (languageId > 0)
                query = query.Where(t => t.LanguageId == languageId || t.LanguageId == 0);

            query = query.OrderBy(t => t.StoreId).ThenBy(t => t.LanguageId).ThenBy(t => t.Name);

            return await query.ToListAsync();
        }

        public virtual async Task InsertPackingSlipTemplateAsync(PackingSlipTemplate template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            template.CreatedOnUtc = DateTime.UtcNow;
            template.UpdatedOnUtc = DateTime.UtcNow;

            await _packingSlipTemplateRepository.InsertAsync(template);
        }

        public virtual async Task UpdatePackingSlipTemplateAsync(PackingSlipTemplate template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));

            template.UpdatedOnUtc = DateTime.UtcNow;

            await _packingSlipTemplateRepository.UpdateAsync(template);
        }

        public virtual async Task DeletePackingSlipTemplateAsync(PackingSlipTemplate template)
        {
            await _packingSlipTemplateRepository.DeleteAsync(template);
        }
    }
}
