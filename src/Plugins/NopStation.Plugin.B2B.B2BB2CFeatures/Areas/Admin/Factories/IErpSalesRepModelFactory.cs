using System.Threading.Tasks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpSalesRep;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public interface IErpSalesRepModelFactory
{
    Task<ErpSalesRepListModel> PrepareErpSalesRepListModelAsync(ErpSalesRepSearchModel searchModel);
    Task<ErpSalesRepModel> PrepareErpSalesRepModelAsync(ErpSalesRepModel model, ErpSalesRep erpSalesRep);
    Task<ErpSalesRepSearchModel> PrepareErpSalesRepSearchModelAsync(ErpSalesRepSearchModel searchModel);
}