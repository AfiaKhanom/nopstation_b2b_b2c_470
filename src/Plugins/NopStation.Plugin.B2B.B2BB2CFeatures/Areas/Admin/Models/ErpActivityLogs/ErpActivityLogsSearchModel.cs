using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpActivityLogs;

/// <summary>
/// Represents an activity log search model
/// </summary>
public partial record ErpActivityLogsSearchModel : BaseSearchModel
{
    #region Ctor

    public ErpActivityLogsSearchModel()
    {
        ErpActivityLogsType = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.CreatedOnFrom")]
    [UIHint("DateNullable")]
    public DateTime? CreatedOnFrom { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.CreatedOnTo")]
    [UIHint("DateNullable")]
    public DateTime? CreatedOnTo { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.ErpActivityLogsType")]
    public int ErpActivityLogsTypeId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.ErpActivityLogsTypes")]
    public IList<SelectListItem> ErpActivityLogsType { get; set; }
    
    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.Fields.IpAddress")]
    public string IpAddress { get; set; }

    #endregion
}