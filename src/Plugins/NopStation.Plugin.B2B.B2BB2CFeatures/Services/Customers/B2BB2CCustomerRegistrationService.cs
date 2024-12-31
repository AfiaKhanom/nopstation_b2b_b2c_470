using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using Nop.Services.Authentication;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.Customers
{
    public class B2BB2CCustomerRegistrationService : CustomerRegistrationService
    {
        #region Fields


        private readonly IErpActivityLogsService _erpActivityLogsService;
        private readonly IErpLogsService _erpLogsService;

        #endregion

        #region Ctor

        public B2BB2CCustomerRegistrationService(CustomerSettings customerSettings,
            IActionContextAccessor actionContextAccessor,
            IAuthenticationService authenticationService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IEncryptionService encryptionService,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IRewardPointService rewardPointService,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IStoreService storeService,
            IUrlHelperFactory urlHelperFactory,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            RewardPointsSettings rewardPointsSettings,
            IErpActivityLogsService erpActivityLogsService,
            IErpLogsService erpLogsService) :
            base(
                 customerSettings,
                 actionContextAccessor,
                 authenticationService,
                 customerActivityService,
                 customerService,
                 encryptionService,
                 eventPublisher,
                 genericAttributeService,
                 localizationService,
                 multiFactorAuthenticationPluginManager,
                 newsLetterSubscriptionService,
                 notificationService,
                 permissionService,
                 rewardPointService,
                 shoppingCartService,
                 storeContext,
                 storeService,
                 urlHelperFactory,
                 workContext,
                 workflowMessageService,
                 rewardPointsSettings
                )
        {
            _erpActivityLogsService = erpActivityLogsService;
            _erpLogsService = erpLogsService;
        }

        #endregion

        #region Methods

        public override async Task<IActionResult> SignInCustomerAsync(Customer customer, string returnUrl, bool isPersist = false)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            if (currentCustomer?.Id != customer.Id)
            {
                //migrate shopping cart
                await _shoppingCartService.MigrateShoppingCartAsync(currentCustomer, customer, true);

                await _workContext.SetCurrentCustomerAsync(customer);
            }

            if (customer.Active)
            {
                await _erpLogsService.InformationAsync($"Customer Logged in as: {customer.Email}, Customer Id: {customer.Id}", ErpSyncLevel.LoginLogout, customer: customer);

                if (!await _customerService.IsAdminAsync(customer))
                {
                    //erp activity log
                    await _erpActivityLogsService.InsertErpActivityAsync(customer, "Erp_ErpCustomerPublicStoreLogin",
                        string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.CustomerPublicStoreLogin"),
                    customer.Email, customer.Id), customer);
                }
                else
                {
                    //activity log
                    await _customerActivityService.InsertActivityAsync(customer, "PublicStore.Login",
                        await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Login"), customer);
                }
            }

            //sign in new customer
            await _authenticationService.SignInAsync(customer, isPersist);

            //raise event       
            await _eventPublisher.PublishAsync(new CustomerLoggedinEvent(customer));

            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            //redirect to the return URL if it's specified
            if (!string.IsNullOrEmpty(returnUrl) && urlHelper.IsLocalUrl(returnUrl))
                return new RedirectResult(returnUrl);

            return new RedirectToRouteResult("Homepage", null);
        }

        #endregion
    }
}