using System.Text;
using Nop.Core;
using Nop.Services.Configuration;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Service
{
    public class IQRetailHttpClient
    {
        #region Fields

        private readonly HttpClient _httpClient;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IErpLogsService _erpLogsService;

        #endregion

        #region Ctor

        public IQRetailHttpClient(IQRetailIntegrationSettings iQRetailIntegrationSettings,
            HttpClient httpClient,
            ISettingService settingService,
            IStoreContext storeContext,
            IErpLogsService erpLogsService)
        { 
            httpClient.BaseAddress = new Uri(iQRetailIntegrationSettings.BaseUrl ?? IQRetailIntegrationDefaults.IQRetailIntegrationDefaultBaseUrl);
            httpClient.Timeout = TimeSpan.FromSeconds(iQRetailIntegrationSettings.ErpCallTimeOut > 0 ? iQRetailIntegrationSettings.ErpCallTimeOut : IQRetailIntegrationDefaults.DefaultTimeOutPeriod);
            _httpClient = httpClient;
            _settingService = settingService;
            _storeContext = storeContext;
            _erpLogsService = erpLogsService;
        }

        #endregion

        #region Method

        public async Task<HttpResponseMessage> HttpCall(string urlExtension, string payloadData, ErpSyncLevel erpSyncLabel)
        {
            try
            {
                //load settings for a chosen store scope
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var iQRetailIntegrationSettings = await _settingService.LoadSettingAsync<IQRetailIntegrationSettings>(storeScope);
                var currentRetries = 0;

                var httpReqContent = new StringContent(payloadData, Encoding.UTF8, "test/xml");

                var httpResponse = new HttpResponseMessage();

                // The loop should run atleast once. Therefore, count started from 0.
                while (currentRetries <= iQRetailIntegrationSettings.HttpCallMaxRetries)
                {
                    httpResponse = await _httpClient.PostAsync(urlExtension, httpReqContent);

                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        await _erpLogsService.InformationAsync(
                            message: $"HTTP Response status is unsuccessful. " +
                            ((iQRetailIntegrationSettings.HttpCallMaxRetries - currentRetries > 0) ?
                            $"HTTP call will be retried after {iQRetailIntegrationSettings.HttpCallRestTimeInMinutes} minutes. Retry attempts left: {iQRetailIntegrationSettings.HttpCallMaxRetries - currentRetries}" :
                            "No retry attempts left."),
                            syncLavel: erpSyncLabel);

                        if (iQRetailIntegrationSettings.HttpCallMaxRetries - currentRetries <= 0)
                            httpResponse.EnsureSuccessStatusCode();

                        await Task.Delay(iQRetailIntegrationSettings.HttpCallRestTimeInMinutes * 60 * 1000); // Converting the minute time into milliseconds.
                    }
                    else
                        break;

                    currentRetries++;
                }

                return httpResponse;
            }
            catch (AggregateException exception)
            { 
                throw exception.InnerException;
            }
        }

        #endregion
    }
}