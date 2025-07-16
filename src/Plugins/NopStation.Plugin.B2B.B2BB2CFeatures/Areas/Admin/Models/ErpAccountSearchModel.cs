using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;

public record ErpAccountSearchModel : BaseSearchModel
{
    #region Ctor

    public ErpAccountSearchModel()
    {
        ErpAccountStatusTypes = new List<SelectListItem>();
        AvailableErpSalesOrgs = new List<SelectListItem>();
        ShowInActiveOption = new List<SelectListItem>();
    }

    #endregion

    #region Properties
    public string ErpAccountId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.AccountNumber")]
    public string AccountNumber { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.AccountName")]
    public string AccountName { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.Email")]
    public string Email { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.ErpSalesOrgId")]
    public int ErpSalesOrgId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.ErpAccountStatusTypeId")]
    public int ErpAccountStatusTypeId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Field.Show")]
    public int ShowInActive { get; set; }

    public IList<SelectListItem> ErpAccountStatusTypes { get; set; }
    public IList<SelectListItem> AvailableErpSalesOrgs { get; set; }
    public IList<SelectListItem> ShowInActiveOption { get; set; }

    #endregion
}