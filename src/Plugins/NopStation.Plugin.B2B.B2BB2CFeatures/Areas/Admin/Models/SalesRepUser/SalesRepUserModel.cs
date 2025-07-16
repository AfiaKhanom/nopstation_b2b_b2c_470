using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.SalesRepUser;

public record SalesRepUserModel : ErpBaseEntityModel
{
    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeaturesErpNopUser.Fields.NopCustomerId")]
    public int NopCustomerId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeaturesErpNopUser.Fields.CustomerFullName")]
    public string CustomerFullName { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeaturesErpNopUser.Fields.CustomerEmail")]
    public string CustomerEmail { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeaturesErpNopUser.Fields.ErpAccountId")]
    public int ErpAccountId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeaturesErpNopUser.Fields.ErpAccountNumber")]
    public string ErpAccountNumber { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeaturesErpNopUser.Fields.ErpAccountName")]
    public string ErpAccountName { get; set; }

    public int ErpShipToAddressId { get; set; }

    public int BillingErpShipToAddressId { get; set; }

    public int ShippingErpShipToAddressId { get; set; }

    public int ErpUserTypeId { get; set; }

    public string ErpUserType { get; set; }
}
