using Nop.Web.Framework.Models;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpActivityLogs;

/// <summary>
/// Represents an activity log list model
/// </summary>
public partial record ErpActivityLogsListModel : BasePagedListModel<ErpActivityLogsModel>
{
}