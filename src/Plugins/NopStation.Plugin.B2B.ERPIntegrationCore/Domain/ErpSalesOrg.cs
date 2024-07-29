namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain
{
    public partial class ErpSalesOrg: ErpBaseEntity
    { 
        public string Name { get; set; }

        public string Code { get; set; }

        public string Email { get; set; }

        public int AddressId { get; set; }

        public string IntegrationClientId { get; set; }

        public string AuthenticationKey { get; set; }
    }
}
