using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public interface IErpWarehouseAdditionalDataService
    {
        Task InsertErpWarehouseAdditionalDataAsync(ErpWarehouseAdditionalData erpWarehouseAdditionalData);

        Task UpdateErpWarehouseAdditionalDataAsync(ErpWarehouseAdditionalData erpWarehouseAdditionalData);

        Task DeleteErpWarehouseAdditionalDataByIdAsync(int id);

        Task<ErpWarehouseAdditionalData> GetErpWarehouseAdditionalDataByIdAsync(int id);

        Task<ErpWarehouseAdditionalData> GetErpWarehouseAdditionalDataByIdWithActiveAsync(int id);

        Task<IPagedList<ErpWarehouseAdditionalData>> GetAllErpWarehouseAdditionalDataAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool getOnlyTotalCount = false);

        Task<ErpWarehouseAdditionalData> GetErpWarehouseAdditionalDataBySalesOrgIdAsync(int salesOrgId);

        Task<ErpWarehouseSalesOrgMap> GetErpSalesOrgWarehouseForProductAsync(Product product, int erpSalesOrgId, int quantity);
    }
}

