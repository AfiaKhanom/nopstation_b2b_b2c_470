using Nop.Web.Framework.Models;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.Checkout
{
    public record ErpCheckoutCompletedModel : BaseNopModel
    {
        public string ERPOrderNumber { get; set; }

        public bool IsErpIntegrationSuccess { get; set; }

        public string IntegrationError { get; set; }

        public bool IsQuoteOrder { get; set; }
        public bool IsB2COrder { get; set; }

        public bool DisplayOverSpendWarningText { get; set; }

        public string OverSpendWarningText { get; set; }
    }
}
