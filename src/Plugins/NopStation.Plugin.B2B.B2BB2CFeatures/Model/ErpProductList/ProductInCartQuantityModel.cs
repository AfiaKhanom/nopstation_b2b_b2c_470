using Nop.Web.Framework.Models;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.ErpProductList
{
    public record ProductInCartQuantityModel : BaseNopEntityModel
    {
        public int Quantity { get; set; }
        public string QuantityInfoText { get; set; }
    }
}
