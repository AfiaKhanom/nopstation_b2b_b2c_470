using System;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services.QuickOrderServices
{
    public interface IQuickOrderTemplateService
    {
        Task<IPagedList<QuickOrderTemplate>> GetAllQuickOrderTemplatesAsync(string name = null, int customerId = 0, DateTime? createdOnUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue);

        Task<QuickOrderTemplate> GetQuickOrderTemplateByIdAsync(int templateId);

        Task<QuickOrderTemplate> GetQuickOrderTemplateByIdWithoutTrackingAsync(int templateId);

        Task InsertQuickOrderTemplateAsync(QuickOrderTemplate quickOrderTemplate);

        Task UpdateQuickOrderTemplateAsync(QuickOrderTemplate quickOrderTemplate);

        Task DeleteQuickOrderTemplateAsync(QuickOrderTemplate quickOrderTemplate);

       /* Task<bool> UpdateTotalPriceOfItemsUsingSqlAsync(int templateId);*/
    }
}
