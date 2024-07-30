using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Areas.Admin.Model
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.BaseUrl")]
        public string BaseUrl { get; set; }
        public bool BaseUrl_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.UserName")]
        public string UserName { get; set; }
        public bool UserName_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.Password")]
        public string Password { get; set; }
        public bool Password_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.Location")]
        public string Location { get; set; }
        public bool Location_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.B2cPriceCode")]
        public string B2cPriceCode { get; set; }
        public bool B2cPriceCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.DefaultLimit")]
        public int DefaultLimit { get; set; }
        public bool DefaultLimit_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.ErpCallTimeOut")]
        public int ErpCallTimeOut { get; set; }
        public bool ErpCallTimeOut_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.DefaultCustomerId")]
        public int DefaultCustomerId { get; set; }
        public bool DefaultCustomerId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.CompanyId")]
        public string CompanyId { get; set; }
        public bool CompanyId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.TerminalNumber")]
        public string TerminalNumber { get; set; }
        public bool TerminalNumber_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice1Code")]
        public string? SellPrice1 { get; set; }
        public bool SellPrice1_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice2Code")]
        public string? SellPrice2 { get; set; }
        public bool SellPrice2_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice3Code")]
        public string? SellPrice3 { get; set; }
        public bool SellPrice3_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice4Code")]
        public string? SellPrice4 { get; set; }
        public bool SellPrice4_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice5Code")]
        public string? SellPrice5 { get; set; }
        public bool SellPrice5_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice6Code")]
        public string? SellPrice6 { get; set; }
        public bool SellPrice6_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice7Code")]
        public string? SellPrice7 { get; set; }
        public bool SellPrice7_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice8Code")]
        public string? SellPrice8 { get; set; }
        public bool SellPrice8_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice9Code")]
        public string? SellPrice9 { get; set; }
        public bool SellPrice9_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.SellPrice10Code")]
        public string? SellPrice10 { get; set; }
        public bool SellPrice10_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.HttpCallMaxRetries")]
        public int? HttpCallMaxRetries { get; set; }
        public bool HttpCallMaxRetries_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.IQRetailIntegration.Configuration.Fields.HttpCallRestTimeInMinutes")]
        public int? HttpCallRestTimeInMinutes { get; set; }
        public bool HttpCallRestTimeInMinutes_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableCustomers { get; set; } =  new List<SelectListItem>();
    }
}
