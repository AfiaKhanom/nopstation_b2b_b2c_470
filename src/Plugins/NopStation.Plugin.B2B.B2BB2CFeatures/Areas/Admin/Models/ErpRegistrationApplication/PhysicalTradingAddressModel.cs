using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpRegistrationApplication
{
    public partial record PhysicalTradingAddressModel : BaseNopEntityModel
    {
        public PhysicalTradingAddressModel()
        {
            PhysicalTradingAddress = new AddressModel();
        }

        public int FormId { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.PhysicalTradingAddress.Fields.FullName")]
        public string FullName { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.PhysicalTradingAddress.Fields.Surname")]
        public string Surname { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.Admin.ErpRegistrationApplication.PhysicalTradingAddress.Fields.PhysicalTradingAddress")]
        public AddressModel PhysicalTradingAddress { get; set; }
    }
}
