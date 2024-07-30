using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public class ErpAccountCustomerRegistrationPremisesService : IErpAccountCustomerRegistrationPremisesService
    {
        #region Fields

        private readonly IRepository<ErpAccountCustomerRegistrationPremises> _erpAccountCustomerRegistrationPremisesRepository;

        #endregion

        #region ctor

        public ErpAccountCustomerRegistrationPremisesService(IRepository<ErpAccountCustomerRegistrationPremises> erpAccountCustomerRegistrationPremisesRepository)
        {
            _erpAccountCustomerRegistrationPremisesRepository = erpAccountCustomerRegistrationPremisesRepository;
        }

        #endregion

        #region Methods

        #region Insert/Update

        public async Task InsertErpAccountCustomerRegistrationPremisesAsync(ErpAccountCustomerRegistrationPremises erpAccountCustomerRegistrationPremises)
        {
            await _erpAccountCustomerRegistrationPremisesRepository.InsertAsync(erpAccountCustomerRegistrationPremises);
        }

        public async Task UpdateErpAccountCustomerRegistrationPremisesAsync(ErpAccountCustomerRegistrationPremises erpAccountCustomerRegistrationPremises)
        {
            await _erpAccountCustomerRegistrationPremisesRepository.UpdateAsync(erpAccountCustomerRegistrationPremises);
        }

        #endregion

        #region Delete

        private async Task DeleteErpAccountCustomerRegistrationPremisesAsync(ErpAccountCustomerRegistrationPremises erpAccountCustomerRegistrationPremises)
        {
            //as ErpBaseEntity dosen't inherit ISoftDelete but has that feature
            erpAccountCustomerRegistrationPremises.IsDeleted = true;
            await _erpAccountCustomerRegistrationPremisesRepository.UpdateAsync(erpAccountCustomerRegistrationPremises);
        }

        public async Task DeleteErpAccountCustomerRegistrationPremisesByIdAsync(int id)
        {
            var erpAccountCustomerRegistrationPremises = await GetErpAccountCustomerRegistrationPremisesByIdAsync(id);
            if (erpAccountCustomerRegistrationPremises != null)
            {
                await DeleteErpAccountCustomerRegistrationPremisesAsync(erpAccountCustomerRegistrationPremises);
            }
        }

        #endregion

        #region Read

        /// <summary>
        /// Gets an ErpAccountCustomerRegistrationPremises by Id
        /// </summary>
        /// <param name="id">ErpAccountCustomerRegistrationPremises identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ErpAccountCustomerRegistrationPremises
        /// </returns>
        public async Task<ErpAccountCustomerRegistrationPremises> GetErpAccountCustomerRegistrationPremisesByIdAsync(int id)
        {
            if (id == 0)
                return null;

            var erpAccountCustomerRegistrationPremises = await _erpAccountCustomerRegistrationPremisesRepository.GetByIdAsync(id, cache => default);

            if (erpAccountCustomerRegistrationPremises == null || erpAccountCustomerRegistrationPremises.IsDeleted)
                return null;

            return erpAccountCustomerRegistrationPremises;
        }

        /// <summary>
        /// Gets an ErpAccountCustomerRegistrationPremises by Id if it is active
        /// </summary>
        /// <param name="id">ErpAccountCustomerRegistrationPremises identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ErpAccountCustomerRegistrationPremises if it is active
        /// </returns>
        public async Task<ErpAccountCustomerRegistrationPremises> GetErpAccountCustomerRegistrationPremisesByIdWithActiveAsync(int id)
        {
            if (id == 0)
                return null;

            var erpAccountCustomerRegistrationPremises = await _erpAccountCustomerRegistrationPremisesRepository.GetByIdAsync(id, cache => default);

            if (erpAccountCustomerRegistrationPremises == null || !erpAccountCustomerRegistrationPremises.IsActive || erpAccountCustomerRegistrationPremises.IsDeleted)
                return null;

            return erpAccountCustomerRegistrationPremises;
        }

        /// <summary>
        /// Gets all ErpAccountCustomerRegistrationPremises
        /// </summary>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">If only total no of account needed or not</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains all the ErpAccountCustomerRegistrationPremises
        /// </returns>
        public async Task<IPagedList<ErpAccountCustomerRegistrationPremises>> GetAllErpAccountCustomerRegistrationPremisesAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool getOnlyTotalCount = false, bool? overridePublished = false, int formId = 0, string nameOfLandlord = null, string emailOfLandlord = null)
        {
            var erpAccountCustomerRegistrationPremises = await _erpAccountCustomerRegistrationPremisesRepository.GetAllPagedAsync(query =>
            {
                if (!showHidden)
                    query = query.Where(egp => egp.IsActive);

                if (formId > 0)
                    query = query.Where(egp => egp.FormId == formId);

                if (!string.IsNullOrEmpty(nameOfLandlord))
                {
                    query = query.Where(egp => egp.NameOfLandlord.Contains(nameOfLandlord));
                }

                if (!string.IsNullOrEmpty(emailOfLandlord))
                {
                    query = query.Where(egp => egp.EmailOfLandlord.Contains(emailOfLandlord));
                }

                query = query.Where(egp => !egp.IsDeleted);

                query = query.OrderBy(egp => egp.Id);
                return query;

            }, pageIndex, pageSize, getOnlyTotalCount);

            return erpAccountCustomerRegistrationPremises;
        }

        public async Task<IList<ErpAccountCustomerRegistrationPremises>> GetErpAccountCustomerRegistrationPremisesByFormIdAsync(int formId)
        {
            if (formId == 0)
                return null;

            return  (from egp in _erpAccountCustomerRegistrationPremisesRepository.Table
                          where egp.FormId == formId && !egp.IsDeleted && egp.IsActive
                     select egp).ToList();
        }

        #endregion

        #endregion
    }
}

