using System.Threading.Tasks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpLogs;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public interface IErpLogsModelFactory
{
    Task<ErpLogsSearchModel> PrepareErpLogsSearchModelAsync(ErpLogsSearchModel searchModel);
    Task<ErpLogsListModel> PrepareErpLogsListModelAsync(ErpLogsSearchModel searchModel);
    Task<ErpLogsModel> PrepareErpLogsModelAsync(ErpLogsModel model, ErpLogs log, bool excludeProperties = false);
}