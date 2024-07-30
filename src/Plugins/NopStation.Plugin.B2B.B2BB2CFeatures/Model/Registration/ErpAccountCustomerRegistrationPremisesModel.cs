using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.Registration
{
    public record ErpAccountCustomerRegistrationPremisesModel
    {
        public ErpAccountCustomerRegistrationPremisesModel()
        {
        }

        public int FormId { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationPremises.Fields.OwnedOrLeased")]
        public string OwnedOrLeased { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationPremises.Fields.NameOfLandlord")]
        public string NameOfLandlord { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationPremises.Fields.AddressOfLandlord")]
        public string AddressOfLandlord { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationPremises.Fields.EmailOfLandlord")]
        public string EmailOfLandlord { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationPremises.Fields.TelephoneNumberOfLandlord")]
        public string TelephoneNumberOfLandlord { get; set; }
    }
}
