using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public class ErpAccountCustomerRegistrationTradeReferencesService : IErpAccountCustomerRegistrationTradeReferencesService
    {
        #region Fields

        private readonly IRepository<ErpAccountCustomerRegistrationTradeReferences> _erpAccountCustomerRegistrationTradeReferencesRepository;

        #endregion

        #region ctor

        public ErpAccountCustomerRegistrationTradeReferencesService(IRepository<ErpAccountCustomerRegistrationTradeReferences> erpAccountCustomerRegistrationTradeReferencesRepository)
        {
            _erpAccountCustomerRegistrationTradeReferencesRepository = erpAccountCustomerRegistrationTradeReferencesRepository;
        }

        #endregion

        #region Methods

        #region Insert/Update

        public async Task InsertErpAccountCustomerRegistrationTradeReferencesAsync(ErpAccountCustomerRegistrationTradeReferences erpAccountCustomerRegistrationTradeReferences)
        {
            await _erpAccountCustomerRegistrationTradeReferencesRepository.InsertAsync(erpAccountCustomerRegistrationTradeReferences);
        }

        public async Task UpdateErpAccountCustomerRegistrationTradeReferencesAsync(ErpAccountCustomerRegistrationTradeReferences erpAccountCustomerRegistrationTradeReferences)
        {
            await _erpAccountCustomerRegistrationTradeReferencesRepository.UpdateAsync(erpAccountCustomerRegistrationTradeReferences);
        }

        #endregion

        #region Delete

        private async Task DeleteErpAccountCustomerRegistrationTradeReferencesAsync(ErpAccountCustomerRegistrationTradeReferences erpAccountCustomerRegistrationTradeReferences)
        {
            //as ErpBaseEntity doesn't inherit ISoftDelete but has that feature
            erpAccountCustomerRegistrationTradeReferences.IsDeleted = true;
            await _erpAccountCustomerRegistrationTradeReferencesRepository.UpdateAsync(erpAccountCustomerRegistrationTradeReferences);
        }

        public async Task DeleteErpAccountCustomerRegistrationTradeReferencesByIdAsync(int id)
        {
            var erpAccountCustomerRegistrationTradeReferences = await GetErpAccountCustomerRegistrationTradeReferencesByIdAsync(id);
            if (erpAccountCustomerRegistrationTradeReferences != null)
            {
                await DeleteErpAccountCustomerRegistrationTradeReferencesAsync(erpAccountCustomerRegistrationTradeReferences);
            }
        }

        #endregion

        #region Read

        /// <summary>
        /// Gets an ErpAccountCustomerRegistrationTradeReferences by Id
        /// </summary>
        /// <param name="id">ErpAccountCustomerRegistrationTradeReferences identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ErpAccountCustomerRegistrationTradeReferences
        /// </returns>
        public async Task<ErpAccountCustomerRegistrationTradeReferences> GetErpAccountCustomerRegistrationTradeReferencesByIdAsync(int id)
        {
            if (id == 0)
                return null;

            var erpAccountCustomerRegistrationTradeReferences = await _erpAccountCustomerRegistrationTradeReferencesRepository.GetByIdAsync(id, cache => default);

            if (erpAccountCustomerRegistrationTradeReferences == null || erpAccountCustomerRegistrationTradeReferences.IsDeleted)
                return null;

            return erpAccountCustomerRegistrationTradeReferences;
        }

        /// <summary>
        /// Gets an ErpAccountCustomerRegistrationTradeReferences by Id if it is active
        /// </summary>
        /// <param name="id">ErpAccountCustomerRegistrationTradeReferences identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ErpAccountCustomerRegistrationTradeReferences if it is active
        /// </returns>
        public async Task<ErpAccountCustomerRegistrationTradeReferences> GetErpAccountCustomerRegistrationTradeReferencesByIdWithActiveAsync(int id)
        {
            if (id == 0)
                return null;

            var erpAccountCustomerRegistrationTradeReferences = await _erpAccountCustomerRegistrationTradeReferencesRepository.GetByIdAsync(id, cache => default);

            if (erpAccountCustomerRegistrationTradeReferences == null || !erpAccountCustomerRegistrationTradeReferences.IsActive || erpAccountCustomerRegistrationTradeReferences.IsDeleted)
                return null;

            return erpAccountCustomerRegistrationTradeReferences;
        }

        /// <summary>
        /// Gets all ErpAccountCustomerRegistrationTradeReferences
        /// </summary>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">If only total no of account needed or not</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains all the ErpAccountCustomerRegistrationTradeReferences
        /// </returns>
        public async Task<IPagedList<ErpAccountCustomerRegistrationTradeReferences>> GetAllErpAccountCustomerRegistrationTradeReferencesAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool getOnlyTotalCount = false, bool? overridePublished = false, int formId = 0, string name = null, string telephone = null)
        {
            var erpAccountCustomerRegistrationTradeReferences = await _erpAccountCustomerRegistrationTradeReferencesRepository.GetAllPagedAsync(query =>
            {
                if (!showHidden)
                    query = query.Where(egp => egp.IsActive);

                if (formId > 0)
                    query = query.Where(egp => egp.FormId == formId);

                if (!string.IsNullOrEmpty(name))
                {
                    query = query.Where(egp => egp.Name.Contains(name));
                }

                if (!string.IsNullOrEmpty(telephone))
                {
                    query = query.Where(egp => egp.Telephone.Contains(telephone));
                }

                query = query.Where(egp => !egp.IsDeleted);

                query = query.OrderBy(egp => egp.Id);
                return query;

            }, pageIndex, pageSize, getOnlyTotalCount);

            return erpAccountCustomerRegistrationTradeReferences;
        }

        public async Task<IList<ErpAccountCustomerRegistrationTradeReferences>> GetErpAccountCustomerRegistrationTradeReferencesByFormIdAsync(int formId)
        {
            if (formId == 0)
                return null;

            return  (from egp in _erpAccountCustomerRegistrationTradeReferencesRepository.Table
                          where egp.FormId == formId && !egp.IsDeleted && egp.IsActive
                     select egp).ToList();
        }

        #endregion

        #endregion
    }
}

