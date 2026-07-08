using System.Collections.Generic;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Services.Models
{
    /// <summary>
    /// Represents an invoice render model with resolved tokens
    /// </summary>
    public class InvoiceRenderModel
    {
        public InvoiceRenderModel()
        {
            Items = new List<InvoiceItemModel>();
            CustomTokens = new Dictionary<string, string>();
        }

        public string OrderNumber { get; set; }
        public string OrderDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string BillingAddress { get; set; }
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }
        public string ShippingMethod { get; set; }
        public string SubTotal { get; set; }
        public string ShippingTotal { get; set; }
        public string Tax { get; set; }
        public string Total { get; set; }
        public string Currency { get; set; }
        public string StoreName { get; set; }
        public string StoreUrl { get; set; }
        public string StoreLogoUrl { get; set; }
        public string BarcodeImageBase64 { get; set; }
        
        public List<InvoiceItemModel> Items { get; set; }
        public Dictionary<string, string> CustomTokens { get; set; }
        
        // Styling properties
        public string FontFamily { get; set; }
        public int FontSize { get; set; }
        public bool EnableRtl { get; set; }
    }

    public class InvoiceItemModel
    {
        public string Sku { get; set; }
        public string Name { get; set; }
        public string Quantity { get; set; }
        public string UnitPrice { get; set; }
        public string Total { get; set; }
        public string PictureUrl { get; set; }
    }
}
