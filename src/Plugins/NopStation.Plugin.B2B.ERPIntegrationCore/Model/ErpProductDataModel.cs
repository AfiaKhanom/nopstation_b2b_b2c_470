using System.Collections.Generic;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Model
{
    public class ErpProductCategory
    {
        public string CategoryCode { get; set; }
        public string CategoryName { get; set; }
    }
    public class ErpProductDataModel
    {
        public string ItemNo { get; set; }
        public string MasterCode { get; set; }
        public string Description { get; set; }
        public bool IsSpecial { get; set; }
        public string FullDescription { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public decimal? Length { get; set; }
        public decimal? Weight { get; set; }
        public decimal? SellingPriceA { get; set; }
        public string UnitOfMeasure { get; set; }
        public decimal? InStockforLocNo { get; set; }
        public IEnumerable<ErpProductCategory> Categories { get; set; } = new List<ErpProductCategory>();
        public List<KeyValuePair<string, string>> Attributes { get; set; } = new List<KeyValuePair<string, string>>();
        //public string CategoryDesc { get; set; } // not in use
        //public string GeneralCode { get; set; } // not in use
        public string VatRate { get; set; } = "DEFAULT"; // DEFAULT will be used to associate the default TaxCategory Id in the mappings file
        public string Active { get; set; } = "Y";
        public string WebList { get; set; } = "Y";
        public string VendorName { get; set; }
        public string Brand { get; set; }
        public string BrandDesc { get; set; }
    }
}
