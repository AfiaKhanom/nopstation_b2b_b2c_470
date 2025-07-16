using Nop.Core;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Domain;
public class QRTZ_SCHEDULER_STATE : BaseEntity
{
    public string SCHED_NAME { get; set; }
    public string INSTANCE_NAME { get; set; }
    public long LAST_CHECKIN_TIME { get; set; }
    public long CHECKIN_INTERVAL { get; set; }
}
