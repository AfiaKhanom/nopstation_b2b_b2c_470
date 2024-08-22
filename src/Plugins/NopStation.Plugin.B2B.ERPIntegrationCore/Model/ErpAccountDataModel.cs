using System;
using System.Collections.Generic;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Model;

public class ErpAccountDataModel
{
    public string AccountNumber { get; set; }
    public string AccountName { get; set; }
    public string ErpSalesOrgCode { get; set; }
    public string BillingSuburb { get; set; }
    public string BillingName { get; set; }
    public string VatNumber { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal CreditLimitUsed { get; set; }
    public decimal CreditLimitAvailable { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool AllowOverspend { get; set; }
    public string PreFilterFacets { get; set; }
    public string PaymentTypeCode { get; set; }
    public string PriceGroupCode { get; set; }
    public string CreditLimitStr { get; set; }
    public string CreditLimitUsedStr { get; set; }
    public string CreditLimitAvailableStr { get; set; }
    public string CurrentBalanceStr { get; set; }
    public decimal? PercentageOfStockAllowed { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string Address3 { get; set; }
    public string StateProvince { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string ZipPostalCode { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string DeliveryRoute { get; set; }
    public string CompanyNo { get; set; }
    public bool IsActive { get; set; }
    public List<KeyValuePair<string, string>> ErpAccountAttributes { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? UpdatedOnUtc { get; set; }
    public bool OverrideBackOrderingConfigSetting { get; set; }
    public bool AllowAccountsBackOrdering { get; set; }
    public bool AllowAccountsAddressEditOnCheckout { get; set; }
}
