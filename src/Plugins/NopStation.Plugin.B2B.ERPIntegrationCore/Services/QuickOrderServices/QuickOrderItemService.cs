using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services.QuickOrderServices
{
    public class QuickOrderItemService : IQuickOrderItemService
    {
        private readonly IRepository<QuickOrderItem> _quickOrderItemRepository;
        private readonly IQuickOrderTemplateService _quickOrderTemplateService;
        private readonly IRepository<ShoppingCartItem> _shoppingCartRepository;
        private readonly IRepository<ProductAttribute> _productAttributeRepository;
        private readonly IRepository<ProductAttributeValue> _productAttributeValueRepository;
        private readonly IRepository<ProductAttributeMapping> _productAttributeMapping;

        public QuickOrderItemService(IRepository<QuickOrderItem> quickOrderItemRepository,
            IQuickOrderTemplateService quickOrderTemplateService,
            IRepository<ShoppingCartItem> shoppingCartRepository,
            IRepository<ProductAttribute> productAttributeRepository,
            IRepository<ProductAttributeValue> productAttributeValueRepository,
            IRepository<ProductAttributeMapping> productAttributeMapping)
        {
            _quickOrderItemRepository = quickOrderItemRepository;
            _quickOrderTemplateService = quickOrderTemplateService;
            _shoppingCartRepository = shoppingCartRepository;
            _productAttributeRepository = productAttributeRepository;
            _productAttributeValueRepository = productAttributeValueRepository;
            _productAttributeMapping = productAttributeMapping;
        }

        public async Task<IPagedList<QuickOrderItem>> GetAllQuickOrderItemsPagedAsync(string productSku = null, int quickOrderTemplateId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _quickOrderItemRepository.Table;

            if (!string.IsNullOrWhiteSpace(productSku))
                query = query.Where(q => q.ProductSku.Contains(productSku));

            if (quickOrderTemplateId > 0)
                query = query.Where(q => q.QuickOrderTemplateId == quickOrderTemplateId);

            query = query.OrderByDescending(q => q.Id);


            var result = query.ToPagedListAsync(pageIndex, pageSize);
            return await result;
        }

        public async Task<QuickOrderItem> GetQuickOrderItemByIdAsync(int itemId)
        {
            if (itemId == 0)
                return null;

            return await _quickOrderItemRepository.GetByIdAsync(itemId);
        }

        public async Task<QuickOrderItem> GetQuickOrderItemByTemplateIdAndSkuAsync(int templateId, string sku, string attributeXml = "")
        {
            if (templateId == 0 || string.IsNullOrEmpty(sku))
                return null;

            var query = _quickOrderItemRepository.Table.Where(x => x.QuickOrderTemplateId == templateId && x.ProductSku.Equals(sku) && x.AttributesXml.Equals(attributeXml));

            return await query.FirstOrDefaultAsync();
        }

        public async Task InsertQuickOrderItemAsync(QuickOrderItem quickOrderItem)
        {
            if (quickOrderItem == null)
                throw new ArgumentNullException(nameof(quickOrderItem));

            await _quickOrderItemRepository.InsertAsync(quickOrderItem);

            var quickOrder = await _quickOrderTemplateService.GetQuickOrderTemplateByIdAsync(quickOrderItem.QuickOrderTemplateId);
            quickOrder.LastPriceCalculatedOnUtc = DateTime.UtcNow; // making it null so that it will get calculated next time
            quickOrder.EditedOnUtc = DateTime.UtcNow;
            await _quickOrderTemplateService.UpdateQuickOrderTemplateAsync(quickOrder);
        }

        public async Task UpdateQuickOrderItemAsync(QuickOrderItem quickOrderItem)
        {
            if (quickOrderItem == null)
                throw new ArgumentNullException(nameof(quickOrderItem));

            await _quickOrderItemRepository.UpdateAsync(quickOrderItem);

            var quickOrder = quickOrderItem.QuickOrderTemplate;
            quickOrder.LastPriceCalculatedOnUtc = DateTime.UtcNow; // making it null so that it will get calculated next time
            quickOrder.EditedOnUtc = DateTime.UtcNow;
            await _quickOrderTemplateService.UpdateQuickOrderTemplateAsync(quickOrder);
        }

        public async Task DeleteQuickOrderItemAsync(QuickOrderItem quickOrderItem)
        {
            if (quickOrderItem == null)
                throw new ArgumentNullException(nameof(quickOrderItem));

            var quickOrder = await _quickOrderTemplateService.GetQuickOrderTemplateByIdAsync(quickOrderItem.QuickOrderTemplateId);

            await _quickOrderItemRepository.DeleteAsync(quickOrderItem);
            quickOrder.EditedOnUtc = DateTime.UtcNow;
            quickOrder.LastPriceCalculatedOnUtc = DateTime.UtcNow;
            await _quickOrderTemplateService.UpdateQuickOrderTemplateAsync(quickOrder);
        }

        public async Task<int> CountTotalQuickOrderItemByTemplateIdAsync(int templateId)
        {
            if (templateId <= 0)
                return 0;

            var query = _quickOrderItemRepository.Table;
            var count = await query.CountAsync(q => q.QuickOrderTemplateId == templateId);

            return count;
        }

        public async Task<IList<QuickOrderItem>> GetAllQuickOrderItemsAsync(int quickOrderTemplateId = 0)
        {
            var query = _quickOrderItemRepository.Table;

            query = query.Where(q => q.QuickOrderTemplateId == quickOrderTemplateId);

            return await query.ToListAsync();
        }

        #region ClearCart
        public async Task<bool> ClearCartAsync(int customerId)
        {
            var query = _shoppingCartRepository.Table;
            query = query.Where(key => key.CustomerId == customerId);
            var items = await query.ToListAsync();

            if (items.Any())
            {
                await _shoppingCartRepository.DeleteAsync(items);

                return true;

            }

            return false;
        }
        #endregion


        public async Task<ProductAttribute> GetProductAttributeByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var query = _productAttributeRepository.Table;

            query = query.Where(x => x.Name == name);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<ProductAttributeValue> GetAttributeValueByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            var query = _productAttributeValueRepository.Table;

            query = query.Where(x => x.Name == name);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<ProductAttributeMapping> GetProductAttributeMapping(int attributeId)
        {
            var query = _productAttributeMapping.Table;
            query = query.Where(x=> x.ProductAttributeId== attributeId);

            return await query.FirstOrDefaultAsync();
        }
    }
}
