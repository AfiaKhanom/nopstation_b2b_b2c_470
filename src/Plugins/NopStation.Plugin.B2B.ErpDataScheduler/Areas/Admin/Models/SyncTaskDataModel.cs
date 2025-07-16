namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models;

public class SyncTaskDataModel
{
    public List<SyncTaskDaySlotModel> DayOfWeekData { get; set; } = new List<SyncTaskDaySlotModel> { new() };
    public bool ContinueEditing { get; set; }
    public string QuartzJobName { get; set; }
}