using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.AdvanceCart;

public partial class AdvanceCartSettings : ISettings
{
    public bool EnableAdvanceCartPlugin { get; set; }

    public bool EnableAdvanceFlyoutCart { get; set; }

    public bool EnableBuyNowButton { get; set; }

    public bool AllowCustomersToSelectQuantityFromProductBox { get; set; }

    public bool OpenQuickViewIfRedirectToDetailsPageRequired { get; set; }

    public bool EnableAddedToCartNotificationPopup { get; set; }

    public int NotificationPopupProductImageSize { get; set; }

    public string ProductBoxAdditionalInfoWidgetZone { get; set; }

    public string ProductBoxAddToCartButtonSelector { get; set; }

    public string ProductDetailsAdditionalInfoWidgetZone { get; set; }

    public string ProductBoxSelector { get; set; }

    public string TopCartSelector { get; set; }

    public string FlyoutCartSelector { get; set; }
}
