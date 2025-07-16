using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Orders;
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services.QuickOrderServices;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.Customers;

public class CommonHelperService : ICommonHelperService
{
    #region Fields

    private readonly ICustomerService _customerService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IProductService _productService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IStoreContext _storeContext;
    private readonly IQuickOrderTemplateService _quickOrderTemplateService;
    private readonly IQuickOrderItemService _quickOrderItemService;
    private readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public CommonHelperService(ICustomerService customerService,
        IShoppingCartService shoppingCartService,
        IProductService productService,
        IStaticCacheManager staticCacheManager,
        IStoreContext storeContext,
        IQuickOrderTemplateService quickOrderTemplateService,
        IQuickOrderItemService quickOrderItemService,
        IWorkContext workContext)
    {
        _customerService = customerService;
        _shoppingCartService = shoppingCartService;
        _productService = productService;
        _staticCacheManager = staticCacheManager;
        _storeContext = storeContext;
        _quickOrderTemplateService = quickOrderTemplateService;
        _quickOrderItemService = quickOrderItemService;
        _workContext = workContext;
    }

    #endregion

    #region Methods

    public async Task<bool> HasB2BSalesRepRoleAsync()
    {
        var salesRepRoles = await _customerService.GetCustomerRolesAsync(await _workContext.GetCurrentCustomerAsync());
        if (!salesRepRoles.Any())
        {
            return false;
        }
        var salesRepRole = salesRepRoles.FirstOrDefault(r => r.SystemName == ERPIntegrationCoreDefaults.B2BSalesRepRoleSystemName);

        if (salesRepRole == null)
        {
            return false;
        }

        return true;
    }

    public async Task ClearUnavailableShoppingCartAndWishlistItemsBeforeImpersonation(Customer customer)
    {
        await _staticCacheManager.RemoveByPrefixAsync("nop.pres.jcarousel.");
        await _staticCacheManager.RemoveByPrefixAsync("Nop.totals.productprice.");
        await _staticCacheManager.RemoveByPrefixAsync(ERPIntegrationCoreDefaults.ErpProductPricingPrefix);

        var store = await _storeContext.GetCurrentStoreAsync();

        var shoppingCartList = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
        foreach (var sci in shoppingCartList)
        {
            var product = await _productService.GetProductByIdAsync(sci.ProductId);

            if (product == null)
            {
                await _shoppingCartService.DeleteShoppingCartItemAsync(sci);
            }
        }

        var wishList = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.Wishlist, store.Id);
        foreach (var wsh in wishList)
        {
            var product = await _productService.GetProductByIdAsync(wsh.ProductId);

            if (product == null)
            {
                await _shoppingCartService.DeleteShoppingCartItemAsync(wsh);
            }
        }

        var quickOrderTemplates = await _quickOrderTemplateService.GetAllQuickOrderTemplatesByCustomerIdAsync(customerId: customer.Id);

        foreach (var quickOrderTemplate in quickOrderTemplates)
        {
            var quickOrderItems = await _quickOrderItemService.GetAllQuickOrderItemsAsync(quickOrderTemplate.Id);

            var itemsToDelete = await quickOrderItems.WhereAwait(async item =>
            {
                var product = await _productService.GetProductBySkuAsync(item.ProductSku);
                return product == null || !product.Published || product.Deleted;
            }).ToListAsync();

            foreach (var itemToDelete in itemsToDelete)
            {
                await _quickOrderItemService.DeleteQuickOrderItemAsync(itemToDelete);
            }

            var remainingItems = await _quickOrderItemService.GetAllQuickOrderItemsAsync(quickOrderTemplate.Id);
            if (!remainingItems.Any())
            {
                await _quickOrderTemplateService.DeleteQuickOrderTemplateAsync(quickOrderTemplate);
            }
        }
    }

    #endregion
}