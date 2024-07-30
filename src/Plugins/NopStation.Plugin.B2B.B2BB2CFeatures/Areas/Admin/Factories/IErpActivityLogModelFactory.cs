using System.Threading.Tasks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories
{
    public interface IErpActivityLogModelFactory
    {
        Task<ErpActivityLogSearchModel> PrepareErpActivityLogSearchModelAsync(ErpActivityLogSearchModel searchModel);
        Task<ErpActivityLogListModel> PrepareErpActivityLogListModelAsync(ErpActivityLogSearchModel searchModel);
        Task<ErpActivityLogModel> PrepareErpActivityLogModelAsync(ErpActivityLogModel model, ErpLogs log, bool excludeProperties = false);
    }
}