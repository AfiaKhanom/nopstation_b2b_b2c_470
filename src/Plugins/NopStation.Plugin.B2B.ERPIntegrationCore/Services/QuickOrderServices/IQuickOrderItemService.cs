using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services.QuickOrderServices
{
    public interface IQuickOrderItemService
    {
        Task<IPagedList<QuickOrderItem>> GetAllQuickOrderItemsPagedAsync(string productSku = null, int quickOrderTemplateId = 0, int pageIndex = 0, int pageSize = int.MaxValue);

        Task<QuickOrderItem> GetQuickOrderItemByIdAsync(int itemId);
        Task<ProductAttribute> GetProductAttributeByName(string name);

        Task<ProductAttributeValue> GetAttributeValueByNameAsync(string name);

        Task<ProductAttributeMapping> GetProductAttributeMapping(int attributeId);
        Task<QuickOrderItem> GetQuickOrderItemByTemplateIdAndSkuAsync(int templateId, string sku, string attributeXml = "");

        Task InsertQuickOrderItemAsync(QuickOrderItem quickOrderItem);

        Task UpdateQuickOrderItemAsync(QuickOrderItem quickOrderItem);

        Task DeleteQuickOrderItemAsync(QuickOrderItem quickOrderItem);

        Task<int> CountTotalQuickOrderItemByTemplateIdAsync(int templateId);

        Task<IList<QuickOrderItem>> GetAllQuickOrderItemsAsync(int quickOrderTemplateId = 0);

        Task<bool> ClearCartAsync(int customerId);




    }
}
