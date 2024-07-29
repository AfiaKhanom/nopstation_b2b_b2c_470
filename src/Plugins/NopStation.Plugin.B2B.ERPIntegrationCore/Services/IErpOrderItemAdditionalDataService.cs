using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public interface IErpOrderItemAdditionalDataService
    {
        Task InsertErpOrderItemAdditionalDataAsync(ErpOrderItemAdditionalData erpOrderItemAdditionalData);

        Task UpdateErpOrderItemAdditionalDataAsync(ErpOrderItemAdditionalData erpOrderItemAdditionalData);

        Task DeleteErpOrderItemAdditionalDataByIdAsync(int id);

        Task<ErpOrderItemAdditionalData> GetErpOrderItemAdditionalDataByIdAsync(int id);
        
        Task<ErpOrderItemAdditionalData> GetErpOrderItemAdditionalDataByNopOrderItemIdAsync(int orderItemId);

        Task<IPagedList<ErpOrderItemAdditionalData>> GetAllErpOrderItemAdditionalDataAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);
        
        Task<IList<ErpOrderItemAdditionalData>> GetAllErpOrderItemAdditionalDataByErpOrderIdAsync(int orderId);

    }
}

