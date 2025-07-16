using Nop.Core;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Domain;
public class QRTZ_SIMPLE_TRIGGERS : BaseEntity
{
    public string SCHED_NAME { get; set; }
    public string TRIGGER_NAME { get; set; }
    public string TRIGGER_GROUP { get; set; }
    public int REPEAT_COUNT { get; set; }
    public long REPEAT_INTERVAL { get; set; }
    public int TIMES_TRIGGERED { get; set; }
}
