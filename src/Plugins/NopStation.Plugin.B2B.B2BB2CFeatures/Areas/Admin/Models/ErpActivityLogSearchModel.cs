using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models
{
    public record ErpActivityLogSearchModel : BaseSearchModel
    {
        #region Ctor

        public ErpActivityLogSearchModel()
        {
            AvailableErpSyncLabel = new List<SelectListItem>();
            AvailableActivityType = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpActivityLog.Field.ErpLogLevelId")]
        public int ActivityLogLevelId { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpActivityLog.Field.ErpSyncLabelId")]
        public int ErpSyncLabelId { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpActivityLog.Field.CreatedFrom")]
        [UIHint("DateNullable")]
        public DateTime? CreatedFrom { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpActivityLog.Field.CreatedTo")]
        [UIHint("DateNullable")]
        public DateTime? CreatedTo { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpActivityLog.Field.IpAddress")]
        public string IpAddress { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpActivityLog.Field.NopCustomerEmail")]
        public string NopCustomerEmail { get; set; }

        public IList<SelectListItem> AvailableActivityType { get; set; }
        public IList<SelectListItem> AvailableErpSyncLabel { get; set; }

        #endregion
    }
}
