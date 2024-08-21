using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Core.Http;
using Nop.Core.Http.Extensions;
using Nop.Services.Attributes;
using Nop.Services.Authentication;
using Nop.Services.Authentication.External;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.ExportImport;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Controllers;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Models.Customer;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.B2BB2CFeatures.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Account;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Registration;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpWorkflowMessage;
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Controllers;

public class B2BB2CCustomerController : CustomerController
{
    #region Fields

    private readonly IB2BB2CWorkContext _b2BB2CWorkContext;
    private readonly IB2BRegisterModelFactory _b2BRegisterModelFactory;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpNopUserService _erpNopUserService;
    private readonly IErpNopUserAccountMapService _erpNopUserAccountMapService;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly ISettingService _settingService;
    private readonly IErpLogsService _erpLogsService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginManager;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IErpActivityLogsService _erpActivityLogsService;
    private readonly IErpAccountCustomerRegistrationFormService _erpAccountCustomerRegistrationFormService;
    private readonly IErpAccountCustomerRegistrationBankingDetailsService _erpAccountCustomerRegistrationBankingDetailsService;
    private readonly IErpAccountCustomerRegistrationPremisesService _erpAccountCustomerRegistrationPremisesService;
    private readonly IErpAccountCustomerRegistrationPhysicalTradingAddressService _erpAccountCustomerRegistrationPhysicalTradingAddressService;
    private readonly IErpAccountCustomerRegistrationTradeReferencesService _erpAccountCustomerRegistrationTradeReferencesService;
    private readonly IErpWorkflowMessageService _erpWorkflowMessageService;

    #endregion

    #region Ctor

    public B2BB2CCustomerController(CaptchaSettings captchaSettings,
        CustomerSettings customerSettings,
        DateTimeSettings dateTimeSettings,
        GdprSettings gdprSettings,
        IAddressService addressService,
        IAuthenticationService authenticationService,
        ICountryService countryService,
        IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
        IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService,
        ICustomerRegistrationService customerRegistrationService,
        ICustomerService customerService,
        IEventPublisher eventPublisher,
        IGdprService gdprService,
        IGenericAttributeService genericAttributeService,
        ILocalizationService localizationService,
        ILogger logger,
        INewsLetterSubscriptionService newsLetterSubscriptionService,
        IStateProvinceService stateProvinceService,
        IStoreContext storeContext,
        ITaxService taxService,
        IWorkContext workContext,
        IB2BB2CWorkContext b2BB2CWorkContext,
        IWorkflowMessageService workflowMessageService,
        LocalizationSettings localizationSettings,
        TaxSettings taxSettings,
        IB2BRegisterModelFactory b2BRegisterModelFactory,
        IErpAccountService erpAccountService,
        IErpNopUserService erpNopUserService,
        IErpNopUserAccountMapService erpNopUserAccountMapService,
        IErpShipToAddressService erpShipToAddressService,
        ISettingService settingService,
        INotificationService notificationService,
        ForumSettings forumSettings,
        Nop.Web.Factories.IAddressModelFactory addressModelFactory,
        Nop.Web.Factories.ICustomerModelFactory customerModelFactory,
        AddressSettings addressSettings,
        HtmlEncoder htmlEncoder,
        IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
        ICurrencyService currencyService,
        ICustomerActivityService customerActivityService,
        IDownloadService downloadService,
        IExportManager exportManager,
        IExternalAuthenticationService externalAuthenticationService,
        IGiftCardService giftCardService,
        IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
        IOrderService orderService,
        IPermissionService permissionService,
        IPictureService pictureService,
        IPriceFormatter priceFormatter,
        IProductService productService,
        MediaSettings mediaSettings,
        MultiFactorAuthenticationSettings multiFactorAuthenticationSettings,
        StoreInformationSettings storeInformationSettings,
        IErpLogsService erpLogsService,
        IErpIntegrationPluginManager erpIntegrationPluginManager,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IStaticCacheManager staticCacheManager,
        IErpActivityLogsService erpActivityLogsService,
        IErpAccountCustomerRegistrationFormService erpAccountCustomerRegistrationFormService,
        IErpAccountCustomerRegistrationBankingDetailsService erpAccountCustomerRegistrationBankingDetailsService,
        IErpAccountCustomerRegistrationPremisesService erpAccountCustomerRegistrationPremisesService,
        IErpAccountCustomerRegistrationPhysicalTradingAddressService erpAccountCustomerRegistrationPhysicalTradingAddressService,
        IErpAccountCustomerRegistrationTradeReferencesService erpAccountCustomerRegistrationTradeReferencesService,
        IErpWorkflowMessageService erpWorkflowMessageService) :  base(addressSettings,
             captchaSettings,
             customerSettings,
             dateTimeSettings,
             forumSettings,
             gdprSettings,
             htmlEncoder,
             addressModelFactory,
             addressService,
             addressAttributeParser,
             customerAttributeParser,
             customerAttributeService,
             authenticationService,
             countryService,
             currencyService,
             customerActivityService,
             customerModelFactory,
             customerRegistrationService,
             customerService,
             downloadService,
             eventPublisher,
             exportManager,
             externalAuthenticationService,
             gdprService,
             genericAttributeService,
             giftCardService,
             localizationService,
             logger,
             multiFactorAuthenticationPluginManager,
             newsLetterSubscriptionService,
             notificationService,
             orderService,
             permissionService,
             pictureService,
             priceFormatter,
             productService,
             stateProvinceService,
             storeContext,
             taxService,
             workContext,
             workflowMessageService,
             localizationSettings,
             mediaSettings,
             multiFactorAuthenticationSettings,
             storeInformationSettings,
             taxSettings)
    {
        
        _b2BB2CWorkContext = b2BB2CWorkContext;
        _b2BRegisterModelFactory = b2BRegisterModelFactory;
        _erpAccountService = erpAccountService;
        _erpNopUserService = erpNopUserService;
        _erpNopUserAccountMapService = erpNopUserAccountMapService;
        _erpShipToAddressService = erpShipToAddressService;
        _settingService = settingService;
        _erpLogsService = erpLogsService;
        _erpIntegrationPluginManager = erpIntegrationPluginManager;
        _staticCacheManager = staticCacheManager;
        _erpActivityLogsService = erpActivityLogsService;
        _erpAccountCustomerRegistrationFormService = erpAccountCustomerRegistrationFormService;
        _erpAccountCustomerRegistrationBankingDetailsService = erpAccountCustomerRegistrationBankingDetailsService;
        _erpAccountCustomerRegistrationPremisesService = erpAccountCustomerRegistrationPremisesService;
        _erpAccountCustomerRegistrationPhysicalTradingAddressService = erpAccountCustomerRegistrationPhysicalTradingAddressService;
        _erpAccountCustomerRegistrationTradeReferencesService = erpAccountCustomerRegistrationTradeReferencesService;
        _erpWorkflowMessageService = erpWorkflowMessageService;
    }

    #endregion

    #region Utilities

