using Nop.Core.Configuration;

namespace NopStation.Plugin.B2B.D365BCIntegration;

public class D365BCIntegrationSettings : ISettings
{
    /// <summary>
    /// Gets or sets the client ID
    /// </summary>
    public string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the client secret
    /// </summary>
    public string ClientSecret { get; set; }

    /// <summary>
    /// Gets or sets the base API URL
    /// </summary>
    public string BaseApiUrl { get; set; }

    /// <summary>
    /// Gets or sets the company name
    /// </summary>
    public string CompanyName { get; set; }

    /// <summary>
    /// Gets or sets the default customer ID for sync operations
    /// </summary>
    public int DefaultCustomerId { get; set; }

    /// <summary>
    /// Gets or sets the HTTP call timeout in seconds
    /// </summary>
    public int ErpCallTimeOut { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of retries for HTTP calls
    /// </summary>
    public int HttpCallMaxRetries { get; set; }

    /// <summary>
    /// Gets or sets the rest time between retries in minutes
    /// </summary>
    public int HttpCallRestTimeInMinutes { get; set; }

    /// <summary>
    /// Gets or sets the number of customers to sync per API call
    /// </summary>
    public int CustomerSyncLimit { get; set; } = 100;

    public int ProductSyncLimit { get; set; } = 100;

    /// <summary>
    /// Gets or sets the number of stocks to sync per API call
    /// </summary>
    public int StockSyncLimit { get; set; } = 100;
    /// <summary>
    /// Gets or sets the number of orders to sync per API call
    /// </summary>
    public int OrderSyncLimit { get; set; } = 100;

    public string Environment { get; set; }

    public string TenantId { get; set; }
}