using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpInvoice;

public record ErpInvoiceModel: BaseNopEntityModel
{
    public ErpInvoiceModel()
    {
        AvailableDocumentTypes = new List<SelectListItem>();
    }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoiceModel.Field.PostingDateUtc")]
    public DateTime PostingDateUtc { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoiceModel.Field.ErpDocumentNumber")]
    public string ErpDocumentNumber { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoiceModel.Field.Description")]
    public string Description { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoiceModel.Field.AmountInclVat")]
    public decimal AmountInclVat { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoiceModel.Field.AmountExclVat")]
    public decimal AmountExclVat { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoiceModel.Field.ErpAccountId")]
    public int ErpAccountId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoiceModel.Field.ErpAccountName")]
    public string ErpAccountName { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoiceModel.Field.CurrencyCode")]
    public string CurrencyCode { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoiceModel.Field.DocumentTypeId")]
    public int DocumentTypeId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoiceModel.Field.DocumentType")]
    public ErpDocumentType DocumentType
    {
        get => (ErpDocumentType)DocumentTypeId;
        set => DocumentTypeId = (int)value;
    }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoiceModel.Field.DocumentDisplayName")]
    public string DocumentDisplayName { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoiceModel.Field.DueDateUtc")]
    public DateTime DueDateUtc { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoiceModel.Field.RelatedDocumentNo")]
    public string RelatedDocumentNo { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoiceModel.Field.ShipmentDateUtc")]
    public DateTime? ShipmentDateUtc { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoiceModel.Field.ItemCount")]
    public int ItemCount { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoiceModel.Field.DocumentDateUtc")]
    public DateTime? DocumentDateUtc { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoiceModel.Field.PODSignedById")]
    public int PODSignedById { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoiceModel.Field.PODSignedOnUtc")]
    public DateTime? PODSignedOnUtc { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoiceModel.Field.ErpOrderNumber")]
    public string ErpOrderNumber { get; set; }

    public int ErpOrderId { get; set; }

    public IList<SelectListItem> AvailableDocumentTypes { get; set; }
}
