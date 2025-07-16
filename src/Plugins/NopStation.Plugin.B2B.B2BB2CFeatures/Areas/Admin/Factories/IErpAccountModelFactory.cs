using System.Threading.Tasks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public interface IErpAccountModelFactory
{
    Task<ErpAccountSearchModel> PrepareErpAccountSearchModelAsync(ErpAccountSearchModel searchModel);
    Task<ErpAccountListModel> PrepareErpAccountListModelAsync(ErpAccountSearchModel searchModel);
    Task<ErpAccountModel> PrepareErpAccountModelAsync(ErpAccountModel model, ErpAccount erpAccount);
}