using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpNopUser : ErpBaseEntity
{
    public int NopCustomerId { get; set; }

    public int ErpAccountId { get; set; }

    public int ErpShipToAddressId { get; set; }

    public int BillingErpShipToAddressId { get; set; }

    public int ShippingErpShipToAddressId { get; set; }

    public int ErpUserTypeId { get; set; }

    public ErpUserType ErpUserType
    {
        get => (ErpUserType)ErpUserTypeId;
        set => ErpUserTypeId = (int)value;
    }
}
