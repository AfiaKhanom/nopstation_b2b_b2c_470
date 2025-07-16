using Nop.Core;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Domain;
public class QRTZ_LOCKS : BaseEntity
{
    public string SCHED_NAME { get; set; }
    public string LOCK_NAME { get; set; }
}
