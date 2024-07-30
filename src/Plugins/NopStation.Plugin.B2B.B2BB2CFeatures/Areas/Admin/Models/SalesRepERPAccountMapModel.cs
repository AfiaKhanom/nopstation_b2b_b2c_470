using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models
{
    public record SalesRepERPAccountMapModel : BaseNopModel
    {
        #region Ctor

        public SalesRepERPAccountMapModel()
        {
            SelectedErpAccountIds = new List<int>();
        }

        #endregion

        #region Properties

        public int SalesRepId { get; set; }
        public IList<int> SelectedErpAccountIds { get; set; }

        #endregion
    }
}