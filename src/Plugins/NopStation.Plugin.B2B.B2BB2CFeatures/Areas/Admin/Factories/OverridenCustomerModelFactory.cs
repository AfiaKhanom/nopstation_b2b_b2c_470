using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Tax;
using Nop.Services.Affiliates;
using Nop.Services.Attributes;
using Nop.Services.Authentication.External;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Factories;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public partial class OverridenCustomerModelFactory : CustomerModelFactory, IOverridenCustomerModelFactory
{
    #region Fields

    private readonly IErpAccountService _erpAccountService;
    private readonly IErpNopUserService _erpNopUserService;

    #endregion

    #region Ctor

    public OverridenCustomerModelFactory(AddressSettings addressSettings,
        CustomerSettings customerSettings,
        DateTimeSettings dateTimeSettings,
        GdprSettings gdprSettings,
        ForumSettings forumSettings,
        IAclSupportedModelFactory aclSupportedModelFactory,
        IAttributeFormatter<AddressAttribute, AddressAttributeValue> addressAttributeFormatter,
        IAddressModelFactory addressModelFactory,
        IAffiliateService affiliateService,
        IAuthenticationPluginManager authenticationPluginManager,
        IBackInStockSubscriptionService backInStockSubscriptionService,
        IBaseAdminModelFactory baseAdminModelFactory,
        ICountryService countryService,
        ICustomerActivityService customerActivityService,
        IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
        IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService,
        ICustomerService customerService,
        IDateTimeHelper dateTimeHelper,
        IExternalAuthenticationService externalAuthenticationService,
        IGdprService gdprService,
        IGenericAttributeService genericAttributeService,
        IGeoLookupService geoLookupService,
        ILocalizationService localizationService,
        INewsLetterSubscriptionService newsLetterSubscriptionService,
        IOrderService orderService,
        IPictureService pictureService,
        IPriceFormatter priceFormatter,
        IProductAttributeFormatter productAttributeFormatter,
        IProductService productService,
        IRewardPointService rewardPointService,
        IShoppingCartService shoppingCartService,
        IStateProvinceService stateProvinceService,
        IStoreContext storeContext,
        IStoreService storeService,
        ITaxService taxService,
        MediaSettings mediaSettings,
        RewardPointsSettings rewardPointsSettings,
        TaxSettings taxSettings,
        IErpNopUserService erpNopUserService,
        IErpAccountService erpAccountService,
        IAddressService addressService,
        IWorkContext workContext) : base(
            addressSettings,
            customerSettings,
            dateTimeSettings,
            gdprSettings,
            forumSettings,
            aclSupportedModelFactory,
            addressModelFactory,
            addressService,
            affiliateService,
            addressAttributeFormatter,
            customerAttributeParser,
            customerAttributeService,
            authenticationPluginManager,
            backInStockSubscriptionService,
            baseAdminModelFactory,
            countryService,
            customerActivityService,
            customerService,
            dateTimeHelper,
            externalAuthenticationService,
            gdprService,
            genericAttributeService,
            geoLookupService,
            localizationService,
            newsLetterSubscriptionService,
            orderService,
            pictureService,
            priceFormatter,
            productAttributeFormatter,
            productService,
            rewardPointService,
            shoppingCartService,
            stateProvinceService,
            storeContext,
            storeService,
            taxService,
            workContext,
            mediaSettings,
            rewardPointsSettings,
            taxSettings)
    {
        _erpNopUserService = erpNopUserService;
        _erpAccountService = erpAccountService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Prepare customer model
    /// </summary>
    /// <param name="model">Customer model</param>
    /// <param name="customer">Customer</param>
    /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the customer model
    /// </returns>
    public override async Task<CustomerModel> PrepareCustomerModelAsync(CustomerModel model, Customer customer, bool excludeProperties = false)
    {
        if (customer != null)
        {
            //fill in model values from the entity
            model ??= new CustomerModel();

            model.Id = customer.Id;
            model.DisplayVatNumber = _taxSettings.EuVatEnabled;
            model.AllowSendingOfPrivateMessage = await _customerService.IsRegisteredAsync(customer) &&
                _forumSettings.AllowPrivateMessages;
            model.AllowSendingOfWelcomeMessage = await _customerService.IsRegisteredAsync(customer) &&
                _customerSettings.UserRegistrationType == UserRegistrationType.AdminApproval;
            model.AllowReSendingOfActivationMessage = await _customerService.IsRegisteredAsync(customer) && !customer.Active &&
                _customerSettings.UserRegistrationType == UserRegistrationType.EmailValidation;
            model.GdprEnabled = _gdprSettings.GdprEnabled;

            model.MultiFactorAuthenticationProvider = await _genericAttributeService
                .GetAttributeAsync<string>(customer, NopCustomerDefaults.SelectedMultiFactorAuthenticationProviderAttribute);

            //whether to fill in some of properties
            if (!excludeProperties)
            {
                model.Email = customer.Email;
                model.Username = customer.Username;
                model.VendorId = customer.VendorId;
                model.AdminComment = customer.AdminComment;
                model.IsTaxExempt = customer.IsTaxExempt;
                model.Active = customer.Active;
                model.FirstName = customer.FirstName;
                model.LastName = customer.LastName;
                model.Gender = customer.Gender;
                model.DateOfBirth = customer.DateOfBirth;
                model.Company = customer.Company;
                model.StreetAddress = customer.StreetAddress;
                model.StreetAddress2 = customer.StreetAddress2;
                model.ZipPostalCode = customer.ZipPostalCode;
                model.City = customer.City;
                model.County = customer.County;
                model.CountryId = customer.CountryId;
                model.StateProvinceId = customer.StateProvinceId;
                model.Phone = customer.Phone;
                model.Fax = customer.Fax;
                model.TimeZoneId = customer.TimeZoneId;
                model.VatNumber = customer.VatNumber;
                model.VatNumberStatusNote = await _localizationService.GetLocalizedEnumAsync(customer.VatNumberStatus);
                model.LastActivityDate = await _dateTimeHelper.ConvertToUserTimeAsync(customer.LastActivityDateUtc, DateTimeKind.Utc);
                model.LastIpAddress = customer.LastIpAddress;
                model.LastVisitedPage = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.LastVisitedPageAttribute);
                model.SelectedCustomerRoleIds = (await _customerService.GetCustomerRoleIdsAsync(customer)).ToList();
                model.RegisteredInStore = (await _storeService.GetAllStoresAsync())
                    .FirstOrDefault(store => store.Id == customer.RegisteredInStoreId)?.Name ?? string.Empty;
                model.DisplayRegisteredInStore = model.Id > 0 && !string.IsNullOrEmpty(model.RegisteredInStore) &&
                    (await _storeService.GetAllStoresAsync()).Select(x => x.Id).Count() > 1;
                model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(customer.CreatedOnUtc, DateTimeKind.Utc);

                //prepare model affiliate
                var affiliate = await _affiliateService.GetAffiliateByIdAsync(customer.AffiliateId);
                if (affiliate != null)
                {
                    model.AffiliateId = affiliate.Id;
                    model.AffiliateName = await _affiliateService.GetAffiliateFullNameAsync(affiliate);
                }

                //prepare model newsletter subscriptions
                if (!string.IsNullOrEmpty(customer.Email))
                {
                    model.SelectedNewsletterSubscriptionStoreIds = await (await _storeService.GetAllStoresAsync())
                        .WhereAwait(async store => await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customer.Email, store.Id) != null)
                        .Select(store => store.Id).ToListAsync();
                }
            }
            //prepare reward points model
            model.DisplayRewardPointsHistory = _rewardPointsSettings.Enabled;
            if (model.DisplayRewardPointsHistory)
                await PrepareAddRewardPointsToCustomerModelAsync(model.AddRewardPoints);

            //prepare nested search models
            PrepareRewardPointsSearchModel(model.CustomerRewardPointsSearchModel, customer);
            PrepareCustomerAddressSearchModel(model.CustomerAddressSearchModel, customer);
            PrepareCustomerOrderSearchModel(model.CustomerOrderSearchModel, customer);
            await PrepareCustomerShoppingCartSearchModelAsync(model.CustomerShoppingCartSearchModel, customer);
            PrepareCustomerActivityLogSearchModel(model.CustomerActivityLogSearchModel, customer);
            PrepareCustomerBackInStockSubscriptionSearchModel(model.CustomerBackInStockSubscriptionSearchModel, customer);
            await PrepareCustomerAssociatedExternalAuthRecordsSearchModelAsync(model.CustomerAssociatedExternalAuthRecordsSearchModel, customer);
        }
        else
        {
            //whether to fill in some of properties
            if (!excludeProperties)
            {
                //precheck Registered Role as a default role while creating a new customer through admin
                var registeredRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName);
                if (registeredRole != null)
                    model.SelectedCustomerRoleIds.Add(registeredRole.Id);
            }
        }

        model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
        model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
        model.FirstNameEnabled = _customerSettings.FirstNameEnabled;
        model.LastNameEnabled = _customerSettings.LastNameEnabled;
        model.GenderEnabled = _customerSettings.GenderEnabled;
        model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
        model.CompanyEnabled = _customerSettings.CompanyEnabled;
        model.StreetAddressEnabled = _customerSettings.StreetAddressEnabled;
        model.StreetAddress2Enabled = _customerSettings.StreetAddress2Enabled;
        model.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;
        model.CityEnabled = _customerSettings.CityEnabled;
        model.CountyEnabled = _customerSettings.CountyEnabled;
        model.CountryEnabled = _customerSettings.CountryEnabled;
        model.StateProvinceEnabled = _customerSettings.StateProvinceEnabled;
        model.PhoneEnabled = _customerSettings.PhoneEnabled;
        model.FaxEnabled = _customerSettings.FaxEnabled;

        //set default values for the new model
        if (customer == null)
        {
            model.Active = true;
            model.DisplayVatNumber = false;
        }

        //prepare available vendors
        await _baseAdminModelFactory.PrepareVendorsAsync(model.AvailableVendors,
            defaultItemText: await _localizationService.GetResourceAsync("Admin.Customers.Customers.Fields.Vendor.None"));

        #region Erp

        if (customer != null)
        {
            var isErpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id) != null;
            var erpUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer.Id);

            if (isErpAccount && erpUser.ErpUserType == ErpUserType.B2CUser)
            {
                //prepare model customer attributes
                await PrepareCustomerAttributeModelsAsync(model.CustomerAttributes, customer);
            }
        }

        #endregion            

        //prepare model stores for newsletter subscriptions
        model.AvailableNewsletterSubscriptionStores = (await _storeService.GetAllStoresAsync()).Select(store => new SelectListItem
        {
            Value = $"{store.Id}",
            Text = store.Name,
            Selected = model.SelectedNewsletterSubscriptionStoreIds.Contains(store.Id)
        }).ToList();

        //prepare model customer roles
        await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(model);

        //prepare available time zones
        await _baseAdminModelFactory.PrepareTimeZonesAsync(model.AvailableTimeZones, false);

        //prepare available countries and states
        if (_customerSettings.CountryEnabled)
        {
            await _baseAdminModelFactory.PrepareCountriesAsync(model.AvailableCountries);
            if (_customerSettings.StateProvinceEnabled)
                await _baseAdminModelFactory.PrepareStatesAndProvincesAsync(model.AvailableStates, model.CountryId == 0 ? null : (int?)model.CountryId);
        }

        return model;
    }

    public async IAsyncEnumerable<CustomerModel> PrepareCustomerModelsAsync(IPagedList<Customer> customers)
    {
        foreach (var customer in customers)
        {
            var customerModel = customer.ToModel<CustomerModel>();

            customerModel.Email = await _customerService.IsRegisteredAsync(customer)
                ? customer.Email
                : await _localizationService.GetResourceAsync("Admin.Customers.Guest");

            customerModel.FullName = await _customerService.GetCustomerFullNameAsync(customer);
            customerModel.Company = await _genericAttributeService.GetAttributeAsync<string>(customer, B2BB2CFeaturesDefaults.CompanyAttribute);
            customerModel.Phone = await _genericAttributeService.GetAttributeAsync<string>(customer, B2BB2CFeaturesDefaults.PhoneAttribute);
            customerModel.ZipPostalCode = await _genericAttributeService.GetAttributeAsync<string>(customer, B2BB2CFeaturesDefaults.ZipPostalCodeAttribute);

            customerModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(customer.CreatedOnUtc, DateTimeKind.Utc);
            customerModel.LastActivityDate = await _dateTimeHelper.ConvertToUserTimeAsync(customer.LastActivityDateUtc, DateTimeKind.Utc);

            var roles = await _customerService.GetCustomerRolesAsync(customer);
            customerModel.CustomerRoleNames = string.Join(", ", roles.Select(role => role.Name));

            yield return customerModel;
        }
    }

    #endregion
}
