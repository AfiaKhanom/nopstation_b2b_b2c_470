using Nop.Core;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Domain;
public class QRTZ_CRON_TRIGGERS : BaseEntity
{
    public string SCHED_NAME { get; set; }
    public string TRIGGER_NAME { get; set; }
    public string TRIGGER_GROUP { get; set; }
    public string CRON_EXPRESSION { get; set; }
    public string TIME_ZONE_ID { get; set; }
}
