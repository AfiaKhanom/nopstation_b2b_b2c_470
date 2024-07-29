using System.Collections.Generic;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Model
{
    public class ErpAccountDataModel
    {
        public bool IsActive { get; set; }
        public string ErpSalesOrgName { get; set; }
        public string PaymentTypeCode { get; set; }
        public string AccNo { get; set; }
        public string Name { get; set; }
        public string Branch { get; set; }
        public string Notes { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string Province { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string TelNo { get; set; }
        public string EMail { get; set; }
        public string EMail1 { get; set; }
        public string DelName { get; set; }
        public string DelInstruc1 { get; set; }
        public string DelInstruc2 { get; set; }
        public string DelInstruc3 { get; set; }
        public string CompanyNo { get; set; }
        public string PrefilterFacets { get; set; }
        public string VatNumber { get; set; } 
        public string PriceGroupCode { get; set; }
        public decimal CreditLimit { get; set; }
        public decimal? CreditLimitUsed { get; set; }
        public decimal? CreditLimitAvailable { get; set; }
        public decimal? Balance { get; set; }   
        public string CreditLimitUsedStr { get; set; }
        public string CreditLimitAvailableStr { get; set; }
        public string BalanceStr { get; set; }
        public bool? AllowSwitchSalesOrg { get; set; }
        public bool? AllowOverspend { get; set; }
        public List<KeyValuePair<string, string>> Attributes { get; set; }
        public string CreditRepresentativeGroup { get; set; }
        public decimal? PercentageOfStockAllowedForCustomer { get; set; }
        public List<ErpShipToAddress> ShipToAddresses { get; set; }  
    }
}
