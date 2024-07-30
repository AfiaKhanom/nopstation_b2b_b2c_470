using Nop.Web.Framework.Models;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.Checkout
{
    public record CheckoutErpBillingAddressModel : BaseNopModel
    {
        public CheckoutErpBillingAddressModel()
        {
            ErpBillingAddress = new ErpBillingAddressModel();
        }

        public ErpBillingAddressModel ErpBillingAddress { get; set; }

        public bool IsQuoteOrder { get; set; }
        public bool ShipToSameAddress { get; set; }
        public bool ShipToSameAddressAllowed { get; set; }
    }
}
