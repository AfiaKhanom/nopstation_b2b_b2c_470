using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public interface IErpAccountCustomerRegistrationBankingDetailsService
    {
        Task InsertErpAccountCustomerRegistrationBankingDetailsAsync(ErpAccountCustomerRegistrationBankingDetails erpAccountCustomerRegistrationBankingDetails);

        Task UpdateErpAccountCustomerRegistrationBankingDetailsAsync(ErpAccountCustomerRegistrationBankingDetails erpAccountCustomerRegistrationBankingDetails);

        Task DeleteErpAccountCustomerRegistrationBankingDetailsByIdAsync(int id);

        Task<ErpAccountCustomerRegistrationBankingDetails> GetErpAccountCustomerRegistrationBankingDetailsByIdAsync(int id);

        Task<ErpAccountCustomerRegistrationBankingDetails> GetErpAccountCustomerRegistrationBankingDetailsByIdWithActiveAsync(int id);

        Task<IPagedList<ErpAccountCustomerRegistrationBankingDetails>> GetAllErpAccountCustomerRegistrationBankingDetailsAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool getOnlyTotalCount = false, bool? overridePublished = false, int formId = 0, string nameOfBanker = null, string accountName = null, string accountNumber = null, string branchCode = null, string branch = null);

        Task<IList<ErpAccountCustomerRegistrationBankingDetails>> GetErpAccountCustomerRegistrationBankingDetailsByFormIdAsync(int formId);
    }
}

