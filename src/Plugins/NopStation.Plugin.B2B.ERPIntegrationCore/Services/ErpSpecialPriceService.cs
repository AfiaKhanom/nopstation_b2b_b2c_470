using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public class ErpSpecialPriceService : IErpSpecialPriceService
    {
        #region Fields

        private readonly IRepository<ErpSpecialPrice> _erpSpecialPriceRepository;

        #endregion

        #region ctor

        public ErpSpecialPriceService(IRepository<ErpSpecialPrice> erpSpecialPriceRepository)
        {
            _erpSpecialPriceRepository = erpSpecialPriceRepository;
        }

        #endregion

        #region Methods

        #region Insert/Update

        public async Task InsertErpSpecialPriceAsync(ErpSpecialPrice erpSpecialPrice)
        {
            await _erpSpecialPriceRepository.InsertAsync(erpSpecialPrice);
        }

        public async Task UpdateErpSpecialPriceAsync(ErpSpecialPrice erpSpecialPrice)
        {
            await _erpSpecialPriceRepository.UpdateAsync(erpSpecialPrice);
        }

        #endregion

        #region Delete

        private async Task DeleteErpSpecialPriceAsync(ErpSpecialPrice erpSpecialPrice)
        {
            await _erpSpecialPriceRepository.DeleteAsync(erpSpecialPrice);
        }

        public async Task DeleteErpSpecialPriceByIdAsync(int id)
        {
            var erpSpecialPrice = await GetErpSpecialPriceByIdAsync(id);
            if (erpSpecialPrice != null)
            {
                await DeleteErpSpecialPriceAsync(erpSpecialPrice);
            }
        }

        #endregion

        #region Read

        public async Task<ErpSpecialPrice> GetErpSpecialPriceByIdAsync(int id)
        {
            if (id == 0)
                return null;

            return await _erpSpecialPriceRepository.GetByIdAsync(id, cache => default);
        }

        public async Task<IPagedList<ErpSpecialPrice>> GetAllErpSpecialPricesAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, bool? overridePublished = null, int productId = 0, int accountId = 0)
        {
            var erpSpecialPrice = await _erpSpecialPriceRepository.GetAllPagedAsync(query =>
            {
                if(productId > 0)
                    query = query.Where(ei => ei.NopProductId == productId);

                if(accountId > 0)
                    query = query.Where(ei => ei.ErpAccountId == accountId);

                query = query.OrderBy(ei => ei.Id);
                return query;

            }, pageIndex, pageSize, getOnlyTotalCount);

            return erpSpecialPrice;
        }

        public async Task<IList<ErpSpecialPrice>> GetErpSpecialPricesByErpAccountIdAsync(int erpAcoountId)
        {
            if (erpAcoountId == 0)
                return null;

            var erpSpecialPrices = await _erpSpecialPriceRepository.GetAllAsync(query =>
            {
                query = query.Where(ei => ei.ErpAccountId == erpAcoountId);
                query = query.OrderBy(ei => ei.Id);
                return query;

            });

            return erpSpecialPrices;
        }

        public async Task<IList<ErpSpecialPrice>> GetErpSpecialPricesByNopProductIdAsync(int nopProductId)
        {
            if (nopProductId == 0)
                return null;

            var erpSpecialPrices = await _erpSpecialPriceRepository.GetAllAsync(query =>
            {
                query = query.Where(ei => ei.NopProductId == nopProductId);
                query = query.OrderBy(ei => ei.Id);
                return query;

            });

            return erpSpecialPrices;
        }

        public async Task<ErpSpecialPrice> GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(int accountId, int nopProductId)
        {
            if (accountId == 0 || nopProductId == 0)
                return null;

            var erpSpecialPrices = await _erpSpecialPriceRepository.GetAllAsync(query =>
            {
                query = query.Where(ei => ei.ErpAccountId == accountId && ei.NopProductId == nopProductId);
                query = query.OrderByDescending(ei => ei.Id);
                return query;
            });

            return erpSpecialPrices.FirstOrDefault();
        }

        public async Task<bool> CheckAnySpecialPriceExistWithAccountIdAndProductId(int accountId, int productId)
        {
            if (accountId == 0 || productId == 0)
                return false;

            var query = _erpSpecialPriceRepository.Table;

            return query.Any(b => b.ErpAccountId == accountId && b.NopProductId == productId);
        }
        #endregion

        #endregion
    }
}

