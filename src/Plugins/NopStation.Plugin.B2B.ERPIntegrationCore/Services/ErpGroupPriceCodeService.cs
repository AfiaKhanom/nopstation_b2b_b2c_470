using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public class ErpGroupPriceCodeService : IErpGroupPriceCodeService
    {
        #region Fields

        private readonly IRepository<ErpGroupPriceCode> _erpGroupPriceCodeRepository;

        #endregion

        #region ctor

        public ErpGroupPriceCodeService(IRepository<ErpGroupPriceCode> erpGroupPriceCodeRepository)
        {
            _erpGroupPriceCodeRepository = erpGroupPriceCodeRepository;
        }

        #endregion

        #region Methods

        #region Insert/Update

        public async Task InsertErpGroupPriceCodeAsync(ErpGroupPriceCode erpGroupPriceCode)
        {
            await _erpGroupPriceCodeRepository.InsertAsync(erpGroupPriceCode);
        }

        public async Task UpdateErpGroupPriceCodeAsync(ErpGroupPriceCode erpGroupPriceCode)
        {
            await _erpGroupPriceCodeRepository.UpdateAsync(erpGroupPriceCode);
        }

        #endregion

        #region Delete

        private async Task DeleteErpGroupPriceCodeAsync(ErpGroupPriceCode erpGroupPriceCode)
        {
            //as ErpBaseEntity dosen't inherit ISoftDelete but has that feature
            erpGroupPriceCode.IsDeleted = true;
            await _erpGroupPriceCodeRepository.UpdateAsync(erpGroupPriceCode);
        }

        public async Task DeleteErpGroupPriceCodeByIdAsync(int id)
        {
            var erpGroupPriceCode = await GetErpGroupPriceCodeByIdAsync(id);
            if (erpGroupPriceCode != null)
            {
                await DeleteErpGroupPriceCodeAsync(erpGroupPriceCode);
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
        public async Task<ErpGroupPriceCode> GetErpGroupPriceCodeByIdAsync(int id)
        {
            if (id == 0)
                return null;

            var erpGroupPriceCode = await _erpGroupPriceCodeRepository.GetByIdAsync(id, cache => default);

            if (erpGroupPriceCode == null || erpGroupPriceCode.IsDeleted)
                return null;

            return erpGroupPriceCode;
        }

        /// <summary>
        /// Gets an ErpAccount by Id if it is active
        /// </summary>
        /// <param name="id">ErpAccount identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ErpAccount if it is activ
        /// </returns>
        public async Task<ErpGroupPriceCode> GetErpGroupPriceCodeByIdWithActiveAsync(int id)
        {
            if (id == 0)
                return null;

            var erpGroupPriceCode = await _erpGroupPriceCodeRepository.GetByIdAsync(id, cache => default);

            if (erpGroupPriceCode == null || !erpGroupPriceCode.IsActive || erpGroupPriceCode.IsDeleted)
                return null;

            return erpGroupPriceCode;
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
        public async Task<IList<ErpGroupPriceCode>> GetAllErpGroupPriceCodesAsync(bool showHidden = false)
        {
            var erpGroupPriceCodes = await _erpGroupPriceCodeRepository.GetAllAsync(query =>
            {
                if (!showHidden)
                    query = query.Where(egpc => egpc.IsActive);

                query = query.Where(egpc => !egpc.IsDeleted);
                query = query.OrderBy(egpc => egpc.Id);
                return query;

            });

            return erpGroupPriceCodes;
        }

        public async Task<IPagedList<ErpGroupPriceCode>> GetAllErpGroupPriceCodesPagedAsync(string groupPriceCode, 
            int pageIndex = 0, 
            int pageSize = int.MaxValue,
            bool? showHidden = null,
            bool getOnlyTotalCount = false)
        {
            var groupPriceCodes = await _erpGroupPriceCodeRepository.GetAllPagedAsync(query =>
            {
                query = query.Where(v => !v.IsDeleted);

                // showHidden is null for getting all, true for only actives and false for only inactives
                if (showHidden.HasValue)
                {
                    if (!showHidden.Value)
                        query = query.Where(v => v.IsActive == true);
                    else
                        query = query.Where(v => v.IsActive == false);
                }

                if (!string.IsNullOrEmpty(groupPriceCode))
                    query = query.Where(b => b.Code.Contains(groupPriceCode));

                query = query.OrderByDescending(ea => ea.Id);
                return query;

            }, pageIndex, pageSize, getOnlyTotalCount);

            return groupPriceCodes;
        }

        #endregion

        public async Task<bool> CheckAnyErpGroupPriceCodeExistByCode(string groupPriceCode)
        {
            if (string.IsNullOrEmpty(groupPriceCode))
                return false;

            var erpGroupPriceCodeExists = await _erpGroupPriceCodeRepository.GetAllAsync(query =>
            {
                query = query.Where(egpc => egpc.Code.Equals(groupPriceCode) && !egpc.IsDeleted);
                return query;
            });

            return erpGroupPriceCodeExists.Any();
        }

        public async Task<ErpGroupPriceCode> GetErpGroupPriceCodeByCodedAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
                return new ErpGroupPriceCode();

            var erpGroupPriceCode =  _erpGroupPriceCodeRepository.GetAll().FirstOrDefault(egpc => egpc.Code.Equals(code) && !egpc.IsDeleted);

            return erpGroupPriceCode;
        }

        #endregion
    }
}

