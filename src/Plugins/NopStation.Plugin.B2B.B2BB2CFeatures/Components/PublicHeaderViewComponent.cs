using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Account;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Components;

public class PublicHeaderViewComponent : NopViewComponent
{
    private readonly IB2BB2CWorkContext _b2BB2CWorkContext;
    private readonly IErpNopUserAccountMapService _erpNopUserAccountMapService;
    private readonly ILocalizationService _localizationService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSalesRepService _erpSalesRepService;

    public PublicHeaderViewComponent(IB2BB2CWorkContext b2BB2CWorkContext,
        IErpNopUserAccountMapService erpNopUserAccountMapService,
        ILocalizationService localizationService,
        IErpAccountService erpAccountService,
        IErpSalesRepService erpSalesRepService)
    {
        _b2BB2CWorkContext = b2BB2CWorkContext;
        _erpNopUserAccountMapService = erpNopUserAccountMapService;
        _localizationService = localizationService;
        _erpAccountService = erpAccountService;
        _erpSalesRepService = erpSalesRepService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var model = new AccountSwitchModel();
        var erpCustomer = await _b2BB2CWorkContext.GetCurrentERPCustomerAsync();
        if (erpCustomer.ErpNopUser != null && erpCustomer.ErpAccount != null)
        {
            var mappedAccounts = await _erpNopUserAccountMapService.GetAllErpNopUserAccountMapsByUserIdAsync(erpCustomer.ErpNopUser.Id);

            #region Check Sales Rep Erp account

            var salesRep = (await _erpSalesRepService.GetErpSalesRepsByNopCustomerIdAsync(erpCustomer.OriginalCustomer.Id)).FirstOrDefault();
            if (salesRep != null && salesRep.IsActive && !salesRep.IsDeleted && salesRep.SalesRepTypeId == (int)SalesRepType.MultiBuyers)
            {
                var erpAccountIdMaps = await _erpAccountService.GetAllErpAccountsBySalesRepIdAsync(salesRep.Id.ToString());
                mappedAccounts = mappedAccounts.Where(x => erpAccountIdMaps.Any(y => y.ErpAccountId == x.ErpAccountId)).ToList();
            }

            #endregion

            model.CustomerId = erpCustomer.Customer.Id;
            model.ErpAccountId = erpCustomer.ErpAccount.Id;
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
                        Selected = erpCustomer.ErpAccount.Id == account.Id
                    });
                }
            }

        }
        return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/Shared/Components/PublicHeader/Default.cshtml", model);

    }
}