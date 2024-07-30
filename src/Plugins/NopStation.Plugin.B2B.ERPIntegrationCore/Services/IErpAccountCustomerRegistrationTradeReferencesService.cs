using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public interface IErpAccountCustomerRegistrationTradeReferencesService
    {
        Task InsertErpAccountCustomerRegistrationTradeReferencesAsync(ErpAccountCustomerRegistrationTradeReferences erpAccountCustomerRegistrationTradeReferences);

        Task UpdateErpAccountCustomerRegistrationTradeReferencesAsync(ErpAccountCustomerRegistrationTradeReferences erpAccountCustomerRegistrationTradeReferences);

        Task DeleteErpAccountCustomerRegistrationTradeReferencesByIdAsync(int id);

        Task<ErpAccountCustomerRegistrationTradeReferences> GetErpAccountCustomerRegistrationTradeReferencesByIdAsync(int id);

        Task<ErpAccountCustomerRegistrationTradeReferences> GetErpAccountCustomerRegistrationTradeReferencesByIdWithActiveAsync(int id);

        Task<IPagedList<ErpAccountCustomerRegistrationTradeReferences>> GetAllErpAccountCustomerRegistrationTradeReferencesAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool getOnlyTotalCount = false, bool? overridePublished = false, int formId = 0, string name = null, string telephone = null);

        Task<IList<ErpAccountCustomerRegistrationTradeReferences>> GetErpAccountCustomerRegistrationTradeReferencesByFormIdAsync(int formId);
    }
}

