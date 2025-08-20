using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.AjaxCart.Components;

namespace NopStation.Plugin.Widgets.AjaxCart;

public class AjaxCartPlugin : BasePlugin, IAdminMenuPlugin, INopStationPlugin, IWidgetPlugin
{
    #region Fields

    public bool HideInWidgetList => false;

    private readonly ILocalizationService _localizationService;
    private readonly IWebHelper _webHelper;
    private readonly ISettingService _settingService;
    private readonly INopStationCoreService _nopStationCoreService;
    private readonly IPermissionService _permissionService;

    #endregion

    #region Ctor

    public AjaxCartPlugin(ILocalizationService localizationService,
        IWebHelper webHelper,
        ISettingService settingService,
        INopStationCoreService nopStationCoreService,
        IPermissionService permissionService)
    {
        _localizationService = localizationService;
        _webHelper = webHelper;
        _settingService = settingService;
        _nopStationCoreService = nopStationCoreService;
        _permissionService = permissionService;
    }

    #endregion

    #region Methods

    public override string GetConfigurationPageUrl()
    {
        return _webHelper.GetStoreLocation() + "Admin/AjaxCart/Configure";
    }

    public override async Task InstallAsync()
    {
        var settings = new AjaxCartSettings
        {
            EnableAjaxCartPlugin = true,
            ApplyDiscountCouponCodeButtonSelector = "#applydiscountcouponcode",
            ApplyDiscountCouponCodeInputSelector = "#discountcouponcode",
            ApplyGiftCardCouponCodeButtonSelector = "#applygiftcardcouponcode",
            ApplyGiftCardCouponCodeInputSelector = "#giftcardcouponcode",
            ShoppingCartInputQuantitySelector = ".cart input[name*='itemquantity']",
            ShoppingCartSelectQuantitySelector = ".cart select[name*='itemquantity']",
            UpdateShoppingCartButtonSelector = "#updatecart",
            ShoppingCartFormSelector = "#shopping-cart-form",
            OrderSummaryContainerSelector = ".order-summary-content"
        };
        await _settingService.SaveSettingAsync(settings);

        await this.InstallPluginAsync(new AjaxCartPermissionProvider());
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await this.UninstallPluginAsync(new AjaxCartPermissionProvider());
        await base.UninstallAsync();
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var menu = new SiteMapNode()
        {
            Title = await _localizationService.GetResourceAsync("Admin.NopStation.AjaxCart.Menu.AjaxCart"),
            Visible = true,
            IconClass = "far fa-dot-circle",
        };

        if (await _permissionService.AuthorizeAsync(AjaxCartPermissionProvider.ManageAjaxCart))
        {
            var configurationItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.AjaxCart.Menu.Configuration"),
                Url = "~/Admin/AjaxCart/Configure",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "AjaxCart.Configuration"
            };
            menu.ChildNodes.Add(configurationItem);
        }

        if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
        {
            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/ajax-cart-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=ajax-cart",
                Visible = true,
                IconClass = "far fa-circle",
                OpenUrlInNewTab = true
            };
            menu.ChildNodes.Add(documentation);
        }

        await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        var list = new Dictionary<string, string>
        {
            ["Admin.NopStation.AjaxCart.Configuration.Fields.EnableAjaxCartPlugin"] = "Enable ajax cart plugin",
            ["Admin.NopStation.AjaxCart.Configuration.Fields.EnableAjaxCartPlugin.Hint"] = "Check to enable ajax cart plugin.",
            ["Admin.NopStation.AjaxCart.Configuration.Fields.ShoppingCartFormSelector"] = "Shopping cart form selector",
            ["Admin.NopStation.AjaxCart.Configuration.Fields.ShoppingCartFormSelector.Hint"] = "Define shopping cart form selector.",
            ["Admin.NopStation.AjaxCart.Configuration.Fields.ShoppingCartInputQuantitySelector"] = "Shopping cart input quantity selector",
            ["Admin.NopStation.AjaxCart.Configuration.Fields.ShoppingCartInputQuantitySelector.Hint"] = "Define shopping cart input quantity selector.",
            ["Admin.NopStation.AjaxCart.Configuration.Fields.ShoppingCartSelectQuantitySelector"] = "Shopping cart select quantity selector",
            ["Admin.NopStation.AjaxCart.Configuration.Fields.ShoppingCartSelectQuantitySelector.Hint"] = "Define shopping cart select quantity selector.",
            ["Admin.NopStation.AjaxCart.Configuration.Fields.ApplyDiscountCouponCodeButtonSelector"] = "\"Apply coupon\" button selector",
            ["Admin.NopStation.AjaxCart.Configuration.Fields.ApplyDiscountCouponCodeButtonSelector.Hint"] = "Define dicount coupon code apply button selector.",
            ["Admin.NopStation.AjaxCart.Configuration.Fields.ApplyDiscountCouponCodeInputSelector"] = "\"Apply coupon\" input selector",
            ["Admin.NopStation.AjaxCart.Configuration.Fields.ApplyDiscountCouponCodeInputSelector.Hint"] = "Define dicount coupon code apply input selector.",
            ["Admin.NopStation.AjaxCart.Configuration.Fields.ApplyGiftCardCouponCodeButtonSelector"] = "\"Add gift card\" button selector",
            ["Admin.NopStation.AjaxCart.Configuration.Fields.ApplyGiftCardCouponCodeButtonSelector.Hint"] = "Define gift card coupon code apply button selector.",
            ["Admin.NopStation.AjaxCart.Configuration.Fields.ApplyGiftCardCouponCodeInputSelector"] = "\"Add gift card\" input selector",
            ["Admin.NopStation.AjaxCart.Configuration.Fields.ApplyGiftCardCouponCodeInputSelector.Hint"] = "Define gift card coupon code apply input selector.",
            ["Admin.NopStation.AjaxCart.Configuration.Fields.OrderSummaryContainerSelector"] = "Order summary container selector",
            ["Admin.NopStation.AjaxCart.Configuration.Fields.OrderSummaryContainerSelector.Hint"] = "Define order summary container selector.",
            ["Admin.NopStation.AjaxCart.Configuration.Fields.UpdateShoppingCartButtonSelector"] = "\"Update shopping cart\" button selector",
            ["Admin.NopStation.AjaxCart.Configuration.Fields.UpdateShoppingCartButtonSelector.Hint"] = "Define shopping cart update button selector.",
            ["Admin.NopStation.AjaxCart.Configuration"] = "Ajax cart settings",

            ["Admin.NopStation.AjaxCart.Menu.AjaxCart"] = "Ajax cart",
            ["Admin.NopStation.AjaxCart.Menu.Configuration"] = "Configuration",

            ["NopStation.AjaxCart.AjaxCartFailure"] = "Failed to load data",
        };

        return list.ToList();
    }

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        var widgetZones = new List<string>
        {
            PublicWidgetZones.Footer
        };

        return Task.FromResult<IList<string>>(widgetZones);
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(AjaxCartViewComponent);
    }

    #endregion
}
