using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.AdvanceCart.Models;

public record AdvanceCartProductDetailsModel : BaseNopEntityModel
{
    public bool EnableBuyNowButton { get; set; }
}
