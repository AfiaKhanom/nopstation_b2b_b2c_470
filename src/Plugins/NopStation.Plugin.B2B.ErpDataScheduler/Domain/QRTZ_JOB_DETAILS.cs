using Nop.Core;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Domain;
public class QRTZ_JOB_DETAILS : BaseEntity
{
    public string SCHED_NAME { get; set; }
    public string JOB_NAME { get; set; }
    public string JOB_GROUP { get; set; }
    public string DESCRIPTION { get; set; }
    public string JOB_CLASS_NAME { get; set; }
    public string IS_DURABLE { get; set; }
    public string IS_NONCONCURRENT { get; set; }
    public string IS_UPDATE_DATA { get; set; }
    public string REQUESTS_RECOVERY { get; set; }
    public byte[] JOB_DATA { get; set; }
}
