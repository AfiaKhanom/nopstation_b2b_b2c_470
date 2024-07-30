using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.Registration
{
    public record ErpAccountCustomerRegistrationBankingDetailsModel
    {
        public ErpAccountCustomerRegistrationBankingDetailsModel()
        {
        }

        public int FormId { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationBankingDetails.Fields.NameOfBanker")]
        public string NameOfBanker { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationBankingDetails.Fields.AccountName")]
        public string AccountName { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationBankingDetails.Fields.AccountNumber")]
        public string AccountNumber { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationBankingDetails.Fields.BranchCode")]
        public string BranchCode { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpAccountCustomerRegistrationBankingDetails.Fields.Branch")]
        public string Branch { get; set; }
    }
}
