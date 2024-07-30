using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.ErpAccountPublic
{
    public record ErpAccountPublicSearchModel : BaseSearchModel
    {
        #region ctor
        public ErpAccountPublicSearchModel()
        {
            AvailableSortOptions = new List<SelectListItem>();
        }
        #endregion

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccountPubLic.Fields.SearchDocumentNumberOrName")]
        public string SearchDocumentNumberOrName { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccountPubLic.Fields.SearchTransactionDate")]
        public DateTime? SearchTransactionDate { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccountPubLic.Fields.SearchSortOption")]
        public int SearchSortOptionId { get; set; }

        public IList<SelectListItem> AvailableSortOptions { get; set; }

        public string ERPOrderNumber { get; set; }

        public int ErpAccountId { get; set; }
        public int ErpNopUserId { get; set; }
    }
}
