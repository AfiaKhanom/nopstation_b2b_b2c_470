using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.QuickOrderModels.QuickOrderItems;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories.QuickOrder
{
    public interface IQuickOrderItemModelFactory
    {
        Task<string> GetValidationResultAsync(Product product, int quantity, string attributeXml);

        Task<QuickOrderItemListModel> PrepareQuickOrderItemListModelAsync(QuickOrderItemSearchModel searchModel);

        Task<QuickOrderItemModel> PrepareQuickOrderItemModelAsync(QuickOrderItemModel model, QuickOrderItem quickOrderItem);

        Task<QuickOrderItemSearchModel> PrepareQuickOrderItemSearchModelAsync(QuickOrderItemSearchModel searchModel);

        Task<IList<string>> ImportQuickOrderItemsFromXlsxAsync(int templateId, Stream stream);

        Task<bool> CreateQuickOrderItemsFromShoppingCartAsync(int templateId);

        Task<bool> CreateQuickOrderItemsFromOrderAsync(int templateId, int orderId);

        Task<string> AddToCartAllItemByTemplateAsync(QuickOrderTemplate quickOrderTemplate);
    }
}