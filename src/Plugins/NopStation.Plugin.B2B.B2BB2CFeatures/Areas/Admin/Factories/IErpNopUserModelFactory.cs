using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Models.Customers;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public interface IErpNopUserModelFactory
{
    Task<CustomerSearchModelForErpuser> PrepareCustomerSearchModelForErpUser(CustomerSearchModelForErpuser searchModel);
    Task<CustomerListModel> PrepareCustomertListModelForErpUser(CustomerSearchModelForErpuser searchModel);
    Task<ErpNopUserSearchModel> PrepareErpNopUserSearchModelAsync(ErpNopUserSearchModel searchModel);
    Task<ErpNopUserListModel> PrepareErpNopUserListModelAsync(ErpNopUserSearchModel searchModel);
    Task<ErpNopUserAccountListModel> PrepareErpNopUserAccountListModelAsync(int nopUserId, ErpNopUserSearchModel searchModel);
    Task<List<SelectListItem>> PrepareShipToAddressDropdownAsync(int accountId, int customerId = 0);
    Task<ErpNopUserModel> PrepareErpNopUserModelAsync(ErpNopUserModel model, ErpNopUser erpNopUser);
    Task<ErpNopUserAccountMapModel> PrepareErpNopUserAccountMapModelAsync(ErpNopUserAccountMapModel model, ErpNopUserAccountMap erpNopUserAccountMap);
}