    protected override async Task<string> ParseCustomCustomerAttributesAsync(IFormCollection form)
    {
        if (form is null)
            throw new ArgumentNullException(nameof(form));

        var attributesXml = string.Empty;
        var customerAttributes = await _customerAttributeService.GetAllAttributesAsync();

        foreach (var attribute in customerAttributes)
        {
            var controlId = $"{NopCustomerServicesDefaults.CustomerAttributePrefix}{attribute.Id}";
            StringValues ctrlAttributes;

            switch (attribute.AttributeControlType)
            {
                case AttributeControlType.DropdownList:
                case AttributeControlType.RadioList:
                    ctrlAttributes = form[controlId];
                    if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                    {
                        var selectedAttributeId = int.Parse(ctrlAttributes);
                        if (selectedAttributeId > 0)
                            attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                attribute, selectedAttributeId.ToString());
                    }

                    break;
                case AttributeControlType.Checkboxes:
                    var cblAttributes = form[controlId];
                    if (!StringValues.IsNullOrEmpty(cblAttributes))
                    {
                        foreach (var item in cblAttributes.ToString()
                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            var selectedAttributeId = int.Parse(item);
                            if (selectedAttributeId > 0)
                                attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                                    attribute, selectedAttributeId.ToString());
                        }
                    }

                    break;
                case AttributeControlType.ReadonlyCheckboxes:
                    //load read-only (already server-side selected) values
                    var attributeValues = await _customerAttributeService.GetAttributeValuesAsync(attribute.Id);
                    foreach (var selectedAttributeId in attributeValues
                        .Where(v => v.IsPreSelected)
                        .Select(v => v.Id)
                        .ToList())
                    {
                        attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                            attribute, selectedAttributeId.ToString());
                    }

                    break;
                case AttributeControlType.TextBox:
                case AttributeControlType.MultilineTextbox:
                    ctrlAttributes = form[controlId];
                    if (!StringValues.IsNullOrEmpty(ctrlAttributes))
                    {
                        var enteredText = ctrlAttributes.ToString().Trim();
                        attributesXml = _customerAttributeParser.AddAttribute(attributesXml,
                            attribute, enteredText);
                    }

