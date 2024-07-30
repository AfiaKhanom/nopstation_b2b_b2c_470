using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories
{
    public interface IErpNopUserModelFactory
    {
        Task<ErpNopUserSearchModel> PrepareErpNopUserSearchModelAsync(ErpNopUserSearchModel searchModel);
        Task<ErpNopUserListModel> PrepareErpNopUserListModelAsync(ErpNopUserSearchModel searchModel);
        Task<ErpNopUserAccountListModel> PrepareErpNopUserAccountListModelAsync(int nopUserId, ErpNopUserSearchModel searchModel);
        Task<List<SelectListItem>> PrepareShipToAddressDropdownAsync(int accountId);
        Task<ErpNopUserModel> PrepareErpNopUserModelAsync(ErpNopUserModel model, ErpNopUser erpNopUser);
        Task<ErpNopUserAccountMapModel> PrepareErpNopUserModelAsync(ErpNopUserAccountMapModel model, ErpNopUserAccountMap erpNopUserAccountMap);
    }
}