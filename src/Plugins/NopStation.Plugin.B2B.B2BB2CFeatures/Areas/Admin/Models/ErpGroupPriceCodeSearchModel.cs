using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models
{
    public record ErpGroupPriceCodeSearchModel : BaseSearchModel
    {
        #region Ctor

        public ErpGroupPriceCodeSearchModel()
        {
            ShowInActiveOption = new List<SelectListItem>();
        }

        #endregion

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPriceCode.Fields.SearchGroupPriceCode")]
        public string SearchGroupPriceCode { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPriceCode.Fields.Show")]
        public int ShowInActive { get; set; }

        public IList<SelectListItem> ShowInActiveOption { get; set; }
    }
}
