using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.ErpAccountPublic
{
    public record ErpAccountInfoModel : BaseSearchModel
    {
        public ErpAccountInfoModel()
        {
            AvailableSortOptions = new List<SelectListItem>();
        }

        public int ErpAccountId { get; set; }
        public int ErpNopUserId { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoices.Fields.AccountNumber")]
        public string AccountNumber { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoices.Fields.AccountName")]
        public string AccountName { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoices.Fields.ErpAccountSalesOrg")]
        public int ErpAccountSalesOrgId { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoices.Fields.ErpAccountSalesOrg")]
        public string ErpAccountSalesOrgName { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoices.Fields.CreditLimit")]
        public string CreditLimit { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoices.Fields.CurrentBalance")]
        public string CurrentBalance { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoices.Fields.AvailableCredit")]
        public string AvailableCredit { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoices.Fields.LastPaymentAmount")]
        public string LastPaymentAmount { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoices.Fields.LastPaymentDate")]
        public string LastPaymentDate { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoices.Fields.CurrentYearOnlineSavings")]
        public string CurrentYearOnlineSavings { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoices.Fields.AllTimeOnlineSavings")]
        public string AllTimeOnlineSavings { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoices.Fields.SearchDocumentNumberOrName")]
        public string SearchDocumentNumberOrName { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoices.Fields.SearchTransactionFromDate")]
        [UIHint("DateNullable")]
        [DataType(DataType.Date)]
        public DateTime? SearchTransactionFromDate { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoices.Fields.SearchTransactionToDate")]
        [UIHint("DateNullable")]
        [DataType(DataType.Date)]
        public DateTime? SearchTransactionToDate { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoices.Fields.SearchSortOption")]
        public int SearchSortOptionId { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpInvoices.Fields.SearchCustomerOrder")]
        public string SearchOrderNumberOrName { get; set; }

        public IList<SelectListItem> AvailableSortOptions { get; set; }

        public bool HasErpQuoteAssistantRole { get; set; }

        public bool HasErpOrderAssistantRole { get; set; }

        public bool HasErpSalesRepRole { get; set; }

        public bool IsShowYearlySavings { get; set; }
        public bool IsShowAllTimeSavings { get; set; }

        public bool IsShowAccountStatementDownloadEnabled { get; set; }

        public bool HasErpCustomerAccountingPersonnelRole { get; set; }

        //public int BillingAddressId { get; set; }

        //public ErpBillingAddressModel ErpBillingAddress { get; set; }
    }
}
