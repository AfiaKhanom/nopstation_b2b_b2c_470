using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models
{
    /// <summary>
    /// Represents a schedule task model
    /// </summary>
    public partial record SyncTaskModel : BaseNopEntityModel
    {
        public SyncTaskModel() 
        {
            SyncLogSearchModel = new();
        }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Enabled")]
        public bool Enabled { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.StopOnError")]
        public bool StopOnError { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.LastStart")]
        public string? LastStartUtc { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.LastEnd")]
        public string? LastEndUtc { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.LastSuccess")]
        public string? LastSuccessUtc { get; set; }

        public List<SyncTaskDaySlotModel> DayOfWeekSlots { get; set; } = new List<SyncTaskDaySlotModel> { new() };

        public SyncLogSearchModel SyncLogSearchModel { get; set; }
    }
}