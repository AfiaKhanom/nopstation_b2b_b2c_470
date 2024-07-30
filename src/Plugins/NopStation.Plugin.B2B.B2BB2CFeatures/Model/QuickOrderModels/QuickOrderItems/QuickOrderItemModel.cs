using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Catalog;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.QuickOrderModels.QuickOrderItems
{
    public record QuickOrderItemModel : BaseNopEntityModel
    {
        public QuickOrderItemModel()
        {
            ProductDetailsModel = new ProductDetailsModel();
        }

        [NopResourceDisplayName("NopStation.B2BB2CFeatures.QuickOrderItemModel.Fields.Product")]
        public int ProductId { get; set; }

        [NopResourceDisplayName("NopStation.B2BB2CFeatures.QuickOrderItemModel.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.QuickOrderItemModel.Fields.SeName")]
        public string SeName { get; set; }

        [NopResourceDisplayName("NopStation.B2BB2CFeatures.QuickOrderItemModel.Fields.ProductSku")]
        public string ProductSku { get; set; }

        [NopResourceDisplayName("NopStation.B2BB2CFeatures.QuickOrderItemModel.Fields.AttributesInfo")]
        public string AttributesInfo { get; set; }

        [Range(1, Int32.MaxValue)]
        [NopResourceDisplayName("NopStation.B2BB2CFeatures.QuickOrderItemModel.Fields.Quantity")]
        public int Quantity { get; set; }

        [NopResourceDisplayName("NopStation.B2BB2CFeatures.QuickOrderItemModel.Fields.QuickOrderTemplate")]
        public int QuickOrderTemplateId { get; set; }

        [NopResourceDisplayName("NopStation.B2BB2CFeatures.QuickOrderItemModel.Fields.ValidationResult")]
        public string ValidationResult { get; set; }

        [NopResourceDisplayName("NopStation.B2BB2CFeatures.QuickOrderItemModel.Fields.ManufacturerPartNumber")]
        public string ManufacturerPartNumber { get; set; }

        [NopResourceDisplayName("NopStation.B2BB2CFeatures.QuickOrderItemModel.Fields.StockAvailability")]
        public string StockAvailability { get; set; }

        [NopResourceDisplayName("NopStation.B2BB2CFeatures.QuickOrderItemModel.Fields.SecondaryStockAvailability")]
        public string SecondaryStockAvailability { get; set; }
        
        public string Price { get; set; }
          
        [NopResourceDisplayName("NopStation.B2BB2CFeatures.QuickOrderItemModel.Fields.UnitPrice")]
        public decimal PriceValue { get; set; }

        [NopResourceDisplayName("NopStation.B2BB2CFeatures.QuickOrderItemModel.Fields.UOM")]
        public string UOM { get; set; }

        [NopResourceDisplayName("NopStation.B2BB2CFeatures.QuickOrderItemModel.Fields.PricingNotes")]
        public string PricingNotes { get; set; }

        public ProductDetailsModel ProductDetailsModel { get; set; }
    }
}
