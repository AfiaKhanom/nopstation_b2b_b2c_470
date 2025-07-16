using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Areas.Admin.Models;

public record ConfigurationModel : BaseNopModel, ISettingsModel
{
    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.Admin.Configuration.Fields.SelectedErpIntegrationPlugin")]
    public string SelectedErpIntegrationPlugin { get; set; }

    public bool SelectedErpIntegrationPlugin_OverrideForStore { get; set; }

    public IList<SelectListItem> AvailableErpIntegrationPlugins { get; set; }

    public int ActiveStoreScopeConfiguration { get; set; }
}