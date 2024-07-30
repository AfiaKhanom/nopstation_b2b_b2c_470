using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models
{
    public record ErpGroupPriceCodeModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPriceCode.Fields.PriceGroupCodeName")]
        public string GroupPriceCode { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPriceCode.Fields.LastPriceUpdatedOnUTC")]
        public DateTime? LastPriceUpdatedOnUTC { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPriceCode.Fields.IsActive")]
        public bool IsActive { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPriceCode.Fields.CreatedOnUtc")]
        public DateTime CreatedOnUtc { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPriceCode.Fields.CreatedById")]
        public int CreatedById { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPriceCode.Fields.UpdatedOnUtc")]
        public DateTime UpdatedOnUtc { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPriceCode.Fields.UpdatedById")]
        public int UpdatedById { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPriceCode.Fields.IsDeleted")]
        public bool IsDeleted { get; set; }
    }
}
