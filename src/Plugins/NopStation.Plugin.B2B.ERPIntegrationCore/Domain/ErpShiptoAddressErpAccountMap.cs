using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpShiptoAddressErpAccountMap : BaseEntity
{
    public int ErpAccountId { get; set; }
    public int ErpShiptoAddressId { get; set; }
    public int ErpShipToAddressCreatedByTypeId { get; set; }

    public ErpShipToAddressCreatedByType ErpShipToAddressCreatedByType
    {
        get => (ErpShipToAddressCreatedByType)ErpShipToAddressCreatedByTypeId;
        set => ErpShipToAddressCreatedByTypeId = (int)value;
    }
}
