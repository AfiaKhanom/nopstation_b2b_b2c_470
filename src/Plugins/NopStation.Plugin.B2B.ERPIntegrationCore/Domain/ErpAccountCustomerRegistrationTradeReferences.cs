namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpAccountCustomerRegistrationTradeReferences : ErpBaseEntity
{
    public int FormId { get; set; }
    public string Name { get; set; }
    public string Telephone { get; set; }
    public decimal Amount { get; set; }
    public string Terms { get; set; }
    public string HowLong { get; set; }
}
