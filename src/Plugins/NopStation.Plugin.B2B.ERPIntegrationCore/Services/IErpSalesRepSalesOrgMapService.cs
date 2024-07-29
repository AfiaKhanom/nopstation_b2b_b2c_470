using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public interface IErpSalesRepSalesOrgMapService
    {
        Task InsertErpSalesRepSalesOrgMapAsync(ErpSalesRepSalesOrgMap erpSalesRepSalesOrgMap);

        Task UpdateErpSalesRepSalesOrgMapAsync(ErpSalesRepSalesOrgMap erpSalesRepSalesOrgMap);

        Task DeleteErpSalesRepSalesOrgMapAsync(ErpSalesRepSalesOrgMap erpSalesRepSalesOrgMap);

        Task DeleteErpSalesRepSalesOrgMapByIdAsync(int id);

        Task<ErpSalesRepSalesOrgMap> GetErpSalesRepSalesOrgMapByIdAsync(int id);

        Task<IPagedList<ErpSalesRepSalesOrgMap>> GetAllErpSalesRepSalesOrgMapsAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);

        Task<IList<ErpSalesRepSalesOrgMap>> GetErpSalesRepSalesOrgMapsByErpSalesOrgIdAsync(int erpSalesRepId);

        Task<IList<ErpSalesRepSalesOrgMap>> GetErpSalesRepSalesOrgMapsByErpSalesRepIdAsync(int erpSalesRepId);

    }
}

