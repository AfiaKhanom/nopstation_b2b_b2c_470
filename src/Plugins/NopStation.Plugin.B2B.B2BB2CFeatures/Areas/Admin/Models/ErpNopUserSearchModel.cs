using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;

public record ErpNopUserSearchModel : BaseSearchModel
{
    #region Ctor

    public ErpNopUserSearchModel()
    {
        AvailableErpUserTypes = new List<SelectListItem>();
        AvailableErpShipToAddresses = new List<SelectListItem>();
        ShowInActiveOption = new List<SelectListItem>();
        CustomerRoleIds = new List<int>();
    }

    #endregion

    #region Properties

    public int NopUserId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserSearchModel.AccountNumber")]
    public int AccountId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserSearchModel.FirstName")]
    public string FirstName { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserSearchModel.LastName")]
    public string LastName { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserSearchModel.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserSearchModel.Email")]
    public string Email { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserSearchModel.SalesOrgId")]
    public int SalesOrgId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserSearchModel.ErpShipToAddressId")]
    public int ErpShipToAddressId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserSearchModel.ErpUserTypeId")]
    public int ErpNopUserTypeId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserSearchModel.Show")]
    public int ShowInActive { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserAccountMap.Field.CustomerRoles")]
    public IList<int> CustomerRoleIds { get; set; }

    public IList<SelectListItem> AvailableErpUserTypes { get; set; }
    public IList<SelectListItem> AvailableErpShipToAddresses { get; set; }
    public IList<SelectListItem> AvailableCustomerRoles { get; set; }
    public IList<SelectListItem> ShowInActiveOption { get; set; }

    #endregion
}
