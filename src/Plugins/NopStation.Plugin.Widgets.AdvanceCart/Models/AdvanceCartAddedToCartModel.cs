using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;

namespace NopStation.Plugin.Widgets.AdvanceCart.Models;

public record AdvanceCartAddedToCartModel : BaseNopEntityModel
{
    public AdvanceCartAddedToCartModel()
    {
        Picture = new PictureModel();
        Warnings = new List<string>();
    }

    public string Sku { get; set; }

    public string VendorName { get; set; }

    public PictureModel Picture { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; }

    public string ProductSeName { get; set; }

    public string UnitPrice { get; set; }
    public decimal UnitPriceValue { get; set; }

    public string SubTotal { get; set; }
    public decimal SubTotalValue { get; set; }

    public string Discount { get; set; }
    public decimal DiscountValue { get; set; }
    public int? MaximumDiscountedQty { get; set; }

    public int Quantity { get; set; }

    public string AttributeInfo { get; set; }

    public string RecurringInfo { get; set; }

    public string RentalInfo { get; set; }

    public IList<string> Warnings { get; set; }
}
