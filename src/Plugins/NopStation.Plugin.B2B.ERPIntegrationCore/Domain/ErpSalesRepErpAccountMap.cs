using Nop.Core;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpSalesRepErpAccountMap : BaseEntity
{
    public int ErpSalesRepId { get; set; }

    public int ErpAccountId { get; set; }
}
