using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.D365BCIntegration.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ClientId")]
        public string ClientId { get; set; }
        public bool ClientId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ClientSecret")]
        public string ClientSecret { get; set; }
        public bool ClientSecret_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.D365BCIntegration.Configuration.Fields.BaseApiUrl")]
        public string BaseApiUrl { get; set; }
        public bool BaseApiUrl_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.D365BCIntegration.Configuration.Fields.CompanyName")]
        public string CompanyName { get; set; }
        public bool CompanyName_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.D365BCIntegration.Configuration.Fields.DefaultCustomerId")]
        public int DefaultCustomerId { get; set; }
        public bool DefaultCustomerId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ErpCallTimeOut")]
        public int ErpCallTimeOut { get; set; }
        public bool ErpCallTimeOut_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.D365BCIntegration.Configuration.Fields.HttpCallMaxRetries")]
        public int HttpCallMaxRetries { get; set; }
        public bool HttpCallMaxRetries_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.D365BCIntegration.Configuration.Fields.HttpCallRestTimeInMinutes")]
        public int HttpCallRestTimeInMinutes { get; set; }
        public bool HttpCallRestTimeInMinutes_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.D365BCIntegration.Configuration.Fields.CustomerSyncLimit")]
        public int CustomerSyncLimit { get; set; }
        public bool CustomerSyncLimit_OverrideForStore { get; set; }
        [NopResourceDisplayName("Plugins.NopStation.D365BCIntegration.Configuration.Fields.ProductSyncLimit")]
        public int ProductSyncLimit { get; set; }
        public bool ProductSyncLimit_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.D365BCIntegration.Configuration.Fields.StockSyncLimit")]
        public int StockSyncLimit { get; set; }
        public bool StockSyncLimit_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.D365BCIntegration.Configuration.Fields.OrderSyncLimit")]
        public int OrderSyncLimit { get; set; }
        public bool OrderSyncLimit_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableCustomers { get; set; } = new List<SelectListItem>();

        [NopResourceDisplayName("Plugins.NopStation.D365BCIntegration.Configuration.Fields.Environment")]
        public string Environment { get; set; }
        public bool Environment_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.NopStation.D365BCIntegration.Configuration.Fields.TenantId")]
        public string TenantId { get; set; }
        public bool TenantId_OverrideForStore { get; set; }
    }
}