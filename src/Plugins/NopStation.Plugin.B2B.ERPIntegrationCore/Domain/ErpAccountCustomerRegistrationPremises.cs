namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpAccountCustomerRegistrationPremises : ErpBaseEntity
{
    public int FormId { get; set; }
    public bool OwnedOrLeased { get; set; }
    public string NameOfLandlord { get; set; }
    public string AddressOfLandlord { get; set; }
    public string EmailOfLandlord { get; set; }
    public string TelephoneNumberOfLandlord { get; set; }
}
