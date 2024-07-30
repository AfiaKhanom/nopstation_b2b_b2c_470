using NopStation.Plugin.B2B.ERPIntegrationCore.ErpInterface;
using Nop.Services.Plugins;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public interface IErpIntegrationPlugin : IPlugin,
        IErpIntegrationAccountService, IErpIntegrationProductService,
        IErpIntegrationOrderService, IErpIntegrationSalesOrgService,
        IErpIntegrationSettingsService
    {
    }
}
