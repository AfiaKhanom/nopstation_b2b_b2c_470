using System;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpOrderAdditionalData : BaseEntity
{
    public int NopOrderId { get; set; }

    public string ErpOrderNumber { get; set; }

    public int ErpOrderOriginTypeId { get; set; }

    public ErpOrderOriginType ErpOrderOriginType
    {
        get => (ErpOrderOriginType)ErpOrderOriginTypeId;
        set => ErpOrderOriginTypeId = (int)value;
    }

    public int ErpOrderTypeId { get; set; }

    public ErpOrderType ErpOrderType
    {
        get => (ErpOrderType)ErpOrderTypeId;
        set => ErpOrderTypeId = (int)value;
    }

    public int OrderPlacedByNopCustomerId { get; set; }

    public DateTime? QuoteExpiryDate { get; set; }

    public int? QuoteSalesOrderId { get; set; }

    public int ErpAccountId { get; set; }

    public int? ErpShipToAddressId { get; set; }

    public string SpecialInstructions { get; set; }

    public string CustomerReference { get; set; }

    public string ERPOrderStatus { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public int IntegrationStatusTypeId { get; set; }

    public IntegrationStatusType IntegrationStatusType
    {
        get => (IntegrationStatusType)IntegrationStatusTypeId;
        set => IntegrationStatusTypeId = (int)value;
    }

    public string IntegrationError { get; set; }

    public int? IntegrationRetries { get; set; }

    public DateTime? IntegrationErrorDateTimeUtc { get; set; }

    public DateTime? LastERPUpdateUtc { get; set; }

    public bool IsShippingAddressModified { get; set; }

    public bool? IsOrderPlaceNotificationSent { get; set; }

    public int ErpOrderPlaceByCustomerTypeId { get; set; }

    public DateTime? ChangedOnUtc { get; set; }

    public int ChangedById { get; set; }
}
