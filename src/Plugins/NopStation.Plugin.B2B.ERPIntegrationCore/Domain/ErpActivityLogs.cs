using System;
using Nop.Core;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpActivityLogs : BaseEntity
{
    public int ErpActivityLogTypeId { get; set; }

    public int? EntityId { get; set; }

    public string EntityName { get; set; }

    public int CustomerId { get; set; }

    public string Comment { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public virtual string IpAddress { get; set; }
}
