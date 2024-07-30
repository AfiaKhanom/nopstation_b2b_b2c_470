using System.Threading.Tasks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories
{
    public interface IErpSalesOrgModelFactory
    {
        Task<ErpSalesOrgSearchModel> PrepareErpSalesOrgSearchModelAsync(ErpSalesOrgSearchModel searchModel);
        Task<ErpSalesOrgListModel> PrepareErpSalesOrgListModelAsync(ErpSalesOrgSearchModel searchModel);
        Task<ErpSalesOrgModel> PrepareErpSalesOrgModelAsync(ErpSalesOrgModel model, ErpSalesOrg erpSalesOrg);
        Task<ErpSalesOrgWarehouseListModel> PrepareErpSalesOrgWarehouseListModel(ErpSalesOrgWarehouseSearchModel searchModel);
    }
}