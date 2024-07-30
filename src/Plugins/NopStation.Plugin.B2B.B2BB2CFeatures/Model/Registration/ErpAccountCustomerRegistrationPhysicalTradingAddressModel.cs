using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.Registration
{
    public record ErpAccountCustomerRegistrationPhysicalTradingAddressModel
    {
        public ErpAccountCustomerRegistrationPhysicalTradingAddressModel()
        {
            PhysicalTradingAddress = new PhysicalTradingAddressModel();
        }

        public int FormId { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationPhysicalTradingAddress.Fields.FullName")]
        public string FullName { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationPhysicalTradingAddress.Fields.Surname")]
        public string Surname { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationPhysicalTradingAddress.Fields.PhysicalTradingAddress")]
        public PhysicalTradingAddressModel PhysicalTradingAddress { get; set; }
    }
}
