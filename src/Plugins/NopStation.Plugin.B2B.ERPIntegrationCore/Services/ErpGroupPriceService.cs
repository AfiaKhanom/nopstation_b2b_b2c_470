using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public class ErpGroupPriceService : IErpGroupPriceService
    {
        #region Fields

        private readonly IRepository<ErpGroupPrice> _erpGroupPriceRepository;
        private readonly IRepository<ErpGroupPriceCode> _erpGroupPriceCodeRepository;

        #endregion

        #region ctor

        public ErpGroupPriceService(IRepository<ErpGroupPrice> erpGroupPriceRepository, IRepository<ErpGroupPriceCode> erpGroupPriceCodeRepository)
        {
            _erpGroupPriceRepository = erpGroupPriceRepository;
            _erpGroupPriceCodeRepository = erpGroupPriceCodeRepository;
        }

        #endregion

        #region Methods

        #region Insert/Update

        public async Task InsertErpGroupPriceAsync(ErpGroupPrice erpGroupPrice)
        {
            await _erpGroupPriceRepository.InsertAsync(erpGroupPrice);
        }

        public async Task UpdateErpGroupPriceAsync(ErpGroupPrice erpGroupPrice)
        {
            await _erpGroupPriceRepository.UpdateAsync(erpGroupPrice);
        }

        #endregion

        #region Delete

        private async Task DeleteErpGroupPriceAsync(ErpGroupPrice erpGroupPrice)
        {
            //as ErpBaseEntity dosen't inherit ISoftDelete but has that feature
            erpGroupPrice.IsDeleted = true;
            await _erpGroupPriceRepository.UpdateAsync(erpGroupPrice);
        }

        public async Task DeleteErpGroupPriceByIdAsync(int id)
        {
            var erpGroupPrice = await GetErpGroupPriceByIdAsync(id);
            if (erpGroupPrice != null)
            {
                await DeleteErpGroupPriceAsync(erpGroupPrice);
            }
        }

        #endregion

        #region Read

        /// <summary>
        /// Gets an ErpGroupPrice by Id
        /// </summary>
        /// <param name="id">ErpGroupPrice identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ErpGroupPrice
        /// </returns>
        public async Task<ErpGroupPrice> GetErpGroupPriceByIdAsync(int id)
        {
            if (id == 0)
                return null;

            var erpGroupPrice = await _erpGroupPriceRepository.GetByIdAsync(id, cache => default);

            if (erpGroupPrice == null || erpGroupPrice.IsDeleted)
                return null;

            return erpGroupPrice;
        }

        /// <summary>
        /// Gets an ErpGroupPrice by Id if it is active
        /// </summary>
        /// <param name="id">ErpGroupPrice identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ErpGroupPrice if it is activ
        /// </returns>
        public async Task<ErpGroupPrice> GetErpGroupPriceByIdWithActiveAsync(int id)
        {
            if (id == 0)
                return null;

            var erpGroupPrice = await _erpGroupPriceRepository.GetByIdAsync(id, cache => default);

            if (erpGroupPrice == null || !erpGroupPrice.IsActive || erpGroupPrice.IsDeleted)
                return null;

            return erpGroupPrice;
        }

        /// <summary>
        /// Gets all ErpGroupPrices
        /// </summary>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">If only total no of account needed or not</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains all the ErpGroupPrices
        /// </returns>
        public async Task<IPagedList<ErpGroupPrice>> GetAllErpGroupPricesAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool getOnlyTotalCount = false, bool? overridePublished = false,
            int productId = 0, string groupCode = null)
        {
            var erpGroupPrices = await _erpGroupPriceRepository.GetAllPagedAsync(query =>
            {
                if (!showHidden)
                    query = query.Where(egp => egp.IsActive);

                if (productId > 0)
                    query = query.Where(egp => egp.NopProductId == productId);

                if (!string.IsNullOrEmpty(groupCode))
                {
                    query = query.Join(_erpGroupPriceCodeRepository.Table, x => x.ErpNopGroupPriceCodeId, y => y.Id,
                            (x, y) => new { ErpGroupPrice = x, ErpGroupPriceCode = y })
                        .Where(z => z.ErpGroupPriceCode.Code.Contains(groupCode))
                        .Select(z => z.ErpGroupPrice)
                        .Distinct();
                }

                query = query.Where(egp => !egp.IsDeleted);

                query = query.OrderBy(egp => egp.Id);
                return query;

            }, pageIndex, pageSize, getOnlyTotalCount);

            return erpGroupPrices;
        }

        public async Task<IList<ErpGroupPrice>> GetErpGroupPriceByProductIdAsync(int productId)
        {
            if (productId == 0)
                return null;

            return  (from egp in _erpGroupPriceRepository.Table
                          where egp.NopProductId == productId && egp.IsDeleted != true && egp.IsActive == true
                          select egp).ToList();
        }

        public async Task<ErpGroupPrice> GetB2BPriceGroupProductPricingByErpPriceGroupCodeAndProductId(int priceGroupCodeId, int productId)
        {
            if (productId == 0 && priceGroupCodeId == 0)
                return null;

            return await (from egp in _erpGroupPriceRepository.Table
                          where egp.NopProductId == productId && egp.ErpNopGroupPriceCodeId == priceGroupCodeId && !egp.IsDeleted && egp.IsActive
                          select egp).FirstOrDefaultAsync();
        }

        public async Task<bool> CheckAnyPriceGroupProductPricingExistWithProductIdAndPriceGroupCodeId(int prouctId, int priceGroupCodeId)
        {
            if (prouctId == 0 || priceGroupCodeId == 0)
                return false;

            var query = _erpGroupPriceRepository.Table;

            return query.Any(b => b.NopProductId == prouctId && b.ErpNopGroupPriceCodeId == priceGroupCodeId && !b.IsDeleted);
        }

        public async Task InActiveAllOldGroupPrice(DateTime syncStartTime)
        {
            if (syncStartTime == DateTime.MinValue)
                return;
            var erpGroupPrices = await _erpGroupPriceRepository.GetAllAsync(query =>
            {
                query = query.Where(a => a.UpdatedOnUtc < syncStartTime);
                return query;
            });
            foreach (var erpAccount in erpGroupPrices)
            {
                erpAccount.IsActive = false;
            }
            await _erpGroupPriceRepository.UpdateAsync(erpGroupPrices);
        }

        #endregion

        #endregion
    }
}

