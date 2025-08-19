using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.AjaxCart.Areas.Admin.Models;

public partial record ConfigurationModel : BaseNopModel, ISettingsModel
{
    [NopResourceDisplayName("Admin.NopStation.AjaxCart.Configuration.Fields.EnableAjaxCartPlugin")]
    public bool EnableAjaxCartPlugin { get; set; }
    public bool EnableAjaxCartPlugin_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AjaxCart.Configuration.Fields.ShoppingCartFormSelector")]
    public string ShoppingCartFormSelector { get; set; }
    public bool ShoppingCartFormSelector_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AjaxCart.Configuration.Fields.ShoppingCartInputQuantitySelector")]
    public string ShoppingCartInputQuantitySelector { get; set; }
    public bool ShoppingCartInputQuantitySelector_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AjaxCart.Configuration.Fields.ShoppingCartSelectQuantitySelector")]
    public string ShoppingCartSelectQuantitySelector { get; set; }
    public bool ShoppingCartSelectQuantitySelector_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AjaxCart.Configuration.Fields.ApplyDiscountCouponCodeButtonSelector")]
    public string ApplyDiscountCouponCodeButtonSelector { get; set; }
    public bool ApplyDiscountCouponCodeButtonSelector_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AjaxCart.Configuration.Fields.ApplyDiscountCouponCodeInputSelector")]
    public string ApplyDiscountCouponCodeInputSelector { get; set; }
    public bool ApplyDiscountCouponCodeInputSelector_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AjaxCart.Configuration.Fields.ApplyGiftCardCouponCodeButtonSelector")]
    public string ApplyGiftCardCouponCodeButtonSelector { get; set; }
    public bool ApplyGiftCardCouponCodeButtonSelector_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AjaxCart.Configuration.Fields.ApplyGiftCardCouponCodeInputSelector")]
    public string ApplyGiftCardCouponCodeInputSelector { get; set; }
    public bool ApplyGiftCardCouponCodeInputSelector_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AjaxCart.Configuration.Fields.OrderSummaryContainerSelector")]
    public string OrderSummaryContainerSelector { get; set; }
    public bool OrderSummaryContainerSelector_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.AjaxCart.Configuration.Fields.UpdateShoppingCartButtonSelector")]
    public string UpdateShoppingCartButtonSelector { get; set; }
    public bool UpdateShoppingCartButtonSelector_OverrideForStore { get; set; }

    public int ActiveStoreScopeConfiguration { get; set; }
}
