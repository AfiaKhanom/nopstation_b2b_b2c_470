using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models
{
    public record ErpSpecialPriceModel : BaseNopEntityModel
    {
        public ErpSpecialPriceModel()
        {
            AvailableErpSalesOrgs = new List<SelectListItem>();
            AvailableErpGroupPriceCodes = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSpecialPrice.Fields.Product")]
        public int ProductId { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSpecialPrice.Fields.ProductSku")]
        public string ProductSku { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSpecialPrice.Fields.ErpAccount")]
        public int ErpAccountId { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSpecialPrice.Fields.ErpAccountNumber")]
        public string ErpAccountNumber { get; set; }


        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSpecialPrice.Fields.ErpGroupPriceCode")]
        public int ErpGroupPriceCodeId { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSpecialPrice.Fields.ErpGroupPriceCode")]
        public string ErpGroupPriceCode { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSpecialPrice.Fields.Price")]
        public decimal Price { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSpecialPrice.Fields.ListPrice")]
        public decimal ListPrice { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSpecialPrice.Fields.ErpAccountSalesOrg")]
        public int ErpAccountSalesOrgId { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSpecialPrice.Fields.ErpAccountSalesOrgName")]
        public string ErpAccountSalesOrgName { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSpecialPrice.Fields.DiscountPerc")]
        public decimal DiscountPerc { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSpecialPrice.Fields.VolumeDiscount")]
        public bool VolumeDiscount { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSpecialPrice.Fields.PricingNote")]
        public string PricingNote { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSpecialPrice.Fields.PercentageOfAllocatedStock")]
        public decimal PercentageOfAllocatedStock { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSpecialPrice.Fields.PercentageOfAllocatedStockResetTimeUtc")]
        public DateTime? PercentageOfAllocatedStockResetTimeUtc { get; set; }

        public int ErpSalesOrgId { get; set; }

        public IList<SelectListItem> AvailableErpSalesOrgs { get; set; }
        public IList<SelectListItem> AvailableErpGroupPriceCodes { get; set; }
    }
}
