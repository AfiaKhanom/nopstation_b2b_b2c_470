namespace NopStation.Plugin.B2B.IQRetailIntegration.Service
{
    public interface IIQRetailService
    {
        bool IsValidIQRetailIntegrationSettings(IQRetailIntegrationSettings iQRetailIntegrationSettings);
        string ParameterizeQueryString(string queryString, dynamic parameters);
    }
}