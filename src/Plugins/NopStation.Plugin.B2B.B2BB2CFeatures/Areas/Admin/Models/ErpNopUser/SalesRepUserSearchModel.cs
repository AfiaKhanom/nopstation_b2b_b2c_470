using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpNopUser
{
    public record SalesRepUserSearchModel: BaseSearchModel
    {
        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeaturesErpNopUser.Fields.SearchERPAccountNumber")]
        public string SearchERPAccountNumber { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeaturesErpNopUser.Fields.SearchERPAccountName")]
        public string SearchERPAccountName { get; set; }
        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeaturesErpNopUser.Fields.SearchCustomerFullName")]
        public string SearchCustomerFullName { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeaturesErpNopUser.Fields.SearchCustomerEmail")]
        public string SearchCustomerEmail { get; set; }
    }
}
