using System;
using Nop.Core;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain
{
    public partial class ErpOrderItemAdditionalData: BaseEntity
    {
        public int NopOrderItemId { get; set; }

        public int ErpOrderId { get; set; }
         
        public string ErpOrderLineNumber { get; set; }
 
        public string ErpSalesUoM { get; set; }
         
        public string ErpOrderLineStatus { get; set; }
         
        public DateTime? ErpDateRequired { get; set; }
         
        public DateTime? ErpDateExpected { get; set; }
         
        public string ErpDeliveryMethod { get; set; }
         
        public string ErpInvoiceNumber { get; set; }
         
        public string ErpOrderLineNotes { get; set; }
         
        public DateTime? LastErpUpdateUtc { get; set; }
         
        public DateTime? ChangedOnUtc { get; set; }
         
        public int ChangedBy { get; set; }
    }
}
