using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;

public record ErpNopUserAccountMapModel : BaseNopEntityModel
{
    #region Ctor

    public ErpNopUserAccountMapModel()
    {

    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserAccountMap.Field.ErpAccountId")]
    public int ErpAccountId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserAccountMap.Field.ErpAccountId")]
    public string ErpAccountNumber { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserAccountMap.Field.ErpUserId")]
    public int ErpUserId { get; set; }

    #endregion
}
