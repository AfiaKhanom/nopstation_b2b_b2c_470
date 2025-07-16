using Nop.Web.Framework.Models;
using System.Collections.Generic;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpActivityLogs;

/// <summary>
/// Represents an erp activity logs type search model
/// </summary>
public partial record ErpActivityLogsTypeSearchModel : BaseSearchModel
{
    #region Properties       

    public IList<ErpActivityLogsTypeModel> ErpActivityLogsTypeListModel { get; set; }

    #endregion
}