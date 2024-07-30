using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.ErpAccountPublic
{
    public record RecentTransactionModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Plugins.Payment.B2BCustomerAccount.B2BAccount.FinancialTransaction.Fields.PostingDate")]
        public DateTime PostingDate { get; set; }

        [NopResourceDisplayName("Plugins.Payment.B2BCustomerAccount.B2BAccount.FinancialTransaction.Fields.DocumentType")]
        public string DocumentType { get; set; }

        [NopResourceDisplayName("Plugins.Payment.B2BCustomerAccount.B2BAccount.FinancialTransaction.Fields.DocumentDisplayName")]
        public string DocumentDisplayName { get; set; }

        [NopResourceDisplayName("Plugins.Payment.B2BCustomerAccount.B2BAccount.FinancialTransaction.Fields.DocumentNo")]
        public string DocumentNo { get; set; }

        [NopResourceDisplayName("Plugins.Payment.B2BCustomerAccount.B2BAccount.FinancialTransaction.Fields.Status")]
        public string Status { get; set; }

        [NopResourceDisplayName("Plugins.Payment.B2BCustomerAccount.B2BAccount.FinancialTransaction.Fields.Remaining")]
        public decimal Remaining { get; set; }

        [NopResourceDisplayName("Plugins.Payment.B2BCustomerAccount.B2BAccount.FinancialTransaction.Fields.AmountExVat")]
        public string AmountExVat { get; set; }

        [NopResourceDisplayName("Plugins.Payments.B2BCustomerAccount.B2BAccount.FinancialTransaction.Fields.CustomerOrder")]
        public string CustomerOrder { get; set; }

        public int NopOrderId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.B2BCustomerAccount.B2BAccount.FinancialTransaction.Fields.ERPOrderNumber")]
        public string ERPOrderNumber { get; set; }

        public bool IsDocumentTypeInvoice { get; set; }
        public bool IsDocumentTypeDownloadable { get; set; }
    }
}
