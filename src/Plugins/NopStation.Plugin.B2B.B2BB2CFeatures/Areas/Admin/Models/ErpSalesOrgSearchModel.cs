using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;

public record ErpSalesOrgSearchModel : BaseSearchModel
{
    #region Ctor

    public ErpSalesOrgSearchModel()
    {
        IntegrationClients = new List<SelectListItem>();
        ShowInActiveOption = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrgSearchModel.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrgSearchModel.Code")]
    public string Code { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrgSearchModel.Email")]
    public string Email { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrgSearchModel.Show")]
    public int ShowInActive { get; set; }

    public IList<SelectListItem> IntegrationClients { get; set; }
    public IList<SelectListItem> ShowInActiveOption { get; set; }

    #endregion
}
