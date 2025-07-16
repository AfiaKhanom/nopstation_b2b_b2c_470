using Nop.Web.Framework.Models;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models;
public record QuartzJobDetailModel : BaseNopEntityModel
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
