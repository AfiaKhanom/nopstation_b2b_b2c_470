using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public class ErpWarehouseSalesOrgMapService : IErpWarehouseSalesOrgMapService
    {
        #region Fields

        private readonly IRepository<ErpWarehouseSalesOrgMap> _erpWarehouseSalesOrgMapRepository;

        #endregion

        #region ctor

        public ErpWarehouseSalesOrgMapService(IRepository<ErpWarehouseSalesOrgMap> erpWarehouseSalesOrgMapRepository)
        {
            _erpWarehouseSalesOrgMapRepository = erpWarehouseSalesOrgMapRepository;
        }

        #endregion

        #region Methods

        #region Insert/Update

        public async Task InsertErpWarehouseSalesOrgMapAsync(ErpWarehouseSalesOrgMap erpWarehouseSalesOrgMap)
        {
            await _erpWarehouseSalesOrgMapRepository.InsertAsync(erpWarehouseSalesOrgMap);
        }

        public async Task UpdateErpWarehouseSalesOrgMapAsync(ErpWarehouseSalesOrgMap erpWarehouseSalesOrgMap)
        {
            await _erpWarehouseSalesOrgMapRepository.UpdateAsync(erpWarehouseSalesOrgMap);
        }

        #endregion

        #region Delete

        private async Task DeleteErpWarehouseSalesOrgMapAsync(ErpWarehouseSalesOrgMap erpWarehouseSalesOrgMap)
        {
            await _erpWarehouseSalesOrgMapRepository.DeleteAsync(erpWarehouseSalesOrgMap);
        }

        public async Task DeleteErpWarehouseSalesOrgMapByIdAsync(int id)
        {
            var erpWarehouseSalesOrgMap = await GetErpWarehouseSalesOrgMapByIdAsync(id);
            if (erpWarehouseSalesOrgMap != null)
            {
                await DeleteErpWarehouseSalesOrgMapAsync(erpWarehouseSalesOrgMap);
            }
        }

        #endregion

        #region Read

        public async Task<ErpWarehouseSalesOrgMap> GetErpWarehouseSalesOrgMapByIdAsync(int id)
        {
            if (id == 0)
                return null;

            return await _erpWarehouseSalesOrgMapRepository.GetByIdAsync(id, cache => default);
        }

        public async Task<IPagedList<ErpWarehouseSalesOrgMap>> GetAllErpWarehouseSalesOrgMapsAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, int salesOrgId = 0)
        {
            var erpWarehouseSalesOrgMap = await _erpWarehouseSalesOrgMapRepository.GetAllPagedAsync(query =>
            {
                query = query.Where(x => x.ErpSalesOrgId == salesOrgId);

                query = query.OrderBy(ei => ei.Id);
                return query;

            }, pageIndex, pageSize, getOnlyTotalCount);

            return erpWarehouseSalesOrgMap;
        }

        public async Task<IList<ErpWarehouseSalesOrgMap>> GetWarehouseSalesOrgMapByErpWarehouseIdAsync(int erpWarehouseId)
        {
            if (erpWarehouseId == 0)
                return null;
            var erpWarehouseOrgMap = await _erpWarehouseSalesOrgMapRepository.GetAllAsync(query =>
            {
                query = query.Where(v => v.ErpWarehouseId == erpWarehouseId);
                query = query.OrderBy(ea => ea.Id);
                return query;
            });

            return erpWarehouseOrgMap;
        }

        public async Task<IList<ErpWarehouseSalesOrgMap>> GetWarehouseSalesOrgMapByNopWarehouseIdAsync(int nopWarehouseId)
        {
            if (nopWarehouseId == 0)
                return null;
            var erpWarehouseOrgMap = await _erpWarehouseSalesOrgMapRepository.GetAllAsync(query =>
            {
                query = query.Where(v => v.ErpWarehouseId == nopWarehouseId);
                query = query.OrderBy(ea => ea.Id);
                return query;
            });

            return erpWarehouseOrgMap;
        }
        public async Task<IList<ErpWarehouseSalesOrgMap>> GetErpWarehouseSalesOrgMapsBySalesOrgIdAsync(int salesOrgId)
        {
            if (salesOrgId == 0)
                return null;
            var erpWarehouseOrgMap = await _erpWarehouseSalesOrgMapRepository.GetAllAsync(query =>
            {
                query = query.Where(v => v.ErpSalesOrgId == salesOrgId);
                query = query.OrderByDescending(ea => ea.Id);
                return query;
            });

            return erpWarehouseOrgMap;
        }
        public async Task<bool> CheckAnyErpSalesOrgWarehouseExistBySalesOrgIdAndNopWarehouseId(int salesOrgId, int nopWarehouseId)
        {
            if (salesOrgId == 0 || nopWarehouseId == 0)
                return true;

            var existing = await _erpWarehouseSalesOrgMapRepository.GetAllAsync(query =>
            {
                query = query.Where(v => v.ErpSalesOrgId == salesOrgId && v.NopWarehouseId == nopWarehouseId);
                query = query.OrderByDescending(ea => ea.Id);
                return query;
            });

            return existing.Any();
        }

        #endregion

        #endregion
    }
}

