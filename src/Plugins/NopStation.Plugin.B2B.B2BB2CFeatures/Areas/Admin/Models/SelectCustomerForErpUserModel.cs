
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
public record SelectCustomerForErpUserModel : BaseNopEntityModel
{
    public int SelectedCustomerId { get; set; }
}
