namespace NopStation.Plugin.B2B.D365BCIntegration;

public static class D365BCIntegrationDefaults
{
    /// <summary>
    /// Gets the plugin system name
    /// </summary>
    public static string AdminMenuSystemName = "NopStation.D365BCIntegration.AdminMenu";

    /// <summary>
    /// Gets the default access token URL
    /// </summary>
    public static string DefaultAccessTokenUrl => "https://login.microsoftonline.com/{0}/oauth2/v2.0/token";

    /// <summary>
    /// Gets the default access token URL
    /// </summary>
    public static string DefaultBaseApiUrl => "https://api.businesscentral.dynamics.com/v2.0";

    /// <summary>
    /// Gets the scope url
    /// </summary>
    public static readonly string ScopeUrl = "https://api.businesscentral.dynamics.com/.default";

    /// <summary>
    /// Gets the default access token URL
    /// </summary>
    public static string DefaultApiProtocol => "OData";

    /// <summary>
    /// Gets the default company name
    /// </summary>
    public static string DefaultCompanyName => "CRONUS International Ltd.";

    /// <summary>
    /// Gets the default timeout period in seconds
    /// </summary>
    public static int DefaultTimeOutPeriod => 100;

    /// <summary>
    /// Gets the customers endpoint path
    /// </summary>
    public static string CustomersEndpoint => "Company('{0}')/customers";

    /// <summary>
    /// Gets the items endpoint path
    /// </summary>
    public static string ProductEndpoint => "Company('{0}')/items";

    /// <summary>
    /// Gets the items endpoint path
    /// </summary>
    public static string ProductCountEndpoint => "Company('{0}')/items/$count";

    /// <summary>
    /// Gets the sales order endpoint path
    /// </summary>
    public static string OrderEndpoint => "Company('{0}')/salesOrders";

    /// <summary>
    /// Gets the sales order endpoint path
    /// </summary>
    public static string OrderCountEndpoint => "Company('{0}')/salesOrders/$count";

    /// <summary>
    /// Gets the OAuth token type
    /// </summary>
    public static string TokenType => "Bearer";

    /// <summary>
    /// Gets the OAuth grant type
    /// </summary>
    public static string GrantType => "client_credentials";
}