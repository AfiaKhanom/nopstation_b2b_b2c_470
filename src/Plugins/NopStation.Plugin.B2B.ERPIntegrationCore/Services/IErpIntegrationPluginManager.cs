using System.Threading.Tasks;
using Nop.Services.Plugins;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public partial interface IErpIntegrationPluginManager : IPluginManager<IErpIntegrationPlugin>
    {
        bool IsPluginActive(IErpIntegrationPlugin paymentMethod);
        Task<IErpIntegrationPlugin> LoadActiveERPIntegrationPlugin(ErpSyncLevel erpSyncLevel);
    }
}
