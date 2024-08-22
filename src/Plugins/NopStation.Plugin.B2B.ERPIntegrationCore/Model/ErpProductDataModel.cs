using System;
using System.Collections.Generic;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Model;

public class ErpProductDataModel
{
    public string Name { get; set; }
    public string Sku { get; set; }
    public string ManufacturerPartNumber { get; set; }
    public string ShortDescription { get; set; }
    public string FullDescription { get; set; }
    public decimal Height { get; set; }
    public decimal Width { get; set; }
    public decimal Length { get; set; }
    public decimal Weight { get; set; }
    public decimal Price { get; set; }
    public decimal StockQuantity { get; set; }
    public IEnumerable<ErpCategoryDataModel> ProductCategories { get; set; } = new List<ErpCategoryDataModel>();
    public List<KeyValuePair<string, string>> ProductAttributes { get; set; } = new List<KeyValuePair<string, string>>();
    public int TaxCategoryId { get; set; }
    public string TaxCategoryName { get; set; }
    public string Published { get; set; }
    public string ManufacturerName { get; set; }
    public string ManufacturerCode { get; set; }
    public string VendorCode { get; set; }
    public string VendorName { get; set; }
    public DateTime LastChangedDate { get; set; }
}
