using System;
using System.Collections.Generic;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Model;

public class ErpInvoiceDataModel
{
    public DateTime? PostingDateUtc { get; set; }
    public string ErpDocumentNumber { get; set; }
    public string Description { get; set; }
    public decimal? AmountInclVat { get; set; }
    public decimal? AmountExclVat { get; set; }
    public int ErpAccountId { get; set; }
    public string CurrencyCode { get; set; }
    public string DocumentType { get; set; }
    public string DocumentDisplayName { get; set; }
    public int PODSignedById { get; set; }
    public DateTime? PODSignedOnUtc { get; set; }
    public DateTime? DueDateUtc { get; set; }
    public string RelatedDocumentNo { get; set; }
    public DateTime? ShipmentDateUtc { get; set; }
    public DateTime? DocumentDateUtc { get; set; }
    public string ErpOrderNumber { get; set; }
    public string Base64PDFData { get; set; }
    public List<ErpOrderItemAdditionalData> Items { get; set; }
}
