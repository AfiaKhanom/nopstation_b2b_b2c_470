using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.ErpProductList
{
    public record ErpProductDataModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.B2BProductList.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.B2BProductList.Fields.ShortDescription")]
        public string ShortDescription { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.B2BProductList.Fields.SeName")]
        public string SeName { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.B2BProductList.Fields.Sku")]
        public string Sku { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.B2BProductList.Fields.ManufacturerPartNumber")]
        public string ManufacturerPartNumber { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.B2BProductList.Fields.Price")]
        public string Price { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.B2BProductList.Fields.PriceValue")]
        public decimal PriceValue { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.B2BProductList.Fields.DisableBuyButton")]
        public bool DisableBuyButton { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.B2BProductList.Fields.StockAvailability")]
        public string StockAvailability { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.B2BProductList.Fields.SecondaryStockAvailability")]
        public string SecondaryStockAvailability { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.B2BProductList.Fields.UOM")]
        public string UOM { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.B2BProductList.Fields.PricingNotes")]
        public string PricingNotes { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.B2BProductList.Fields.Quantity")]
        public decimal Weight { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.B2BProductList.Fields.Weight")]
        public string WeightValue { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.B2BProductList.Fields.Quantity")]
        public int Quantity { get; set; }

        public bool IsOutOfStock { get; set; }

        public bool DisplayBackInStockSubscription { get; set; }
    }
}
