using System;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpGroupPriceCode : ErpBaseEntity
{
    public string Code { get; set; }

    public DateTime LastUpdateTime { get; set; }
}
