using System.Threading.Tasks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpNopUser;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories
{
    public interface ISalesRepUserModelFactory
    {
        Task<SalesRepUserListModel> PrepareSalesRepUserListModelForSalesRep(SalesRepUserSearchModel searchModel, ErpSalesRep erpSalesRep);
        Task<SalesRepUserListModel> PreparePublicSalesRepUserListModelForSalesRep(SalesRepUserSearchModel searchModel, ErpSalesRep erpSalesRep);
        Task<ErpAccountListModel> PrepareSalesRepErpUserListModelForSalesRep(ErpAccountSearchModel searchModel, ErpSalesRep erpSalesRep);
        Task<SalesRepUserSearchModel> PrepareSalesRepUserSearchModelAsync(SalesRepUserSearchModel searchModel);
    }
}