namespace NopStation.Plugin.B2B.ERPIntegrationCore.Model
{
    public class ErpPlaceOrderItemDataModel
    {
        public string ItemNo { get; set; }
        public int LineNo { get; set; }
        public string BatchCode { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public string UOM { get; set; }
        public string SpecInstruct { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal LineTotalExcl { get; set; }
        public decimal LineTotalIncl { get; set; }
    }
}
