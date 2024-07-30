using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models
{
    public record ErpSpecialPriceSearchModel : BaseSearchModel
    {
        public int ProductId { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSpecialPrice.Fields.SearchErpAccount")]
        public int SearchErpAccountId{ get; set; }
    }
}
