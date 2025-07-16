namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpAccountCustomerRegistrationPhysicalTradingAddress : ErpBaseEntity
{
    public int FormId { get; set; }
    public string FullName { get; set; }
    public string Surname { get; set; }
    public int PhysicalTradingAddressId { get; set; }
}
