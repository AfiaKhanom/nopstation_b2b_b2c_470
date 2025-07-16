using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;

public record ErpOrderAdditionalDataModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.OrderNumber")]
    public int NopOrderId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.OrderNumber")]
    public string OrderNumber { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.OrderOriginType")]
    public int ErpOrderOriginTypeId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.OrderOriginType")]
    public string ErpOrderOriginType { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.OrderType")]
    public int ErpOrderTypeId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.OrderType")]
    public string ErpOrderType { get; set; }

    public int OrderPlacedByNopCustomerId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.OrderPlacedByNopCustomerEmail")]
    public string OrderPlacedByNopCustomerEmail { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.QuoteExpiryDate")]
    public DateTime? QuoteExpiryDate { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.QuoteSalesOrder")]
    public int QuoteSalesOrderId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.QuoteSalesOrder")]
    public int QuoteSalesOrderNopOrderId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.QuoteSalesOrder")]
    public string QuoteSalesOrderNumber { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.ErpAccount")]
    public int ErpAccountId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.ErpAccount")]
    public string ErpAccountName { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.ErpAccountSalesOrganisation")]
    public int ErpAccountSalesOrganisationId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.ErpAccountSalesOrganisation")]
    public string ErpAccountSalesOrganisationName { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.ShipToAddress")]
    public int? ErpShipToAddressId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.ShipToAddress")]
    public string ErpShipToName { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.ShiptoSalesOrganisation")]
    public int ErpShiptoSalesOrganisationId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.ErpShiptoSalesOrganisation")]
    public string ErpShiptoSalesOrganisationName { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.ERPOrderNumber")]
    public string ErpOrderNumber { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.SpecialInstructions")]
    public string SpecialInstructions { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.CustomerReference")]
    public string CustomerReference { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.ERPOrderStatus")]
    public string ERPOrderStatus { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.ExpectedDeliveryDate")]
    public DateTime? ExpectedDeliveryDate { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.IntegrationStatusType")]
    public int IntegrationStatusTypeId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.IntegrationStatusType")]
    public string IntegrationStatusType { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.IntegrationRetries")]
    public int IntegrationRetries { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.IntegrationError")]
    public string IntegrationError { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.IntegrationErrorDateTime")]
    public DateTime? IntegrationErrorDateTime { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.LastERPUpdate")]
    public DateTime? LastERPUpdate { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.ChangedOn")]
    public DateTime? ChangedOn { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.ChangedByCustomerEmail")]
    public int ChangedById { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.ChangedByCustomerEmail")]
    public string ChangedByCustomerEmail { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpQouteOrder.Fields.OrderDate")]
    public DateTime QouteDate { get; set; }
}
