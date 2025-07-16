using Nop.Core;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Domain;
public class QRTZ_FIRED_TRIGGERS : BaseEntity
{
    #region Properties

    public string SCHED_NAME { get; set; }
    public string ENTRY_ID { get; set; }
    public string TRIGGER_NAME { get; set; }
    public string TRIGGER_GROUP { get; set; }
    public string INSTANCE_NAME { get; set; }
    public long FIRED_TIME { get; set; }
    public long SCHED_TIME { get; set; }
    public int PRIORITY { get; set; }
    public string STATE { get; set; }
    public string JOB_NAME { get; set; }
    public string JOB_GROUP { get; set; }
    public bool? IS_NONCONCURRENT { get; set; }
    public bool? REQUESTS_RECOVERY { get; set; }

    #endregion
}
