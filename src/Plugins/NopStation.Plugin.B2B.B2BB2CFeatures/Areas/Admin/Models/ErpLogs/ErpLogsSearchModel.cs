using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpLogs;

public record ErpLogsSearchModel : BaseSearchModel
{
    #region Ctor

    public ErpLogsSearchModel()
    {
        AvailableErpSyncLabel = new List<SelectListItem>();
        AvailableLogType = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpLogs.Field.ErpLogLevelId")]
    public int ErpLogLevelId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpLogs.Field.ErpSyncLabelId")]
    public int ErpSyncLabelId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpLogs.Field.CreatedFrom")]
    [UIHint("DateNullable")]
    public DateTime? CreatedFrom { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpLogs.Field.CreatedTo")]
    [UIHint("DateNullable")]
    public DateTime? CreatedTo { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpLogs.Field.IpAddress")]
    public string IpAddress { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpLogs.Field.NopCustomerEmail")]
    public string NopCustomerEmail { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpLogs.Field.ShortMessage")]
    public string ShortMessage { get; set; }

    public IList<SelectListItem> AvailableLogType { get; set; }
    public IList<SelectListItem> AvailableErpSyncLabel { get; set; }

    #endregion
}
