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
using NopStation.Plugin.Widgets.AdvanceCart.Components;

namespace NopStation.Plugin.Widgets.AdvanceCart;

public class AdvanceCartPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
{
    #region Fields

    public bool HideInWidgetList => false;

    private readonly IWebHelper _webHelper;
    private readonly INopStationCoreService _nopStationCoreService;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly AdvanceCartSettings _advanceCartSettings;

    #endregion

    #region Ctor

    public AdvanceCartPlugin(IWebHelper webHelper,
        INopStationCoreService nopStationCoreService,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        ISettingService settingService,
        AdvanceCartSettings advanceCartSettings)
    {
        _webHelper = webHelper;
        _nopStationCoreService = nopStationCoreService;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _settingService = settingService;
        _advanceCartSettings = advanceCartSettings;
    }

    #endregion

    #region Methods

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/AdvanceCart/Configure";
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone == _advanceCartSettings.ProductBoxAdditionalInfoWidgetZone)
            return typeof(AdvanceCartOverviewViewComponent);

        if (widgetZone == _advanceCartSettings.ProductDetailsAdditionalInfoWidgetZone)
            return typeof(AdvanceCartDetailsViewComponent);

        return typeof(AdvanceCartViewComponent);
    }

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string> { _advanceCartSettings.ProductBoxAdditionalInfoWidgetZone,
            _advanceCartSettings.ProductDetailsAdditionalInfoWidgetZone, PublicWidgetZones.Footer });
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var menu = new SiteMapNode()
        {
            Title = await _localizationService.GetResourceAsync("Admin.NopStation.AdvanceCart.Menu.AdvanceCart"),
            Visible = true,
            IconClass = "far fa-dot-circle",
        };

        if (await _permissionService.AuthorizeAsync(AdvanceCartPermissionProvider.ManageAdvanceCart))
        {
            var configurationItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.AdvanceCart.Menu.Configuration"),
                Url = "~/Admin/AdvanceCart/Configure",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "AdvanceCart.Configuration"
            };
            menu.ChildNodes.Add(configurationItem);
        }

        if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
        {
            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/advance-cart-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=advance-cart",
                Visible = true,
                IconClass = "far fa-circle",
                OpenUrlInNewTab = true
            };
            menu.ChildNodes.Add(documentation);
        }

        await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);
    }

    public override async Task InstallAsync()
    {
        var advanceCartSettings = new AdvanceCartSettings()
        {
            EnableAdvanceCartPlugin = true,
            EnableAdvanceFlyoutCart = true,
            AllowCustomersToSelectQuantityFromProductBox = true,
            EnableBuyNowButton = true,
            EnableAddedToCartNotificationPopup = true,
            NotificationPopupProductImageSize = 180,
            OpenQuickViewIfRedirectToDetailsPageRequired = true,
            ProductBoxAdditionalInfoWidgetZone = PublicWidgetZones.ProductBoxAddinfoAfter,
            ProductBoxAddToCartButtonSelector = ".product-item .button-2.product-box-add-to-cart-button",
            ProductDetailsAdditionalInfoWidgetZone = PublicWidgetZones.ProductDetailsAddInfo,
            TopCartSelector = ".header-links .cart-qty",
            FlyoutCartSelector = "#flyout-cart",
            ProductBoxSelector = ".product-item"
        };
        await _settingService.SaveSettingAsync(advanceCartSettings);

        await this.InstallPluginAsync(new AdvanceCartPermissionProvider());
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await this.UninstallPluginAsync(new AdvanceCartPermissionProvider());
        await base.UninstallAsync();
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        var list = new Dictionary<string, string>
        {
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.EnableAdvanceCartPlugin"] = "Enable advance cart plugin",
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.EnableAdvanceCartPlugin.Hint"] = "Determines whether to enable advance cart plugin.",
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.EnableAdvanceFlyoutCart"] = "Enable advance flyout cart",
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.EnableAdvanceFlyoutCart.Hint"] = "Determines whether to enable advance flyout cart.",
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.EnableBuyNowButton"] = "Enable \"Buy now\" button",
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.EnableBuyNowButton.Hint"] = "Determines whether to enable \"Buy now\" button.",
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.AllowCustomersToSelectQuantityFromProductBox"] = "Allow customers to select quantity from product box",
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.AllowCustomersToSelectQuantityFromProductBox.Hint"] = "Determines whether to allow customers to select quantity from product box.",
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.OpenQuickViewIfRedirectToDetailsPageRequired"] = "Open quick view if redirect to details page required",
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.OpenQuickViewIfRedirectToDetailsPageRequired.Hint"] = "Determines whether to open quick view if redirect to details page required.",
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.EnableAddedToCartNotificationPopup"] = "Enable added to cart notification popup",
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.EnableAddedToCartNotificationPopup.Hint"] = "Determines whether to enable added to cart notification popup.",
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.NotificationPopupProductImageSize"] = "Notification popup product image size",
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.NotificationPopupProductImageSize.Hint"] = "Define notification popup product image size.",
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.ProductBoxAdditionalInfoWidgetZone"] = "Product box additional info widget zone",
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.ProductBoxAdditionalInfoWidgetZone.Hint"] = "Define product box additional info widget zone.",
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.ProductBoxAddToCartButtonSelector"] = "Product box \"Add to cart\" button selector",
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.ProductBoxAddToCartButtonSelector.Hint"] = "Define product box \"Add to cart\" button selector.",
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.ProductDetailsAdditionalInfoWidgetZone"] = "Product details additional info widget zone",
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.ProductDetailsAdditionalInfoWidgetZone.Hint"] = "Define product details additional info widget zone.",
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.TopCartSelector"] = "Top cart selector",
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.TopCartSelector.Hint"] = "Define top cart selector.",
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.FlyoutCartSelector"] = "Flyout cart selector",
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.FlyoutCartSelector.Hint"] = "Define flyout cart selector.",
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.ProductBoxSelector"] = "Product box selector",
            ["Admin.NopStation.AdvanceCart.Configuration.Fields.ProductBoxSelector.Hint"] = "Define product box selector.",
            ["Admin.NopStation.AdvanceCart.Configuration"] = "Advance cart settings",

            ["Admin.NopStation.AdvanceCart.Configuration.QuickViewInatallation.Hint"] = "Make sure \"Nop-Station Quick View\" plugin is installed and enabled (<a href=\"{0}Admin/Plugin/List\">Local plugins</a> page).",

            ["Admin.Nopstation.AdvanceCart.Menu.AdvanceCart"] = "Advance cart",
            ["Admin.Nopstation.AdvanceCart.Menu.Configuration"] = "Configuration",

            ["NopStation.AdvanceCart.AdvanceCartFailure"] = "Failed to load data",
            ["NopStation.AdvanceCart.BuyNow"] = "Buy Now",
            ["NopStation.AdvanceCart.ContinueShopping"] = "Continue shopping",
            ["NopStation.AdvanceCart.ViewCart"] = "View cart",
            ["NopStation.AdvanceCart.Checkout"] = "Checkout",
            ["NopStation.AdvanceCart.AddedToCart"] = "Added to cart",
        };

        return list.ToList();
    }

    #endregion
}
