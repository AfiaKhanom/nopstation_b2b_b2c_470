using System.Collections.Generic;
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
using Nop.Web.Models.Common;
using Nop.Web.Models.Customer;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories;

public class OverridenCustomerModelFactory : CustomerModelFactory
{
    #region Fields

    private readonly IErpAccountService _erpAccountService;
    private readonly IErpNopUserService _erpNopUserService;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly IAddressService _addressService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;

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
        IErpAccountService erpAccountService,
        IErpNopUserService erpNopUserService,
        IErpShipToAddressService erpShipToAddressService,
        IAddressService addressService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService) : base(addressSettings,
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
        _erpAccountService = erpAccountService;
        _erpNopUserService = erpNopUserService;
        _erpShipToAddressService = erpShipToAddressService;
        _addressService = addressService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
    }

    #endregion

    #region Utilites

    private async Task<List<Address>> GetCustomerAddressesByCustomerId(int customerId)
    {
        return await (await _customerService.GetAddressesByCustomerIdAsync(customerId))
                //enabled for the current store
                .WhereAwait(async a => a.CountryId == null || await _storeMappingService.AuthorizeAsync(await _countryService.GetCountryByAddressAsync(a)))
                .ToListAsync();
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

        #region Erp

        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(currCustomer.Id);
        var erpUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(currCustomer.Id);
        var isErpAccount = erpAccount is not null;
        var store = await _storeContext.GetCurrentStoreAsync();

        if (isErpAccount)
        {
            model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
            {
                RouteName = "ErpAccountOrders",
                Title = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.MyAccount.Title.ErpAccountOrders"),
                Tab = (int)CustomerNavigationEnum.Orders,
                ItemClass = "customer-orders"
            });

            if (_b2BB2CFeaturesSettings.EnableQuoteFunctionality)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "ErpQuoteOrderList",
                    Title = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpQuoteOrderList.Title.ErpQuoteOrderList"),
                    Tab = B2BB2CFeaturesDefaults.ErpCustomerNavigationEnum_ErpAccountQuoteOrders,
                    ItemClass = "b2bcustomer-quotes",
                });
            }

            model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
            {
                RouteName = "ErpAccountInvoices",
                Title = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccount.Title.ErpAccountTransactions"),
                Tab = B2BB2CFeaturesDefaults.ErpCustomerNavigationEnum_ErpAccountTransactionsInfo,
                ItemClass = "erpaccount-invoices",
            });

            if (erpUser.ErpUserType == ErpUserType.B2BUser || 
                (erpUser.ErpUserType == ErpUserType.B2CUser && !_b2BB2CFeaturesSettings.UseDefaultAccountForB2CUser))
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "ErpAccountInfo",
                    Title = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccount.Title.ErpAccountInfo"),
                    Tab = B2BB2CFeaturesDefaults.ErpCustomerNavigationEnum_ErpAccountInfo,
                    ItemClass = "erpaccount-info",
                });
            }
        }

        #endregion

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

        var customer = await _workContext.GetCurrentCustomerAsync();

        if (_orderSettings.ReturnRequestsEnabled && 
            (await _returnRequestService.SearchReturnRequestsAsync(store.Id, customer.Id, pageIndex: 0, pageSize: 1)).Any())
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

    /// <summary>
    /// Prepare the customer address list model
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the customer address list model
    /// </returns>
    public override async Task<CustomerAddressListModel> PrepareCustomerAddressListModelAsync()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var addresses = new List<Address>();

        var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);
        var erpUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer.Id);

        if (erpAccount != null)
        {
            var erpShipToAddresses = new List<ErpShipToAddress>();

            if (_b2BB2CFeaturesSettings.UseDefaultAccountForB2CUser && erpUser.ErpUserType == ErpUserType.B2CUser)
            {
                erpShipToAddresses = await _erpShipToAddressService.GetErpShipToAddressesByCustomerAddressesAsync(customerId: customer.Id, erpAccountId: erpAccount.Id);
            }
            else
            {
                erpShipToAddresses = (List<ErpShipToAddress>)await _erpShipToAddressService.GetErpShipToAddressesByAccountIdAsync(accountId: erpAccount.Id);                
            }

            foreach (var shipTo in erpShipToAddresses)
            {
                addresses.Add(await _addressService.GetAddressByIdAsync(shipTo.AddressId));
            }
        }
        else
        {
            addresses = await GetCustomerAddressesByCustomerId(customer.Id);
        }

        var model = new CustomerAddressListModel();
        foreach (var address in addresses)
        {
            var addressModel = new AddressModel();
            await _addressModelFactory.PrepareAddressModelAsync(addressModel,
                address: address,
                excludeProperties: false,
                addressSettings: _addressSettings,
                loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id));
            model.Addresses.Add(addressModel);
        }
        return model;
    }

    #endregion
}