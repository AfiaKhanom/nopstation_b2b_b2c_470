using System;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpLogs : BaseEntity
{
    public int ErpLogLevelId { get; set; }

    public int ErpSyncLevelId { get; set; }
     
    public string ShortMessage { get; set; }
     
    public string FullMessage { get; set; }
     
    public string IpAddress { get; set; }
     
    public int? CustomerId { get; set; }
     
    public string PageUrl { get; set; }
     
    public string ReferrerUrl { get; set; }
     
    public DateTime CreatedOnUtc { get; set; }
     
    public ErpLogLevel ErpLogLevel
    {
        get => (ErpLogLevel)ErpLogLevelId;
        set => ErpLogLevelId = (int)value;
    }

    public ErpSyncLevel ErpSyncLevel
    {
        get => (ErpSyncLevel)ErpSyncLevelId;
        set => ErpSyncLevelId = (int)value;
    }
}
