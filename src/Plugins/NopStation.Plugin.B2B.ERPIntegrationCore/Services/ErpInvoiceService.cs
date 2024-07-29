using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public class ErpInvoiceService : IErpInvoiceService
    {
        #region Fields

        private readonly IRepository<ErpInvoice> _erpInvoiceRepository;
        private readonly IRepository<ErpOrderAdditionalData> _erpErpOrderAdditionalDataRepository;
        private readonly IRepository<Order> _orderRepository;

        #endregion

        #region ctor

        public ErpInvoiceService(IRepository<ErpInvoice> erpInvoiceRepository,
            IRepository<ErpOrderAdditionalData> erpErpOrderAdditionalDataRepository,
            IRepository<Order> orderRepository
            )
        {
            _erpInvoiceRepository = erpInvoiceRepository;
            _erpErpOrderAdditionalDataRepository = erpErpOrderAdditionalDataRepository;
            _orderRepository = orderRepository;
        }

        #endregion

        #region Methods

        #region Insert/Update

        public async Task InsertErpInvoiceAsync(ErpInvoice erpInvoice)
        {
            await _erpInvoiceRepository.InsertAsync(erpInvoice);
        }

        public async Task UpdateErpInvoiceAsync(ErpInvoice erpInvoice)
        {
            await _erpInvoiceRepository.UpdateAsync(erpInvoice);
        }

        #endregion

        #region Delete

        private async Task DeleteErpInvoiceAsync(ErpInvoice erpInvoice)
        {
            await _erpInvoiceRepository.DeleteAsync(erpInvoice);
        }

        public async Task DeleteErpInvoiceByIdAsync(int id)
        {
            var erpInvoice = await GetErpInvoiceByIdAsync(id);
            if (erpInvoice != null)
            {
                await DeleteErpInvoiceAsync(erpInvoice);
            }
        }

        #endregion

        #region Read

        public async Task<ErpInvoice> GetErpInvoiceByIdAsync(int id)
        {
            if (id == 0)
                return null;

            return await _erpInvoiceRepository.GetByIdAsync(id, cache => default);
        }

        public async Task<IPagedList<ErpInvoice>> GetAllErpInvoiceAsync(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool getOnlyTotalCount = false,
            string erpOrderNumber = null,
            string documentName = null,
            DateTime? documentDateUtc = null,
            int erpAccountId = 0,
            int documentTypeId = 0,
            string erpDocumentNumber = null,
            string customOrderNumber = null,
            DateTime? postingFromDateUtc = null,
            DateTime? postingToDateUtc = null)
        {
            var erpInvoices = await _erpInvoiceRepository.GetAllPagedAsync(query =>
            {
                if (erpAccountId > 0)
                    query = query.Where(ei => ei.ErpAccountId == erpAccountId);

                if (documentTypeId > 0)
                    query = query.Where(ei => ei.DocumentTypeId == documentTypeId);

                if (!string.IsNullOrEmpty(erpOrderNumber))
                    query = query.Where(ei => ei.ErpOrderNumber.Contains(erpOrderNumber));

                if (!string.IsNullOrEmpty(erpDocumentNumber) || !string.IsNullOrEmpty(documentName))
                    query = query.Where(ei => ei.ErpDocumentNumber.Contains(erpDocumentNumber) || ei.DocumentDisplayName.Contains(documentName));

                if (documentDateUtc != null)
                    query = query.Where(ei => ei.DocumentDateUtc.Value >= documentDateUtc.Value);

                if (postingFromDateUtc != null)
                    query = query.Where(ei =>
                        ei.PostingDateUtc >= postingFromDateUtc.Value);

                if (postingToDateUtc != null)
                    query = query.Where(ei =>
                        ei.PostingDateUtc <= postingToDateUtc.Value);

                if (!string.IsNullOrEmpty(customOrderNumber))
                {
                    query = from erpInvoice in query
                            join erpOrderAdditionalData in _erpErpOrderAdditionalDataRepository.Table
                            on erpInvoice.ErpOrderNumber equals erpOrderAdditionalData.ErpOrderNumber
                            join order in _orderRepository.Table
                            on erpOrderAdditionalData.NopOrderId equals order.Id
                            where order.CustomOrderNumber == customOrderNumber
                            select erpInvoice;
                }

                query = query.OrderBy(ei => ei.Id);
                return query;

            }, pageIndex, pageSize, getOnlyTotalCount);

            return erpInvoices;
        }

        public async Task<IList<ErpInvoice>> GetErpInvoicesByErpAccountIdAsync(int erpAccountId)
        {
            if (erpAccountId == 0)
                return null;

            var erpInvoices = await _erpInvoiceRepository.GetAllPagedAsync(query =>
            {
                query = query.Where(ei => ei.ErpAccountId == erpAccountId);
                query = query.OrderBy(ei => ei.Id);
                return query;

            });

            return erpInvoices;
        }

        public async Task<IList<ErpInvoice>> GetErpInvoicesByOrderNumberAsync(string orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber))
                return null;

            var erpInvoices = await _erpInvoiceRepository.GetAllAsync(query =>
            {
                query = query.Where(ei => ei.ErpOrderNumber == orderNumber);
                query = query.OrderBy(ei => ei.Id);
                return query;

            });

            return erpInvoices;
        }

        public (int, int) GetTotalNumberOfInvoicesAndNumOfShippedItemsByErpAccountIdERPOrderNumber(int erpAccountId, string erpOrderNumber)
        {
            if (erpAccountId == 0 && string.IsNullOrEmpty(erpOrderNumber))
                return (0, 0);

            var query = _erpInvoiceRepository.Table;
            query = query.Where(x => x.ErpAccountId == erpAccountId &&
                    x.DocumentTypeId == (int)ErpDocumentType.Invoice &&
                    x.ErpOrderNumber.Equals(erpOrderNumber));

            return (query.Count(), query.Sum(x => x.ItemCount));
        }

        #endregion

        #endregion
    }
}

