using System.Threading.Tasks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpActivityLogs;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

/// <summary>
/// Represents the erp activity logs model factory
/// </summary>
public partial interface IErpActivityLogsModelFactory
{
    /// <summary>
    /// Prepare erp activity logs search model
    /// </summary>
    /// <param name="searchModel">Erp Activity logs search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the erp activity logs search model
    /// </returns>
    Task<ErpActivityLogsSearchModel> PrepareErpActivityLogsSearchModelAsync(ErpActivityLogsSearchModel searchModel);

    /// <summary>
    /// Prepare erp activity logs types search model
    /// </summary>
    /// <param name="searchModel">Erp Activity logs Types search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the erp activity logs types search model
    /// </returns>
    Task<ErpActivityLogsTypeSearchModel> PrepareErpActivityLogsTypeSearchModelAsync(ErpActivityLogsTypeSearchModel searchModel);

    /// <summary>
    /// Prepare paged erp activity logs list model
    /// </summary>
    /// <param name="searchModel">Erp Activity logs search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the erp activity logs list model
    /// </returns>
    Task<ErpActivityLogsListModel> PrepareErpActivityLogsListModelAsync(ErpActivityLogsSearchModel searchModel);
}