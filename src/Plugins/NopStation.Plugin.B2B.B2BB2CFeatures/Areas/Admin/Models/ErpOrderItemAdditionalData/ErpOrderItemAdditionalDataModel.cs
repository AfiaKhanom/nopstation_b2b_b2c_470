using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpOrderItemAdditionalData;

public record ErpOrderItemAdditionalDataModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpOrderItemAdditionalData.Field.NopOrderItemId")]
    public int NopOrderItemId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpOrderItemAdditionalData.Field.ErpOrderId")]
    public int ErpOrderId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpOrderItemAdditionalData.Field.ErpOrderLineNumber")]
    public string ErpOrderLineNumber { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpOrderItemAdditionalData.Field.ErpSalesUoM")]
    public string ErpSalesUoM { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpOrderItemAdditionalData.Field.ErpOrderLineStatus")]
    public string ErpOrderLineStatus { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpOrderItemAdditionalData.Field.ErpDateRequired")]
    public DateTime? ErpDateRequired { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpOrderItemAdditionalData.Field.ErpDateExpected")]
    public DateTime? ErpDateExpected { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpOrderItemAdditionalData.Field.ErpDeliveryMethod")]
    public string ErpDeliveryMethod { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpOrderItemAdditionalData.Field.ErpInvoiceNumber")]
    public string ErpInvoiceNumber { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpOrderItemAdditionalData.Field.ErpOrderLineNotes")]
    public string ErpOrderLineNotes { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpOrderItemAdditionalData.Field.LastErpUpdateUtc")]
    public DateTime? LastErpUpdateUtc { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpOrderItemAdditionalData.Field.ChangedOnUtc")]
    public DateTime? ChangedOnUtc { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpOrderItemAdditionalData.Field.ChangedBy")]
    public int ChangedBy { get; set; }
}
