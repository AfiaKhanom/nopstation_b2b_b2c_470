using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.AdvanceCart.Areas.Admin.Models;

public partial record ConfigurationModel : BaseNopModel, ISettingsModel
{
    [NopResourceDisplayName("Admin.NopStation.AdvanceCart.Configuration.Fields.EnableAdvanceCartPlugin")]
    public bool EnableAdvanceCartPlugin { get; set; }
    public bool EnableAdvanceCartPlugin_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdvanceCart.Configuration.Fields.EnableAdvanceFlyoutCart")]
    public bool EnableAdvanceFlyoutCart { get; set; }
    public bool EnableAdvanceFlyoutCart_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdvanceCart.Configuration.Fields.EnableBuyNowButton")]
    public bool EnableBuyNowButton { get; set; }
    public bool EnableBuyNowButton_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdvanceCart.Configuration.Fields.AllowCustomersToSelectQuantityFromProductBox")]
    public bool AllowCustomersToSelectQuantityFromProductBox { get; set; }
    public bool AllowCustomersToSelectQuantityFromProductBox_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdvanceCart.Configuration.Fields.OpenQuickViewIfRedirectToDetailsPageRequired")]
    public bool OpenQuickViewIfRedirectToDetailsPageRequired { get; set; }
    public bool OpenQuickViewIfRedirectToDetailsPageRequired_OverrideForStore { get; set; }
    public bool QuickViewPluginActivated { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdvanceCart.Configuration.Fields.EnableAddedToCartNotificationPopup")]
    public bool EnableAddedToCartNotificationPopup { get; set; }
    public bool EnableAddedToCartNotificationPopup_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdvanceCart.Configuration.Fields.NotificationPopupProductImageSize")]
    public int NotificationPopupProductImageSize { get; set; }
    public bool NotificationPopupProductImageSize_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdvanceCart.Configuration.Fields.ProductBoxAdditionalInfoWidgetZone")]
    public string ProductBoxAdditionalInfoWidgetZone { get; set; }
    public bool ProductBoxAdditionalInfoWidgetZone_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdvanceCart.Configuration.Fields.ProductBoxAddToCartButtonSelector")]
    public string ProductBoxAddToCartButtonSelector { get; set; }
    public bool ProductBoxAddToCartButtonSelector_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdvanceCart.Configuration.Fields.ProductDetailsAdditionalInfoWidgetZone")]
    public string ProductDetailsAdditionalInfoWidgetZone { get; set; }
    public bool ProductDetailsAdditionalInfoWidgetZone_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdvanceCart.Configuration.Fields.TopCartSelector")]
    public string TopCartSelector { get; set; }
    public bool TopCartSelector_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdvanceCart.Configuration.Fields.FlyoutCartSelector")]
    public string FlyoutCartSelector { get; set; }
    public bool FlyoutCartSelector_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AdvanceCart.Configuration.Fields.ProductBoxSelector")]
    public string ProductBoxSelector { get; set; }
    public bool ProductBoxSelector_OverrideForStore { get; set; }

    public int ActiveStoreScopeConfiguration { get; set; }
}
