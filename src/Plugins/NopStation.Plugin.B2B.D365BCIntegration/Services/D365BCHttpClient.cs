using System.Net.Http.Headers;
using Newtonsoft.Json;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.D365BCIntegration.Services;

public class D365BCHttpClient
{
    #region Fields

    private readonly HttpClient _httpClient;
    private readonly IErpLogsService _erpLogsService;
    private readonly D365BCIntegrationSettings _settings;
    private string _accessToken = string.Empty;
    private DateTime _tokenExpiryTime;

    #endregion

    #region Ctor

    public D365BCHttpClient(HttpClient httpClient,
        IErpLogsService erpLogsService,
        D365BCIntegrationSettings settings)
    {
        _httpClient = httpClient;
        _erpLogsService = erpLogsService;
        _settings = settings;

        _httpClient.Timeout = TimeSpan.FromSeconds(settings.ErpCallTimeOut > 0 ? settings.ErpCallTimeOut 
            : D365BCIntegrationDefaults.DefaultTimeOutPeriod);
    }

    #endregion

    #region Utilities

    private async Task<string> GetAccessTokenAsync()
    {
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow <= _tokenExpiryTime)
            return _accessToken;

        var tokenRequest = new Dictionary<string, string>
        {
            ["grant_type"] = D365BCIntegrationDefaults.GrantType,
            ["client_id"] = _settings.ClientId,
            ["client_secret"] = _settings.ClientSecret,
            ["scope"] = D365BCIntegrationDefaults.ScopeUrl
        };

        var accessTokenUrl = string.Format(D365BCIntegrationDefaults.DefaultAccessTokenUrl, _settings.TenantId);

        var tokenResponse = await _httpClient.PostAsync(accessTokenUrl, new FormUrlEncodedContent(tokenRequest));

        if (!tokenResponse.IsSuccessStatusCode)
            throw new Exception($"Failed to obtain access token. Status: {tokenResponse.StatusCode}");

        var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
        var tokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(tokenContent);

        if(tokenData is not null)
        {
            _accessToken = tokenData["access_token"];
            _tokenExpiryTime = DateTime.UtcNow.AddSeconds(int.Parse(tokenData["expires_in"]));
        }

        return _accessToken;
    }

    #endregion

    #region Methods

    public async Task<HttpResponseMessage?> SendAsync(HttpRequestMessage request, ErpSyncLevel syncLevel)
    {
        var currentRetries = 0;
        HttpResponseMessage? response = null;

        while (currentRetries <= _settings.HttpCallMaxRetries)
        {
            try
            {
                // Get fresh token and add to request
                var token = await GetAccessTokenAsync();
                request.Headers.Authorization = new AuthenticationHeaderValue(D365BCIntegrationDefaults.TokenType, token);

                response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                    break;

                await _erpLogsService.InformationAsync(
                    $"HTTP Response status is unsuccessful. " +
                    (_settings.HttpCallMaxRetries - currentRetries > 0 ?
                    $"HTTP call will be retried after {_settings.HttpCallRestTimeInMinutes} minutes. Retry attempts left: {_settings.HttpCallMaxRetries - currentRetries}" :
                    "No retry attempts left."),
                    syncLevel);

                if (_settings.HttpCallMaxRetries - currentRetries <= 0)
                    response.EnsureSuccessStatusCode();

                await Task.Delay(_settings.HttpCallRestTimeInMinutes * 60 * 1000);
            }
            catch (Exception ex)
            {
                await _erpLogsService.ErrorAsync(ex.Message, syncLevel);
                if (currentRetries >= _settings.HttpCallMaxRetries)
                    throw;
            }

            currentRetries++;
        }

        return response;
    }

    public async Task<HttpResponseMessage?> GetAsync(string requestUri, ErpSyncLevel syncLevel)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        return await SendAsync(request, syncLevel);
    }

    public async Task<HttpResponseMessage?> PostAsync(string requestUri, HttpContent content, ErpSyncLevel syncLevel)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = content
        };
        return await SendAsync(request, syncLevel);
    }

    #endregion
}