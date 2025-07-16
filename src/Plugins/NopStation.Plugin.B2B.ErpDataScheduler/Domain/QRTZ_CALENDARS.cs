using Nop.Core;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Domain;
public class QRTZ_CALENDARS : BaseEntity
{
    public string SCHED_NAME { get; set; }
    public string CALENDAR_NAME { get; set; }
    public byte[] CALENDAR { get; set; }
}
