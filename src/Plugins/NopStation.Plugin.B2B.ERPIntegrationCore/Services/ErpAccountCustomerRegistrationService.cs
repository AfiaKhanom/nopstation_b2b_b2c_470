using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public class ErpAccountCustomerRegistrationFormService : IErpAccountCustomerRegistrationFormService
    {
        #region Fields

        private readonly IRepository<ErpAccountCustomerRegistrationForm> _erpAccountCustomerRegistrationFormRepository;

        #endregion

        #region ctor

        public ErpAccountCustomerRegistrationFormService(IRepository<ErpAccountCustomerRegistrationForm> erpAccountCustomerRegistrationFormRepository)
        {
            _erpAccountCustomerRegistrationFormRepository = erpAccountCustomerRegistrationFormRepository;
        }

        #endregion

        #region Methods

        #region Insert/Update

        public async Task InsertErpAccountCustomerRegistrationFormAsync(ErpAccountCustomerRegistrationForm erpAccountCustomerRegistrationForm)
        {
            await _erpAccountCustomerRegistrationFormRepository.InsertAsync(erpAccountCustomerRegistrationForm);
        }

        public async Task UpdateErpAccountCustomerRegistrationFormAsync(ErpAccountCustomerRegistrationForm erpAccountCustomerRegistrationForm)
        {
            await _erpAccountCustomerRegistrationFormRepository.UpdateAsync(erpAccountCustomerRegistrationForm);
        }

        #endregion

        #region Delete

        private async Task DeleteErpAccountCustomerRegistrationFormAsync(ErpAccountCustomerRegistrationForm erpAccountCustomerRegistrationForm)
        {
            //as ErpBaseEntity doesn't inherit ISoftDelete but has that feature
            erpAccountCustomerRegistrationForm.IsDeleted = true;
            await _erpAccountCustomerRegistrationFormRepository.UpdateAsync(erpAccountCustomerRegistrationForm);
        }

        public async Task DeleteErpAccountCustomerRegistrationFormByIdAsync(int id)
        {
            var erpAccountCustomerRegistrationForm = await GetErpAccountCustomerRegistrationFormByIdAsync(id);
            if (erpAccountCustomerRegistrationForm != null)
            {
                await DeleteErpAccountCustomerRegistrationFormAsync(erpAccountCustomerRegistrationForm);
            }
        }

        #endregion

        #region Read

        /// <summary>
        /// Gets an ErpAccountCustomerRegistrationForm by Id
        /// </summary>
        /// <param name="id">ErpAccountCustomerRegistrationForm identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ErpAccountCustomerRegistrationForm
        /// </returns>
        public async Task<ErpAccountCustomerRegistrationForm> GetErpAccountCustomerRegistrationFormByIdAsync(int id)
        {
            if (id == 0)
                return null;

            var erpAccountCustomerRegistrationForm = await _erpAccountCustomerRegistrationFormRepository.GetByIdAsync(id, cache => default);

            if (erpAccountCustomerRegistrationForm == null || erpAccountCustomerRegistrationForm.IsDeleted)
                return null;

            return erpAccountCustomerRegistrationForm;
        }

        /// <summary>
        /// Gets all ErpAccountCustomerRegistrationForm
        /// </summary>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">If only total no of account needed or not</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains all the ErpAccountCustomerRegistrationForm
        /// </returns>
        public async Task<IPagedList<ErpAccountCustomerRegistrationForm>> GetAllErpAccountCustomerRegistrationFormAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool? showHidden = false, bool? showApproved = null, bool getOnlyTotalCount = false, bool? overridePublished = false, int formId = 0, string fullRegisteredName = null, string registrationNumber = null, string accountsEmail = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var erpAccountCustomerRegistrationForm = await _erpAccountCustomerRegistrationFormRepository.GetAllPagedAsync(query =>
            {
                if (showApproved != null)
                    query = query.Where(egp => egp.IsApproved == showApproved);

                if (formId > 0)
                    query = query.Where(egp => egp.Id == formId);

                if (!string.IsNullOrEmpty(fullRegisteredName))
                {
                    query = query.Where(egp => egp.FullRegisteredName.Contains(fullRegisteredName));
                }

                if (!string.IsNullOrEmpty(registrationNumber))
                {
                    query = query.Where(egp => egp.RegistrationNumber.Contains(registrationNumber));
                }

                if (!string.IsNullOrEmpty(accountsEmail))
                {
                    query = query.Where(egp => egp.AccountsEmail.Contains(accountsEmail));
                }

                if (fromDate != null)
                {
                    query = query.Where(egp => egp.CreatedOnUtc >= fromDate.Value);
                }

                if (toDate != null)
                {
                    query = query.Where(egp => egp.CreatedOnUtc <= toDate.Value);
                }

                query = query.Where(egp => !egp.IsDeleted);

                query = query.OrderBy(egp => egp.Id);
                return query;

            }, pageIndex, pageSize, getOnlyTotalCount);

            return erpAccountCustomerRegistrationForm;
        }

        #endregion

        #endregion
    }
}

