using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Areas.Admin.Models;

public record ConfigurationModel : BaseNopModel, ISettingsModel
{
    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.Admin.Configuration.Fields.SelectedErpIntegrationPlugin")]
    public string SelectedErpIntegrationPlugin { get; set; }
    public bool SelectedErpIntegrationPlugin_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.Admin.Configuration.Fields.SelectedErpIntegrationPluginForOrder")]
    public string SelectedErpIntegrationPluginForOrder { get; set; }
    public bool SelectedErpIntegrationPluginForOrder_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.Admin.Configuration.Fields.SelectedErpIntegrationPluginForProduct")]
    public string SelectedErpIntegrationPluginForProduct { get; set; }
    public bool SelectedErpIntegrationPluginForProduct_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.Admin.Configuration.Fields.SelectedErpIntegrationPluginForAccount")]
    public string SelectedErpIntegrationPluginForAccount { get; set; }
    public bool SelectedErpIntegrationPluginForAccount_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.Admin.Configuration.Fields.SelectedErpIntegrationPluginForShiptoAddress")]
    public string SelectedErpIntegrationPluginForShiptoAddress { get; set; }
    public bool SelectedErpIntegrationPluginForShiptoAddress_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.Admin.Configuration.Fields.SelectedErpIntegrationPluginForGroupPrice")]
    public string SelectedErpIntegrationPluginForGroupPrice { get; set; }
    public bool SelectedErpIntegrationPluginForGroupPrice_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.Admin.Configuration.Fields.SelectedErpIntegrationPluginForSpecialPrice")]
    public string SelectedErpIntegrationPluginForSpecialPrice { get; set; }
    public bool SelectedErpIntegrationPluginForSpecialPrice_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.Admin.Configuration.Fields.SelectedErpIntegrationPluginForStock")]
    public string SelectedErpIntegrationPluginForStock { get; set; }
    public bool SelectedErpIntegrationPluginForStock_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.Admin.Configuration.Fields.SelectedErpIntegrationPluginForInvoice")]
    public string SelectedErpIntegrationPluginForInvoice { get; set; }
    public bool SelectedErpIntegrationPluginForInvoice_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.Admin.Configuration.Fields.UseSingleIntegrationPluginForAllSync")]
    public bool UseSingleIntegrationPluginForAllSync { get; set; }
    public bool UseSingleIntegrationPluginForAllSync_OverrideForStore { get; set; }

    public IList<SelectListItem> AvailableErpIntegrationPlugins { get; set; }
    public int ActiveStoreScopeConfiguration { get; set; }
}