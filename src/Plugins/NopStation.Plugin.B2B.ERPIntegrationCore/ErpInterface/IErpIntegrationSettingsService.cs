using System.Threading.Tasks;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.ErpInterface
{
    public interface IErpIntegrationSettingsService
    {
        Task<string> GetSalesOrgCodeFromIQIntegrationSettings();
    }
}
