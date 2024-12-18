using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using Nop.Services.Orders;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public class ErpOrderAdditionalDataService : IErpOrderAdditionalDataService
    {
        #region Fields

        private readonly IRepository<ErpOrderAdditionalData> _erpOrderAdditionalDataRepository;
        private readonly IRepository<ErpAccount> _erpAccountRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IOrderService _orderService;

        #endregion

        #region ctor

        public ErpOrderAdditionalDataService(IRepository<ErpOrderAdditionalData> erpOrderAdditionalDataRepository,
            IRepository<ErpAccount> erpAccountRepository,
            IRepository<Order> orderRepository,
            IOrderService orderService)
        {
            _erpOrderAdditionalDataRepository = erpOrderAdditionalDataRepository;
            _erpAccountRepository = erpAccountRepository;
            _orderRepository = orderRepository;
            _orderService = orderService;
        }

        #endregion

        #region Methods

        #region Insert/Update

        public async Task InsertErpOrderAdditionalDataAsync(ErpOrderAdditionalData erpOrderAdditionalData)
        {
            await _erpOrderAdditionalDataRepository.InsertAsync(erpOrderAdditionalData);
        }

        public async Task UpdateErpOrderAdditionalDataAsync(ErpOrderAdditionalData erpOrderAdditionalData)
        {
            await _erpOrderAdditionalDataRepository.UpdateAsync(erpOrderAdditionalData);
        }

        #endregion

        #region Delete

        private async Task DeleteErpOrderAdditionalDataAsync(ErpOrderAdditionalData erpOrderAdditionalData)
        {
            await _erpOrderAdditionalDataRepository.DeleteAsync(erpOrderAdditionalData);
        }

        public async Task DeleteErpOrderAdditionalDataByIdAsync(int id)
        {
            var erpOrderAdditionalData = await GetErpOrderAdditionalDataByIdAsync(id);
            if (erpOrderAdditionalData != null)
            {
                await DeleteErpOrderAdditionalDataAsync(erpOrderAdditionalData);
            }
        }

        #endregion

        #region Read

        public async Task<ErpOrderAdditionalData> GetErpOrderAdditionalDataByIdAsync(int id)
        {
            if (id == 0)
                return null;

            return await _erpOrderAdditionalDataRepository.GetByIdAsync(id, cache => default);
        }

        public async Task<IPagedList<ErpOrderAdditionalData>> GetAllErpOrderAdditionalDataAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, int accountId = 0, int nopCustomerId = 0, string email = null,  string erpOrderNumber = null, string nopOrderNumber = null, int erpOrderOriginTypeId = 0, int erpOrderTypeId = 0, int integrationStatusTypeId = 0, DateTime? searchOrderDateFrom = null, DateTime? searchOrderDateTo = null)
        {
            var erpOrderAdditionalData = await _erpOrderAdditionalDataRepository.GetAllPagedAsync(query =>
            {
                if (accountId > 0)
                    query = query.Where(x => x.ErpAccountId == accountId); 
                if (erpOrderTypeId > 0)
                    query = query.Where(x => x.ErpOrderTypeId == erpOrderTypeId);
                if (integrationStatusTypeId > 0)
                    query = query.Where(x => x.IntegrationStatusTypeId == integrationStatusTypeId);
                if (erpOrderOriginTypeId > 0)
                    query = query.Where(x => x.ErpOrderOriginTypeId == erpOrderOriginTypeId);
                if (!string.IsNullOrEmpty(erpOrderNumber))
                {
                    query = query.Where(x => x.ErpOrderNumber.Contains(erpOrderNumber.ToLower()) || x.NopOrderId.ToString().Contains(erpOrderNumber.ToLower()));
                }
                if (!string.IsNullOrEmpty(nopOrderNumber))
                {
                    query = query.Where(x => x.ErpOrderNumber.Contains(nopOrderNumber.ToLower()) || x.NopOrderId.ToString().Contains(nopOrderNumber.ToLower()));
                } 
                if (searchOrderDateFrom != null && searchOrderDateFrom.HasValue)
                {
                    query = from or in _orderRepository.Table
                            join q in query
                            on or.Id equals q.NopOrderId
                            where or.CreatedOnUtc >= searchOrderDateFrom.Value
                            select q;
                }
                if (searchOrderDateTo != null && searchOrderDateTo.HasValue)
                {
                    query = from or in _orderRepository.Table
                            join q in query
                            on or.Id equals q.NopOrderId
                            where or.CreatedOnUtc <= searchOrderDateTo.Value
                            select q;
                }

                if (nopCustomerId > 0)
                    query = query.Where(x => x.OrderPlacedByNopCustomerId ==  nopCustomerId);


                query = query.OrderByDescending(ei => ei.Id); 

                return query;
            }, pageIndex, pageSize, getOnlyTotalCount);

            return erpOrderAdditionalData;
        }

        public async Task<ErpOrderAdditionalData> GetErpOrderAdditionalDataByNopOrderIdAsync(int nopOrderId)
        {
            if (nopOrderId <= 0)
                return null;

            var query = from c in _erpOrderAdditionalDataRepository.Table
                        where c.NopOrderId == nopOrderId
                        orderby c.Id
                        select c;
            return await query.FirstOrDefaultAsync();
        }

        public async Task<IList<ErpOrderAdditionalData>> GetErpOrderAdditionalDatasByAccountIdAsync(int accountId)
        {
            var erpOrderAdditionalData = await _erpOrderAdditionalDataRepository.GetAllAsync(query =>
            {
                if (accountId > 0)
                    query = query.Where(x => x.ErpAccountId == accountId);

                query = query.OrderBy(ei => ei.Id);
                return query;

            });

            return erpOrderAdditionalData;
        }

        public async Task<Order> GetNopOrderByErpOrderNumberAsync(string erpOrderNumber)
        {
            if (string.IsNullOrEmpty(erpOrderNumber))
                return null;

            var erpOrderAdditionalData = _erpOrderAdditionalDataRepository.Table.FirstOrDefault(od => od.ErpOrderNumber == erpOrderNumber);
            if (erpOrderAdditionalData != null && erpOrderAdditionalData.NopOrderId > 0)
                return await _orderService.GetOrderByIdAsync(erpOrderAdditionalData.NopOrderId);
            return null;
        }

        #endregion

        #region Customer functionality

        public async Task<bool> CheckQuoteOrderStatusAsync(ErpOrderAdditionalData erpOrderAdditionalData)
        {
            if (erpOrderAdditionalData == null || erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2BSalesOrder || erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2CSalesOrder)
                return false;

            if (erpOrderAdditionalData.QuoteExpiryDate == null || string.IsNullOrEmpty(erpOrderAdditionalData.ERPOrderStatus))
                return false;

            if (erpOrderAdditionalData.QuoteExpiryDate.Value.Date < DateTime.UtcNow.Date)
                return false;

            // a quote can be placed only once (so if any order placed already with this quote order then it is false)
            if (erpOrderAdditionalData.QuoteSalesOrderId.HasValue && erpOrderAdditionalData.QuoteSalesOrderId.Value > 0)
                return false;

            return (erpOrderAdditionalData.ERPOrderStatus == ERPIntegrationCoreDefaults.ERPOrderStatusApproved || erpOrderAdditionalData.ERPOrderStatus == ERPIntegrationCoreDefaults.ERPOrderStatusPendingApproval) ? true : false;
        }
        public async Task<IDictionary<string, string>> GetAllCustomerReferencesByERPOrderNumbersAsync(IList<string> erpOrderNumbers)
        {
            if (erpOrderNumbers == null || !erpOrderNumbers.Any())
                return null;

            var query = _erpOrderAdditionalDataRepository.Table
                .Where(opa => erpOrderNumbers.Contains(opa.ErpOrderNumber));

            return await query.ToDictionaryAsync(t => t.ErpOrderNumber, t => t.CustomerReference);
        }

        public async Task<IList<ErpOrderAdditionalData>> GetAllFailedOrProcessingOrQueuedErpOrders(int maxIntegrationRetries = 0)
        {
            var erpOrderAdditionalData = await _erpOrderAdditionalDataRepository.GetAllPagedAsync(query =>
            {
                query = query.Where(x => x.IntegrationStatusTypeId == (int)IntegrationStatusType.Failed
                                || x.IntegrationStatusTypeId == (int)IntegrationStatusType.Processing
                                || x.IntegrationStatusTypeId == (int)IntegrationStatusType.Queued);

                query = query.Where(x => x.IntegrationRetries < maxIntegrationRetries);

                query = query.OrderByDescending(ei => ei.Id);

                return query;
            });

            return erpOrderAdditionalData;
        }


        #endregion

        #endregion
    }
}

