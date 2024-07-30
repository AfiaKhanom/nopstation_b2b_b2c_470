namespace NopStation.Plugin.B2B.IQRetailIntegration.Service
{
    public class IQRetailService : IIQRetailService
    {
        #region Fields

        private readonly IQRetailIntegrationSettings _iQRetailIntegrationSettings;

        #endregion

        #region Ctor

        public IQRetailService(IQRetailIntegrationSettings iQRetailIntegrationSettings)
        {
            _iQRetailIntegrationSettings = iQRetailIntegrationSettings;
        }

        #endregion

        #region Method

        public bool IsValidIQRetailIntegrationSettings(IQRetailIntegrationSettings iQRetailIntegrationSettings)
        {
            if (string.IsNullOrEmpty(iQRetailIntegrationSettings.BaseUrl) ||
            string.IsNullOrEmpty(iQRetailIntegrationSettings.CompanyId) ||
            string.IsNullOrEmpty(iQRetailIntegrationSettings.TerminalNumber) ||
            string.IsNullOrEmpty(iQRetailIntegrationSettings.UserName) ||
            string.IsNullOrEmpty(iQRetailIntegrationSettings.Password))
            {
                return false;
            }
            if (!Uri.IsWellFormedUriString(iQRetailIntegrationSettings.BaseUrl, UriKind.Absolute))
                return false;

                return true;
        }
         

        public string ParameterizeQueryString(string queryString, dynamic parameters)
        {
            if (!string.IsNullOrEmpty(queryString))
            {
                foreach (var prop in parameters.GetType().GetProperties())
                {
                    var paramName = $"@{prop.Name}";
                    var paramValue = prop.GetValue(parameters);
                    queryString = queryString.Replace(paramName, paramValue.ToString());
                }
            }

            return queryString;
        }

        #endregion
    }
}