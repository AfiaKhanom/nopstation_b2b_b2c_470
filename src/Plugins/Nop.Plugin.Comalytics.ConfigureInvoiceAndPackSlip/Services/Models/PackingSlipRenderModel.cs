using System.Collections.Generic;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Services.Models
{
    /// <summary>
    /// Represents a packing slip render model with resolved tokens
    /// </summary>
    public class PackingSlipRenderModel
    {
        public PackingSlipRenderModel()
        {
            Items = new List<PackingSlipItemModel>();
            CustomTokens = new Dictionary<string, string>();
        }

        public string OrderNumber { get; set; }
        public string OrderDate { get; set; }
        public string ShipmentDate { get; set; }
        public string CustomerName { get; set; }
        public string ShippingAddress { get; set; }
        public string ShippingMethod { get; set; }
        public string TrackingNumber { get; set; }
        public string StoreName { get; set; }
        public string StoreUrl { get; set; }
        public string StoreLogoUrl { get; set; }
        public string BarcodeImageBase64 { get; set; }
        
        public List<PackingSlipItemModel> Items { get; set; }
        public Dictionary<string, string> CustomTokens { get; set; }
        
        // Styling properties
        public string FontFamily { get; set; }
        public int FontSize { get; set; }
        public bool EnableRtl { get; set; }
    }

    public class PackingSlipItemModel
    {
        public string Sku { get; set; }
        public string Name { get; set; }
        public string Quantity { get; set; }
        public string PictureUrl { get; set; }
    }
}
