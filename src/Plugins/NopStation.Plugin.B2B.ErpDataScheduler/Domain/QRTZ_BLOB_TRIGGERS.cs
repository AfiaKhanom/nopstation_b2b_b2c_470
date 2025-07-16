using Nop.Core;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Domain;
public class QRTZ_BLOB_TRIGGERS : BaseEntity
{
    public string SCHED_NAME { get; set; }
    public string TRIGGER_NAME { get; set; }
    public string TRIGGER_GROUP { get; set; }
    public byte[] BLOB_DATA { get; set; }
}
