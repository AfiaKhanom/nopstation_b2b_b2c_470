namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models;

public class SyncTaskDaySlotModel
{
    public int DayOfWeek { get; set; }
    public List<SyncTaskTimeSlotModel> TimeSlots { get; set; } = new List<SyncTaskTimeSlotModel> { new() };
    public bool IsSelected { get; set; }
}