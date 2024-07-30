using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.ErpAccountPublic
{
    public record ErpAccountOrderDetailsModel : BaseNopEntityModel
    {
        public int NopOrderId { get; set; }

        public string NopOrderNumber { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccountOrderDetails.Fields.B2BOrderOriginType")]
        public string ErpOrderOriginType { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccountOrderDetails.Fields.ERPOrderNumber")]
        public string ERPOrderNumber { get; set; }

        [NopResourceDisplayName("plugin.Misc.NopStation.B2BB2CFeatures.ErpAccountOrderDetails.Fields.ErpAccountSalesOrg")]
        public int ErpAccountSalesOrgId { get; set; }

        [NopResourceDisplayName("plugin.Misc.NopStation.B2BB2CFeatures.ErpAccountOrderDetails.Fields.ErpAccountSalesOrg")]
        public string ErpAccountSalesOrgName { get; set; }

        /// <summary>
        //The is PO reference number the customer types in on checkout.
        /// </summary>
        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccountOrderDetails.Fields.CustomerOrder")]
        public string CustomerOrder { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccountOrderDetails.Fields.PaygateReferenceNumber")]
        public string PaygateReferenceNumber { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccountOrderDetails.Fields.PlacedByCustomer")]
        public string PlacedByCustomer { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccountOrderDetails.Fields.ERPOrderStatus")]
        public string ERPOrderStatus { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccountOrderDetails.Fields.Invoices")]
        public int Invoices { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccountOrderDetails.Fields.TotalOrderItems")]
        public int TotalOrderItems { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccountOrderDetails.Fields.Unshipped")]
        public int Unshipped { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccountOrderDetails.Fields.OrderPlacedOn")]
        public DateTime OrderPlacedOn { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccountOrderDetails.Fields.ExpectedDelivery")]
        public DateTime? ExpectedDelivery { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccountOrderDetails.Fields.OrderTotalAmount")]
        public string OrderTotalAmount { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccountOrderDetails.Fields.Warehouse")]
        public int WarehouseId { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccountOrderDetails.Fields.Warehouse")]
        public string WarehouseName { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccountOrderDetails.Fields.SpecialInstructions")]
        public string SpecialInstructions { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccountOrderDetails.Fields.CustomerReference")]
        public string CustomerReference { get; set; }
    }
}
