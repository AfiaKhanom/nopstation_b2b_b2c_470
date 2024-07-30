using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public class ErpAccountCustomerRegistrationBankingDetailsService : IErpAccountCustomerRegistrationBankingDetailsService
    {
        #region Fields

        private readonly IRepository<ErpAccountCustomerRegistrationBankingDetails> _erpAccountCustomerRegistrationBankingDetailsRepository;

        #endregion

        #region ctor

        public ErpAccountCustomerRegistrationBankingDetailsService(IRepository<ErpAccountCustomerRegistrationBankingDetails> erpAccountCustomerRegistrationBankingDetailsRepository)
        {
            _erpAccountCustomerRegistrationBankingDetailsRepository = erpAccountCustomerRegistrationBankingDetailsRepository;
        }

        #endregion

        #region Methods

        #region Insert/Update

        public async Task InsertErpAccountCustomerRegistrationBankingDetailsAsync(ErpAccountCustomerRegistrationBankingDetails erpAccountCustomerRegistrationBankingDetails)
        {
            await _erpAccountCustomerRegistrationBankingDetailsRepository.InsertAsync(erpAccountCustomerRegistrationBankingDetails);
        }

        public async Task UpdateErpAccountCustomerRegistrationBankingDetailsAsync(ErpAccountCustomerRegistrationBankingDetails erpAccountCustomerRegistrationBankingDetails)
        {
            await _erpAccountCustomerRegistrationBankingDetailsRepository.UpdateAsync(erpAccountCustomerRegistrationBankingDetails);
        }

        #endregion

        #region Delete

        private async Task DeleteErpAccountCustomerRegistrationBankingDetailsAsync(ErpAccountCustomerRegistrationBankingDetails erpAccountCustomerRegistrationBankingDetails)
        {
            //as ErpBaseEntity dosen't inherit ISoftDelete but has that feature
            erpAccountCustomerRegistrationBankingDetails.IsDeleted = true;
            await _erpAccountCustomerRegistrationBankingDetailsRepository.UpdateAsync(erpAccountCustomerRegistrationBankingDetails);
        }

        public async Task DeleteErpAccountCustomerRegistrationBankingDetailsByIdAsync(int id)
        {
            var erpAccountCustomerRegistrationBankingDetails = await GetErpAccountCustomerRegistrationBankingDetailsByIdAsync(id);
            if (erpAccountCustomerRegistrationBankingDetails != null)
            {
                await DeleteErpAccountCustomerRegistrationBankingDetailsAsync(erpAccountCustomerRegistrationBankingDetails);
            }
        }

        #endregion

        #region Read

        /// <summary>
        /// Gets an ErpAccountCustomerRegistrationBankingDetails by Id
        /// </summary>
        /// <param name="id">ErpAccountCustomerRegistrationBankingDetails identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ErpAccountCustomerRegistrationBankingDetails
        /// </returns>
        public async Task<ErpAccountCustomerRegistrationBankingDetails> GetErpAccountCustomerRegistrationBankingDetailsByIdAsync(int id)
        {
            if (id == 0)
                return null;

            var erpAccountCustomerRegistrationBankingDetails = await _erpAccountCustomerRegistrationBankingDetailsRepository.GetByIdAsync(id, cache => default);

            if (erpAccountCustomerRegistrationBankingDetails == null || erpAccountCustomerRegistrationBankingDetails.IsDeleted)
                return null;

            return erpAccountCustomerRegistrationBankingDetails;
        }

        /// <summary>
        /// Gets an ErpAccountCustomerRegistrationBankingDetails by Id if it is active
        /// </summary>
        /// <param name="id">ErpAccountCustomerRegistrationBankingDetails identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ErpAccountCustomerRegistrationBankingDetails if it is active
        /// </returns>
        public async Task<ErpAccountCustomerRegistrationBankingDetails> GetErpAccountCustomerRegistrationBankingDetailsByIdWithActiveAsync(int id)
        {
            if (id == 0)
                return null;

            var erpAccountCustomerRegistrationBankingDetails = await _erpAccountCustomerRegistrationBankingDetailsRepository.GetByIdAsync(id, cache => default);

            if (erpAccountCustomerRegistrationBankingDetails == null || !erpAccountCustomerRegistrationBankingDetails.IsActive || erpAccountCustomerRegistrationBankingDetails.IsDeleted)
                return null;

            return erpAccountCustomerRegistrationBankingDetails;
        }

        /// <summary>
        /// Gets all ErpAccountCustomerRegistrationBankingDetails
        /// </summary>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">If only total no of account needed or not</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains all the ErpAccountCustomerRegistrationBankingDetails
        /// </returns>
        public async Task<IPagedList<ErpAccountCustomerRegistrationBankingDetails>> GetAllErpAccountCustomerRegistrationBankingDetailsAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool getOnlyTotalCount = false, bool? overridePublished = false, int formId = 0, string nameOfBanker = null, string accountName = null, string accountNumber = null, string branchCode = null, string branch = null)
        {
            var erpAccountCustomerRegistrationBankingDetails = await _erpAccountCustomerRegistrationBankingDetailsRepository.GetAllPagedAsync(query =>
            {
                if (!showHidden)
                    query = query.Where(egp => egp.IsActive);

                if (formId > 0)
                    query = query.Where(egp => egp.FormId == formId);

                if (!string.IsNullOrEmpty(nameOfBanker))
                {
                    query = query.Where(egp => egp.NameOfBanker.Contains(nameOfBanker));
                }

                if (!string.IsNullOrEmpty(accountName))
                {
                    query = query.Where(egp => egp.AccountName.Contains(accountName));
                }

                if (!string.IsNullOrEmpty(accountNumber))
                {
                    query = query.Where(egp => egp.AccountNumber.Contains(accountNumber));
                }

                if (!string.IsNullOrEmpty(branchCode))
                {
                    query = query.Where(egp => egp.BranchCode.Contains(branchCode));
                }

                if (!string.IsNullOrEmpty(branch))
                {
                    query = query.Where(egp => egp.Branch.Contains(branch));
                }

                query = query.Where(egp => !egp.IsDeleted);

                query = query.OrderBy(egp => egp.Id);
                return query;

            }, pageIndex, pageSize, getOnlyTotalCount);

            return erpAccountCustomerRegistrationBankingDetails;
        }

        public async Task<IList<ErpAccountCustomerRegistrationBankingDetails>> GetErpAccountCustomerRegistrationBankingDetailsByFormIdAsync(int formId)
        {
            if (formId == 0)
                return null;

            return  (from egp in _erpAccountCustomerRegistrationBankingDetailsRepository.Table
                          where egp.FormId == formId && egp.IsDeleted != true && egp.IsActive == true
                          select egp).ToList();
        }

        #endregion

        #endregion
    }
}

