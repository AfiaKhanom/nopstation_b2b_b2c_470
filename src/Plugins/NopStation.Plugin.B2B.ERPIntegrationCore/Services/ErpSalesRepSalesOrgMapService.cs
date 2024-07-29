using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public class ErpSalesRepSalesOrgMapService : IErpSalesRepSalesOrgMapService
    {
        #region Fields

        private readonly IRepository<ErpSalesRepSalesOrgMap> _erpSalesRepSalesOrgMapRepository;

        #endregion

        #region ctor

        public ErpSalesRepSalesOrgMapService(IRepository<ErpSalesRepSalesOrgMap> erpSalesRepSalesOrgMapRepository)
        {
            _erpSalesRepSalesOrgMapRepository= erpSalesRepSalesOrgMapRepository;
        }

        #endregion

        #region Methods

        #region Insert/Update

        public async Task InsertErpSalesRepSalesOrgMapAsync(ErpSalesRepSalesOrgMap erpSalesRepSalesOrgMap)
        {
            await _erpSalesRepSalesOrgMapRepository.InsertAsync(erpSalesRepSalesOrgMap);
        }

        public async Task UpdateErpSalesRepSalesOrgMapAsync(ErpSalesRepSalesOrgMap erpSalesRepSalesOrgMap)
        {
            await _erpSalesRepSalesOrgMapRepository.UpdateAsync(erpSalesRepSalesOrgMap);
        }

        #endregion

        #region Delete

        public async Task DeleteErpSalesRepSalesOrgMapAsync(ErpSalesRepSalesOrgMap erpSalesRepSalesOrgMap)
        {
            await _erpSalesRepSalesOrgMapRepository.DeleteAsync(erpSalesRepSalesOrgMap);
        }

        public async Task DeleteErpSalesRepSalesOrgMapByIdAsync(int id)
        {
            var erpSalesRepSalesOrgMap = await GetErpSalesRepSalesOrgMapByIdAsync(id);
            if (erpSalesRepSalesOrgMap != null)
            {
                await DeleteErpSalesRepSalesOrgMapAsync(erpSalesRepSalesOrgMap);
            }
        }

        #endregion

        #region Read

        public async Task<ErpSalesRepSalesOrgMap> GetErpSalesRepSalesOrgMapByIdAsync(int id)
        {
            if (id == 0)
                return null;

            return await _erpSalesRepSalesOrgMapRepository.GetByIdAsync(id, cache => default);
        }

        public async Task<IPagedList<ErpSalesRepSalesOrgMap>> GetAllErpSalesRepSalesOrgMapsAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            var erpSalesRepSalesOrgMaps = await _erpSalesRepSalesOrgMapRepository.GetAllPagedAsync(query =>
            {
                query = query.OrderBy(ei => ei.Id);
                return query;

            }, pageIndex, pageSize, getOnlyTotalCount);

            return erpSalesRepSalesOrgMaps;
        }

        public async Task<IList<ErpSalesRepSalesOrgMap>> GetErpSalesRepSalesOrgMapsByErpSalesRepIdAsync(int erpSalesRepId)
        {
            if (erpSalesRepId == 0)
                return null;

            var erpSalesRepSalesOrgMaps = await _erpSalesRepSalesOrgMapRepository.GetAllAsync(query =>
            {
                query = query.Where(ei => ei.ErpSalesRepId == erpSalesRepId);
                query = query.OrderBy(ei => ei.Id);
                return query;

            });

            return erpSalesRepSalesOrgMaps;
        }

        public async Task<IList<ErpSalesRepSalesOrgMap>> GetErpSalesRepSalesOrgMapsByErpSalesOrgIdAsync(int erpSalesOrgId)
        {
            if (erpSalesOrgId == 0)
                return null;

            var erpSalesRepSalesOrgMaps = await _erpSalesRepSalesOrgMapRepository.GetAllAsync(query =>
            {
                query = query.Where(ei => ei.ErpSalesOrgId == erpSalesOrgId);
                query = query.OrderBy(ei => ei.Id);
                return query;

            });

            return erpSalesRepSalesOrgMaps;
        }
        #endregion

        #endregion
    }
}

