namespace NopStation.Plugin.Widgets.AdvanceCart.Models
{
    public class MiniShoppingCartResponseModel
    {
        public int ShoppingCartItemId { get; set; }
        public int ShoppingCartItemUpdatedQuantity { get; set; }
        public string Subtotal { get; set; }
        public string UnitPrice { get; set; }

        public int TotalQuantity { get; set; }
    }
}