                    break;
                case AttributeControlType.Datepicker:
                case AttributeControlType.ColorSquares:
                case AttributeControlType.ImageSquares:
                case AttributeControlType.FileUpload:
                //not supported customer attributes
                default:
                    break;
            }
        }

        return attributesXml;
    }

    protected override async Task LogGdprAsync(Customer customer, CustomerInfoModel oldCustomerInfoModel,
        CustomerInfoModel newCustomerInfoModel, IFormCollection form)
    {
        try
        {
            //consents
            var consents = (await _gdprService.GetAllConsentsAsync()).Where(consent => consent.DisplayOnCustomerInfoPage).ToList();
            foreach (var consent in consents)
            {
                var previousConsentValue = await _gdprService.IsConsentAcceptedAsync(consent.Id, customer.Id);
                var controlId = $"consent{consent.Id}";
                var cbConsent = form[controlId];
                if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.ToString().Equals("on"))
                {
                    //agree
                    if (!previousConsentValue.HasValue || !previousConsentValue.Value)
                    {
                        await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentAgree, consent.Message);
                    }
                }
                else
                {
                    //disagree
                    if (!previousConsentValue.HasValue || previousConsentValue.Value)
                    {
                        await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentDisagree, consent.Message);
                    }
                }
            }

            //newsletter subscriptions
            if (_gdprSettings.LogNewsletterConsent)
            {
                if (oldCustomerInfoModel.Newsletter && !newCustomerInfoModel.Newsletter)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentDisagree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                if (!oldCustomerInfoModel.Newsletter && newCustomerInfoModel.Newsletter)
                    await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
            }

            //user profile changes
            if (!_gdprSettings.LogUserProfileChanges)
                return;

            if (oldCustomerInfoModel.Gender != newCustomerInfoModel.Gender)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Gender")} = {newCustomerInfoModel.Gender}");

            if (oldCustomerInfoModel.FirstName != newCustomerInfoModel.FirstName)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.FirstName")} = {newCustomerInfoModel.FirstName}");

            if (oldCustomerInfoModel.LastName != newCustomerInfoModel.LastName)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.LastName")} = {newCustomerInfoModel.LastName}");

            if (oldCustomerInfoModel.ParseDateOfBirth() != newCustomerInfoModel.ParseDateOfBirth())
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.DateOfBirth")} = {newCustomerInfoModel.ParseDateOfBirth()}");

            if (oldCustomerInfoModel.Email != newCustomerInfoModel.Email)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Email")} = {newCustomerInfoModel.Email}");

            if (oldCustomerInfoModel.Company != newCustomerInfoModel.Company)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Company")} = {newCustomerInfoModel.Company}");

            if (oldCustomerInfoModel.StreetAddress != newCustomerInfoModel.StreetAddress)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.StreetAddress")} = {newCustomerInfoModel.StreetAddress}");

            if (oldCustomerInfoModel.StreetAddress2 != newCustomerInfoModel.StreetAddress2)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.StreetAddress2")} = {newCustomerInfoModel.StreetAddress2}");

            if (oldCustomerInfoModel.ZipPostalCode != newCustomerInfoModel.ZipPostalCode)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.ZipPostalCode")} = {newCustomerInfoModel.ZipPostalCode}");

            if (oldCustomerInfoModel.City != newCustomerInfoModel.City)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.City")} = {newCustomerInfoModel.City}");

            if (oldCustomerInfoModel.County != newCustomerInfoModel.County)
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.County")} = {newCustomerInfoModel.County}");

            if (oldCustomerInfoModel.CountryId != newCustomerInfoModel.CountryId)
            {
                var countryName = (await _countryService.GetCountryByIdAsync(newCustomerInfoModel.CountryId))?.Name;
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.Country")} = {countryName}");
            }

            if (oldCustomerInfoModel.StateProvinceId != newCustomerInfoModel.StateProvinceId)
            {
                var stateProvinceName = (await _stateProvinceService.GetStateProvinceByIdAsync(newCustomerInfoModel.StateProvinceId))?.Name;
                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ProfileChanged, $"{await _localizationService.GetResourceAsync("Account.Fields.StateProvince")} = {stateProvinceName}");
            }
        }
        catch (Exception exception)
        {
            await _logger.ErrorAsync(exception.Message, exception, customer);
        }
    }

    protected virtual void ValidateRequiredConsentsAsync(List<GdprConsent> consents, IFormCollection form)
    {
        foreach (var consent in consents)
        {
            var controlId = $"consent{consent.Id}";
            var cbConsent = form[controlId];
            if (StringValues.IsNullOrEmpty(cbConsent) || !cbConsent.ToString().Equals("on"))
            {
                ModelState.AddModelError("", consent.RequiredMessage);
            }
        }
    }

    protected virtual async Task<IActionResult> SaveNopCustomerAsync(B2BRegisterModel model, string returnUrl, bool captchaValid, IFormCollection form, Customer customer)
    {
        try
        {
            if (await _customerService.IsRegisteredAsync(customer))
            {
                await _authenticationService.SignOutAsync();

                await _eventPublisher.PublishAsync(new CustomerLoggedOutEvent(customer));

                await _b2BB2CWorkContext.SetCurrentERPCustomerAsync(await _customerService.InsertGuestCustomerAsync());
            }

            var store = await _storeContext.GetCurrentStoreAsync();
            var b2BB2CFeaturesSettings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(store.Id);
            customer.RegisteredInStoreId = store.Id;

            var customerAttributesXml = await ParseCustomCustomerAttributesAsync(form);

            if (!model.IsB2BUser)
            {
                var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);

                foreach (var error in customerAttributeWarnings)
                {
                    ModelState.AddModelError("", error);
                }
            }

            if (_captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage && !captchaValid)
            {
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage"));
            }

            if (_gdprSettings.GdprEnabled)
            {
                var consents = (await _gdprService
                    .GetAllConsentsAsync()).Where(consent => consent.DisplayDuringRegistration && consent.IsRequired).ToList();

                ValidateRequiredConsentsAsync(consents, form);
            }

            if (ModelState.IsValid)
            {
                var addErpCustomerActivityLogSystemKeyword = string.Empty;
                var customerEmail = model.Email?.Trim();
                var accountNumber = string.Empty;
                var isDefaultPaymentAccount = false;

                ErpAccount erpAccount = null;
                var existingShipToAddressList = new List<ErpShipToAddress>();
                var shipToAddressListFromErp = new List<ErpShipToAddressDataModel>();

                if (model.IsB2BUser)
                {
                    #region B2B user

                    addErpCustomerActivityLogSystemKeyword = "Erp_AddNewB2BCustomer";

                    await _genericAttributeService.SaveAttributeAsync<string?>(customer, nameof(ErpUserType), nameof(ErpUserType.B2BUser));

                    //check erpAccount and shipToAddress for B2B user
                    erpAccount = await _erpAccountService.GetErpAccountByErpAccountNumberAsync(model.AccountNumber);
                    if (erpAccount is null)
                    {
                        ModelState.AddModelError("", await _localizationService.GetResourceAsync("B2BB2C.Account.Registration.ERPAccountNotFound"));
                        await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Error, ErpSyncLavel.Account, await _localizationService.GetResourceAsync("B2BB2C.Account.Registration.ERPAccountNotFound"));
                    }
                    else
                    {
                        existingShipToAddressList = (await _erpShipToAddressService.GetErpShipToAddressesByErpAccountIdAsync(erpAccountId: erpAccount.Id))?.ToList();
                        if (existingShipToAddressList is null || !existingShipToAddressList.Any())
                        {
                            ModelState.AddModelError("", await _localizationService.GetResourceAsync("B2BB2C.Account.Registration.ShipToAddressNotFound"));
                            await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Error, ErpSyncLavel.Account, await _localizationService.GetResourceAsync("B2BB2C.Account.Registration.ShipToAddressNotFound"));
                        }
                    }

                    #endregion
                }
                else
                {
                    #region B2C user

                    addErpCustomerActivityLogSystemKeyword = "Erp_AddNewB2CCustomer";

                    await _genericAttributeService.SaveAttributeAsync<string?>(customer, nameof(ErpUserType), nameof(ErpUserType.B2CUser));

                    //ensure unique email and NID
                    if (await _customerService.GetCustomerByEmailAsync(customerEmail) != null)
                    {
                        ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Register.Errors.EmailAlreadyExists"));
                    }
                    else if (!b2BB2CFeaturesSettings.UseDefaultAccountForB2CUser && (await _erpAccountService.GetErpAccountByErpAccountNumberAsync(model.B2CIdentificationNumber) != null))
                    {
                        ModelState.AddModelError("", await _localizationService.GetResourceAsync("B2BB2C.Account.Registration.NIDAlreadyExists"));
                    }
                    else
                    {
                        var erpIntegrationPlugin = await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();
                        if (erpIntegrationPlugin == null)
                        {
                            ModelState.AddModelError("", await _localizationService.GetResourceAsync("B2BB2C.Account.Registration.AccountNotCreated"));
                            await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Error, ErpSyncLavel.Account, "Integration method not found.");
                        }
                        else if (!b2BB2CFeaturesSettings.UseDefaultAccountForB2CUser)
                        {
                            //create erpAccount for b2c user at ERP
                            var stateProvidence = await _stateProvinceService.GetStateProvinceByIdAsync(model.StateProvinceId);
                            var country = await _countryService.GetCountryByIdAsync(model.CountryId);

                            //If UseERPIntegration is not true, then ERPIntegrations will not be called
                            if (b2BB2CFeaturesSettings.UseERPIntegration)
                            {
                                var erpAccountInfo = await erpIntegrationPlugin.CreateAccountNoErpAsync(new ErpCreateAccountModel
                                {
                                    VatNumber = model.VatNumber ?? "Normal",
                                    StateProvince = stateProvidence == null ? string.Empty : stateProvidence.Name,
                                    Country = country == null ? " " : country.Name,
                                    County = model.County ?? string.Empty,
                                    City = model.City ?? string.Empty,
                                    ContactName = string.Concat(model.FirstName, " ", model.LastName),
                                    FaxNumber = model.Fax ?? string.Empty,
                                    PhoneNumber = model.Phone ?? string.Empty,
                                    PostalCode = model.ZipPostalCode ?? string.Empty,
                                    ZipPostalCode = model.ZipPostalCode ?? string.Empty,
                                    Address1 = model.StreetAddress ?? string.Empty,
                                    Address2 = model.StreetAddress2 ?? string.Empty,
                                    Email = model.Email ?? string.Empty,
                                    AccountNumber = model.AccountNumber ?? model.B2CIdentificationNumber,
                                    AccountName = model.AccountName ?? string.Empty,
                                });

                                if (erpAccountInfo.IsError)
                                {
                                    ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("B2BB2C.Account.Registration.AccountNotCreated"));
                                    ModelState.AddModelError(string.Empty, erpAccountInfo.ErrorShortMessage);
                                    await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Error, ErpSyncLavel.Account, erpAccountInfo.ErrorShortMessage, erpAccountInfo.ErrorFullMessage);
                                }
                                else
                                {
                                    await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Information, ErpSyncLavel.Account, $"ERP Account created successfully at ERP with Account Number: {erpAccountInfo.AccountNumber}");

                                    //create shipToAddress for b2c user at ERP
                                    if (erpAccountInfo != null && !string.IsNullOrEmpty(erpAccountInfo.AccountNumber))
                                    {
                                        isDefaultPaymentAccount = true;
                                        accountNumber = erpAccountInfo.AccountNumber;
                                        try
                                        {
                                            //fetch shipToAddress from ERP for new ErpAccount
                                            var erpShipToAddressesbyErpAccount = await erpIntegrationPlugin.GetShipToAddressByAccountNumberFromErpAsync(new ErpGetRequestModel
                                            {
                                                AccountNumber = erpAccountInfo.AccountNumber
                                            });

                                            if (erpShipToAddressesbyErpAccount.Data is null || erpShipToAddressesbyErpAccount.ErpResponseModel.IsError)
                                            {
                                                ModelState.AddModelError("", await _localizationService.GetResourceAsync("B2BB2C.Account.Registration.AccountNotCreated"));
                                                await _erpLogsService.ErrorAsync(await _localizationService.GetResourceAsync("B2BB2C.Account.Registration.AccountNotCreated.ShipToAddressNotCreatedAtERP"), ErpSyncLavel.Account);
                                            }
                                            else
                                            {
                                                shipToAddressListFromErp = erpShipToAddressesbyErpAccount.Data.ToList();
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            ModelState.AddModelError("", await _localizationService.GetResourceAsync("B2BB2C.Account.Registration.AccountNotCreated"));
                                            await _erpLogsService.ErrorAsync(ex.Message, ErpSyncLavel.Account);
                                            await _logger.ErrorAsync(ex.Message, ex, customer);
                                        }
                                    }
                                    else
                                    {
                                        ModelState.AddModelError("", await _localizationService.GetResourceAsync("B2BB2C.Account.Registration.AccountNotCreated"));
                                        await _erpLogsService.ErrorAsync(await _localizationService.GetResourceAsync("B2BB2C.Account.Registration.AccountNotCreated.AccountNotCreatedAtERP"), ErpSyncLavel.Account);
                                    }
                                }
                            }
                        }
                    }

                    #endregion
                }

                if (ModelState.ErrorCount == 0)
                {
                    var customerUserName = model.Username?.Trim();
                    var isApproved = false;
                    if (model.IsB2BUser)
                        isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.AdminApproval;
                    else
                        isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;

                    var registrationRequest = new CustomerRegistrationRequest(customer,
                        customerEmail,
                        _customerSettings.UsernamesEnabled ? customerUserName : customerEmail,
                        model.Password,
                        _customerSettings.DefaultPasswordFormat,
                        store.Id,
                        isApproved);

                    var registrationResult = await _customerRegistrationService.RegisterCustomerAsync(registrationRequest);

                    if (registrationResult.Success)
                    {
                        //properties
                        if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                            customer.TimeZoneId = model.TimeZoneId;

                        //VAT number
                        if (_taxSettings.EuVatEnabled)
                        {
                            customer.VatNumber = model.VatNumber;

                            var (vatNumberStatus, _, vatAddress) = await _taxService.GetVatNumberStatusAsync(model.VatNumber);
                            customer.VatNumberStatusId = (int)vatNumberStatus;
                            //send VAT number admin notification
                            if (!string.IsNullOrEmpty(model.VatNumber) && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted)
                                await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotificationAsync(customer, model.VatNumber, vatAddress, _localizationSettings.DefaultAdminLanguageId);
                        }

                        #region Customer Settings check

                        if (_customerSettings.GenderEnabled)
                            customer.Gender = model.Gender;
                        if (_customerSettings.FirstNameEnabled)
                            customer.FirstName = model.FirstName;
                        if (_customerSettings.LastNameEnabled)
                            customer.LastName = model.LastName;
                        if (_customerSettings.DateOfBirthEnabled)
                            customer.DateOfBirth = model.ParseDateOfBirth();
                        if (_customerSettings.CompanyEnabled)
                            customer.Company = model.Company;
                        if (_customerSettings.StreetAddressEnabled)
                            customer.StreetAddress = model.StreetAddress;
                        if (_customerSettings.StreetAddress2Enabled)
                            customer.StreetAddress2 = model.StreetAddress2;
                        if (_customerSettings.ZipPostalCodeEnabled)
                            customer.ZipPostalCode = model.ZipPostalCode;
                        if (_customerSettings.CityEnabled)
                            customer.City = model.City;
                        if (_customerSettings.CountyEnabled)
                            customer.County = model.County;
                        if (_customerSettings.CountryEnabled)
                            customer.CountryId = model.CountryId;
                        if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                            customer.StateProvinceId = model.StateProvinceId;
                        if (_customerSettings.PhoneEnabled)
                            customer.Phone = model.Phone;
                        if (_customerSettings.FaxEnabled)
                            customer.Fax = model.Fax;

                        //save customer attributes
                        customer.CustomCustomerAttributesXML = customerAttributesXml;
                        await _customerService.UpdateCustomerAsync(customer);

                        //newsletter
                        if (_customerSettings.NewsletterEnabled)
                        {
                            var isNewsletterActive = _customerSettings.UserRegistrationType != UserRegistrationType.EmailValidation;

                            //save newsletter value
                            var newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customerEmail, store.Id);
                            if (newsletter != null)
                            {
                                if (model.Newsletter)
                                {
                                    newsletter.Active = isNewsletterActive;
                                    await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(newsletter);

                                    //GDPR
                                    if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                                    {
                                        await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                                    }
                                }
                            }
                            else
                            {
                                if (model.Newsletter)
                                {
                                    await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new NewsLetterSubscription
                                    {
                                        NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                        Email = customerEmail,
                                        Active = isNewsletterActive,
                                        StoreId = store.Id,
                                        CreatedOnUtc = DateTime.UtcNow
                                    });

                                    //GDPR
                                    if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                                    {
                                        await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.Newsletter"));
                                    }
                                }
                            }
                        }

                        if (_customerSettings.AcceptPrivacyPolicyEnabled)
                        {
                            //privacy policy is required
                            //GDPR
                            if (_gdprSettings.GdprEnabled && _gdprSettings.LogPrivacyPolicyConsent)
                            {
                                await _gdprService.InsertLogAsync(customer, 0, GdprRequestType.ConsentAgree, await _localizationService.GetResourceAsync("Gdpr.Consent.PrivacyPolicy"));
                            }
                        }

                        //GDPR
                        if (_gdprSettings.GdprEnabled)
                        {
                            var consents = (await _gdprService.GetAllConsentsAsync()).Where(consent => consent.DisplayDuringRegistration).ToList();
                            foreach (var consent in consents)
                            {
                                var controlId = $"consent{consent.Id}";
                                var cbConsent = form[controlId];
                                if (!StringValues.IsNullOrEmpty(cbConsent) && cbConsent.ToString().Equals("on"))
                                {
                                    //agree
                                    await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentAgree, consent.Message);
                                }
                                else
                                {
                                    //disagree
                                    await _gdprService.InsertLogAsync(customer, consent.Id, GdprRequestType.ConsentDisagree, consent.Message);
                                }
                            }
                        }

                        #endregion

                        if (model.CountryId == 0)
                        {
                            model.CountryId = b2BB2CFeaturesSettings.DefaultCountryId;
                        }
                        if (model.StateProvinceId == 0)
                        {
                            model.StateProvinceId = (await _stateProvinceService.GetStateProvincesByCountryIdAsync(model.CountryId))?[0].Id ?? 0;
                        }

                        //insert default address (if possible)
                        var defaultAddress = new Address
                        {
                            FirstName = model.FirstName ?? string.Empty,
                            LastName = model.LastName ?? string.Empty,
                            Email = model.Email ?? string.Empty,
                            Company = model.Company ?? string.Empty,
                            CountryId = model.CountryId,
                            StateProvinceId = model.StateProvinceId,
                            County = model.County ?? string.Empty,
                            City = model.City ?? string.Empty,
                            Address1 = model.StreetAddress ?? string.Empty,
                            Address2 = model.StreetAddress2 ?? string.Empty,
                            ZipPostalCode = model.ZipPostalCode ?? string.Empty,
                            PhoneNumber = model.Phone ?? string.Empty,
                            FaxNumber = model.Fax ?? string.Empty,
                            CreatedOnUtc = customer.CreatedOnUtc
                        };

                        var defaultAddressValidation = await _addressService.IsAddressValidAsync(defaultAddress);

                        if (defaultAddressValidation)
                        {
                            await _addressService.InsertAddressAsync(defaultAddress);

                            await _customerService.InsertCustomerAddressAsync(customer, defaultAddress);

                            customer.BillingAddressId = defaultAddress.Id;
                            customer.ShippingAddressId = defaultAddress.Id;

                            await _customerService.UpdateCustomerAsync(customer);
                        }
                        else
                        {
                            await _erpLogsService.ErrorAsync("New address was not created due to invalidity.", ErpSyncLavel.Account);
                        }

                        //notifications
                        if (_customerSettings.NotifyNewCustomerRegistration)
                            await _workflowMessageService.SendCustomerRegisteredStoreOwnerNotificationMessageAsync(customer,
                                _localizationSettings.DefaultAdminLanguageId);

                        //raise event       
                        await _eventPublisher.PublishAsync(new CustomerRegisteredEvent(customer));
                        var shipToAddressIdForB2BB2CUserId = 0;

                        if (!model.IsB2BUser)
                        {
                            if (!b2BB2CFeaturesSettings.UseDefaultAccountForB2CUser)
                            {
                                #region Prepare and save ErpAccount 

                                erpAccount = new ErpAccount
                                {
                                    AccountNumber = accountNumber,
                                    AccountName = model.AccountName,
                                    BillingAddressId = defaultAddress.Id,
                                    BillingSuburb = "",
                                    AllowOverspend = true,
                                    PercentageOfStockAllowed = b2BB2CFeaturesSettings.PercentageOfStockAllowed,
                                    AllowAccountsAddressEditOnCheckout = true,
                                    IsDefaultPaymentAccount = isDefaultPaymentAccount,
                                    ErpAccountStatusTypeId = (int)ErpAccountStatusType.Normal,
                                    IsActive = true,
                                    CreatedOnUtc = DateTime.UtcNow,
                                    CreatedById = customer.Id,
                                    ErpSalesOrgId = b2BB2CFeaturesSettings.DefaultB2COrganizationId
                                };

                                await _erpAccountService.InsertErpAccountAsync(erpAccount);
                                await _erpLogsService.InformationAsync($"{await _localizationService.GetResourceAsync("Plugin.Misc.Nopstation.ERPIntegrationCore.ErpAccount.Added")}, Erp Account Id: {erpAccount.Id}. For register customer Id: {customer.Id}", ErpSyncLavel.Account, customer: customer);

                                //erp activity log
                                await _erpActivityLogsService.InsertErpActivityAsync("Erp_AddNewErpAccount",
                                    string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.AddNewErpAccount"),
                                    erpAccount.Id),
                                    erpAccount);

                                #endregion

                                if (defaultAddressValidation)
                                {
                                    #region Prepare and save ErpShipToAddress

                                    foreach (var shipToAddressModel in shipToAddressListFromErp)
                                    {
                                        var erpShipToAddress = new ErpShipToAddress
                                        {
                                            EmailAddresses = customer.Email,
                                            AddressId = defaultAddress.Id,
                                            ShipToCode = shipToAddressModel?.ShipToCode ?? string.Empty,
                                            ShipToName = shipToAddressModel?.ShipToName ?? string.Empty,
                                            DeliveryNotes = shipToAddressModel?.DeliveryNotes ?? string.Empty,
                                            RepNumber = shipToAddressModel?.RepNumber ?? string.Empty,
                                            RepFullName = shipToAddressModel?.RepFullName ?? string.Empty,
                                            RepPhoneNumber = shipToAddressModel?.RepPhoneNumber ?? string.Empty,
                                            RepEmail = shipToAddressModel?.RepEmail ?? string.Empty,
                                            ProvinceCode = (await _stateProvinceService.GetStateProvinceByIdAsync(defaultAddress.StateProvinceId ?? 0))?.Abbreviation ?? "",
                                            IsActive = true,
                                            CreatedById = customer.Id,
                                            CreatedOnUtc = DateTime.UtcNow,
                                        };

                                        await _erpShipToAddressService.InsertErpShipToAddressAsync(erpShipToAddress);
                                        await _erpShipToAddressService.InsertErpShipToAddressErpAccountMapAsync(erpAccount, erpShipToAddress);
                                        await _erpLogsService.InformationAsync($"{await _localizationService.GetResourceAsync("Admin.ErpShipToAddresss.Added")}, Erp Ship To Address Id: {erpShipToAddress.Id}. For register customer Id: {customer.Id}", ErpSyncLavel.Account, customer: customer);

                                        //erp activity log
                                        await _erpActivityLogsService.InsertErpActivityAsync("Erp_AddNewErpShipToAddress",
                                            string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.AddNewErpShipToAddress"),
                                            erpShipToAddress.Id, erpAccount.Id),
                                            erpShipToAddress);

                                        if (shipToAddressIdForB2BB2CUserId == 0)
                                            shipToAddressIdForB2BB2CUserId = erpShipToAddress.Id;
                                    }

                                    #endregion
                                }
                            }
                            else
                            {
                                erpAccount = await _erpAccountService.GetErpAccountByIdAsync(b2BB2CFeaturesSettings.DefaultB2CErpAccountId);

                                if (defaultAddressValidation)
                                {
                                    #region Prepare and save ErpShipToAddress

                                    var erpShipToAddress = new ErpShipToAddress
                                    {
                                        EmailAddresses = customer.Email,
                                        AddressId = defaultAddress.Id,
                                        ShipToCode = $"STC_{model.B2CIdentificationNumber}",
                                        ShipToName = $"{model.FirstName} {model.LastName}",
                                        DeliveryNotes = string.Empty,
                                        RepNumber = model?.RepNumber ?? string.Empty,
                                        RepFullName = model?.RepFullName ?? string.Empty,
                                        RepPhoneNumber = model?.RepPhoneNumber ?? string.Empty,
                                        RepEmail = model?.RepEmail ?? string.Empty,
                                        ProvinceCode = (await _stateProvinceService.GetStateProvinceByIdAsync(defaultAddress.StateProvinceId ?? 0))?.Abbreviation ?? "",
                                        IsActive = true,
                                        CreatedById = customer.Id,
                                        CreatedOnUtc = DateTime.UtcNow,
                                    };

                                    await _erpShipToAddressService.InsertErpShipToAddressAsync(erpShipToAddress);
                                    await _erpShipToAddressService.InsertErpShipToAddressErpAccountMapAsync(erpAccount, erpShipToAddress);
                                    await _erpLogsService.InformationAsync($"{await _localizationService.GetResourceAsync("Admin.ErpShipToAddresss.Added")}, Erp Ship To Address Id: {erpShipToAddress.Id}. For register customer Id: {customer.Id}", ErpSyncLavel.Account, customer: customer);

                                    //erp activity log
                                    await _erpActivityLogsService.InsertErpActivityAsync("Erp_AddNewErpShipToAddress",
                                        string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.AddNewErpShipToAddress"),
                                        erpShipToAddress.Id, erpAccount.Id),
                                        erpShipToAddress);

                                    if (shipToAddressIdForB2BB2CUserId == 0)
                                        shipToAddressIdForB2BB2CUserId = erpShipToAddress.Id;

                                    #endregion
                                }
                            }
                        }
                        else
                        {
                            if (defaultAddressValidation)
                            {
                                shipToAddressIdForB2BB2CUserId = existingShipToAddressList.Any() ? existingShipToAddressList[0].Id : defaultAddress.Id;
                            }
                        }

                        #region Prepare and save ErpNopUser

                        if (!defaultAddressValidation && shipToAddressIdForB2BB2CUserId == 0)
                        {
                            await _erpLogsService.ErrorAsync($"New ShipToAddress was not created for Erp Account ({erpAccount.AccountNumber}), Customer Id: {customer.Id} due to invalid address.", ErpSyncLavel.Account);
                        }

                        var erpNopUser = new ErpNopUser
                        {
                            NopCustomerId = customer.Id,
                            ErpAccountId = erpAccount.Id,
                            ErpShipToAddressId = shipToAddressIdForB2BB2CUserId,
                            BillingErpShipToAddressId = 0,
                            ShippingErpShipToAddressId = 0,
                            ErpUserTypeId = model.IsB2BUser ? (int)ErpUserType.B2BUser : (int)ErpUserType.B2CUser,
                            IsActive = true,
                            CreatedOnUtc = DateTime.UtcNow,
                            CreatedById = customer.Id,
                            UpdatedOnUtc = DateTime.UtcNow,
                            UpdatedById = customer.Id
                        };
                        await _erpNopUserService.InsertErpNopUserAsync(erpNopUser);

                        await _erpLogsService.InformationAsync($"{await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Added")}, Erp Nop User Id: {erpNopUser.Id}. For register customer Id: {customer.Id}", ErpSyncLavel.Account, customer: customer);

                        //erp activity log
                        await _erpActivityLogsService.InsertErpActivityAsync("Erp_AddNewErpNopUser",
                            string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.AddNewErpNopUser"),
                            erpNopUser.Id),
                            erpNopUser);

                        var customerRolesIds = (await _customerService.GetCustomerRoleIdsAsync(customer)).ToList();

                        var b2BB2CustomerRole = await _customerService.GetCustomerRoleBySystemNameAsync(model.IsB2BUser ? ERPIntegrationCoreDefaults.B2BCustomerRole : ERPIntegrationCoreDefaults.B2CCustomerRole);
                        if (b2BB2CustomerRole != null)
                        {
                            customerRolesIds.Add(b2BB2CustomerRole.Id);
                        }

                        var commaSeparatedRolesIds = string.Empty;
                        if (customerRolesIds.Any())
                        {
                            commaSeparatedRolesIds = string.Join(",", customerRolesIds);
                        }

                        //prepare and save erpNopUser 
                        var erpNopUserAccountMap = new ErpNopUserAccountMap
                        {
                            ErpAccountId = erpAccount.Id,
                            ErpUserId = erpNopUser.Id,
                            CustomerRolesIds = commaSeparatedRolesIds
                        };
                        await _erpNopUserAccountMapService.InsertErpNopUserAccountMapAsync(erpNopUserAccountMap);

                        //erp activity log
                        await _erpActivityLogsService.InsertErpActivityAsync("Erp_AddNewErpNopUserAccountMap",
                            string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.AddNewErpNopUserAccountMap"),
                            erpNopUserAccountMap.Id),
                            erpNopUserAccountMap);

                        #endregion

                        var currentLanguage = await _workContext.GetWorkingLanguageAsync();

                        if (string.IsNullOrEmpty(customer.Email) && !string.IsNullOrEmpty(customerEmail))
                            customer.Email = customerEmail;

                        await _erpLogsService.InformationAsync($"Registration successful! Customer Id: {customer.Id}", ErpSyncLavel.Account, customer: customer);

                        //erp activity log
                        await _erpActivityLogsService.InsertErpActivityAsync(addErpCustomerActivityLogSystemKeyword,
                            string.Format(
                            addErpCustomerActivityLogSystemKeyword == "Erp_AddNewB2BCustomer" ?
                            await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.AddNewB2BCustomer") :
                            await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.AddNewB2CCustomer"),
                            customer.Id),
                            customer);

                        switch (_customerSettings.UserRegistrationType)
                        {
                            case UserRegistrationType.EmailValidation:
                                //email validation message
                                await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AccountActivationTokenAttribute, Guid.NewGuid().ToString());
                                await _workflowMessageService.SendCustomerEmailValidationMessageAsync(customer, currentLanguage.Id);

                                //result
                                return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.EmailValidation, returnUrl });

                            case UserRegistrationType.AdminApproval:
                                return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.AdminApproval, returnUrl });

                            case UserRegistrationType.Standard:
                                //send customer welcome message
                                await _workflowMessageService.SendCustomerWelcomeMessageAsync(customer, currentLanguage.Id);

                                //raise event       
                                await _eventPublisher.PublishAsync(new CustomerActivatedEvent(customer));

                                returnUrl = Url.RouteUrl("RegisterResult", new { resultId = (int)UserRegistrationType.Standard, returnUrl });
                                return await _customerRegistrationService.SignInCustomerAsync(customer, returnUrl, true);

                            default:
                                //If we got this far, something failed, redisplay form
                                model = await _b2BRegisterModelFactory.PrepareB2BRegisterModelAsync(model, false, setDefaultValues: true);
                                return View(model);
                        }
                    }

                    //errors
                    foreach (var error in registrationResult.Errors)
                        ModelState.AddModelError("", error);
                }
            }
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Error, ErpSyncLavel.Account, "Error while saving the Erp Nop User!" + (customer.Id > 0 ? $"Customer: {customer.FirstName} (Id - {customer.Id})" : string.Empty) + $" Exception Message: {ex.Message}", ex.StackTrace, customer: customer);
        }

        //If we got this far, something failed, redisplay form
        model = await _b2BRegisterModelFactory.PrepareB2BRegisterModelAsync(model, false, setDefaultValues: true);
        return View(model);
    }

    protected async Task<bool> B2BUserRegisterAllowedAsync()
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(storeScope);

        return settings.IsB2BUserRegisterAllowed;
    }

    protected async Task<bool> B2CUserRegisterAllowedAsync()
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(storeScope);

        return settings.IsB2CUserRegisterAllowed;
    }

    #endregion

    #region Methods

    #region B2BRegister

    //[HttpsRequirement(SslRequirement.Yes)]
    //available even when navigation is not allowed
    [CheckAccessPublicStore(true)]
    public virtual async Task<IActionResult> B2BRegister()
    {
        //check whether registration is allowed
        if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled || !await B2BUserRegisterAllowedAsync())
            return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled });

        var model = new B2BRegisterModel();
        model.IsB2BUser = true;
        model = await _b2BRegisterModelFactory.PrepareB2BRegisterModelAsync(model, false, setDefaultValues: true);

        return View(model);
    }

    [HttpPost]
    [ValidateCaptcha]
    [ValidateHoneypot]
    //[PublicAntiForgery]
    //available even when navigation is not allowed
    [CheckAccessPublicStore(true)]
    public virtual async Task<IActionResult> B2BRegister(B2BRegisterModel model, string returnUrl, bool captchaValid, IFormCollection form)
    {
        //check whether registration is allowed
        if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled || !await B2BUserRegisterAllowedAsync())
            return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled, returnUrl });
        var erpCustomer = await _b2BB2CWorkContext.GetCurrentERPCustomerAsync();

        return await SaveNopCustomerAsync(model, returnUrl, captchaValid, form, erpCustomer.Customer);
    }

    #endregion

    #region B2CRegister

    //available even when navigation is not allowed
    [CheckAccessPublicStore(true)]
    public virtual async Task<IActionResult> B2CRegister()
    {
        //check whether registration is allowed
        if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled || !await B2CUserRegisterAllowedAsync())
            return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled });

        var model = new B2BRegisterModel();
        model = await _b2BRegisterModelFactory.PrepareB2BRegisterModelAsync(model, false, setDefaultValues: true);
        model.IsB2BUser = false;

        return View(model);
    }

    [HttpPost]
    [ValidateCaptcha]
    [ValidateHoneypot]
    //[PublicAntiForgery]
    //available even when navigation is not allowed
    [CheckAccessPublicStore(true)]
    public virtual async Task<IActionResult> B2CRegister(B2BRegisterModel model, string returnUrl, bool captchaValid, IFormCollection form)
    {
        //check whether registration is allowed
        if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled || !await B2CUserRegisterAllowedAsync())
            return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled, returnUrl });
        var erpCustomer = await _b2BB2CWorkContext.GetCurrentERPCustomerAsync();

        return await SaveNopCustomerAsync(model, returnUrl, captchaValid, form, erpCustomer.Customer);
    }

    #endregion

    #region Login / logout

    //available even when a store is closed
    [CheckAccessClosedStore(ignore: true)]
    //available even when navigation is not allowed
    [CheckAccessPublicStore(ignore: true)]
    public override async Task<IActionResult> Login(bool? checkoutAsGuest)
    {
        var model = await _customerModelFactory.PrepareLoginModelAsync(checkoutAsGuest);
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (await _customerService.IsRegisteredAsync(customer))
        {
            var fullName = await _customerService.GetCustomerFullNameAsync(customer);
            var message = await _localizationService.GetResourceAsync("Account.Login.AlreadyLogin");
            _notificationService.SuccessNotification(string.Format(message, _htmlEncoder.Encode(fullName)));
        }

        return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/B2BB2CCustomer/Login.cshtml", model);
    }


    [HttpPost]
    [ValidateCaptcha]
    //available even when a store is closed
    [CheckAccessClosedStore(ignore: true)]
    //available even when navigation is not allowed
    [CheckAccessPublicStore(ignore: true)]
    public override async Task<IActionResult> Login(LoginModel model, string returnUrl, bool captchaValid)
    {
        //validate CAPTCHA
        if (_captchaSettings.Enabled && _captchaSettings.ShowOnLoginPage && !captchaValid)
        {
            ModelState.AddModelError("", await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage"));
        }

        if (ModelState.IsValid)
        {
            var customerUserName = model.Username?.Trim();
            var customerEmail = model.Email?.Trim();
            var userNameOrEmail = _customerSettings.UsernamesEnabled ? customerUserName : customerEmail;

            var loginResult = await _customerRegistrationService.ValidateCustomerAsync(userNameOrEmail, model.Password);
            switch (loginResult)
            {
                case CustomerLoginResults.Successful:
                    {
                        var customer = _customerSettings.UsernamesEnabled
                            ? await _customerService.GetCustomerByUsernameAsync(customerUserName)
                            : await _customerService.GetCustomerByEmailAsync(customerEmail);

                        return await _customerRegistrationService.SignInCustomerAsync(customer, returnUrl, model.RememberMe);
                    }
                case CustomerLoginResults.MultiFactorAuthenticationRequired:
                    {
                        var customerMultiFactorAuthenticationInfo = new CustomerMultiFactorAuthenticationInfo
                        {
                            UserName = userNameOrEmail,
                            RememberMe = model.RememberMe,
                            ReturnUrl = returnUrl
                        };
                        await HttpContext.Session.SetAsync(NopCustomerDefaults.CustomerMultiFactorAuthenticationInfo, customerMultiFactorAuthenticationInfo);
                        return RedirectToRoute("MultiFactorVerification");
                    }
                case CustomerLoginResults.CustomerNotExist:
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.CustomerNotExist"));
                    break;
                case CustomerLoginResults.Deleted:
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.Deleted"));
                    break;
                case CustomerLoginResults.NotActive:
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotActive"));
                    break;
                case CustomerLoginResults.NotRegistered:
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotRegistered"));
                    break;
                case CustomerLoginResults.LockedOut:
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.LockedOut"));
                    break;
                case CustomerLoginResults.WrongPassword:
                default:
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials"));
                    break;
            }
        }

        //If we got this far, something failed, redisplay form
        model = await _customerModelFactory.PrepareLoginModelAsync(model.CheckoutAsGuest);
        return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/B2BB2CCustomer/Login.cshtml", model);
    }

    //available even when a store is closed
    [CheckAccessClosedStore(ignore: true)]
    //available even when navigation is not allowed
    [CheckAccessPublicStore(ignore: true)]
    public override async Task<IActionResult> Logout()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (_workContext.OriginalCustomerIfImpersonated != null)
        {
            await _erpLogsService.InformationAsync($"Customer impersonation finished as: {customer.Email}, Customer Id: {customer.Id}. Original Customer Email: {_workContext.OriginalCustomerIfImpersonated.Email}, Id: {_workContext.OriginalCustomerIfImpersonated.Id}", ErpSyncLavel.LoginLogout, customer: customer);

            //erp activity log
            await _erpActivityLogsService.InsertErpActivityAsync(_workContext.OriginalCustomerIfImpersonated, "Erp_CustomerImpersonationEnd",
                string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.CustomerImpersonationEnded"),
            customer.Email, customer.Id, _workContext.OriginalCustomerIfImpersonated.Email, _workContext.OriginalCustomerIfImpersonated.Id), customer);

            //logout impersonated customer
            await _genericAttributeService
                .SaveAttributeAsync<int?>(_workContext.OriginalCustomerIfImpersonated, NopCustomerDefaults.ImpersonatedCustomerIdAttribute, null);

            if (await _customerService.IsAdminAsync(_workContext.OriginalCustomerIfImpersonated))
            {
                //redirect back to customer details page (admin area)
                return RedirectToAction("Edit", "Customer", new { id = customer.Id, area = AreaNames.ADMIN });
            }

            //redirect back to customer details page (admin area)
            return RedirectToAction("List");
        }

        //standard logout 
        await _authenticationService.SignOutAsync();

        //activity log
        await _erpLogsService.InformationAsync($"Customer Logged out as: {customer.Email}, Customer Id: {customer.Id}", ErpSyncLavel.LoginLogout, customer: customer);

        if (!await _customerService.IsAdminAsync(customer))
        {
            //erp activity log
            await _erpActivityLogsService.InsertErpActivityAsync(customer, "Erp_ErpCustomerPublicStoreLogOut",
            string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.CustomerPublicStoreLogOut"),
            customer.Email, customer.Id), customer);
        }
        else
        {
            //activity log
            await _customerActivityService.InsertActivityAsync(customer, "PublicStore.Login",
                await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Login"), customer);
        }

        //raise logged out event       
        await _eventPublisher.PublishAsync(new CustomerLoggedOutEvent(customer));

        //EU Cookie
        if (_storeInformationSettings.DisplayEuCookieLawWarning)
        {
            //the cookie law message should not pop up immediately after logout.
            //otherwise, the user will have to click it again...
            //and thus next visitor will not click it... so violation for that cookie law..
            //the only good solution in this case is to store a temporary variable
            //indicating that the EU cookie popup window should not be displayed on the next page open (after logout redirection to homepage)
            //but it'll be displayed for further page loads
            TempData[$"{NopCookieDefaults.Prefix}{NopCookieDefaults.IgnoreEuCookieLawWarning}"] = true;
        }
        return RedirectToRoute("Homepage");
    }

    #endregion

    #region ErpUserAccount

    [HttpPost]
    public async Task<IActionResult> SetErpAccount(AccountSwitchModel model)
    {
        var currentCustomer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();

        try
        {
            var erpUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(model.CustomerId);
            if (erpUser != null && model.ErpAccountId > 0 && erpUser.ErpAccountId != model.ErpAccountId)
            {
                erpUser.ErpAccountId = model.ErpAccountId;

                var defaultShipToAddress = (await _erpShipToAddressService.GetErpShipToAddressesByAccountIdAsync(showHidden: false, isActiveOnly: true, accountId: erpUser.ErpAccountId)).FirstOrDefault();
                erpUser.ErpShipToAddressId = defaultShipToAddress?.Id ?? 0;

                await _erpNopUserService.UpdateErpNopUserAsync(erpUser);

                await _staticCacheManager.RemoveAsync(_staticCacheManager.PrepareKeyForDefaultCache(NopEntityCacheDefaults<ErpNopUser>.ByIdCacheKey, erpUser.Id));

                await _b2BB2CWorkContext.SetCurrentERPCustomerAsync(erpAccountId: model.ErpAccountId);

                await _erpLogsService.InformationAsync($"Erp Account (Id: {erpUser.ErpAccountId}) set to Erp User Id: {erpUser.Id}", ErpSyncLavel.Account, customer: currentCustomer);

                //erp activity log
                await _erpActivityLogsService.InsertErpActivityAsync("Erp_ErpNopUserAccountSwitch",
                    string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.ErpNopUserAccountSwitch"),
                    erpUser.Id, erpUser.ErpAccountId),
                    erpUser);
            }
        }
        catch (Exception ex)
        {
            _notificationService.ErrorNotification(ex.Message);
            _logger.Error(ex.Message, ex, customer: currentCustomer);
            await _erpLogsService.ErrorAsync(ex.Message, ErpSyncLavel.Account, ex, customer: currentCustomer);
        }

        return Redirect(model.RedirectUrl);
    }

    #endregion

    #region B2BRegister

    //[HttpsRequirement(SslRequirement.Yes)]
    //available even when navigation is not allowed
    [CheckAccessPublicStore(true)]
    public virtual async Task<IActionResult> ErpAccountCustomerRegistrationForm()
    {
        //check whether registration is allowed
        if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled || !await B2BUserRegisterAllowedAsync())
            return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled });

        var model = new ErpAccountCustomerRegistrationFormModel();
        model = await _b2BRegisterModelFactory.PrepareErpAccountCustomerRegistrationFormModelAsync(model, setDefaultValues: true);

        return View(model);
    }

    [HttpPost]
    [ValidateCaptcha]
    [ValidateHoneypot]
    //[PublicAntiForgery]
    //available even when navigation is not allowed
    [CheckAccessPublicStore(true)]
    public virtual async Task<IActionResult> ErpAccountCustomerRegistrationForm(ErpAccountCustomerRegistrationFormModel model, string returnUrl, bool captchaValid, IFormCollection form)
    {
        //check whether registration is allowed
        if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled || !await B2BUserRegisterAllowedAsync())
            return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Disabled, returnUrl });

        if (ModelState.IsValid)
        {
            var registeredOfficeAddress = new Address()
            {
                Id = model.RegisteredOfficeAddress.Id,
                FirstName = model.RegisteredOfficeAddress.FirstName_ROA,
                LastName = model.RegisteredOfficeAddress.LastName_ROA,
                Email = model.RegisteredOfficeAddress.Email_ROA,
                Company = model.RegisteredOfficeAddress.Company_ROA,
                CountryId = model.RegisteredOfficeAddress.CountryId_ROA,
                StateProvinceId = model.RegisteredOfficeAddress.StateProvinceId_ROA,
                Address1 = model.RegisteredOfficeAddress.Address1_ROA,
                Address2 = model.RegisteredOfficeAddress.Address2_ROA,
                ZipPostalCode = model.RegisteredOfficeAddress.ZipPostalCode_ROA,
                PhoneNumber = model.RegisteredOfficeAddress.PhoneNumber_ROA,
                FaxNumber = model.RegisteredOfficeAddress.FaxNumber_ROA,
                CreatedOnUtc = DateTime.UtcNow,
            };

            await _addressService.InsertAddressAsync(registeredOfficeAddress);

            var applicationForm = new ErpAccountCustomerRegistrationForm
            {
                FullRegisteredName = model.FullRegisteredName ?? string.Empty,
                RegistrationNumber = model.RegistrationNumber ?? string.Empty,
                VatNumber = model.VatNumber ?? string.Empty,
                TelephoneNumber1 = model.TelephoneNumber1 ?? string.Empty,
                TelephoneNumber2 = model.TelephoneNumber2 ?? string.Empty,
                TelefaxNumber = model.TelefaxNumber ?? string.Empty,
                AccountsContactPersonNameSurname = model.AccountsContactPersonNameSurname ?? string.Empty,
                AccountsEmail = model.AccountsEmail ?? string.Empty,
                AccountsTelephoneNumber = model.AccountsTelephoneNumber ?? string.Empty,
                AccountsCellphoneNumber = model.AccountsCellphoneNumber ?? string.Empty,
                BuyerContactPersonNameSurname = model.BuyerContactPersonNameSurname ?? string.Empty,
                BuyerEmail = model.BuyerEmail ?? string.Empty,
                NatureOfBusiness = model.NatureOfBusiness ?? string.Empty,
                RegisteredOfficeAddressId = registeredOfficeAddress.Id,
                TypeOfBusiness = model.TypeOfBusiness ?? string.Empty,
                EstimatePurchasesPerMonthZAR = model.EstimatePurchasesPerMonthZAR,
                CreditLimitRequired = model.CreditLimitRequired,
                IsActive = true
            };

            applicationForm.CreatedOnUtc = DateTime.UtcNow;
            applicationForm.CreatedById = (await _b2BB2CWorkContext.GetCurrentCustomerAsync()).Id;

            await _erpAccountCustomerRegistrationFormService.InsertErpAccountCustomerRegistrationFormAsync(applicationForm);

            //Additional Info
            if (model.BankingDetailsModel != null)
            {
                var erpBankingDetails = new ErpAccountCustomerRegistrationBankingDetails
                {
                    FormId = applicationForm.Id,
                    NameOfBanker = model.BankingDetailsModel.NameOfBanker,
                    AccountNumber = model.BankingDetailsModel.AccountNumber,
                    AccountName = model.BankingDetailsModel.AccountName,
                    BranchCode = model.BankingDetailsModel.BranchCode,
                    Branch = model.BankingDetailsModel.Branch,
                    IsActive = true
                };

                await _erpAccountCustomerRegistrationBankingDetailsService.InsertErpAccountCustomerRegistrationBankingDetailsAsync(erpBankingDetails);
            }

            if (model.PhysicalTradingAddressModel != null)
            {
                var physicalTradingAddress = new Address()
                {
                    Id = model.RegisteredOfficeAddress.Id,
                    FirstName = model.RegisteredOfficeAddress.FirstName_ROA,
                    LastName = model.RegisteredOfficeAddress.LastName_ROA,
                    Email = model.RegisteredOfficeAddress.Email_ROA,
                    Company = model.RegisteredOfficeAddress.Company_ROA,
                    CountryId = model.RegisteredOfficeAddress.CountryId_ROA,
                    StateProvinceId = model.RegisteredOfficeAddress.StateProvinceId_ROA,
                    Address1 = model.RegisteredOfficeAddress.Address1_ROA,
                    Address2 = model.RegisteredOfficeAddress.Address2_ROA,
                    ZipPostalCode = model.RegisteredOfficeAddress.ZipPostalCode_ROA,
                    PhoneNumber = model.RegisteredOfficeAddress.PhoneNumber_ROA,
                    FaxNumber = model.RegisteredOfficeAddress.FaxNumber_ROA,
                    CreatedOnUtc = DateTime.UtcNow,
                };

                await _addressService.InsertAddressAsync(physicalTradingAddress);

                var erpPhysicalTradingAddress = new ErpAccountCustomerRegistrationPhysicalTradingAddress
                {
                    FormId = applicationForm.Id,
                    FullName = model.PhysicalTradingAddressModel.FullName,
                    Surname = model.PhysicalTradingAddressModel.Surname,
                    PhysicalTradingAddressId = physicalTradingAddress.Id,
                    IsActive = true
                };

                await _erpAccountCustomerRegistrationPhysicalTradingAddressService.InsertErpAccountCustomerRegistrationPhysicalTradingAddressAsync(erpPhysicalTradingAddress);
            }

            if (model.PremisesModel != null)
            {
                var erpPremises = new ErpAccountCustomerRegistrationPremises
                {
                    FormId = applicationForm.Id,
                    OwnedOrLeased = model.PremisesModel.OwnedOrLeased?.Trim().ToLower() == "true",
                    NameOfLandlord = model.PremisesModel.NameOfLandlord,
                    AddressOfLandlord = model.PremisesModel.AddressOfLandlord,
                    EmailOfLandlord = model.PremisesModel.EmailOfLandlord,
                    TelephoneNumberOfLandlord = model.PremisesModel.TelephoneNumberOfLandlord,
                    IsActive = true
                };

                await _erpAccountCustomerRegistrationPremisesService.InsertErpAccountCustomerRegistrationPremisesAsync(erpPremises);
            }

            if (model.TradeReferencesModel != null)
            {
                var erpTradeReferences = new ErpAccountCustomerRegistrationTradeReferences
                {
                    FormId = applicationForm.Id,
                    Name = model.TradeReferencesModel.Name,
                    Telephone = model.TradeReferencesModel.Telephone,
                    Amount = model.TradeReferencesModel.Amount,
                    Terms = model.TradeReferencesModel.Terms,
                    HowLong = model.TradeReferencesModel.HowLong,
                    IsActive = true
                };

                await _erpAccountCustomerRegistrationTradeReferencesService.InsertErpAccountCustomerRegistrationTradeReferencesAsync(erpTradeReferences);
            }

            var successMsg = await _localizationService.GetResourceAsync("B2BB2CFeatures.ErpAccountCustomerRegistrationForm.Added");
            _notificationService.SuccessNotification(successMsg);

            await _erpLogsService.InformationAsync(successMsg + " Erp Account Customer Registration Form Id: " + applicationForm.Id, ErpSyncLavel.Account, customer: await _b2BB2CWorkContext.GetCurrentCustomerAsync());

            //Send Email to Admin and Customer
            await _erpWorkflowMessageService.SendERPCustomerRegistrationApplicationCreatedNotificationAsync(applicationForm, (await _b2BB2CWorkContext.GetWorkingLanguageAsync()).Id);

            return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Standard, returnUrl });
        }
        return View(model);
    }

    #endregion

    #region Get Infos

    [HttpGet]
    public async Task<IActionResult> GetCountryTwoLetterIsoCode(string code)
    {
        return Ok(await _countryService.GetCountryByTwoLetterIsoCodeAsync(code));
    }

    #endregion

    #endregion
}