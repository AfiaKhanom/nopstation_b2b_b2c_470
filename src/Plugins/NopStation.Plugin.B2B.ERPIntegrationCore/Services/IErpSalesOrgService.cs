using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public interface IErpSalesOrgService
    {
        Task InsertErpSalesOrgAsync(ErpSalesOrg erpSalesOrg);

        Task UpdateErpSalesOrgAsync(ErpSalesOrg erpSalesOrg);

        Task DeleteErpSalesOrgByIdAsync(int id);

        Task<ErpSalesOrg> GetErpSalesOrgByIdAsync(int id);

        Task<ErpSalesOrg> GetErpSalesOrgByIdWithActiveAsync(int id);

        Task<IPagedList<ErpSalesOrg>> GetAllErpSalesOrgAsync(int pageIndex = 0, 
            int pageSize = int.MaxValue, 
            string name = null, 
            string email = null, 
            string code = null, 
            bool? showHidden = null, 
            bool getOnlyTotalCount = false);

        Task<IList<ErpSalesOrg>> GetAllErpSalesOrgsAsync();

        Task<bool> IsMappedWithAnyERPAccountAsync(int erpSalesOrgId);

    }
}

