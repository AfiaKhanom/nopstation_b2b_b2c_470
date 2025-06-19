using System.Net.Http.Headers;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.D365BCIntegration.Services;
public class D365BCHttpService : ID365BCHttpService
{
    #region Fields
    private readonly HttpClient _httpClient;
    private readonly ID365BCAuthService _authService;
    private readonly D365BCIntegrationSettings _settings;
    private readonly IErpLogsService _erpLogsService;
    #endregion

    #region Ctor
    public D365BCHttpService(ID365BCAuthService authService,
        D365BCIntegrationSettings settings,
        HttpClient httpClient,
        IErpLogsService erpLogsService)
    {
        _authService = authService;
        _settings = settings;
        _httpClient = httpClient;
        _erpLogsService = erpLogsService;
    }
    #endregion

    #region Methods
    public async Task<HttpResponseMessage?> GetAsync(string requestUri, ErpSyncLevel syncLevel)
    {
        HttpResponseMessage? response;

        try
        {
            // Get fresh token and add to request
            var token = await _authService.GetAccessTokenAsync(_settings.TenantId, _settings.ClientId, _settings.ClientSecret, _settings.ErpCallTimeOut);

            // Create the request with the token
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            response = await _httpClient.GetAsync(requestUri);
        }
        catch(Exception ex)
        {
            await _erpLogsService.ErrorAsync(ex.Message, syncLevel);
            throw;
        }

        return response;
    }

    public async Task<HttpResponseMessage?> PostAsync(string requestUri, HttpContent content, ErpSyncLevel syncLevel)
    {
        HttpResponseMessage? response;

        try
        {
            // Get fresh token and add to request
            var token = await _authService.GetAccessTokenAsync(_settings.TenantId, _settings.ClientId, _settings.ClientSecret, _settings.ErpCallTimeOut);

            // Create the request with the token
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            response = await _httpClient.PostAsync(requestUri, content);
        }
        catch (Exception ex)
        {
            await _erpLogsService.ErrorAsync(ex.Message, syncLevel);
            throw;
        }

        return response;
    }
    #endregion
}
