using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpSalesRep
{
    public record ErpSalesRepSearchModel: BaseSearchModel
    {
        public ErpSalesRepSearchModel()
        {
            SelectedSalesOrgIds = new List<int>();
            AvailableSalesReps = new List<SelectListItem>();
            AvailableSalesRepType = new List<SelectListItem>();
            AvailableSalesOrgs = new List<SelectListItem>();
        }

        [NopResourceDisplayName("B2BB2CFeatures.ErpSalesRep.Fields.NopCustomerId")]
        public int NopCustomerId { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpSalesRep.Fields.SalesRepTypeId")]
        public int SalesRepTypeId { get; set; }

        [NopResourceDisplayName("B2BB2CFeatures.ErpSalesRep.Fields.SalesOrgIds")]
        public IList<int> SelectedSalesOrgIds { get; set; }

        public IList<SelectListItem> AvailableSalesReps { get; set; }
        public IList<SelectListItem> AvailableSalesRepType { get; set; }
        public IList<SelectListItem> AvailableSalesOrgs { get; set; }
    }
}
