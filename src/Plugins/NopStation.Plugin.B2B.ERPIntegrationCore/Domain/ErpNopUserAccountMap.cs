using Nop.Core;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpNopUserAccountMap : BaseEntity
{
    public int ErpAccountId { get; set; }

    public int ErpUserId { get; set; }
}
