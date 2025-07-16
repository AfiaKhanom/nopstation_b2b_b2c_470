namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpAccountCustomerRegistrationBankingDetails : ErpBaseEntity
{
    public int FormId { get; set; }
    public string NameOfBanker { get; set; }
    public string AccountName { get; set; }
    public string AccountNumber { get; set; }
    public string BranchCode { get; set; }
    public string Branch { get; set; }
}
