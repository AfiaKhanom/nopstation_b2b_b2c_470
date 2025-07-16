using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models;

public partial record SyncLogModel : BaseNopModel
{
    #region Properties

    [NopResourceDisplayName("Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.FileName")]
    public string Name { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.FileSize")]
    public string Length { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.TaskId")]
    public int TaskId { get; set; }

    #endregion
}