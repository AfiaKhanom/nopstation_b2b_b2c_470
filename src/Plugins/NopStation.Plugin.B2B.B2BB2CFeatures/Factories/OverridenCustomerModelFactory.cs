using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services.Attributes;
using Nop.Services.Authentication.External;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Models.Customer;
using NopStation.Plugin.B2B.B2BB2CFeatures.Infrastructure;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories
{
    public class OverridenCustomerModelFactory : CustomerModelFactory
    {
        #region Fields

        private readonly AddressSettings _addressSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly CommonSettings _commonSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;
        private readonly ForumSettings _forumSettings;
        private readonly GdprSettings _gdprSettings;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IAuthenticationPluginManager _authenticationPluginManager;
        private readonly ICountryService _countryService;
        private readonly IAttributeParser<CustomerAttribute, CustomerAttributeValue> _customerAttributeParser;
        private readonly IAttributeService<CustomerAttribute, CustomerAttributeValue> _customerAttributeService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IExternalAuthenticationService _externalAuthenticationService;
        private readonly IDownloadService _downloadService;
        private readonly IGdprService _gdprService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IMultiFactorAuthenticationPluginManager _multiFactorAuthenticationPluginManager;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IOrderService _orderService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;
        private readonly OrderSettings _orderSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly SecuritySettings _securitySettings;
        private readonly TaxSettings _taxSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly ISettingService _settingService;
        private readonly IErpAccountService _erpAccountService;
        private readonly IErpNopUserService _erpNopUserService;

        #endregion

        #region Ctor

        public OverridenCustomerModelFactory(AddressSettings addressSettings,
            CaptchaSettings captchaSettings,
            CatalogSettings catalogSettings,
            CommonSettings commonSettings,
            CustomerSettings customerSettings,
            DateTimeSettings dateTimeSettings,
            ExternalAuthenticationSettings externalAuthenticationSettings,
            ForumSettings forumSettings,
            GdprSettings gdprSettings,
            IAddressModelFactory addressModelFactory,
            IAuthenticationPluginManager authenticationPluginManager,
            ICountryService countryService,
            IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
            IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IExternalAuthenticationService externalAuthenticationService,
            IDownloadService downloadService,
            IGdprService gdprService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IOrderService orderService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IProductService productService,
            IReturnRequestService returnRequestService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            MediaSettings mediaSettings,
            OrderSettings orderSettings,
            RewardPointsSettings rewardPointsSettings,
            SecuritySettings securitySettings,
            TaxSettings taxSettings,
            VendorSettings vendorSettings,
            ISettingService settingService,
            IErpAccountService erpAccountService,
            IErpNopUserService erpNopUserService) : base(addressSettings,
            captchaSettings,
            catalogSettings,
            commonSettings,
            customerSettings,
            dateTimeSettings,
            externalAuthenticationSettings,
            forumSettings,
            gdprSettings,
            addressModelFactory,
            customerAttributeParser,
            customerAttributeService,
            authenticationPluginManager,
            countryService,
            customerService,
            dateTimeHelper,
            externalAuthenticationService,
            gdprService,
            genericAttributeService,
            localizationService,
            multiFactorAuthenticationPluginManager,
            newsLetterSubscriptionService,
            orderService,
            permissionService,
            pictureService,
            productService,
            returnRequestService,
            stateProvinceService,
            storeContext,
            storeMappingService,
            urlRecordService,
            workContext,
            mediaSettings,
            orderSettings,
            rewardPointsSettings,
            securitySettings,
            taxSettings,
            vendorSettings)
        {
            _addressSettings = addressSettings;
            _captchaSettings = captchaSettings;
            _catalogSettings = catalogSettings;
            _commonSettings = commonSettings;
            _customerSettings = customerSettings;
            _dateTimeSettings = dateTimeSettings;
            _externalAuthenticationSettings = externalAuthenticationSettings;
            _forumSettings = forumSettings;
            _gdprSettings = gdprSettings;
            _addressModelFactory = addressModelFactory;
            _authenticationPluginManager = authenticationPluginManager;
            _countryService = countryService;
            _customerAttributeParser = customerAttributeParser;
            _customerAttributeService = customerAttributeService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _externalAuthenticationService = externalAuthenticationService;
            _downloadService = downloadService;
            _gdprService = gdprService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _multiFactorAuthenticationPluginManager = multiFactorAuthenticationPluginManager;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _orderService = orderService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _productService = productService;
            _returnRequestService = returnRequestService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _orderSettings = orderSettings;
            _rewardPointsSettings = rewardPointsSettings;
            _securitySettings = securitySettings;
            _taxSettings = taxSettings;
            _vendorSettings = vendorSettings;
            _settingService = settingService;
            _erpAccountService = erpAccountService;
            _erpNopUserService = erpNopUserService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare the customer navigation model
        /// </summary>
        /// <param name="selectedTabId">Identifier of the selected tab</param>
        /// <returns>Customer navigation model</returns>
        public override async Task<CustomerNavigationModel> PrepareCustomerNavigationModelAsync(int selectedTabId = 0)
        {
            var model = new CustomerNavigationModel();
            var currCustomer = await _workContext.GetCurrentCustomerAsync();

            model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
            {
                RouteName = "CustomerInfo",
                Title = await _localizationService.GetResourceAsync("Account.CustomerInfo"),
                Tab = (int)CustomerNavigationEnum.Info,
                ItemClass = "customer-info"
            });

            model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
            {
                RouteName = "CustomerAddresses",
                Title = await _localizationService.GetResourceAsync("Account.CustomerAddresses"),
                Tab = (int)CustomerNavigationEnum.Addresses,
                ItemClass = "customer-addresses"
            });

            #region Erp

            var isErpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(currCustomer.Id) != null;
            var erpUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(currCustomer.Id);
            var store = await _storeContext.GetCurrentStoreAsync();
            var b2BB2CFeaturesSettings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(store.Id);

            model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
            {
                RouteName = "ErpAccountOrders",
                Title = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.MyAccount.Title.ErpAccountOrders"),
                Tab = (int)CustomerNavigationEnum.Orders,
                ItemClass = "customer-orders"
            });

            if (isErpAccount)
            {
                if (erpUser.ErpUserType == ErpUserType.B2BUser || (erpUser.ErpUserType == ErpUserType.B2CUser && !b2BB2CFeaturesSettings.UseDefaultAccountForB2CUser))
                {
                    model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                    {
                        RouteName = "ErpAccountInfo",
                        Title = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccount.Title.ErpAccountInfo"),
                        Tab = B2BB2CFeaturesDefaults.ErpCustomerNavigationEnum_ErpAccountInfo,
                        ItemClass = "erpaccount-info",
                    });
                }
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "ErpAccountInvoices",
                    Title = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccount.Title.ErpAccountTransactions"),
                    Tab = B2BB2CFeaturesDefaults.ErpCustomerNavigationEnum_ErpAccountTransactionsInfo,
                    ItemClass = "erpaccount-invoices",
                });
                if (erpUser.ErpUserType == ErpUserType.B2BUser)
                {
                    model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                    {
                        RouteName = "ErpQuoteOrderList",
                        Title = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpQuoteOrderList.Title.ErpQuoteOrderList"),
                        Tab = B2BB2CFeaturesDefaults.ErpCustomerNavigationEnum_ErpAccountQuoteOrders,
                        ItemClass = "b2bcustomer-quotes",
                    });
                }
            }

            #endregion

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (_orderSettings.ReturnRequestsEnabled &&
                (await _returnRequestService.SearchReturnRequestsAsync(store.Id,
                    customer.Id, pageIndex: 0, pageSize: 1)).Any())
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CustomerReturnRequests",
                    Title = await _localizationService.GetResourceAsync("Account.CustomerReturnRequests"),
                    Tab = (int)CustomerNavigationEnum.ReturnRequests,
                    ItemClass = "return-requests"
                });
            }

            if (!_customerSettings.HideDownloadableProductsTab)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CustomerDownloadableProducts",
                    Title = await _localizationService.GetResourceAsync("Account.DownloadableProducts"),
                    Tab = (int)CustomerNavigationEnum.DownloadableProducts,
                    ItemClass = "downloadable-products"
                });
            }

            if (!_customerSettings.HideBackInStockSubscriptionsTab)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CustomerBackInStockSubscriptions",
                    Title = await _localizationService.GetResourceAsync("Account.BackInStockSubscriptions"),
                    Tab = (int)CustomerNavigationEnum.BackInStockSubscriptions,
                    ItemClass = "back-in-stock-subscriptions"
                });
            }

            if (_rewardPointsSettings.Enabled)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CustomerRewardPoints",
                    Title = await _localizationService.GetResourceAsync("Account.RewardPoints"),
                    Tab = (int)CustomerNavigationEnum.RewardPoints,
                    ItemClass = "reward-points"
                });
            }

            model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
            {
                RouteName = "CustomerChangePassword",
                Title = await _localizationService.GetResourceAsync("Account.ChangePassword"),
                Tab = (int)CustomerNavigationEnum.ChangePassword,
                ItemClass = "change-password"
            });

            if (_customerSettings.AllowCustomersToUploadAvatars)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CustomerAvatar",
                    Title = await _localizationService.GetResourceAsync("Account.Avatar"),
                    Tab = (int)CustomerNavigationEnum.Avatar,
                    ItemClass = "customer-avatar"
                });
            }

            if (_forumSettings.ForumsEnabled && _forumSettings.AllowCustomersToManageSubscriptions)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CustomerForumSubscriptions",
                    Title = await _localizationService.GetResourceAsync("Account.ForumSubscriptions"),
                    Tab = (int)CustomerNavigationEnum.ForumSubscriptions,
                    ItemClass = "forum-subscriptions"
                });
            }
            if (_catalogSettings.ShowProductReviewsTabOnAccountPage)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CustomerProductReviews",
                    Title = await _localizationService.GetResourceAsync("Account.CustomerProductReviews"),
                    Tab = (int)CustomerNavigationEnum.ProductReviews,
                    ItemClass = "customer-reviews"
                });
            }
            if (_vendorSettings.AllowVendorsToEditInfo && await _workContext.GetCurrentVendorAsync() != null)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CustomerVendorInfo",
                    Title = await _localizationService.GetResourceAsync("Account.VendorInfo"),
                    Tab = (int)CustomerNavigationEnum.VendorInfo,
                    ItemClass = "customer-vendor-info"
                });
            }
            if (_gdprSettings.GdprEnabled)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "GdprTools",
                    Title = await _localizationService.GetResourceAsync("Account.Gdpr"),
                    Tab = (int)CustomerNavigationEnum.GdprTools,
                    ItemClass = "customer-gdpr"
                });
            }

            if (_captchaSettings.Enabled && _customerSettings.AllowCustomersToCheckGiftCardBalance)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CheckGiftCardBalance",
                    Title = await _localizationService.GetResourceAsync("CheckGiftCardBalance"),
                    Tab = (int)CustomerNavigationEnum.CheckGiftCardBalance,
                    ItemClass = "customer-check-gift-card-balance"
                });
            }

            if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableMultiFactorAuthentication) &&
                await _multiFactorAuthenticationPluginManager.HasActivePluginsAsync())
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "MultiFactorAuthenticationSettings",
                    Title = await _localizationService.GetResourceAsync("PageTitle.MultiFactorAuthentication"),
                    Tab = (int)CustomerNavigationEnum.MultiFactorAuthentication,
                    ItemClass = "customer-multiFactor-authentication"
                });
            }


            model.CustomProperties.Add("isErpAccount", isErpAccount.ToString());
            if (isErpAccount && selectedTabId > B2BB2CFeaturesDefaults.ErpCustomerNavigationEnum_ErpCompareValue)
            {
                model.CustomProperties.Add("ErpSelectedTab", selectedTabId.ToString());
                model.SelectedTab = selectedTabId;
            }
            else
            {
                model.SelectedTab = selectedTabId;
            }

            return model;
        }

        #endregion
    }
}