using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.D365BCIntegration.Services;

/// <summary>
/// This class should registered as singleton in the dependency injection container.
/// </summary>
public class D365BCAuthService : ID365BCAuthService
{
    private string _accessToken;
    private DateTime _tokenExpiryTime;
    private readonly HttpClient _httpClient;

    public D365BCAuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _accessToken = string.Empty;
    }

    public async Task<string> GetAccessTokenAsync(string tenantId, string clientId, string clientSecret, int timeOut = default)
    {
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow <= _tokenExpiryTime)
            return _accessToken;

        var tokenRequest = new Dictionary<string, string>
        {
            ["grant_type"] = D365BCIntegrationDefaults.GrantType,
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["scope"] = "https://api.businesscentral.dynamics.com/.default"
        };

        _httpClient.Timeout = TimeSpan.FromSeconds(timeOut > 0 ? timeOut : D365BCIntegrationDefaults.DefaultTimeOutPeriod);

        var accessTokenUrl = string.Format(D365BCIntegrationDefaults.DefaultAccessTokenUrl, tenantId);

        var tokenResponse = await _httpClient.PostAsync(accessTokenUrl, new FormUrlEncodedContent(tokenRequest));

        if (!tokenResponse.IsSuccessStatusCode)
            throw new Exception($"Failed to obtain access token. Status: {tokenResponse.StatusCode}");

        var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
        var tokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(tokenContent);

        if (tokenData is not null)
        {
            _accessToken = tokenData["access_token"];
            _tokenExpiryTime = DateTime.UtcNow.AddSeconds(int.Parse(tokenData["expires_in"]));
        }

        return _accessToken;
    }
}
