using Nop.Web.Framework.Models;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models
{
    public record ErpSalesOrgWarehouseSearchModel : BaseSearchModel
    {
        public int ErpSalesOrgId { get; set; }
    }
}
