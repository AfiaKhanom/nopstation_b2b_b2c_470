using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Account;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Components
{
    public class PublicHeaderViewComponent : NopViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly IErpNopUserAccountMapService _erpNopUserAccountMapService;
        private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
        private readonly ILocalizationService _localizationService;
        private readonly IErpAccountService _erpAccountService;
        private readonly IErpSalesRepService _erpSalesRepService;

        public PublicHeaderViewComponent(IWorkContext workContext,
            IErpNopUserAccountMapService erpNopUserAccountMapService,
            IErpCustomerFunctionalityService erpCustomerFunctionalityService,
            ILocalizationService localizationService,
            IErpAccountService erpAccountService,
            IErpSalesRepService erpSalesRepService)
        {
            _workContext = workContext;
            _erpNopUserAccountMapService = erpNopUserAccountMapService;
            _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
            _localizationService = localizationService;
            _erpAccountService = erpAccountService;
            _erpSalesRepService = erpSalesRepService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var model = new AccountSwitchModel();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var erpNopUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(customer);
            var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(customer);
            if (erpNopUser != null && erpAccount != null)
            {
                var mappedAccounts = await _erpNopUserAccountMapService.GetAllErpNopUserAccountMapsByUserIdAsync(erpNopUser.Id);

                #region Check Sales Rep Erp account

                var salesRep = (await _erpSalesRepService.GetErpSalesRepsByNopCustomerIdAsync(erpNopUser.NopCustomerId)).FirstOrDefault();
                if (salesRep != null && salesRep.IsActive && !salesRep.IsDeleted && salesRep.SalesRepTypeId == (int)SalesRepType.MultiBuyers)
                {
                    var erpAccountIdMaps = await _erpAccountService.GetAllErpAccountsBySalesRepIdAsync(salesRep.Id.ToString());
                    mappedAccounts = mappedAccounts.Where(x => erpAccountIdMaps.Any(y => y.ErpAccountId == x.ErpAccountId)).ToList();
                }

                #endregion

                model.CustomerId = customer.Id;
                model.ErpAccountId = erpAccount.Id;
                model.RedirectUrl = Request.Path + Request.QueryString;
                model.AvailableErpAccounts.Add(new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.List.Select"),
                    Value = "0"
                });

                foreach (var map in mappedAccounts)
                {
                    var account = await _erpAccountService.GetErpAccountByIdAsync(map.ErpAccountId);
                    if (account != null)
                    {
                        model.AvailableErpAccounts.Add(new SelectListItem
                        {
                            Text = account.AccountName ?? "",
                            Value = account.Id.ToString(),
                            Selected = (erpAccount.Id == account.Id)
                        });
                    }
                }

            }
            return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/Shared/Components/PublicHeader/Default.cshtml", model);

        }
    }
}