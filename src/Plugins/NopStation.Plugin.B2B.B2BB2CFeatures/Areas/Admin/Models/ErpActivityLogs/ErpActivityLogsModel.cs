using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpActivityLogs
{
    /// <summary>
    /// Represents an activity log model
    /// </summary>
    public partial record ErpActivityLogsModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.ErpActivityLogsType")]
        public string ErpActivityLogTypeName { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.Customer")]
        public int CustomerId { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.CustomerEmail")]
        [DataType(DataType.EmailAddress)]
        public string CustomerEmail { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.Comment")]
        public string Comment { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.IpAddress")]
        public string IpAddress { get; set; }

        #endregion
    }
}