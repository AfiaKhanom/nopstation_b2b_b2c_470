using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.Registration
{
    public record ErpAccountCustomerRegistrationTradeReferencesModel
    {
        public ErpAccountCustomerRegistrationTradeReferencesModel()
        {
        }

        public int FormId { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationTradeReferences.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationTradeReferences.Fields.Telephone")]
        public string Telephone { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationTradeReferences.Fields.Amount")]
        public decimal Amount { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationTradeReferences.Fields.Terms")]
        public string Terms { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationTradeReferences.Fields.HowLong")]
        public string HowLong { get; set; }
    }
}
