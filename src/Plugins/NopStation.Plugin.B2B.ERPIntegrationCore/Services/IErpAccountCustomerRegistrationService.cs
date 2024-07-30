using System;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public interface IErpAccountCustomerRegistrationFormService
    {
        Task InsertErpAccountCustomerRegistrationFormAsync(ErpAccountCustomerRegistrationForm erpAccountCustomerRegistrationForm);

        Task UpdateErpAccountCustomerRegistrationFormAsync(ErpAccountCustomerRegistrationForm erpAccountCustomerRegistrationForm);

        Task DeleteErpAccountCustomerRegistrationFormByIdAsync(int id);

        Task<ErpAccountCustomerRegistrationForm> GetErpAccountCustomerRegistrationFormByIdAsync(int id);

        Task<IPagedList<ErpAccountCustomerRegistrationForm>> GetAllErpAccountCustomerRegistrationFormAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool? showHidden = false, bool? showApproved = false, bool getOnlyTotalCount = false, bool? overridePublished = false, int formId = 0, string fullRegisteredName = null, string registrationNumber = null, string accountsEmail = null, DateTime? fromDate = null, DateTime? toDate = null);
    }
}

