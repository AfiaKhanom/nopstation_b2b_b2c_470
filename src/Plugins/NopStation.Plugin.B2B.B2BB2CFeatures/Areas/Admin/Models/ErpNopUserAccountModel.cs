using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;

public record ErpNopUserAccountModel : BaseNopEntityModel
{
    #region Ctor

    public ErpNopUserAccountModel()
    {
        BillingAddress = new AddressModel();
        ErpAccountStatusTypes = new List<SelectListItem>();
        AvailableErpSalesOrgs = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.AccountNumber")]
    public string AccountNumber { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.AccountName")]
    public string AccountName { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.ErpSalesOrgId")]
    public int ErpSalesOrgId { get; set; }
    public int? BillingAddressId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.BillingAddress")]
    public AddressModel BillingAddress { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.BillingSuburb")]
    public string BillingSuburb { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.VATNumber")]
    public string VatNumber { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.CreditLimit")]
    public decimal CreditLimit { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.CreditLimitAvailable")]
    public decimal CreditLimitAvailable { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.CurrentBalance")]
    public decimal CurrentBalance { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.LastPaymentAmount")]
    public decimal? LastPaymentAmount { get; set; }

    [UIHint("DateNullable")]
    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.LastPaymentDate")]
    public DateTime? LastPaymentDate { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.AllowOverspend")]
    public bool AllowOverspend { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.PreFilterFacets")]
    public string PreFilterFacets { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.PaymentTypeCode")]
    public string PaymentTypeCode { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.OverrideBackOrderingConfigSetting")]
    public bool OverrideBackOrderingConfigSetting { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.AllowAccountsBackOrdering")]
    public bool AllowAccountsBackOrdering { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.OverrideAddressEditOnCheckoutConfigSetting")]
    public bool OverrideAddressEditOnCheckoutConfigSetting { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.AllowAccountsAddressEditOnCheckout")]
    public bool AllowAccountsAddressEditOnCheckout { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.OverrideStockDisplayFormatConfigSetting")]
    public bool OverrideStockDisplayFormatConfigSetting { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.ErpAccountStatusTypeId")]
    public int ErpAccountStatusTypeId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.ErpAccountStatusType")]
    public string ErpAccountStatusType { get; set; }

    [UIHint("DateNullable")]
    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.LastErpAccountSyncDate")]
    public DateTime? LastErpAccountSyncDate { get; set; }

    [UIHint("DateNullable")]
    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.LastPriceRefresh")]
    public DateTime? LastPriceRefresh { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.B2BPriceGroupCodeId")]
    public int? B2BPriceGroupCodeId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.TotalSavingsForthisYear")]
    public decimal? TotalSavingsForthisYear { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.TotalSavingsForAllTime")]
    public decimal? TotalSavingsForAllTime { get; set; }

    [UIHint("DateNullable")]
    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.TotalSavingsForthisYearUpdatedOnUtc")]
    public DateTime? TotalSavingsForthisYearUpdatedOnUtc { get; set; }

    [UIHint("DateNullable")]
    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.TotalSavingsForAllTimeUpdatedOnUtc")]
    public DateTime? TotalSavingsForAllTimeUpdatedOnUtc { get; set; }

    [UIHint("DateNullable")]
    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.LastTimeOrderSyncOnUtc")]
    public DateTime? LastTimeOrderSyncOnUtc { get; set; }
        
    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.CreatedOnUtc")]
    public DateTime CreatedOn { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.UpdatedOnUtc")]
    public DateTime UpdatedOn { get; set; }
        
    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.CreatedBy")]
    public string CreatedBy { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.UpdatedBy")]
    public string UpdatedBy { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.IsActive")]
    public bool IsActive { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.IsDefault")]
    public bool IsDefault { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.SelectedCustomerRoles")]
    public string SelectedCustomerRoles { get; set; }

    public IList<SelectListItem> ErpAccountStatusTypes { get; set; }

    public IList<SelectListItem> AvailableErpSalesOrgs { get; set; }

    #endregion
}
