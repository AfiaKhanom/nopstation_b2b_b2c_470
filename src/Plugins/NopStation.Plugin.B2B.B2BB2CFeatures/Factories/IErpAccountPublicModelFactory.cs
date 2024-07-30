using System.Threading.Tasks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.ErpAccountPublic;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories
{
    public interface IErpAccountPublicModelFactory
    {
        Task<ErpAccountPublicSearchModel> PrepareErpAccountSearchModelAsync(ErpAccountPublicSearchModel searchModel);

        Task<ErpAccountPublicListModel> PrepareErpAccountListModelAsync(ErpAccountPublicSearchModel searchModel);

        Task<RecentTransactionListModel> PrepareRecentTransactionListAsync(ErpAccountInfoModel erpAccountInfoModel);

        Task<ErpAccountInfoModel> PrepareErpAccountInfoModelAsync(ErpAccount b2BAccount, ErpAccountInfoModel model, bool enableErpAccountUpdate = false);

        Task<ErpAccountOrderSearchModel> PrepareErpAccountOrderSearchModelAsync(ErpAccount erpAccount, ErpNopUser erpNopUser, ErpAccountOrderSearchModel model);

        Task<ErpAccountOrderListModel> PrepareErpOrderListModelAsync(ErpAccountOrderSearchModel searchModel);

        Task<ErpAccountQuoteOrderSearchModel> PrepareErpAccountQuoteOrderSearchModelAsync(ErpAccount erpAccount, ErpNopUser erpNopUser, ErpAccountQuoteOrderSearchModel searchModel);

        Task<ErpQuoteOrderListModel> PrepareErpQuoteOrderListModelAsync(ErpAccountQuoteOrderSearchModel searchModel);
    }
}