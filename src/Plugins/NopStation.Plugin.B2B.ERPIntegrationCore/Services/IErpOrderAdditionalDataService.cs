using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public interface IErpOrderAdditionalDataService
    {
        Task InsertErpOrderAdditionalDataAsync(ErpOrderAdditionalData erpOrderAdditionalData);

        Task UpdateErpOrderAdditionalDataAsync(ErpOrderAdditionalData erpOrderAdditionalData);

        Task DeleteErpOrderAdditionalDataByIdAsync(int id);

        Task<ErpOrderAdditionalData> GetErpOrderAdditionalDataByIdAsync(int id);

        Task<IPagedList<ErpOrderAdditionalData>> GetAllErpOrderAdditionalDataAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, int accountId = 0, int nopCustomerId = 0, string email = null,  string erpOrderNumber = null, string nopOrderNumber = null, int erpOrderOriginTypeId = 0, int erpOrderTypeId = 0, int integrationStatusTypeId = 0, DateTime? searchOrderDateFrom = null, DateTime? searchOrderDateTo = null);
        Task<ErpOrderAdditionalData> GetErpOrderAdditionalDataByNopOrderIdAsync(int nopOrderId);

        Task<IList<ErpOrderAdditionalData>> GetErpOrderAdditionalDatasByAccountIdAsync(int accountId);

        Task<bool> CheckQuoteOrderStatusAsync(ErpOrderAdditionalData erpOrderAdditionalData);

        Task<IDictionary<string, string>> GetAllCustomerReferencesByERPOrderNumbersAsync(IList<string> erpOrderNumbers);

        Task<Order> GetNopOrderByErpOrderNumberAsync(string erpOrderNumber);
        Task<IList<ErpOrderAdditionalData>> GetAllFailedOrProcessingOrQueuedErpOrders(int maxIntegrationRetries = 0);
    }
}

