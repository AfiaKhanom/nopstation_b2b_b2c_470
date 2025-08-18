using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace Nop.Plugin.Payments.B2BCustomerAccount.Controllers;

public class HandleLiveErpCallController : BasePublicController
{
    #region Fields

    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IProductService _productService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IErpAccountService _erpAccountService;
    private readonly ILocalizationService _localizationService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginService;

    #endregion

    #region Ctor

    public HandleLiveErpCallController(IGenericAttributeService genericAttributeService,
        IStoreContext storeContext,
        IWorkContext workContext,
        IShoppingCartService shoppingCartService,
        IProductService productService,
        B2BB2CFeaturesSettings b2BCustomerAccountSettings,
        IDateTimeHelper dateTimeHelper,
        IErpAccountService erpAccountService,
        ILocalizationService localizationService,
        IErpIntegrationPluginManager erpIntegrationPluginService)
    {
        _genericAttributeService = genericAttributeService;
        _storeContext = storeContext;
        _workContext = workContext;
        _shoppingCartService = shoppingCartService;
        _productService = productService;
        _b2BB2CFeaturesSettings = b2BCustomerAccountSettings;
        _dateTimeHelper = dateTimeHelper;
        _erpAccountService = erpAccountService;
        _localizationService = localizationService;
        _erpIntegrationPluginService = erpIntegrationPluginService;
    }

    #endregion

    #region Methods

    protected async Task<string> UpdateCartItemProductLivePrice()
    {
        var currStore = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, currStore.Id);

        if (!cart.Any())
            return string.Empty;

        var productIds = cart.Select(x => x.ProductId).ToList();
        var products = await _productService.GetProductsByIdsAsync(productIds.ToArray());

        return string.Join(',', products.Select(x => x.Sku));
    }

    public async Task<IActionResult> CurrentCartItemsLiveStockCheck()
    {
        if (!_b2BB2CFeaturesSettings.EnableLiveStockChecks)
        {
            return new NullJsonResult();
        }

        var currStore = await _storeContext.GetCurrentStoreAsync();
        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var b2BAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(currCustomer.Id);

        if (b2BAccount == null)
        {
            return new NullJsonResult();
        }

        var cart = await _shoppingCartService.GetShoppingCartAsync(currCustomer, ShoppingCartType.ShoppingCart, currStore.Id);

        if (!cart.Any())
            return new NullJsonResult();

        var productIds = cart.Select(x => x.ProductId).ToList();
        var products = await _productService.GetProductsByIdsAsync(productIds.ToArray());

        var erpIntegrationPlugin = await _erpIntegrationPluginService.LoadActiveERPIntegrationPlugin(ErpSyncLevel.Stock);

        if (erpIntegrationPlugin is null)
        {
            return Json(new
            {
                success = false,
                message = "No active erp integration method found."
            });
        }

        await erpIntegrationPlugin.ProductListLiveStockDataAsync(b2BAccount, products, _productService);

        return Json(new
        {
            success = true,
            message = "Product list live stock sync successful."
        });
    }

    public async Task<IActionResult> CurrentCartItemsLivePriceCheck()
    {
        if (!_b2BB2CFeaturesSettings.EnableLivePriceChecks)
        {
            return new NullJsonResult();
        }

        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var currStore = await _storeContext.GetCurrentStoreAsync();
        var b2BAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(currCustomer.Id);

        if (b2BAccount == null)
        {
            return new NullJsonResult();
        }

        var updatedPriceProductSkus = string.Empty;

        if (!b2BAccount.LastPriceRefresh.HasValue)
        {
            updatedPriceProductSkus = await UpdateCartItemProductLivePrice();
        }
        else
        {
            var priceUpdateOnLocalTime = _dateTimeHelper.ConvertToUtcTime(b2BAccount.LastPriceRefresh.Value, DateTimeKind.Utc);

            if (priceUpdateOnLocalTime < DateTime.UtcNow)
            {
                updatedPriceProductSkus = await UpdateCartItemProductLivePrice();
            }
        }

        await _genericAttributeService.SaveAttributeAsync(currCustomer, B2BB2CFeaturesDefaults.CartItemsLivePriceSyncProcessing, false, currStore.Id);

        if (!string.IsNullOrEmpty(updatedPriceProductSkus))
        {
            var msg = string.Format(await _localizationService.GetResourceAsync("Plugins.Payment.B2BCustomerAccount.LivePriceSync.CartItemPriceUpdated"), updatedPriceProductSkus);

            return Json(new
            {
                success = true,
                data = updatedPriceProductSkus,
                message = msg
            });
        }

        return new NullJsonResult();
    }

    #endregion
}