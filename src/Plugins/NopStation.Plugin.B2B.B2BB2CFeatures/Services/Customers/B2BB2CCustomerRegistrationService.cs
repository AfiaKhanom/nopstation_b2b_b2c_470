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

        private readonly CustomerSettings _customerSettings;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IEncryptionService _encryptionService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IMultiFactorAuthenticationPluginManager _multiFactorAuthenticationPluginManager;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IRewardPointService _rewardPointService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly IB2BB2CWorkContext _b2BB2CWorkContext;
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
            IB2BB2CWorkContext b2BB2CWorkContext,
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
            _customerSettings = customerSettings;
            _actionContextAccessor = actionContextAccessor;
            _authenticationService = authenticationService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _encryptionService = encryptionService;
            _eventPublisher = eventPublisher;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _multiFactorAuthenticationPluginManager = multiFactorAuthenticationPluginManager;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _rewardPointService = rewardPointService;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _storeService = storeService;
            _urlHelperFactory = urlHelperFactory;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _rewardPointsSettings = rewardPointsSettings;
            _b2BB2CWorkContext = b2BB2CWorkContext;
            _erpActivityLogsService = erpActivityLogsService;
            _erpLogsService = erpLogsService;
        }

        #endregion

        #region Methods

        public override async Task<IActionResult> SignInCustomerAsync(Customer customer, string returnUrl, bool isPersist = false)
        {
            var currentCustomer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();
            if (currentCustomer?.Id != customer.Id)
            {
                //migrate shopping cart
                await _shoppingCartService.MigrateShoppingCartAsync(currentCustomer, customer, true);

                await _b2BB2CWorkContext.SetCurrentCustomerAsync(customer);
            }

            if (customer.Active)
            {
                await _erpLogsService.InformationAsync($"Customer Logged in as: {customer.Email}, Customer Id: {customer.Id}", ErpSyncLavel.LoginLogout, customer: customer);

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