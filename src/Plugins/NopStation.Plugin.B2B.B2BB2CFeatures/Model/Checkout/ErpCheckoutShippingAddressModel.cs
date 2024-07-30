using System.Collections.Generic;
using Nop.Web.Models.Checkout;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.Checkout
{
    public record ErpCheckoutShippingAddressModel
    {
        public ErpCheckoutShippingAddressModel()
        {
            Warnings = new List<string>();
            ExistingErpShipToAddresses = new List<ErpShipToAddressModelForCheckout>();
            ErpShipToAddress = new ErpShipToAddressModelForCheckout();
            PickupPoints = new List<CheckoutPickupPointModel>();
        }

        public IList<string> Warnings { get; set; }

        public IList<ErpShipToAddressModelForCheckout> ExistingErpShipToAddresses { get; set; }
        public ErpShipToAddressModelForCheckout ErpShipToAddress { get; set; }
        public bool NewAddressPreselected { get; set; }
        public bool AllowAddressEdit { get; set; }

        public IList<CheckoutPickupPointModel> PickupPoints { get; set; }
        public bool AllowPickupInStore { get; set; }
        public bool PickupInStore { get; set; }
        public bool PickupInStoreOnly { get; set; }
        public bool DisplayPickupPointsOnMap { get; set; }
        public string GoogleMapsApiKey { get; set; }
    }
}
