using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.AjaxCart.Models;

public partial record SuccessNotificationPublicModel : BaseNopModel
{
    public SuccessNotificationPublicModel()
    {
        ProductAttributeValues = new List<ProductAttributeValue>();
    }
    public int ProductId { get; set; }
    public string ProductSeName { get; set; }
    public string ProductPictureThumbUrl { get; set; }
    public int Quantity { get; set; }
    public int ShoppingCartId { get; set; }
    public string Price { get; set; }
    public IList<ProductAttributeValue> ProductAttributeValues { get; set; }
}
