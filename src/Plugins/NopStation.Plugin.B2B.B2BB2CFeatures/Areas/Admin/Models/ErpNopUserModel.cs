using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;

public record ErpNopUserModel : BaseNopEntityModel
{
    #region Ctor

    public ErpNopUserModel()
    {
        AvailableErpAccounts = new List<SelectListItem>();
        AvailableNopCustomers = new List<SelectListItem>();
        AvailableErpUserTypes = new List<SelectListItem>();
        AvailableAddresses = new List<SelectListItem>();
        AvailableCustomerRoles = new List<SelectListItem>();
        SelectedCustomerRoleIds = new List<int>();
        ErpNopUserSearchModel = new ErpNopUserSearchModel();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.NopCustomerId")]
    public int NopCustomerId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.NopCustomer")]
    public string NopCustomer { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.NopCustomerEmail")]
    public string NopCustomerEmail { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.ErpAccountId")]
    public int ErpAccountId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.ErpAccount")]
    public string ErpAccountInfo { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.ErpSalesOrg")]
    public string ErpSalesOrg { get; set; }
    public int ErpSalesOrgId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.ErpShipToAddressId")]
    public int ErpShipToAddressId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.ErpShipToAddress")]
    public AddressModel ErpShipToAddress { get; set; }
    public string Email { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.BillingErpShipToAddressId")]
    public int BillingErpShipToAddressId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.BillingErpShipToAddress")]
    public AddressModel BillingErpShipToAddress { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.ShippingErpShipToAddressId")]
    public int ShippingErpShipToAddressId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.ShippingErpShipToAddress")]
    public AddressModel ShippingErpShipToAddress { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.ErpUserTypeId")]
    public int ErpUserTypeId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.ErpUserType")]
    public string ErpUserType { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.CreatedOnUtc")]
    public DateTime CreatedOn { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.UpdatedOnUtc")]
    public DateTime UpdatedOn { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.CreatedBy")]
    public string CreatedBy { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.UpdatedBy")]
    public string UpdatedBy { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.IsActive")]
    public bool IsActive { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.SelectedCustomerRoles")]
    public IList<int>? SelectedCustomerRoleIds { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.SelectedCustomerRoles")]
    public string? SelectedCustomerRoles { get; set; }

    public IList<SelectListItem> AvailableErpAccounts { get; set; }

    public IList<SelectListItem> AvailableNopCustomers { get; set; }

    public IList<SelectListItem> AvailableErpUserTypes { get; set; }

    public IList<SelectListItem> AvailableAddresses { get; set; }

    public IList<SelectListItem> AvailableCustomerRoles { get; set; }

    public ErpNopUserSearchModel ErpNopUserSearchModel { get; set; }

    #endregion
}
