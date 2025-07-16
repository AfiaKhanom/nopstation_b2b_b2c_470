using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.OrderSummary;

public record ErpOrderSummaryModel : BaseNopEntityModel
{
    public ErpOrderSummaryModel()
    {
        Items = new List<ErpOrderSummaryItemModel>();
    }

    public decimal TotalWeight { get; set; }
    public string TotalWeightValue { get; set; }
    public string TotalPriceWithOutSavings { get; set; }
    public string ErpOnlineOrderDiscount { get; set; }
    public IList<ErpOrderSummaryItemModel> Items { get; set; }

    // nested class
    public record ErpOrderSummaryItemModel : BaseNopEntityModel
    {
        public decimal Weight { get; set; }

        public string WeightValue { get; set; }

        public string Uom { get; set; }

        public string StockAvailability { get; set; }

        public string DiscountForPerUnitProduct { get; set; }

        public string UnitPriceWithOutDiscount { get; set; }

        public bool IsBackOrder { get; set; }
    }
}
