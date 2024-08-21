using System;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpInvoice : BaseEntity
{
    public DateTime PostingDateUtc { get; set; }
     
    public string ErpDocumentNumber { get; set; }
     
    public string Description { get; set; }
     
    public decimal AmountInclVat { get; set; }
     
    public decimal AmountExclVat { get; set; }
     
    public int ErpAccountId { get; set; }
     
    public string CurrencyCode { get; set; }
     
    public int DocumentTypeId { get; set; }
     
    public ErpDocumentType DocumentType
    {
        get => (ErpDocumentType)DocumentTypeId;
        set => DocumentTypeId = (int)value;
    }

    public string DocumentDisplayName { get; set; }

    public DateTime DueDateUtc { get; set; }

    public string RelatedDocumentNo { get; set; }

    public DateTime? ShipmentDateUtc { get; set; }

    public int ItemCount { get; set; }

    public DateTime? DocumentDateUtc { get; set; }

    public int PODSignedById { get; set; }

    public DateTime? PODSignedOnUtc { get; set; }

    public string ErpOrderNumber { get; set; }
}
