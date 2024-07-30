using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using Nop.Core;
using NopStation.Plugin.Payments.B2B.Account.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using Nop.Services.Orders;
using Nop.Services.Catalog;

namespace NopStation.Plugin.Payments.B2B.Account.Components
{
    public class B2BAccountPaymentViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IErpNopUserService _erpNopUserService;
        private readonly IWorkContext _workContext;
        private readonly IErpAccountService _erpAccountService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPriceFormatter _priceFormatter;


        #endregion

        #region Ctor

        public B2BAccountPaymentViewComponent(IErpNopUserService erpNopUserService,
                                              IWorkContext workContext,
                                              IErpAccountService erpAccountService,
                                              IOrderTotalCalculationService orderTotalCalculationService,
                                              IPriceFormatter priceFormatter)
        {
            _erpNopUserService = erpNopUserService;
            _workContext = workContext;
            _erpAccountService = erpAccountService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _priceFormatter = priceFormatter;
        }

        #endregion        

        #region Methods

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new B2BAccountPaymentInfoModel();

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer == null)
                return Content(string.Empty);

            var nopErpUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer.Id);
            if (nopErpUser == null || nopErpUser.ErpUserType != ErpUserType.B2BUser)
                return Content(string.Empty);

            var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(nopErpUser.ErpAccountId);
            if (erpAccount == null)
                return Content(string.Empty);

            model.CreditLimitAvailableStr = await _priceFormatter.FormatPriceAsync(erpAccount.CreditLimitAvailable, true, false);
            model.CreditLimitStr = await _priceFormatter.FormatPriceAsync(erpAccount.CreditLimit, true, false);
            model.CurrentBalanceStr = await _priceFormatter.FormatPriceAsync(erpAccount.CurrentBalance, true, false);


            return View("~/Plugins/NopStation.Plugin.Payments.B2BAccount/Views/Shared/Components/B2BAccountPayment/Default.cshtml", model);
        }

        #endregion

    }
}
