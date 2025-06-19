namespace NopStation.Plugin.B2B.D365BCIntegration.Services;
public interface ID365BCAuthService
{
    Task<string> GetAccessTokenAsync(string tenantId, string clientId, string clientSecret, int timeOut = default);
}
