using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;

public record CustomerErpAccountInfoModel : BaseNopEntityModel
{
    #region Properties

    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.Admin.Customer.ErpAccountInfo.Field.NopCustomer")]
    public int NopCustomerId { get; set; }
    public string NopCustomer { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.Admin.Customer.ErpAccountInfo.Field.ErpAccountInfo")]
    public int ErpAccountId { get; set; }
    public string ErpAccountInfo { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.Admin.Customer.ErpAccountInfo.Field.SalesOrgName")]
    public int ErpSalesOrgId { get; set; }
    public string ErpSalesOrg { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.Admin.Customer.ErpAccountInfo.Field.ErpShipToAddressInfo")]
    public int ErpShipToAddressId { get; set; }
    public string ErpShipToAddressInfo { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.Admin.Customer.ErpAccountInfo.Field.ErpUserType")]
    public string ErpUserType { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.CreatedOnUtc")]
    public DateTime CreatedOn { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.UpdatedOnUtc")]
    public DateTime UpdatedOn { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.CreatedBy")]
    public string CreatedBy { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Field.UpdatedBy")]
    public string UpdatedBy { get; set; }

    [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.Admin.Customer.ErpAccountInfo.Field.ErpUserIsActive")]
    public bool IsActive { get; set; }

    #endregion
}
