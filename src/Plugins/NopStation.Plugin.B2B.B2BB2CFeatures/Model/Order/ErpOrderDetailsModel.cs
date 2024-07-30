using System;
using System.Collections.Generic;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Common;
using Nop.Web.Models.Media;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.OrderSummary
{
    public record ErpOrderDetailsModel : BaseNopEntityModel
    {
        public ErpOrderDetailsModel()
        {
            Items = new List<ErpOrderItemDataModel>();
            ErpPickUpAddressModel = new AddressModel();
            ErpBillingAddressModel = new AddressModel();
            TaxRates = new List<TaxRate>();
            ShipmentBriefModels = new List<ShipmentBriefModel>();
            OrderNotes = new List<OrderNote>();
            GiftCards = new List<GiftCard>();
        }

        public string ERPOrderNumber { get; set; }

        public string ERPOrderStatus { get; set; }

        public bool IsQuoteOrder { get; set; }

        public decimal? TotalWeight { get; set; }
        public string TotalWeightValue { get; set; }

        public string TotalPriceWithOutSavingsExcTax { get; set; }

        public string VatNumber { get; set; }

        public string ErpOnlineOrderDiscountExcTax { get; set; }

    
        public DateTime CreatedOn { get; set; }

        public IList<ErpOrderItemDataModel> Items { get; set; }

        public AddressModel ErpBillingAddressModel { get; set; }

        public AddressModel ErpPickUpAddressModel { get; set; }

        public bool IsPicUpAddressActive { get; set; }
        public ErpShipToAddressDataModel ErpShippingAddressModel { get; set; }

        public Dictionary<string, object> CustomValues { get; set; }
        public decimal OrderSubTotalDiscountValue { get; set; }
        public string OrderSubTotalDiscount { get; set; }
        public decimal OrderSubtotalValue { get; set; }
        public string OrderSubtotal { get; set; }
        public string PaymentMethodStatus { get; set; }
        public bool CanRePostProcessPayment { get; set; }
        public string PaymentMethod { get; set; }

        public decimal OrderShippingValue { get; set; }
        public string OrderShipping { get; set; }
        public decimal PaymentMethodAdditionalFeeValue { get; set; }
        public string PaymentMethodAdditionalFee { get; set; }

        public bool DisplayTaxShippingInfo { get; set; }

        public bool DisplayTax { get; set; }

        public bool DisplayTaxRates { get; set; }
        public string RedeemedRewardPointsAmount { get; set; }
        public int RedeemedRewardPoints { get; set; }

        public string OrderTotalDiscount { get; set; }

        public decimal OrderTotalDiscountValue { get; set; }

        public string OrderTotal { get; set; }
        public decimal OrderTotalValue { get; set; }
        public string CheckoutAttributeInfo { get; set; }

        public string ShippingMethod { get; set; }

        public string ShippingStatus { get; set; }

        public bool PricesIncludeTax { get; set; }

        public ErpAccountDataModel ErpAccountDataModel { get; set; }

        public List<ShipmentBriefModel> ShipmentBriefModels { get; set; }

        public IList<OrderNote> OrderNotes { get; set; }
        public IList<GiftCard> GiftCards { get; set; }
        public IList<TaxRate> TaxRates { get; set; }

        public string Tax { get; set; }

        public record ShipmentBriefModel : BaseNopEntityModel
        {
            public string TrackingNumber { get; set; }
            public DateTime? ShippedDate { get; set; }
            public DateTime? ReadyForPickupDate { get; set; }
            public DateTime? DeliveryDate { get; set; }
        }

        public partial record TaxRate : BaseNopModel
        {
            public string Rate { get; set; }
            public string Value { get; set; }
        }

        public partial record GiftCard : BaseNopModel
        {
            public string CouponCode { get; set; }
            public string Amount { get; set; }
        }

        public partial record OrderNote : BaseNopEntityModel
        {
            public bool HasDownload { get; set; }
            public string Note { get; set; }
            public DateTime CreatedOn { get; set; }
        }

        public record ErpOrderItemDataModel : BaseNopEntityModel
        {

            public string ProductName { get; set; }

            public string LastERPUpdateLocalStr { get; set; }
            public int Quantity { get; set; }

            public string DiscountForPerUnitProductExcTax { get; set; }

            public string TotalUnitPriceWithOutDiscountExcTax { get; set; }
            public string UnitPriceWithOutDiscountExcTax { get; set; }

            public decimal? ItemWeight { get; set; }

            public string ItemWeightValue { get; set; }

            public int NopOrderItemId { get; set; }

            public string ERPOrderLineNumber { get; set; }

            public string ERPSalesUoM { get; set; }

            public string ERPOrderLineStatus { get; set; }

            public string ERPDateRequired { get; set; }

            public string ERPDateExpected { get; set; }

            public string ERPDeliveryMethod { get; set; }

            public string ERPInvoiceNumber { get; set; }

            public string ERPOrderLineNotes { get; set; }

            public string SpecialInstructions { get; set; }

            public string WarehouseName { get; set; }

            public string DeliveryDate { get; set; }

            public string Sku { get; set; }

            public string ProductSeName { get; set; }

            public string AttributeInfo { get; set; }

            //public int DownloadId { get; set; }

            //public Guid OrderItemGuid { get; set; }

            public string RentalInfo { get; set; }

            public string VendorName { get; set; }

            public PictureModel Picture { get; set; }

        }
    }
}
