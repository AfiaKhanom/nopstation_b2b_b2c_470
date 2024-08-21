using System;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpAccount: ErpBaseEntity
{ 
    public string AccountNumber { get; set; }

    public string AccountName { get; set; }

    public int ErpSalesOrgId { get; set; }

    public int? BillingAddressId { get; set; }

    public string BillingSuburb { get; set; }

    public string VatNumber { get; set; }

    public decimal CreditLimit { get; set; }

    public decimal CreditLimitAvailable { get; set; }

    public decimal CurrentBalance { get; set; }

    public decimal? LastPaymentAmount { get; set; }

    public DateTime? LastPaymentDate { get; set; }

    public bool AllowOverspend { get; set; }

    public string PreFilterFacets { get; set; }

    public string PaymentTypeCode { get; set; }

    public bool OverrideBackOrderingConfigSetting { get; set; }

    public bool AllowAccountsBackOrdering { get; set; }

    public bool OverrideAddressEditOnCheckoutConfigSetting { get; set; }

    public bool AllowAccountsAddressEditOnCheckout { get; set; }

    public bool OverrideStockDisplayFormatConfigSetting { get; set; }

    public int ErpAccountStatusTypeId { get; set; }

    public ErpAccountStatusType ErpAccountStatusType
    {
        get => (ErpAccountStatusType)ErpAccountStatusTypeId;
        set => ErpAccountStatusTypeId = (int)value;
    }

    public DateTime? LastErpAccountSyncDate { get; set; }

    public DateTime? LastPriceRefresh { get; set; }

    public int? B2BPriceGroupCodeId { get; set; }

    public decimal? TotalSavingsForthisYear { get; set; }

    public decimal? TotalSavingsForAllTime { get; set; }

    public DateTime? TotalSavingsForthisYearUpdatedOnUtc { get; set; }

    public DateTime? TotalSavingsForAllTimeUpdatedOnUtc { get; set; }

    public DateTime? LastTimeOrderSyncOnUtc { get; set; }

    public bool IsDefaultPaymentAccount { get; set; }

    public int StockDisplayFormatTypeId { get; set; }

    public decimal PercentageOfStockAllowed { get; set; }

    public ErpSalesOrg ErpSalesOrg { get; set; }
}
