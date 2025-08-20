using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.Widgets.AdvanceCart.Models;

namespace NopStation.Plugin.Widgets.AdvanceCart.Factories;

public interface IAdvanceCartModelFactory
{
    Task<AdvanceCartProductOverviewModel> PrepareProductOverviewQuantityModelAsync(Product product);

    Task<AdvanceCartAddedToCartModel> PrepareAdvanceCartAddedToCartModelAsync(Product product, Customer customer,
        ShoppingCartType shoppingCartType,
        int storeId,
        string attributesXml = "",
        decimal customerEnteredPrice = decimal.Zero);
}