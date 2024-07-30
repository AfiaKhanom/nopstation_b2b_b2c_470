using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.ErpAccountPublic
{
    public record ErpQuoteOrderModel : BaseNopEntityModel
    {
        public int NopOrderId { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpQuoteOrder.Fields.ErpOrderOriginType")]
        public string ErpOrderOriginType { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpQuoteOrder.Fields.QuoteNumber")]
        public string QuoteNumber { get; set; }

        /// <summary>
        //The is PO reference number the customer types in on checkout.
        /// </summary>
        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpQuoteOrder.Fields.CustomerOrder")]
        public string CustomerOrder { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpQuoteOrder.Fields.PaygateReferenceNumber")]
        public string PaygateReferenceNumber { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpQuoteOrder.Fields.PlacedByCustomerFullName")]
        public string PlacedByCustomerFullName { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpQuoteOrder.Fields.PlacedByCustomerEmail")]
        public string PlacedByCustomerEmail { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpQuoteOrder.Fields.ErpAccountSalesOrg")]
        public int ErpAccountSalesOrganisationId { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpQuoteOrder.Fields.ErpAccountSalesOrg")]
        public string ErpAccountSalesOrganisationName { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpQuoteOrder.Fields.ERPOrderStatus")]
        public string ERPOrderStatus { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpQuoteOrder.Fields.QuoteDate")]
        public DateTime QuoteDate { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpQuoteOrder.Fields.ExpiryDate")]
        public DateTime? ExpiryDate { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpQuoteOrder.Fields.Amount")]
        public string TotalAmount { get; set; }

        public bool IsQuoteActive { get; set; }

        public bool IsQuoteConvertedToOrder { get; set; }

        public bool IsQuoteExpired { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpQuoteOrder.Fields.Warehouse")]
        public string WarehouseName { get; set; }
    }
}
