using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpActivityLogs;

/// <summary>
/// Represents an activity log type model
/// </summary>
public partial record ErpActivityLogsTypeModel : BaseNopEntityModel
{
    #region Properties

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogsTypes.Fields.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogsTypes.Fields.Enabled")]
    public bool Enabled { get; set; }

    #endregion
}