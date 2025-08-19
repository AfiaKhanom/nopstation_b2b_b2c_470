using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.AjaxCart;

public class AjaxCartSettings : ISettings
{
    public bool EnableAjaxCartPlugin { get; set; }

    public string ShoppingCartFormSelector { get; set; }

    public string ShoppingCartInputQuantitySelector { get; set; }

    public string ShoppingCartSelectQuantitySelector { get; set; }

    public string ApplyDiscountCouponCodeButtonSelector { get; set; }

    public string ApplyDiscountCouponCodeInputSelector { get; set; }

    public string ApplyGiftCardCouponCodeButtonSelector { get; set; }

    public string ApplyGiftCardCouponCodeInputSelector { get; set; }

    public string OrderSummaryContainerSelector { get; set; }

    public string UpdateShoppingCartButtonSelector { get; set; }
}
