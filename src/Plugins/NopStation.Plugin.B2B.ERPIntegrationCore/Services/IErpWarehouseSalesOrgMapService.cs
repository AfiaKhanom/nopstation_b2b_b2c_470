using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public interface IErpWarehouseSalesOrgMapService
    {
        Task InsertErpWarehouseSalesOrgMapAsync(ErpWarehouseSalesOrgMap erpWarehouseSalesOrgMap);

        Task UpdateErpWarehouseSalesOrgMapAsync(ErpWarehouseSalesOrgMap erpWarehouseSalesOrgMap);

        Task DeleteErpWarehouseSalesOrgMapByIdAsync(int id);

        Task<ErpWarehouseSalesOrgMap> GetErpWarehouseSalesOrgMapByIdAsync(int id);

        Task<IList<ErpWarehouseSalesOrgMap>> GetErpWarehouseSalesOrgMapsBySalesOrgIdAsync(int salesOrgId);

        Task<IList<ErpWarehouseSalesOrgMap>> GetWarehouseSalesOrgMapByErpWarehouseIdAsync(int erpWarehouseId);
        Task<IList<ErpWarehouseSalesOrgMap>> GetWarehouseSalesOrgMapByNopWarehouseIdAsync(int nopWarehouseId);

        Task<IPagedList<ErpWarehouseSalesOrgMap>> GetAllErpWarehouseSalesOrgMapsAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, int salesOrgId = 0);

        Task<bool> CheckAnyErpSalesOrgWarehouseExistBySalesOrgIdAndNopWarehouseId(int salesOrgId, int nopWarehouseId);

    }
}

