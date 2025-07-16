using System;
using Nop.Core;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpBaseEntity : BaseEntity
{
    public bool IsActive { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public int CreatedById { get; set; }

    public DateTime UpdatedOnUtc { get; set; }

    public int UpdatedById { get; set; }

    public bool IsDeleted { get; set; }
}
