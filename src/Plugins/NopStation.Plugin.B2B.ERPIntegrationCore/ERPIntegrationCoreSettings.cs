using Nop.Core.Configuration;

namespace NopStation.Plugin.B2B.ERPIntegrationCore;

public class ERPIntegrationCoreSettings : ISettings
{
    public string SelectedErpIntegrationPlugin { get; set; }
    public string SelectedErpIntegrationPluginForOrder { get; set; }
    public string SelectedErpIntegrationPluginForProduct { get; set; }
    public string SelectedErpIntegrationPluginForAccount { get; set; }
    public string SelectedErpIntegrationPluginForShiptoAddress { get; set; }
    public string SelectedErpIntegrationPluginForSpecialPrice { get; set; }
    public string SelectedErpIntegrationPluginForInvoice { get; set; }
    public string SelectedErpIntegrationPluginForGroupPrice { get; set; }
    public string SelectedErpIntegrationPluginForStock { get; set; }
    public bool UseSingleIntegrationPluginForAllSync { get; set; }
}