using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.ErpAccountPublic
{
    public record RecentTransactionSearchModel : BaseNopEntityModel
    {
        #region ctor
        public RecentTransactionSearchModel()
        {
            AvailableSortOptions = new List<SelectListItem>();
        }

        #endregion

        [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.B2BAccountInfo.Fields.SearchDocumentNumberOrName")]
        public string SearchDocumentNumberOrName { get; set; }

        [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.B2BAccountInfo.Fields.SearchTransactionDate")]
        public DateTime? SearchTransactionDate { get; set; }

        [NopResourceDisplayName("NopStation.Plugin.B2B.B2BB2CFeatures.B2BAccountInfo.Fields.SearchSortOption")]
        public int SearchSortOptionId { get; set; }

        public IList<SelectListItem> AvailableSortOptions { get; set; }

        public string ERPOrderNumber { get; set; }

        public int ErpAccountId { get; set; }
        public int ErpNopUserId { get; set; }
    }
}
