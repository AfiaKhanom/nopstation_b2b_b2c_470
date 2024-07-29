using System;
using System.Collections.Generic;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Model
{
    public class ErpInvoiceDataModel
    {
        public string CustomerReference { get; set; }
        public string InvoiceNumber { get; set; }
        public string DocumentNo { get; set; }
        public string DocumentType { get; set; }
        public List<ErpOrderItemAdditionalData> Items { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string Reference { get; set; }
        public string BranchNo { get; set; }
        public string BranchShort { get; set; }
        public string BranchName { get; set; }
        public string OrderNo { get; set; }
        public string Status { get; set; }
        public decimal Balance { get; set; }
        public string Base64PDFData { get; set; }

    }
}
