using Nop.Core.Configuration;

namespace NopStation.Plugin.B2B.IQRetailIntegration
{
    public class IQRetailIntegrationSettings: ISettings
    {
        public string BaseUrl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Location { get; set; }
        public string B2cPriceCode { get; set; }
        public int DefaultLimit { get; set; }
        public int ErpCallTimeOut { get; set; }
        public int DefaultCustomerId { get; set; }
        public string CompanyId { get; set;}
        public string TerminalNumber { get; set; }
        public string SellPrice1 { get; set; }
        public string SellPrice2 { get; set; }
        public string SellPrice3 { get; set; }
        public string SellPrice4 { get; set; }
        public string SellPrice5 { get; set; }
        public string SellPrice6 { get; set; }
        public string SellPrice7 { get; set; }
        public string SellPrice8 { get; set; }
        public string SellPrice9 { get; set; }
        public string SellPrice10 { get; set; }
        public int HttpCallMaxRetries { get; set; }
        public int HttpCallRestTimeInMinutes { get; set; }
    }
}
