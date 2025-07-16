using System.Threading.Tasks;
using Nop.Web.Areas.Admin.Models.Orders;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public interface IErpOrderModelFactory
{
    Task<ErpOrderAdditionalDataSearchModel> PrepareErpOrderPerAccountSearchModel(ErpOrderAdditionalDataSearchModel orderSearchModel);

    Task<ErpOrderAdditionalDataListModel> PrepareErpOrderPerAccountListModel(ErpOrderAdditionalDataSearchModel orderSearchModel);

    Task<ErpOrderAdditionalDataModel> PrepareErpOrderPerAccountModel(ErpOrderAdditionalDataModel model, ErpOrderAdditionalData erpOrderAdditionalData);

    Task<ErpOrderModel> PrepareErpOrderModel(ErpOrderModel erpOrderModel, OrderModel orderModel);
}