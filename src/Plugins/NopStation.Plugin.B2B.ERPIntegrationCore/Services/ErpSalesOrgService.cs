using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public class ErpSalesOrgService : IErpSalesOrgService
    {
        #region Fields

        private readonly IRepository<ErpSalesOrg> _erpErpSalesOrgRepository;
        private readonly IRepository<ErpAccount> _erpAccountRepository;

        #endregion

        #region ctor

        public ErpSalesOrgService(IRepository<ErpSalesOrg> erpSalesOrgRepository, IRepository<ErpAccount> erpAccountRepository)
        {
            _erpErpSalesOrgRepository = erpSalesOrgRepository;
            _erpAccountRepository = erpAccountRepository;
        }

        #endregion

        #region Methods

        #region Insert/Update

        public async Task InsertErpSalesOrgAsync(ErpSalesOrg erpSalesOrg)
        {
            await _erpErpSalesOrgRepository.InsertAsync(erpSalesOrg);
        }

        public async Task UpdateErpSalesOrgAsync(ErpSalesOrg erpSalesOrg)
        {
            await _erpErpSalesOrgRepository.UpdateAsync(erpSalesOrg);
        }

        #endregion

        #region Delete

        private async Task DeleteErpSalesOrgAsync(ErpSalesOrg erpSalesOrg)
        {
            //as ErpBaseEntity dosen't inherit ISoftDelete but has that feature
            erpSalesOrg.IsDeleted = true;
            await _erpErpSalesOrgRepository.UpdateAsync(erpSalesOrg);
        }

        public async Task DeleteErpSalesOrgByIdAsync(int id)
        {
            var erpSalesOrg = await GetErpSalesOrgByIdAsync(id);
            if (erpSalesOrg != null)
            {
                await DeleteErpSalesOrgAsync(erpSalesOrg);
            }
        }

        #endregion

        #region Read

        /// <summary>
        /// Gets an ErpAccount by Id
        /// </summary>
        /// <param name="id">ErpAccount identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ErpAccount
        /// </returns>
        public async Task<ErpSalesOrg> GetErpSalesOrgByIdAsync(int id)
        {
            if (id == 0)
                return null;

            var erpSalesOrg = await _erpErpSalesOrgRepository.GetByIdAsync(id, cache => default);

            if (erpSalesOrg == null || erpSalesOrg.IsDeleted)
                return null;

            return erpSalesOrg;
        }

        /// <summary>
        /// Gets an ErpAccount by Id if it is active
        /// </summary>
        /// <param name="id">ErpAccount identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ErpAccount if it is activ
        /// </returns>
        public async Task<ErpSalesOrg> GetErpSalesOrgByIdWithActiveAsync(int id)
        {
            if (id == 0)
                return null;

            var erpSalesOrg = await _erpErpSalesOrgRepository.GetByIdAsync(id, cache => default);

            if (erpSalesOrg == null || !erpSalesOrg.IsActive || erpSalesOrg.IsDeleted)
                return null;

            return erpSalesOrg;
        }

        /// <summary>
        /// Gets all ErpAccounts
        /// </summary>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">If only total no of account needed or not</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains all the ErpAccounts
        /// </returns>
        public async Task<IPagedList<ErpSalesOrg>> GetAllErpSalesOrgAsync(int pageIndex = 0, int pageSize = int.MaxValue, string name = null, string email = null, string code = null, bool? showHidden = null, bool getOnlyTotalCount = false)
        {
            var erpSalesOrgs = await _erpErpSalesOrgRepository.GetAllPagedAsync(query =>
            {
                // showHidden is null for getting all, true for only actives and false for only inactives
                if (showHidden.HasValue)
                {
                    if (!showHidden.Value)
                        query = query.Where(v => v.IsActive == true);
                    else
                        query = query.Where(v => v.IsActive == false);
                }

                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where(c => c.Name.Contains(name));

                if (!string.IsNullOrWhiteSpace(email))
                    query = query.Where(c => c.Email.Contains(email));

                if (!string.IsNullOrWhiteSpace(code))
                    query = query.Where(c => c.Code.Contains(code));

                query = query.Where(egp => !egp.IsDeleted);
                query = query.OrderBy(egp => egp.Id);
                return query;

            }, pageIndex, pageSize, getOnlyTotalCount);

            return erpSalesOrgs;
        }

        public async Task<IList<ErpSalesOrg>> GetAllErpSalesOrgsAsync()
        {
            var erpAccounts = await _erpErpSalesOrgRepository.GetAllAsync(query =>
            {
                query = query.Where(v => v.IsActive && !v.IsDeleted);
                query = query.OrderBy(ea => ea.Id);
                return query;
            });

            return erpAccounts;
        }

        public async Task<bool> IsMappedWithAnyERPAccountAsync(int erpSalesOrgId)
        {
            var isMapped = await _erpAccountRepository.Table.AnyAsync(ea => !ea.IsDeleted && ea.ErpSalesOrgId == erpSalesOrgId);
            return isMapped;
        }

        #endregion

        #endregion
    }
}

