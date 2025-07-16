using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpLogs;

public record ErpLogsModel : BaseNopEntityModel
{
    #region Properties

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpLogs.Field.ErpLogLevel")]
    public string ErpLogLevel { get; set; }

    public int ErpLogLevelId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpLogs.Field.ErpSyncLevel")]
    public string ErpSyncLevel { get; set; }

    public int ErpSyncLevelId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpLogs.Field.CreatedOnUtc")]
    public DateTime CreatedOnUtc { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpLogs.Field.CustomerEmail")]
    public string ChangedByCustomerEmail { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpLogs.Field.CustomerId")]
    public int CustomerId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpLogs.Field.ShortMessage")]
    public string ShortMessage { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpLogs.Field.FullMessage")]
    public string FullMessage { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpLogs.Field.IpAddress")]
    public string IpAddress { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpLogs.Field.PageUrl")]
    public string PageUrl { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpLogs.Field.ReferrerUrl")]
    public string ReferrerUrl { get; set; }

    #endregion
}
