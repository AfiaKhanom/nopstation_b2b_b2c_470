using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpRegistrationApplication
{
    public partial record PremisesModel : BaseNopEntityModel
    {
        public PremisesModel()
        {
        }

        public int FormId { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.Premises.Fields.OwnedOrLeased")]
        public string OwnedOrLeased { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.Premises.Fields.NameOfLandlord")]
        public string NameOfLandlord { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.Premises.Fields.AddressOfLandlord")]
        public string AddressOfLandlord { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.Premises.Fields.EmailOfLandlord")]
        public string EmailOfLandlord { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.Premises.Fields.TelephoneNumberOfLandlord")]
        public string TelephoneNumberOfLandlord { get; set; }
    }
}
