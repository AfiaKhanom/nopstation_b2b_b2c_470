using Nop.Web.Framework.Models;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models
{
    /// <summary>
    /// Represents a schedule task list model
    /// </summary>
    public partial record SyncTaskListModel : BasePagedListModel<SyncTaskModel>
    {

    }
}