namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpAccountCustomerRegistrationForm : ErpBaseEntity
{
    public string FullRegisteredName { get; set; }
    public string RegistrationNumber { get; set; }
    public string VatNumber { get; set; }
    public string TelephoneNumber1 { get; set; }
    public string TelephoneNumber2 { get; set; }
    public string TelefaxNumber { get; set; }
    public string AccountsContactPersonNameSurname { get; set; }
    public string AccountsEmail { get; set; }
    public string AccountsTelephoneNumber { get; set; }
    public string AccountsCellphoneNumber { get; set; }
    public string BuyerContactPersonNameSurname { get; set; }
    public string BuyerEmail { get; set; }
    public string NatureOfBusiness { get; set; }
    public int RegisteredOfficeAddressId { get; set; }
    public string TypeOfBusiness { get; set; }
    public decimal EstimatePurchasesPerMonthZAR { get; set; }
    public bool CreditLimitRequired { get; set; }
    public bool IsApproved { get; set; }
}
