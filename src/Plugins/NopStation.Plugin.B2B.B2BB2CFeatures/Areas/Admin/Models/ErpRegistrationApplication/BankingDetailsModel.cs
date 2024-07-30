using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpRegistrationApplication
{
    public partial record BankingDetailsModel : BaseNopEntityModel
    {
        public BankingDetailsModel()
        {
        }

        public int FormId { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.BankingDetails.Fields.NameOfBanker")]
        public string NameOfBanker { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.BankingDetails.Fields.AccountName")]
        public string AccountName { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.BankingDetails.Fields.AccountNumber")]
        public string AccountNumber { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.BankingDetails.Fields.BranchCode")]
        public string BranchCode { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.BankingDetails.Fields.Branch")]
        public string Branch { get; set; }
    }
}
