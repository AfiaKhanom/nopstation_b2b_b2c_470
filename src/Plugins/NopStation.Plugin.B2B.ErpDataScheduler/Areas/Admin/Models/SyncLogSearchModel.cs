using Nop.Web.Framework.Models;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models;

public partial record SyncLogSearchModel : BaseSearchModel
{
    public string? SyncTaskName { get; set; }
    public int SyncTaskId { get; set; }
}