using Nop.Core.Configuration;

namespace NopStation.Plugin.B2B.ERPIntegrationCore
{
    public class ERPIntegrationCoreSettings : ISettings
    {
        public string SelectedErpIntegrationPlugin { get; set; }
    }
}