using Nop.Web.Framework.Models;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.Checkout
{
    public record ErpOnePageCheckoutModel : BaseNopModel
    {
        public bool IsQuoteOrder { get; set; }
        public bool ShippingRequired { get; set; }
        public bool DisableBillingAddressCheckoutStep { get; set; }
        public bool DisplayCaptcha { get; set; }
        public bool IsReCaptchaV3 { get; set; }
        public string ReCaptchaPublicKey { get; set; }

        public bool AllowErpShipToAddressEdit { get; set; }
        public CheckoutErpBillingAddressModel CheckoutErpBillingAddress { get; set; }
        public CheckoutErpShippingAddressModel CheckoutErpShipToAddress { get; set; }
    }
}
