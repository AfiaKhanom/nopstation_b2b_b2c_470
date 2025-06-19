using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.D365BCIntegration.Services;
public interface ID365BCHttpService
{
    Task<HttpResponseMessage?> GetAsync(string requestUri, ErpSyncLevel syncLevel);

    Task<HttpResponseMessage?> PostAsync(string requestUri, HttpContent content, ErpSyncLevel syncLevel);
}
