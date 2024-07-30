using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Infrastructure;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Pickup;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Models.Checkout;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.B2BB2CFeatures.Infrastructure;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Checkout;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpSpecificationAttributeService;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories
{
    public class ErpCheckoutModelFactory : IErpCheckoutModelFactory
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly OrderSettings _orderSettings;
        private readonly IAddressService _addressService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICustomerService _customerService;
        private readonly ICurrencyService _currencyService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly AddressSettings _addressSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IErpAccountService _erpAccountService;
        private readonly IErpSalesOrgService _erpSalesOrgService;
        private readonly ISettingService _settingService;
        private readonly IShippingService _shippingService;
        private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
        private readonly IB2BB2CWorkContext _b2BB2CWorkContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IErpShipToAddressService _erpShipToAddressService;
        private readonly ShippingSettings _shippingSettings;
        private readonly IPickupPluginManager _pickupPluginManager;
        private readonly ITaxService _taxService;
        private readonly IShippingPluginManager _shippingPluginManager;
        private readonly CaptchaSettings _captchaSettings;

        #endregion

        #region Ctor

        public ErpCheckoutModelFactory(IWorkContext workContext,
            CatalogSettings catalogSettings,
            ILocalizationService localizationService,
            OrderSettings orderSettings,
            IDateTimeHelper dateTimeHelper,
            IAddressService addressService,
            IStoreService storeService,
            IPriceFormatter priceFormatter,
            ICustomerService customerService,
            IAffiliateService affiliateService,
            ICurrencyService currencyService,
            CurrencySettings currencySettings,
            TaxSettings taxSettings,
            IOrderService orderService,
            IOrderReportService orderReportService,
            IDiscountService discountService,
            IRewardPointService rewardPointService,
            IGiftCardService giftCardService,
            IProductService productService,
            IVendorService vendorService,
            IPictureService pictureService,
            IDownloadService downloadService,
            IReturnRequestService returnRequestService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IAddressModelFactory addressModelFactory,
            AddressSettings addressSettings,
            IProductAttributeService productAttributeService,
            ILogger logger,
            IStoreContext storeContext,
            IStaticCacheManager staticCacheManager,
            IGenericAttributeService genericAttributeService,
            IErpAccountService erpAccountService,
            IErpSalesOrgService erpSalesOrgService,
            IErpInvoiceService erpInvoiceService,
            IErpOrderAdditionalDataService erpOrderAdditionalDataService,
            ISettingService settingService,
            IPermissionService permissionService,
            IShippingService shippingService,
            IErpWarehouseAdditionalDataService erpWarehouseAdditionalDataService,
            IErpWarehouseSalesOrgMapService erpWarehouseSalesOrgMapService,
            IErpCustomerFunctionalityService erpCustomerFunctionalityService,
            IB2BB2CWorkContext b2BB2CWorkContext,
            IErpNopUserService erpNopUserService,
            IUrlRecordService urlRecordService,
            IErpSpecialPriceService erpSpecialPriceService,
            IShoppingCartService shoppingCartService,
            IPriceCalculationService priceCalculationService,
            ICategoryService categoryService,
            IErpShipToAddressService erpShipToAddressService,
            ShippingSettings shippingSettings,
            IPickupPluginManager pickupPluginManager,
            ITaxService taxService,
            IShippingPluginManager shippingPluginManager,
            IShoppingCartModelFactory shoppingCartModelFactory,
            IErpSpecificationAttributeService erpSpecificationAttributeService,
            CaptchaSettings captchaSettings
            )
        {
            _workContext = workContext;
            _localizationService = localizationService;
            _orderSettings = orderSettings;
            _addressService = addressService;
            _priceFormatter = priceFormatter;
            _customerService = customerService;
            _currencyService = currencyService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _addressSettings = addressSettings;
            _storeContext = storeContext;
            _genericAttributeService = genericAttributeService;
            _erpAccountService = erpAccountService;
            _erpSalesOrgService = erpSalesOrgService;
            _settingService = settingService;
            _shippingService = shippingService;
            _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
            _b2BB2CWorkContext = b2BB2CWorkContext;
            _shoppingCartService = shoppingCartService;
            _erpShipToAddressService = erpShipToAddressService;
            _shippingSettings = shippingSettings;
            _pickupPluginManager = pickupPluginManager;
            _taxService = taxService;
            _shippingPluginManager = shippingPluginManager;
            _captchaSettings = captchaSettings;
        }

        #endregion

        #region Method

        public async Task PrepareB2BShipToAddressModelAsync(ErpShipToAddressModelForCheckout b2BShipToAddressModel, ErpShipToAddress b2BShipToAddress, ErpAccount b2BAccount,
           bool loadAvailableAreas = false, bool loadCountriesAndStates = false)
        {
            if (b2BShipToAddress != null)
            {
                var currentCustomer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();
                var shipToAddressAccountMap = await _erpShipToAddressService.GetErpShipToAddressErpAccountMapByErpShipToAddressIdAsync(b2BShipToAddress.Id);
                var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(shipToAddressAccountMap.ErpAccountId);
                b2BShipToAddressModel = b2BShipToAddressModel ?? new ErpShipToAddressModelForCheckout();
                b2BShipToAddressModel.Id = b2BShipToAddress.Id;
                b2BShipToAddressModel.ShipToCode = b2BShipToAddress.ShipToCode;
                b2BShipToAddressModel.ShipToName = b2BShipToAddress.ShipToName;
                b2BShipToAddressModel.AddressId = b2BShipToAddress.AddressId;
                b2BShipToAddressModel.Suburb = b2BShipToAddress.Suburb;
                b2BShipToAddressModel.DeliveryNotes = b2BShipToAddress.DeliveryNotes;
                b2BShipToAddressModel.Email = b2BShipToAddress.EmailAddresses;
                b2BShipToAddressModel.IsActive = b2BShipToAddress.IsActive;
                b2BShipToAddressModel.ErpAccountId = erpAccount.Id;
                b2BShipToAddressModel.ErpAccountNumber = erpAccount.AccountNumber;
                b2BShipToAddressModel.ErpSalesOrganizationId = erpAccount.ErpSalesOrgId ;
                b2BShipToAddressModel.SalesOrganisationCode = (await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId))?.Code;

                if (b2BShipToAddress.AddressId > 0)
                {
                    var address = await _addressService.GetAddressByIdAsync(b2BShipToAddress.AddressId);
                    var country = await _countryService.GetCountryByIdAsync(address.CountryId ?? 0);
                    var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId ?? 0);
                    b2BShipToAddressModel.Company = address.Company;
                    b2BShipToAddressModel.CountryId = address.CountryId;
                    b2BShipToAddressModel.CountryName = country?.Name;
                    b2BShipToAddressModel.CountryCode = country?.ThreeLetterIsoCode;
                    b2BShipToAddressModel.StateProvinceId = address.StateProvinceId;
                    b2BShipToAddressModel.StateProvinceName = stateProvince?.Name;
                    b2BShipToAddressModel.Address1 = address.Address1;
                    b2BShipToAddressModel.City = address.City;
                    b2BShipToAddressModel.Address2 = address.Address2;
                    b2BShipToAddressModel.ZipPostalCode = address.ZipPostalCode;
                    b2BShipToAddressModel.PhoneNumber = address.PhoneNumber;
                }

                b2BShipToAddressModel.AllowEdit = await _erpCustomerFunctionalityService.CheckAllowAddressEdit(b2BAccount);
                b2BShipToAddressModel.IsQuoteOrder = await _genericAttributeService.GetAttributeAsync<bool>(currentCustomer, B2BB2CFeaturesDefaults.B2BQouteOrderAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
                b2BShipToAddressModel.OnePageCheckoutEnabled = _orderSettings.OnePageCheckoutEnabled;
                b2BShipToAddressModel.SpecialInstructions = await _genericAttributeService.GetAttributeAsync<String>(currentCustomer, B2BB2CFeaturesDefaults.ProvidedB2BSpecialInstructions, (await _storeContext.GetCurrentStoreAsync()).Id);

                // we have to load this data as well even if ERPToDetermineDate is enabled (if erp call to determine date failed, we will use this)
                (var minDeliveryDate, var maxDeliveryDate) = await _erpCustomerFunctionalityService.GetMinimumAndMaximumDeliveryDateForShippingAddress();

                b2BShipToAddressModel.DeliveryDate = minDeliveryDate;
                b2BShipToAddressModel.FormatedDeliveryDate = minDeliveryDate.ToString("dd/MM/yyyy");
                b2BShipToAddressModel.MinDeliveryDate = minDeliveryDate.ToString("yyyy-MM-dd"); // html only support this format
                b2BShipToAddressModel.MaxDeliveryDate = maxDeliveryDate.ToString("yyyy-MM-dd"); // html only support this format

                if (b2BShipToAddressModel.AllowEdit)
                {
                    if (loadAvailableAreas)
                    {
                        //load settings for current Store
                        var b2BB2CFeaturesSettings = _settingService.LoadSetting<B2BB2CFeaturesSettings>((await _storeContext.GetCurrentStoreAsync()).Id);

                        // we will load area codes only if ERPToDetermineDate is enabled
                        b2BShipToAddressModel.ErpToDetermineDate = b2BB2CFeaturesSettings.ERPToDetermineDate;

                        if (b2BB2CFeaturesSettings.ERPToDetermineDate)
                        {
                            //ToDo - will come from ERP
                            //var areaCodes = _b2BERPIntegrationService.GetAreaCodesForSalesOrgAsLocation(b2BAccount);
                            var areaCodes = new List<ErpAreaCodeResponseModel>();
                            if (areaCodes != null && areaCodes.Any())
                            {
                                b2BShipToAddressModel.AvailableAreas = areaCodes
                                .Select(areaCode => new SelectListItem(areaCode.AREA, areaCode.AREA))
                                .ToList();
                            }
                            else
                            {
                                b2BShipToAddressModel.ErpToDetermineDate = false;
                            }
                        }

                        if (b2BShipToAddressModel.ErpToDetermineDate)
                        {
                            b2BShipToAddressModel.AvailableDeliveryDates.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("B2B.DeliveryDate.SelectDeliveryDate"), Value = string.Empty, Selected = true });
                        }

                        /*// we have to load this data as well even if ERPToDetermineDate is enabled (if erp call to determine date failed, we will use this)
                        (var minDeliveryDate, var maxDeliveryDate) = await _erpCustomerFunctionalityService.GetMinimumAndMaximumDeliveryDateForShippingAddress();

                        b2BShipToAddressModel.DeliveryDate = minDeliveryDate;
                        b2BShipToAddressModel.FormatedDeliveryDate = minDeliveryDate.ToString("dd/MM/yyyy");
                        b2BShipToAddressModel.MinDeliveryDate = minDeliveryDate.ToString("yyyy-MM-dd"); // html only support this format
                        b2BShipToAddressModel.MaxDeliveryDate = maxDeliveryDate.ToString("yyyy-MM-dd"); // html only support this format*/
                    }

                    if (loadCountriesAndStates)
                    {
                        //countries and states
                        var countries = await _countryService.GetAllCountriesForShippingAsync();

                        if (_addressSettings.PreselectCountryIfOnlyOne && countries.Count == 1)
                        {
                            b2BShipToAddressModel.CountryId = countries[0].Id;
                        }
                        else
                        {
                            b2BShipToAddressModel.AvailableCountries.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectCountry"), Value = string.Empty });
                        }

                        foreach (var c in countries)
                        {
                            b2BShipToAddressModel.AvailableCountries.Add(new SelectListItem
                            {
                                Text = await _localizationService.GetLocalizedAsync(c, x => x.Name),
                                Value = c.Id.ToString(),
                                Selected = c.Id == b2BShipToAddressModel.CountryId
                            });
                        }

                        if (_addressSettings.StateProvinceEnabled)
                        {
                            var languageId = EngineContext.Current.Resolve<IWorkContext>().GetWorkingLanguageAsync().Id;
                            var states = (await _stateProvinceService
                                .GetStateProvincesByCountryIdAsync(b2BShipToAddressModel.CountryId.HasValue ? b2BShipToAddressModel.CountryId.Value : 0, languageId)).ToList();
                            if (states.Any())
                            {
                                b2BShipToAddressModel.AvailableStates.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectState"), Value = string.Empty });

                                foreach (var s in states)
                                {
                                    b2BShipToAddressModel.AvailableStates.Add(new SelectListItem
                                    {
                                        Text = await _localizationService.GetLocalizedAsync(s, x => x.Name),
                                        Value = s.Id.ToString(),
                                        Selected = (s.Id == b2BShipToAddressModel.StateProvinceId)
                                    });
                                }
                            }
                            else
                            {
                                var anyCountrySelected = b2BShipToAddressModel.AvailableCountries.Any(x => x.Selected);
                                b2BShipToAddressModel.AvailableStates.Add(new SelectListItem
                                {
                                    Text = await _localizationService.GetResourceAsync(anyCountrySelected ? "Address.OtherNonUS" : "Address.SelectState"),
                                    Value = "0"
                                });
                            }
                        }
                    }
                }
            }
        }

        public async Task PrepareB2CShipToAddressModelAsync(ErpShipToAddressModelForCheckout b2BShipToAddressModel, ErpShipToAddress b2cShipToAddress, ErpAccount b2BAccount,
            bool loadAvailableSuburbs = false, bool loadCountriesAndStates = false)
        {
            var erpAccount = await _erpAccountService.GetErpAccountByErpShipToAddressAsync(b2cShipToAddress);
            var currentCustomer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();
            b2BShipToAddressModel = b2BShipToAddressModel ?? new ErpShipToAddressModelForCheckout();
            b2BShipToAddressModel.Id = b2cShipToAddress.Id;
            b2BShipToAddressModel.ShipToCode = b2cShipToAddress.ShipToCode;
            b2BShipToAddressModel.ShipToName = b2cShipToAddress.ShipToName;
            b2BShipToAddressModel.AddressId = b2cShipToAddress.AddressId;
            b2BShipToAddressModel.Suburb = b2cShipToAddress.Suburb;
            b2BShipToAddressModel.DeliveryNotes = b2cShipToAddress.DeliveryNotes;
            b2BShipToAddressModel.Email = b2cShipToAddress.EmailAddresses;
            b2BShipToAddressModel.IsActive = b2cShipToAddress.IsActive;
            b2BShipToAddressModel.ErpAccountId = erpAccount.Id;
            b2BShipToAddressModel.ErpAccountNumber =erpAccount?.AccountNumber;
            b2BShipToAddressModel.ErpSalesOrganizationId = erpAccount?.ErpSalesOrgId ?? 0;
            b2BShipToAddressModel.SalesOrganisationCode =
            (await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount?.ErpSalesOrgId ?? 0))?.Code;

            if (b2cShipToAddress.AddressId > 0)
            {
                var address = await _addressService.GetAddressByIdAsync(b2cShipToAddress.AddressId);
                var country = await _countryService.GetCountryByIdAsync(address.CountryId ?? 0);
                var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId ?? 0);
                b2BShipToAddressModel.Company = address.Company;
                b2BShipToAddressModel.CountryId = address.CountryId;
                b2BShipToAddressModel.CountryName = country?.Name;
                b2BShipToAddressModel.CountryCode = country?.ThreeLetterIsoCode;
                b2BShipToAddressModel.StateProvinceId = address.StateProvinceId;
                b2BShipToAddressModel.StateProvinceName = stateProvince?.Name;
                b2BShipToAddressModel.Address1 = address.Address1;
                b2BShipToAddressModel.City = address.City;
                b2BShipToAddressModel.Address2 = address.Address2;
                b2BShipToAddressModel.ZipPostalCode = address.ZipPostalCode;
                b2BShipToAddressModel.PhoneNumber = address.PhoneNumber;
            }

            b2BShipToAddressModel.AllowEdit = await _erpCustomerFunctionalityService.CheckAllowAddressEdit(b2BAccount);
            b2BShipToAddressModel.IsQuoteOrder = await _genericAttributeService.GetAttributeAsync<bool>(currentCustomer, B2BB2CFeaturesDefaults.B2CQouteOrderAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
            b2BShipToAddressModel.OnePageCheckoutEnabled = _orderSettings.OnePageCheckoutEnabled;
            b2BShipToAddressModel.SpecialInstructions = await _genericAttributeService.GetAttributeAsync<String>(currentCustomer, B2BB2CFeaturesDefaults.B2CSpecialInstructions, (await _storeContext.GetCurrentStoreAsync()).Id);

            // we have to load this data as well even if ERPToDetermineDate is enabled (if erp call to determine date failed, we will use this)
            (var minDeliveryDate, var maxDeliveryDate) = await _erpCustomerFunctionalityService.GetMinimumAndMaximumDeliveryDateForShippingAddress();

            b2BShipToAddressModel.DeliveryDate = minDeliveryDate;
            b2BShipToAddressModel.FormatedDeliveryDate = minDeliveryDate.ToString("dd/MM/yyyy");
            b2BShipToAddressModel.MinDeliveryDate = minDeliveryDate.ToString("yyyy-MM-dd"); // html only support this format
            b2BShipToAddressModel.MaxDeliveryDate = maxDeliveryDate.ToString("yyyy-MM-dd"); // html only support this format

            if (b2BShipToAddressModel.AllowEdit)
            {
                if (loadAvailableSuburbs)
                {
                    //load settings for current Store
                    var b2BB2CFeaturesSettings = _settingService.LoadSetting<B2BB2CFeaturesSettings>((await _storeContext.GetCurrentStoreAsync()).Id);

                    // we will load area codes only if ERPToDetermineDate is enabled
                    b2BShipToAddressModel.ErpToDetermineDate = b2BB2CFeaturesSettings.ERPToDetermineDate;

                    if (b2BB2CFeaturesSettings.ERPToDetermineDate)
                    {
                        //ToDo - will come from ERP
                        //var areaCodes = _b2BERPIntegrationService.GetAreaCodesForSalesOrgAsLocation(b2BAccount);
                        var areaCodes = new List<ErpAreaCodeResponseModel>();
                        if (areaCodes != null && areaCodes.Any())
                        {
                            b2BShipToAddressModel.AvailableAreas = areaCodes
                            .Select(areaCode => new SelectListItem(areaCode.AREA, areaCode.AREA))
                            .ToList();
                        }
                        else
                        {
                            b2BShipToAddressModel.ErpToDetermineDate = false;
                        }
                    }

                    if (b2BShipToAddressModel.ErpToDetermineDate)
                    {
                        b2BShipToAddressModel.AvailableDeliveryDates.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("B2B.DeliveryDate.SelectDeliveryDate"), Value = string.Empty, Selected = true });
                    }

                    /*// we have to load this data as well even if ERPToDetermineDate is enabled (if erp call to determine date failed, we will use this)
                    (var minDeliveryDate, var maxDeliveryDate) = await _erpCustomerFunctionalityService.GetMinimumAndMaximumDeliveryDateForShippingAddress();

                    b2BShipToAddressModel.DeliveryDate = minDeliveryDate;
                    b2BShipToAddressModel.FormatedDeliveryDate = minDeliveryDate.ToString("dd/MM/yyyy");
                    b2BShipToAddressModel.MinDeliveryDate = minDeliveryDate.ToString("yyyy-MM-dd"); // html only support this format
                    b2BShipToAddressModel.MaxDeliveryDate = maxDeliveryDate.ToString("yyyy-MM-dd"); // html only support this format*/
                }

                if (loadCountriesAndStates)
                {
                    //countries and states
                    var countries = await _countryService.GetAllCountriesForShippingAsync();

                    if (_addressSettings.PreselectCountryIfOnlyOne && countries.Count == 1)
                    {
                        b2BShipToAddressModel.CountryId = countries[0].Id;
                    }
                    else
                    {
                        b2BShipToAddressModel.AvailableCountries.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectCountry"), Value = string.Empty });
                    }

                    foreach (var c in countries)
                    {
                        b2BShipToAddressModel.AvailableCountries.Add(new SelectListItem
                        {
                            Text = await _localizationService.GetLocalizedAsync(c, x => x.Name),
                            Value = c.Id.ToString(),
                            Selected = c.Id == b2BShipToAddressModel.CountryId
                        });
                    }

                    if (_addressSettings.StateProvinceEnabled)
                    {
                        var languageId = EngineContext.Current.Resolve<IWorkContext>().GetWorkingLanguageAsync().Id;
                        var states = (await _stateProvinceService
                            .GetStateProvincesByCountryIdAsync(b2BShipToAddressModel.CountryId.HasValue ? b2BShipToAddressModel.CountryId.Value : 0, languageId)).ToList();
                        if (states.Any())
                        {
                            b2BShipToAddressModel.AvailableStates.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectState"), Value = string.Empty });

                            foreach (var s in states)
                            {
                                b2BShipToAddressModel.AvailableStates.Add(new SelectListItem
                                {
                                    Text = await _localizationService.GetLocalizedAsync(s, x => x.Name),
                                    Value = s.Id.ToString(),
                                    Selected = (s.Id == b2BShipToAddressModel.StateProvinceId)
                                });
                            }
                        }
                        else
                        {
                            var anyCountrySelected = b2BShipToAddressModel.AvailableCountries.Any(x => x.Selected);
                            b2BShipToAddressModel.AvailableStates.Add(new SelectListItem
                            {
                                Text = await _localizationService.GetResourceAsync(anyCountrySelected ? "Address.OtherNonUS" : "Address.SelectState"),
                                Value = "0"
                            });
                        }
                    }
                }
            }
        }

        public async Task<CheckoutErpBillingAddressModel> PrepareCheckoutErpBillingAddressModelAsync(IList<ShoppingCartItem> cart, ErpAccount b2BAccount)
        {
            var currentCustomer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();
            var model = new CheckoutErpBillingAddressModel
            {
                ShipToSameAddressAllowed = _shippingSettings.ShipToSameAddress && await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart),
                //allow customers to enter (choose) a shipping address if "Disable Billing address step" setting is enabled
                ShipToSameAddress = !_orderSettings.DisableBillingAddressCheckoutStep
            };

            model.ErpBillingAddress = model.ErpBillingAddress ?? new ErpBillingAddressModel();
            if (b2BAccount != null)
            {
                model.ErpBillingAddress.ErpAccountId = b2BAccount.Id;
                model.ErpBillingAddress.Suburb = b2BAccount.BillingSuburb;
                if (b2BAccount.BillingAddressId != null && b2BAccount.BillingAddressId.Value > 0)
                {
                    var address = await _addressService.GetAddressByIdAsync(b2BAccount.BillingAddressId.Value);
                    var country = await _countryService.GetCountryByIdAsync(address.CountryId ?? 0);
                    var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId ?? 0);
                    model.ErpBillingAddress.Id = address.Id;
                    model.ErpBillingAddress.FirstName = currentCustomer.FirstName;
                    model.ErpBillingAddress.LastName = currentCustomer.LastName;
                    model.ErpBillingAddress.Email = currentCustomer.Email;
                    model.ErpBillingAddress.Company = address.Company;
                    model.ErpBillingAddress.CountryId = address.CountryId;
                    model.ErpBillingAddress.CountryName = country?.Name;
                    model.ErpBillingAddress.StateProvinceId = address.StateProvinceId;
                    model.ErpBillingAddress.StateProvinceName = stateProvince?.Name;
                    model.ErpBillingAddress.Address1 = address.Address1;
                    model.ErpBillingAddress.City = address.City;
                    model.ErpBillingAddress.Address2 = address.Address2;
                    model.ErpBillingAddress.ZipPostalCode = address.ZipPostalCode;
                    model.ErpBillingAddress.PhoneNumber = address.PhoneNumber;
                }
            }

            return model;
        }

        public async Task<CheckoutErpShippingAddressModel> PrepareCheckoutB2BShippingAddressModelAsync(IList<ShoppingCartItem> cart, ErpNopUser b2BUser, ErpAccount b2BAccount)
        {
            var model = new CheckoutErpShippingAddressModel();
            model.PickupPointsModel = new CheckoutPickupPointsModel
            {
                //allow pickup in store?
                AllowPickupInStore = _shippingSettings.AllowPickupInStore
            };

            var currentCustomer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();
            var b2BSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(b2BAccount.ErpSalesOrgId);
            model.IsB2BUser = true;
            model.ThemeName = _settingService.LoadSetting<StoreInformationSettings>((await _storeContext.GetCurrentStoreAsync()).Id).DefaultStoreTheme;
            model.SpecialInstructions = await _genericAttributeService.GetAttributeAsync<string>(currentCustomer, B2BB2CFeaturesDefaults.ProvidedB2BSpecialInstructions, (await _storeContext.GetCurrentStoreAsync()).Id);
            model.DisplayPickupInStore = _orderSettings.DisplayPickupInStoreOnShippingMethodPage;
            var languageId = (await _b2BB2CWorkContext.GetWorkingLanguageAsync()).Id;

            if (model.PickupPointsModel.AllowPickupInStore)
            {
                model.PickupPointsModel.DisplayPickupPointsOnMap = _shippingSettings.DisplayPickupPointsOnMap;
                model.PickupPointsModel.GoogleMapsApiKey = _shippingSettings.GoogleMapsApiKey;
                var pickupPointProviders = await _pickupPluginManager.LoadActivePluginsAsync(currentCustomer, (await _storeContext.GetCurrentStoreAsync()).Id);
                if (pickupPointProviders.Any())
                {
                    var pickupPointsResponse = await _shippingService.GetPickupPointsAsync(cart, address: await _addressService.GetAddressByIdAsync(currentCustomer.BillingAddressId ?? 0), customer: currentCustomer, storeId: (await _storeContext.GetCurrentStoreAsync()).Id);
                    if (pickupPointsResponse.Success)
                        model.PickupPointsModel.PickupPoints = await pickupPointsResponse.PickupPoints.SelectAwait(async point =>
                        {
                            var country = await _countryService.GetCountryByTwoLetterIsoCodeAsync(point.CountryCode);
                            var state = await _stateProvinceService.GetStateProvinceByAbbreviationAsync(point.StateAbbreviation, country?.Id);

                            var pickupPointModel = new CheckoutPickupPointModel
                            {
                                Id = point.Id,
                                Name = point.Name,
                                Description = point.Description,
                                ProviderSystemName = point.ProviderSystemName,
                                Address = point.Address,
                                City = point.City,
                                County = point.County,
                                StateName = state != null ? await _localizationService.GetLocalizedAsync(state, x => x.Name, languageId) : string.Empty,
                                CountryName = country != null ? await _localizationService.GetLocalizedAsync(country, x => x.Name, languageId) : string.Empty,
                                ZipPostalCode = point.ZipPostalCode,
                                Latitude = point.Latitude,
                                Longitude = point.Longitude,
                                OpeningHours = point.OpeningHours
                            };
                            if (point.PickupFee > 0)
                            {
                                var amount = await _taxService.GetShippingPriceAsync(point.PickupFee, currentCustomer);
                                var priceAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(amount.price, await _workContext.GetWorkingCurrencyAsync());
                                pickupPointModel.PickupFee = await _priceFormatter.FormatShippingPriceAsync(priceAmount, true);
                            }

                            return pickupPointModel;
                        }).ToListAsync();
                    else
                        foreach (var error in pickupPointsResponse.Errors)
                            model.PickupPointsModel.Warnings.Add(error);
                }

                //only available pickup points
                var shippingProviders = await _shippingPluginManager.LoadActivePluginsAsync(currentCustomer, (await _storeContext.GetCurrentStoreAsync()).Id);
                if (!shippingProviders.Any())
                {
                    if (!pickupPointProviders.Any())
                    {
                        model.PickupPointsModel.Warnings.Add(await _localizationService.GetResourceAsync("Checkout.ShippingIsNotAllowed"));
                        model.PickupPointsModel.Warnings.Add(await _localizationService.GetResourceAsync("Checkout.PickupPoints.NotAvailable"));
                    }
                    model.PickupPointsModel.PickupInStoreOnly = true;
                    model.PickupPointsModel.PickupInStore = true;
                    return model;
                }
            }

            // Prepare B2B ShipToAddress Model of B2B User
            model.ErpShipToAddressId = b2BUser.ErpShipToAddressId;
            var b2CShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(b2BUser.ErpShipToAddressId);
            var modifiedShipToAddressIdOnCheckout = await _genericAttributeService.GetAttributeAsync<int>(currentCustomer, B2BB2CFeaturesDefaults.ShippingAddressModifiedIdInCheckoutAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
            var nopAddress = new Address();
            if (modifiedShipToAddressIdOnCheckout > 0)
            {
                model.ErpShipToAddressId = modifiedShipToAddressIdOnCheckout;
                var modifiedShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(modifiedShipToAddressIdOnCheckout);
                if (modifiedShipToAddress != null)
                {
                    var nopAddressForModifiedShipToAddress = await _addressService.GetAddressByIdAsync(modifiedShipToAddress.AddressId);
                    var country = await _countryService.GetCountryByIdAsync(nopAddressForModifiedShipToAddress.CountryId ?? 0);
                    var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(nopAddressForModifiedShipToAddress.StateProvinceId ?? 0);
                    model.ExistingErpShipToAddresses.Add(new ErpShipToAddressModelForCheckout
                    {
                        Id = modifiedShipToAddress.Id,
                        ShipToCode = modifiedShipToAddress.ShipToCode,
                        ShipToName = modifiedShipToAddress.ShipToName,
                        Address1 = nopAddressForModifiedShipToAddress?.Address1,
                        Address2 = nopAddressForModifiedShipToAddress?.Address2,
                        Suburb = modifiedShipToAddress.Suburb,
                        City = nopAddressForModifiedShipToAddress?.City,
                        StateProvinceName = stateProvince?.Name,
                        ZipPostalCode = nopAddressForModifiedShipToAddress?.ZipPostalCode,
                        CountryName = country?.Name
                    });
                }
            }
            else
            {
                //Prepare B2B Ship To Address
                var shipToAddresses = await _erpShipToAddressService.GetErpShipToAddressesByAccountIdAsync(showHidden: false, isActiveOnly: true, accountId: b2BAccount.Id);
                foreach (var shipTo in shipToAddresses)
                {
                    nopAddress = await _addressService.GetAddressByIdAsync(shipTo.AddressId) ?? new Address();
                    var country = await _countryService.GetCountryByIdAsync(nopAddress.CountryId ?? 0);
                    var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(nopAddress.StateProvinceId ?? 0);
                    model.ExistingErpShipToAddresses.Add(new ErpShipToAddressModelForCheckout
                    {
                        Id = shipTo.Id,
                        ShipToCode = shipTo.ShipToCode,
                        ShipToName = shipTo.ShipToName,
                        Address1 = nopAddress?.Address1,
                        Address2 = nopAddress?.Address2,
                        Suburb = shipTo.Suburb,
                        City = nopAddress?.City,
                        StateProvinceName = stateProvince?.Name,
                        ZipPostalCode = nopAddress?.ZipPostalCode,
                        CountryName = country?.Name
                    });
                }
            }

            model.AllowAddressEdit = await _erpCustomerFunctionalityService.CheckAllowAddressEdit(b2BAccount);
            model.IsQuoteOrder = await _genericAttributeService.GetAttributeAsync<bool>(currentCustomer, B2BB2CFeaturesDefaults.B2BQouteOrderAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);

            //countries and states
            var countries = await _countryService.GetAllCountriesForShippingAsync();
            var erpShippingAddressModel = new ErpShipToAddressModelForCheckout();
            if (_addressSettings.PreselectCountryIfOnlyOne && countries.Count == 1)
            {
                erpShippingAddressModel.CountryId = countries[0].Id;
            }
            else
            {
                erpShippingAddressModel.AvailableCountries.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectCountry"), Value = string.Empty });
            }

            foreach (var c in countries)
            {
                erpShippingAddressModel.AvailableCountries.Add(new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedAsync(c, x => x.Name),
                    Value = c.Id.ToString(),
                    Selected = c.Id == erpShippingAddressModel.CountryId
                });
            }

            var states = (await _stateProvinceService
                .GetStateProvincesByCountryIdAsync(erpShippingAddressModel.CountryId.HasValue ? erpShippingAddressModel.CountryId.Value : 0, languageId)).ToList();
            if (states.Any())
            {
                erpShippingAddressModel.AvailableStates.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectState"), Value = string.Empty });

                foreach (var s in states)
                {
                    erpShippingAddressModel.AvailableStates.Add(new SelectListItem
                    {
                        Text = await _localizationService.GetLocalizedAsync(s, x => x.Name),
                        Value = s.Id.ToString(),
                        Selected = (s.Id == erpShippingAddressModel.StateProvinceId)
                    });
                }
            }
            else
            {
                var anyCountrySelected = erpShippingAddressModel.AvailableCountries.Any(x => x.Selected);
                erpShippingAddressModel.AvailableStates.Add(new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync(anyCountrySelected ? "Address.OtherNonUS" : "Address.SelectState"),
                    Value = "0"
                });
            }
            model.SelectedShipToAddress = erpShippingAddressModel;

            //load settings for current Store
            var b2BB2CFeaturesSettings = _settingService.LoadSetting<B2BB2CFeaturesSettings>((await _storeContext.GetCurrentStoreAsync()).Id);

            model.ErpToDetermineDate = b2BB2CFeaturesSettings.ERPToDetermineDate;
            if (model.ErpToDetermineDate)
            {
                model.AvailableDeliveryDates.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("B2B.DeliveryDate.SelectDeliveryDate"), Value = string.Empty, Selected = true });
            }

            // we have to load this data as well even if ERPToDetermineDate is enabled (if erp call to determine date failed, we will use this)
            (var minDeliveryDate, var maxDeliveryDate) = await _erpCustomerFunctionalityService.GetMinimumAndMaximumDeliveryDateForShippingAddress();

            model.DeliveryDate = minDeliveryDate;
            model.FormatedDeliveryDate = minDeliveryDate.ToString("dd/MM/yyyy");
            model.MinDeliveryDate = minDeliveryDate.ToString("yyyy-MM-dd"); // html only support this format
            model.MaxDeliveryDate = maxDeliveryDate.ToString("yyyy-MM-dd"); // html only support this format
            model.SelectedShipToAddress = new ErpShipToAddressModelForCheckout
            {
                Id = b2CShipToAddress.Id,
                ShipToCode = b2CShipToAddress.ShipToCode,
                ShipToName = b2CShipToAddress.ShipToName,
                Address1 = nopAddress.Address1,
                Address2 = nopAddress.Address2,
                Suburb = b2CShipToAddress.Suburb,
                City = nopAddress.City,
                StateProvinceName = (await _stateProvinceService.GetStateProvinceByIdAsync(nopAddress.StateProvinceId ?? 0))?.Name,
                ZipPostalCode = nopAddress.ZipPostalCode,
                CountryName = (await _countryService.GetCountryByIdAsync(nopAddress.CountryId ?? 0))?.Name,
                ErpSalesOrganizationId = b2BSalesOrg.Id,
                SalesOrganisationCode = b2BSalesOrg.Code
            };

            return model;
        }

        public virtual async Task<ErpCheckoutShippingAddressModel> PrepareShippingAddressModelAsync(ErpNopUser erpUser, ErpAccount b2BAccount, int? selectedCountryId = null,
            bool prePopulateNewAddressWithCustomerFields = false, string overrideAttributesXml = "")
        {
            var currentCustomer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();
            var model = new ErpCheckoutShippingAddressModel
            {
                AllowPickupInStore = _shippingSettings.AllowPickupInStore
            };
            if (model.AllowPickupInStore)
            {
                model.DisplayPickupPointsOnMap = _shippingSettings.DisplayPickupPointsOnMap;
                model.GoogleMapsApiKey = _shippingSettings.GoogleMapsApiKey;
                var pickupPointProviders = await _pickupPluginManager.LoadActivePluginsAsync(currentCustomer, (await _storeContext.GetCurrentStoreAsync()).Id);
                if (pickupPointProviders.Any())
                {
                    var cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

                    var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
                    var pickupPointsResponse = await _shippingService.GetPickupPointsAsync(cart, await _addressService.GetAddressByIdAsync(currentCustomer.BillingAddressId ?? 0),
                        currentCustomer, storeId: (await _storeContext.GetCurrentStoreAsync()).Id);
                    if (pickupPointsResponse.Success)
                        model.PickupPoints = await pickupPointsResponse.PickupPoints.SelectAwait(async point =>
                        {
                            var country = await _countryService.GetCountryByTwoLetterIsoCodeAsync(point.CountryCode);
                            var state = await _stateProvinceService.GetStateProvinceByAbbreviationAsync(point.StateAbbreviation, country?.Id);

                            var pickupPointModel = new CheckoutPickupPointModel
                            {
                                Id = point.Id,
                                Name = point.Name,
                                Description = point.Description,
                                ProviderSystemName = point.ProviderSystemName,
                                Address = point.Address,
                                City = point.City,
                                County = point.County,
                                StateName = state != null ? await _localizationService.GetLocalizedAsync(state, x => x.Name, languageId) : string.Empty,
                                CountryName = country != null ? await _localizationService.GetLocalizedAsync(country, x => x.Name, languageId) : string.Empty,
                                ZipPostalCode = point.ZipPostalCode,
                                Latitude = point.Latitude,
                                Longitude = point.Longitude,
                                OpeningHours = point.OpeningHours
                            };
                            if (point.PickupFee > 0)
                            {
                                var amount = await _taxService.GetShippingPriceAsync(point.PickupFee, currentCustomer);
                                var priceAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(amount.price, await _workContext.GetWorkingCurrencyAsync());
                                pickupPointModel.PickupFee = await _priceFormatter.FormatShippingPriceAsync(priceAmount, true);
                            }

                            return pickupPointModel;
                        }).ToListAsync();
                    else
                        foreach (var error in pickupPointsResponse.Errors)
                            model.Warnings.Add(error);
                }

                //only available pickup points
                var shippingProviders = await _shippingPluginManager.LoadActivePluginsAsync(currentCustomer, (await _storeContext.GetCurrentStoreAsync()).Id);
                if (!shippingProviders.Any())
                {
                    if (!pickupPointProviders.Any())
                    {
                        model.Warnings.Add(await _localizationService.GetResourceAsync("Checkout.ShippingIsNotAllowed"));
                        model.Warnings.Add(await _localizationService.GetResourceAsync("Checkout.PickupPoints.NotAvailable"));
                    }
                    model.PickupInStoreOnly = true;
                    model.PickupInStore = true;
                    return model;
                }
            }

            //Prepare B2B Ship To Addresses
            var shipToAddresses = await _erpShipToAddressService.GetErpShipToAddressesByAccountIdAsync(showHidden: false, isActiveOnly: true, accountId: b2BAccount.Id);
            foreach (var shipto in shipToAddresses)
            {
                model.ExistingErpShipToAddresses.Add(new ErpShipToAddressModelForCheckout
                {
                    Id = shipto.Id,
                    ShipToCode = shipto.ShipToCode,
                    ShipToName = shipto.ShipToName
                });
            }

            // Prepare B2B ShipToAddress Model of B2B User
            if (erpUser != null)
                model.ErpShipToAddress.Id = erpUser.ErpShipToAddressId;
            model.AllowAddressEdit = await _erpCustomerFunctionalityService.CheckAllowAddressEdit(b2BAccount);
            return model;
        }

        public virtual async Task<ErpOnePageCheckoutModel> PrepareB2BOnePageCheckoutModelAsync(IList<ShoppingCartItem> cart, ErpNopUser b2BUser)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            var b2BAccount = await _erpAccountService.GetErpAccountByIdAsync(b2BUser.ErpAccountId);
            var model = new ErpOnePageCheckoutModel
            {
                ShippingRequired = true,
                DisableBillingAddressCheckoutStep = _orderSettings.DisableBillingAddressCheckoutStep && b2BAccount?.BillingAddressId != null,
                CheckoutErpBillingAddress = await PrepareCheckoutErpBillingAddressModelAsync(cart, b2BAccount),
                CheckoutErpShipToAddress = await PrepareCheckoutB2BShippingAddressModelAsync(cart, b2BUser, b2BAccount),
                IsQuoteOrder = await _genericAttributeService.GetAttributeAsync<bool>(await _b2BB2CWorkContext.GetWorkingCurrencyAsync(), B2BB2CFeaturesDefaults.B2BQouteOrderAttribute, (await _storeContext.GetCurrentStoreAsync()).Id),
                DisplayCaptcha = await _customerService.IsGuestAsync(await _customerService.GetShoppingCartCustomerAsync(cart))
                    && _captchaSettings.Enabled && _captchaSettings.ShowOnCheckoutPageForGuests,
                IsReCaptchaV3 = _captchaSettings.CaptchaType == CaptchaType.ReCaptchaV3,
                ReCaptchaPublicKey = _captchaSettings.ReCaptchaPublicKey
            };

            return model;
        }


        /// <summary>
        /// Prepare B2C one page checkout model
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <returns>B2C One page checkout model</returns>
        public virtual async Task<ErpOnePageCheckoutModel> PrepareB2COnePageCheckoutModelAsync(IList<ShoppingCartItem> cart, ErpNopUser b2CUser)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            var b2BAccount = await _erpAccountService.GetErpAccountByIdAsync(b2CUser.ErpAccountId);
            var model = new ErpOnePageCheckoutModel
            {
                ShippingRequired = true,
                DisableBillingAddressCheckoutStep = _orderSettings.DisableBillingAddressCheckoutStep && b2BAccount?.BillingAddressId != null,
                CheckoutErpBillingAddress = await PrepareCheckoutErpBillingAddressModelAsync(cart, b2BAccount),
                CheckoutErpShipToAddress = await PrepareCheckoutB2CShippingAddressModelAsync(cart, b2CUser, b2BAccount),
                IsQuoteOrder = await _genericAttributeService.GetAttributeAsync<bool>(await _b2BB2CWorkContext.GetCurrentCustomerAsync(), B2BB2CFeaturesDefaults.B2CQouteOrderAttribute, (await _storeContext.GetCurrentStoreAsync()).Id),
                DisplayCaptcha = await _customerService.IsGuestAsync(await _customerService.GetShoppingCartCustomerAsync(cart))
                    && _captchaSettings.Enabled && _captchaSettings.ShowOnCheckoutPageForGuests,
                IsReCaptchaV3 = _captchaSettings.CaptchaType == CaptchaType.ReCaptchaV3,
                ReCaptchaPublicKey = _captchaSettings.ReCaptchaPublicKey
            };

            return model;
        }

        public async Task<(IList<SelectListItem>, bool)> GetDeliveryDatesBySuburbOrCityAsync(string suburb, string city)
        {
            //load settings for current Store
            var B2BB2CFeaturesSettings = _settingService.LoadSetting<B2BB2CFeaturesSettings>((await _storeContext.GetCurrentStoreAsync()).Id);

            if (!B2BB2CFeaturesSettings.ERPToDetermineDate)
                return (null, false);

            var b2bAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync((await _b2BB2CWorkContext.GetCurrentCustomerAsync()).Id);

            #region Call By Suburb

            // ToDo, will get delivery dates from ERP
            //var deliveryDateResponse = _erpIntegrationService.GetDeliveryDatesForSuburbOrCity(b2bAccount, suburb?.ToUpper());
            var deliveryDateResponse = new ErpDeliveryDateResponseModel();
            if (deliveryDateResponse != null && deliveryDateResponse.IsFullLoadRequired)
                return (null, true);

            #endregion

            #region Call By City

            // ToDo, will get delivery dates from ERP
            if (deliveryDateResponse == null || deliveryDateResponse.DeliveryDates.Count < 1)
            {
                //deliveryDateResponse = _b2BERPIntegrationService.GetDeliveryDatesForSuburbOrCity(b2bAccount, city?.ToUpper());
                deliveryDateResponse = new ErpDeliveryDateResponseModel();
            }

            if (deliveryDateResponse != null && deliveryDateResponse.IsFullLoadRequired)
                return (null, true);

            #endregion

            if (deliveryDateResponse == null || deliveryDateResponse.DeliveryDates.Count < 1)
            {
                //deliveryDateResponse = _b2BERPIntegrationService.GetDeliveryDatesForSuburbOrCity(b2bAccount, "OTHER");
                deliveryDateResponse = new ErpDeliveryDateResponseModel();

                //if (deliveryDateResponse == null || deliveryDateResponse.DeliveryDates?.Count < 1)
                //    _b2bWorkflowMessageService.SendOrderOrDeliveryDatesOrShippingCostBAPIFailedMessage(currentCustomer, (int)ERPFailedTypes.DeliveryDateFails, 0);

                if (deliveryDateResponse == null)
                    return (null, false);

                if (deliveryDateResponse.IsFullLoadRequired)
                    return (null, true);

                if (deliveryDateResponse.DeliveryDates.Count < 1)
                    return (null, false);
            }

            var deliveryDates = deliveryDateResponse.DeliveryDates;
            var availableDeliveryDates = new List<SelectListItem>();
            availableDeliveryDates = deliveryDates
                    .Select(deliveryDate => new SelectListItem(deliveryDate.DELDATE, deliveryDate.DELDATE))
                    .ToList();

            return (availableDeliveryDates, true);
        }

        public async Task<(IList<SelectListItem>, bool)> GetDeliveryDatesByAreaAndPlantAsync(string suburb, string city, string warehouseCode)
        {
            //load settings for current Store
            var b2BB2CFeaturesSettings = _settingService.LoadSetting<B2BB2CFeaturesSettings>((await _storeContext.GetCurrentStoreAsync()).Id);
            if (!b2BB2CFeaturesSettings.ERPToDetermineDate)
                return (null, false);

            var b2bAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync((await _b2BB2CWorkContext.GetCurrentCustomerAsync()).Id);

            #region Call By Suburb

            //ToDo - will come from ERP
            //var deliveryDateResponse = _b2BERPIntegrationService.GetDeliveryDatesForAreaAndWarehouse(b2bAccount, suburb?.ToUpper(), warehouseCode);
            var deliveryDateResponse = new ErpDeliveryDateResponseModel();
            if (deliveryDateResponse != null && deliveryDateResponse.IsFullLoadRequired)
                return (null, true);

            #endregion

            #region Call By City

            if (deliveryDateResponse == null || deliveryDateResponse.DeliveryDates.Count < 1)
                deliveryDateResponse = new ErpDeliveryDateResponseModel();
            //deliveryDateResponse = _b2BERPIntegrationService.GetDeliveryDatesForAreaAndWarehouse(b2bAccount, city?.ToUpper(), warehouseCode);

            if (deliveryDateResponse != null && deliveryDateResponse.IsFullLoadRequired)
                return (null, true);

            #endregion

            if (deliveryDateResponse == null || deliveryDateResponse.DeliveryDates.Count < 1)
            {
                deliveryDateResponse = new ErpDeliveryDateResponseModel();
                //deliveryDateResponse = _b2BERPIntegrationService.GetDeliveryDatesForAreaAndWarehouse(b2bAccount, "OTHER", warehouseCode);

                //if (deliveryDateResponse == null || deliveryDateResponse.DeliveryDates?.Count < 1)
                //    _b2bWorkflowMessageService.SendOrderOrDeliveryDatesOrShippingCostBAPIFailedMessage(currentCustomer, (int)ERPFailedTypes.DeliveryDateFails, 0);

                if (deliveryDateResponse == null)
                    return (null, false);

                if (deliveryDateResponse.IsFullLoadRequired)
                    return (null, true);

                if (deliveryDateResponse.DeliveryDates.Count < 1)
                    return (null, false);
            }

            var deliveryDates = deliveryDateResponse.DeliveryDates;
            var availableDeliveryDates = new List<SelectListItem>();
            availableDeliveryDates = deliveryDates
                    .Select(deliveryDate => new SelectListItem(deliveryDate.DELDATE, deliveryDate.DELDATE))
                    .ToList();

            return (availableDeliveryDates, true);
        }

        public async Task<CheckoutErpShippingAddressModel> PrepareCheckoutB2CShippingAddressModelAsync(IList<ShoppingCartItem> cart, ErpNopUser b2CUser, ErpAccount b2BAccount)
        {
            var model = new CheckoutErpShippingAddressModel();
            model.PickupPointsModel = new CheckoutPickupPointsModel
            {
                //allow pickup in store?
                AllowPickupInStore = _shippingSettings.AllowPickupInStore
            };

            var currentCustomer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();
            var b2BSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(b2BAccount.ErpSalesOrgId);
            model.IsB2BUser = false;
            model.ThemeName = _settingService.LoadSetting<StoreInformationSettings>((await _storeContext.GetCurrentStoreAsync()).Id).DefaultStoreTheme;
            model.SpecialInstructions = await _genericAttributeService.GetAttributeAsync<string>(currentCustomer, B2BB2CFeaturesDefaults.B2CSpecialInstructions, (await _storeContext.GetCurrentStoreAsync()).Id);
            model.DisplayPickupInStore = _orderSettings.DisplayPickupInStoreOnShippingMethodPage;
            var languageId = (await _b2BB2CWorkContext.GetWorkingLanguageAsync()).Id;

            if (model.PickupPointsModel.AllowPickupInStore)
            {
                model.PickupPointsModel.DisplayPickupPointsOnMap = _shippingSettings.DisplayPickupPointsOnMap;
                model.PickupPointsModel.GoogleMapsApiKey = _shippingSettings.GoogleMapsApiKey;
                var pickupPointProviders = await _pickupPluginManager.LoadActivePluginsAsync(currentCustomer, (await _storeContext.GetCurrentStoreAsync()).Id);
                if (pickupPointProviders.Any())
                {
                    var pickupPointsResponse = await _shippingService.GetPickupPointsAsync(cart, address: await _addressService.GetAddressByIdAsync(currentCustomer.BillingAddressId ?? 0), customer: currentCustomer, storeId: (await _storeContext.GetCurrentStoreAsync()).Id);
                    if (pickupPointsResponse.Success)
                        model.PickupPointsModel.PickupPoints = await pickupPointsResponse.PickupPoints.SelectAwait(async point =>
                        {
                            var country = await _countryService.GetCountryByTwoLetterIsoCodeAsync(point.CountryCode);
                            var state = await _stateProvinceService.GetStateProvinceByAbbreviationAsync(point.StateAbbreviation, country?.Id);

                            var pickupPointModel = new CheckoutPickupPointModel
                            {
                                Id = point.Id,
                                Name = point.Name,
                                Description = point.Description,
                                ProviderSystemName = point.ProviderSystemName,
                                Address = point.Address,
                                City = point.City,
                                County = point.County,
                                StateName = state != null ? await _localizationService.GetLocalizedAsync(state, x => x.Name, languageId) : string.Empty,
                                CountryName = country != null ? await _localizationService.GetLocalizedAsync(country, x => x.Name, languageId) : string.Empty,
                                ZipPostalCode = point.ZipPostalCode,
                                Latitude = point.Latitude,
                                Longitude = point.Longitude,
                                OpeningHours = point.OpeningHours
                            };
                            if (point.PickupFee > 0)
                            {
                                var amount = await _taxService.GetShippingPriceAsync(point.PickupFee, currentCustomer);
                                var priceAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(amount.price, await _workContext.GetWorkingCurrencyAsync());
                                pickupPointModel.PickupFee = await _priceFormatter.FormatShippingPriceAsync(priceAmount, true);
                            }

                            return pickupPointModel;
                        }).ToListAsync();
                    else
                        foreach (var error in pickupPointsResponse.Errors)
                            model.PickupPointsModel.Warnings.Add(error);
                }

                //only available pickup points
                var shippingProviders = await _shippingPluginManager.LoadActivePluginsAsync(currentCustomer, (await _storeContext.GetCurrentStoreAsync()).Id);
                if (!shippingProviders.Any())
                {
                    if (!pickupPointProviders.Any())
                    {
                        model.PickupPointsModel.Warnings.Add(await _localizationService.GetResourceAsync("Checkout.ShippingIsNotAllowed"));
                        model.PickupPointsModel.Warnings.Add(await _localizationService.GetResourceAsync("Checkout.PickupPoints.NotAvailable"));
                    }
                    model.PickupPointsModel.PickupInStoreOnly = true;
                    model.PickupPointsModel.PickupInStore = true;
                    return model;
                }
            }

            // Prepare ERP ShipToAddress Model of B2C User
            model.ErpShipToAddressId = b2CUser.ErpShipToAddressId;
            var b2CShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(b2CUser.ErpShipToAddressId);
            var modifiedShipToAddressIdOnCheckout = await _genericAttributeService.GetAttributeAsync<int>(currentCustomer, B2BB2CFeaturesDefaults.ShippingAddressModifiedIdInCheckoutAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
            var nopAddress = new Address();
            if (modifiedShipToAddressIdOnCheckout > 0)
            {
                model.ErpShipToAddressId = modifiedShipToAddressIdOnCheckout;
                var modifiedShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(modifiedShipToAddressIdOnCheckout);
                if (modifiedShipToAddress != null)
                {
                    var nopAddressForModifiedShipToAddress = await _addressService.GetAddressByIdAsync(modifiedShipToAddress.AddressId);
                    var country = await _countryService.GetCountryByIdAsync(nopAddressForModifiedShipToAddress.CountryId ?? 0);
                    var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(nopAddressForModifiedShipToAddress.StateProvinceId ?? 0);
                    model.ExistingErpShipToAddresses.Add(new ErpShipToAddressModelForCheckout
                    {
                        Id = modifiedShipToAddress.Id,
                        ShipToCode = modifiedShipToAddress.ShipToCode,
                        ShipToName = modifiedShipToAddress.ShipToName,
                        Address1 = nopAddressForModifiedShipToAddress?.Address1,
                        Address2 = nopAddressForModifiedShipToAddress?.Address2,
                        Suburb = modifiedShipToAddress.Suburb,
                        City = nopAddressForModifiedShipToAddress?.City,
                        StateProvinceName = stateProvince?.Name,
                        ZipPostalCode = nopAddressForModifiedShipToAddress?.ZipPostalCode,
                        CountryName = country?.Name
                    });
                }
            }
            else
            {
                //Prepare ERP Ship To Address
                var shipToAddresses = await _erpShipToAddressService.GetErpShipToAddressesByAccountIdAsync(showHidden: false, isActiveOnly: true, accountId: b2BAccount.Id);
                foreach (var shipTo in shipToAddresses)
                {
                    nopAddress = await _addressService.GetAddressByIdAsync(shipTo.AddressId);
                    var country = await _countryService.GetCountryByIdAsync(nopAddress.CountryId ?? 0);
                    var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(nopAddress.StateProvinceId ?? 0);
                    model.ExistingErpShipToAddresses.Add(new ErpShipToAddressModelForCheckout
                    {
                        Id = shipTo.Id,
                        ShipToCode = shipTo.ShipToCode,
                        ShipToName = shipTo.ShipToName,
                        Address1 = nopAddress?.Address1,
                        Address2 = nopAddress?.Address2,
                        Suburb = shipTo.Suburb,
                        City = nopAddress?.City,
                        StateProvinceName = stateProvince?.Name,
                        ZipPostalCode = nopAddress?.ZipPostalCode,
                        CountryName = country?.Name
                    });
                }
            }

            model.AllowAddressEdit = await _erpCustomerFunctionalityService.CheckAllowAddressEdit(b2BAccount);
            model.IsQuoteOrder = false;

            //countries and states
            var countries = await _countryService.GetAllCountriesForShippingAsync();
            var erpShippingAddressModel = new ErpShipToAddressModelForCheckout();
            if (_addressSettings.PreselectCountryIfOnlyOne && countries.Count == 1)
            {
                erpShippingAddressModel.CountryId = countries[0].Id;
            }
            else
            {
                erpShippingAddressModel.AvailableCountries.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectCountry"), Value = string.Empty });
            }

            foreach (var c in countries)
            {
                erpShippingAddressModel.AvailableCountries.Add(new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedAsync(c, x => x.Name),
                    Value = c.Id.ToString(),
                    Selected = c.Id == erpShippingAddressModel.CountryId
                });
            }

            var states = (await _stateProvinceService
                .GetStateProvincesByCountryIdAsync(erpShippingAddressModel.CountryId.HasValue ? erpShippingAddressModel.CountryId.Value : 0, languageId)).ToList();
            if (states.Any())
            {
                erpShippingAddressModel.AvailableStates.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectState"), Value = string.Empty });

                foreach (var s in states)
                {
                    erpShippingAddressModel.AvailableStates.Add(new SelectListItem
                    {
                        Text = await _localizationService.GetLocalizedAsync(s, x => x.Name),
                        Value = s.Id.ToString(),
                        Selected = (s.Id == erpShippingAddressModel.StateProvinceId)
                    });
                }
            }
            else
            {
                var anyCountrySelected = erpShippingAddressModel.AvailableCountries.Any(x => x.Selected);
                erpShippingAddressModel.AvailableStates.Add(new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync(anyCountrySelected ? "Address.OtherNonUS" : "Address.SelectState"),
                    Value = "0"
                });
            }
            model.SelectedShipToAddress = erpShippingAddressModel;

            //load settings for current Store
            var b2BB2CFeaturesSettings = _settingService.LoadSetting<B2BB2CFeaturesSettings>((await _storeContext.GetCurrentStoreAsync()).Id);

            model.ErpToDetermineDate = b2BB2CFeaturesSettings.ERPToDetermineDate;
            if (model.ErpToDetermineDate)
            {
                model.AvailableDeliveryDates.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("B2B.DeliveryDate.SelectDeliveryDate"), Value = string.Empty, Selected = true });
            }

            // we have to load this data as well even if ERPToDetermineDate is enabled (if erp call to determine date failed, we will use this)
            (var minDeliveryDate, var maxDeliveryDate) = await _erpCustomerFunctionalityService.GetMinimumAndMaximumDeliveryDateForShippingAddress();

            model.DeliveryDate = minDeliveryDate;
            model.FormatedDeliveryDate = minDeliveryDate.ToString("dd/MM/yyyy");
            model.MinDeliveryDate = minDeliveryDate.ToString("yyyy-MM-dd"); // html only support this format
            model.MaxDeliveryDate = maxDeliveryDate.ToString("yyyy-MM-dd"); // html only support this format
            model.SelectedShipToAddress = new ErpShipToAddressModelForCheckout
            {
                Id = b2CShipToAddress.Id,
                ShipToCode = b2CShipToAddress.ShipToCode,
                ShipToName = b2CShipToAddress.ShipToName,
                Address1 = nopAddress?.Address1,
                Address2 = nopAddress?.Address2,
                Suburb = b2CShipToAddress.Suburb,
                City = nopAddress?.City,
                StateProvinceName = (await _stateProvinceService.GetStateProvinceByIdAsync(nopAddress.StateProvinceId ?? 0))?.Name,
                ZipPostalCode = nopAddress?.ZipPostalCode,
                CountryName = (await _countryService.GetCountryByIdAsync(nopAddress.CountryId ?? 0))?.Name,
                ErpSalesOrganizationId = b2BSalesOrg.Id,
                SalesOrganisationCode = b2BSalesOrg.Code
            };

            return model;
        }

        #endregion

    }
}