using System.Collections.Generic;
using Nop.Web.Framework.Models;
using System;
using Nop.Core.Domain.Catalog;
using static Nop.Web.Areas.Admin.Models.Orders.OrderItemModel;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models
{
    public record ErpOrderItemAdditionalDataModel : BaseNopEntityModel
    {
        #region ctor
        public ErpOrderItemAdditionalDataModel()
        {
            PurchasedGiftCardIds = new List<int>();
            ReturnRequests = new List<ReturnRequestBriefModel>();
        }

        #endregion

        #region Properties

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string VendorName { get; set; }

        public string Sku { get; set; }

        public string PictureThumbnailUrl { get; set; }

        public string UnitPriceInclTax { get; set; }

        public string UnitPriceExclTax { get; set; }

        public decimal UnitPriceInclTaxValue { get; set; }

        public decimal UnitPriceExclTaxValue { get; set; }

        public int Quantity { get; set; }

        public string DiscountInclTax { get; set; }

        public string DiscountExclTax { get; set; }

        public decimal DiscountInclTaxValue { get; set; }

        public decimal DiscountExclTaxValue { get; set; }

        public string SubTotalInclTax { get; set; }

        public string SubTotalExclTax { get; set; }

        public decimal SubTotalInclTaxValue { get; set; }

        public decimal SubTotalExclTaxValue { get; set; }

        public string AttributeInfo { get; set; }

        public string RecurringInfo { get; set; }

        public string RentalInfo { get; set; }

        public IList<ReturnRequestBriefModel> ReturnRequests { get; set; }

        public IList<int> PurchasedGiftCardIds { get; set; }

        public bool IsDownload { get; set; }

        public int DownloadCount { get; set; }

        public DownloadActivationType DownloadActivationType { get; set; }

        public bool IsDownloadActivated { get; set; }

        public Guid LicenseDownloadGuid { get; set; }

        // ERP 
        public int NopOrderItemId { get; set; }

        public string ERPOrderLineNumber { get; set; }

        public string ERPSalesUoM { get; set; }

        public string ERPOrderLineStatus { get; set; }

        public DateTime? ERPDateRequired { get; set; }

        public DateTime? ERPDateExpected { get; set; }

        public string ERPDeliveryMethod { get; set; }

        public string ERPInvoiceNumber { get; set; }

        public string ERPOrderLineNotes { get; set; }

        public DateTime? LastERPUpdateUtc { get; set; }

        #endregion
    }
}
