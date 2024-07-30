using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpRegistrationApplication
{
    public partial record TradeReferencesModel : BaseNopEntityModel
    {
        public TradeReferencesModel()
        {
        }

        public int FormId { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.TradeReferences.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.TradeReferences.Fields.Telephone")]
        public string Telephone { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.TradeReferences.Fields.Amount")]
        public decimal Amount { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.TradeReferences.Fields.Terms")]
        public string Terms { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.TradeReferences.Fields.HowLong")]
        public string HowLong { get; set; }
    }
}
