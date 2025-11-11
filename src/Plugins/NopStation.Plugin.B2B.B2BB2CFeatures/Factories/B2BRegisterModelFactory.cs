using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tax;
using Nop.Services.Attributes;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Models.Customer;
using NopStation.Plugin.B2B.B2BB2CFeatures.Helpers;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Registration;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories;

public class B2BRegisterModelFactory : IB2BRegisterModelFactory
{
    #region Fields

    private readonly CaptchaSettings _captchaSettings;
    private readonly CommonSettings _commonSettings;
    private readonly CustomerSettings _customerSettings;
    private readonly DateTimeSettings _dateTimeSettings;
    private readonly GdprSettings _gdprSettings;
    private readonly ICountryService _countryService;
    private readonly IAttributeParser<CustomerAttribute, CustomerAttributeValue> _customerAttributeParser;
    private readonly IAttributeService<CustomerAttribute, CustomerAttributeValue> _customerAttributeService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IGdprService _gdprService;
    private readonly ILocalizationService _localizationService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly IWorkContext _workContext;
    private readonly SecuritySettings _securitySettings;
    private readonly TaxSettings _taxSettings;
    private readonly ICommonHelper _commonHelper;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly IErpAccountService _erpAccountService;
    private readonly AddressSettings _addressSettings;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;

    #endregion

    #region Ctor

    public B2BRegisterModelFactory(CaptchaSettings captchaSettings,
        CommonSettings commonSettings,
        CustomerSettings customerSettings,
        DateTimeSettings dateTimeSettings,
        GdprSettings gdprSettings,
        ICountryService countryService,
        IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
        IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService,
        IDateTimeHelper dateTimeHelper,
        IGdprService gdprService,
        ILocalizationService localizationService,
        IStateProvinceService stateProvinceService,
        IWorkContext workContext,
        SecuritySettings securitySettings,
        TaxSettings taxSettings,
        ICommonHelper commonHelper,
        IErpSalesOrgService erpSalesOrgService,
        IStoreContext storeContext,
        IErpAccountService erpAccountService,
        ISettingService settingService,
        AddressSettings addressSettings,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings)
    {
        _captchaSettings = captchaSettings;
        _commonSettings = commonSettings;
        _customerSettings = customerSettings;
        _dateTimeSettings = dateTimeSettings;
        _gdprSettings = gdprSettings;
        _countryService = countryService;
        _customerAttributeParser = customerAttributeParser;
        _customerAttributeService = customerAttributeService;
        _dateTimeHelper = dateTimeHelper;
        _gdprService = gdprService;
        _localizationService = localizationService;
        _stateProvinceService = stateProvinceService;
        _workContext = workContext;
        _securitySettings = securitySettings;
        _taxSettings = taxSettings;
        _commonHelper = commonHelper;
        _erpSalesOrgService = erpSalesOrgService;
        _erpAccountService = erpAccountService;
        _settingService = settingService;
        _storeContext = storeContext;
        _addressSettings = addressSettings;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
    }

    #endregion

    #region Utilities

    /// <returns>A task that represents the asynchronous operation</returns>
    protected virtual async Task<GdprConsentModel> PrepareGdprConsentModelAsync(GdprConsent consent, bool accepted)
    {
        ArgumentNullException.ThrowIfNull(consent);

        var requiredMessage = await _localizationService.GetLocalizedAsync(consent, x => x.RequiredMessage);
        return new GdprConsentModel
        {
            Id = consent.Id,
            Message = await _localizationService.GetLocalizedAsync(consent, x => x.Message),
            IsRequired = consent.IsRequired,
            RequiredMessage = !string.IsNullOrEmpty(requiredMessage) ? requiredMessage : $"'{consent.Message}' is required",
            Accepted = accepted
        };
    }

    protected virtual async Task PrepareB2BSalesOrganisationsAsync(B2BRegisterModel model)
    {
        //get list of sales org here and prepare the dropdown data
        var salesOrganizations = await _erpSalesOrgService.GetAllErpSalesOrgAsync(showHidden: false);
        if (salesOrganizations.Count == 1)
        {
            model.B2BSalesOrganizationId = salesOrganizations.Select(x => x.Id).FirstOrDefault();
        }
        else
        {
            foreach (var salesOrg in salesOrganizations)
            {
                var items = new SelectListItem
                {
                    Text = salesOrg.Name,
                    Value = salesOrg.Id.ToString()
                };
                model.AvailableB2BSalesOrganizations.Add(items);
            }
            await _commonHelper.PrepareDefaultItemAsync(model.AvailableB2BSalesOrganizations, true);
        }
    }

    protected virtual void SetCustomerAddressFieldsAsRequired(B2BRegisterModel model)
    {
        model.PhoneEnabled = true;
        model.PhoneRequired = true;
        model.StreetAddressRequired = true;
        model.StreetAddressEnabled = true;
        model.StreetAddress2Enabled = true;
        model.StreetAddress2Required = _customerSettings.StreetAddress2Required;
        model.ZipPostalCodeEnabled = true;
        model.ZipPostalCodeRequired = true;
        model.CountryEnabled = true;
        model.CountryRequired = true;
        model.CityEnabled = true;
        model.CityRequired = true;
    }

    #endregion

    #region Methods

    public virtual async Task<B2BRegisterModel> PrepareB2BRegisterModelAsync(B2BRegisterModel model, bool excludeProperties,
        string overrideCustomCustomerAttributesXml = "", bool setDefaultValues = false)
    {
        ArgumentNullException.ThrowIfNull(model);

        var customer = await _workContext.GetCurrentCustomerAsync();

        model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
        foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
            model.AvailableTimeZones.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id, Selected = (excludeProperties ? tzi.Id == model.TimeZoneId : tzi.Id == (await _dateTimeHelper.GetCurrentTimeZoneAsync()).Id) });

        //VAT
        model.DisplayVatNumber = _taxSettings.EuVatEnabled;
        if (_taxSettings.EuVatEnabled && _taxSettings.EuVatEnabledForGuests)
            model.VatNumber = customer.VatNumber;

        //form fields
        model.FirstNameEnabled = _customerSettings.FirstNameEnabled;
        model.LastNameEnabled = _customerSettings.LastNameEnabled;
        model.FirstNameRequired = _customerSettings.FirstNameRequired;
        model.LastNameRequired = _customerSettings.LastNameRequired;
        model.GenderEnabled = _customerSettings.GenderEnabled;
        model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
        model.DateOfBirthRequired = _customerSettings.DateOfBirthRequired;
        model.CompanyEnabled = _customerSettings.CompanyEnabled;
        model.CompanyRequired = _customerSettings.CompanyRequired;
        model.StreetAddressEnabled = _customerSettings.StreetAddressEnabled;
        model.StreetAddressRequired = _customerSettings.StreetAddressRequired;
        model.StreetAddress2Enabled = _customerSettings.StreetAddress2Enabled;
        model.StreetAddress2Required = _customerSettings.StreetAddress2Required;
        model.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;
        model.ZipPostalCodeRequired = _customerSettings.ZipPostalCodeRequired;
        model.CityEnabled = _customerSettings.CityEnabled;
        model.CityRequired = _customerSettings.CityRequired;
        model.CountyEnabled = _customerSettings.CountyEnabled;
        model.CountyRequired = _customerSettings.CountyRequired;
        model.CountryEnabled = _customerSettings.CountryEnabled;
        model.CountryRequired = _customerSettings.CountryRequired;
        model.StateProvinceEnabled = _customerSettings.StateProvinceEnabled;
        model.StateProvinceRequired = _customerSettings.StateProvinceRequired;
        model.PhoneEnabled = _customerSettings.PhoneEnabled;
        model.PhoneRequired = _customerSettings.PhoneRequired;
        model.FaxEnabled = _customerSettings.FaxEnabled;
        model.FaxRequired = _customerSettings.FaxRequired;
        model.NewsletterEnabled = _customerSettings.NewsletterEnabled;
        model.AcceptPrivacyPolicyEnabled = _customerSettings.AcceptPrivacyPolicyEnabled;
        model.AcceptPrivacyPolicyPopup = _commonSettings.PopupForTermsOfServiceLinks;
        model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
        model.CheckUsernameAvailabilityEnabled = _customerSettings.CheckUsernameAvailabilityEnabled;
        model.HoneypotEnabled = _securitySettings.HoneypotEnabled;
        model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage;
        model.EnteringEmailTwice = _customerSettings.EnteringEmailTwice;

        if (!model.IsB2BUser)
        {
            if (_b2BB2CFeaturesSettings.DefaultB2CErpAccountId > 0)
            {
                var erpAccountInfo = await _erpAccountService.GetErpAccountByIdAsync(_b2BB2CFeaturesSettings.DefaultB2CErpAccountId);
                model.B2CIdentificationNumber = erpAccountInfo.AccountNumber;
                model.AccountName = erpAccountInfo.AccountName;
            }
        }

        if (setDefaultValues)
        {
            //enable newsletter by default
            model.Newsletter = _customerSettings.NewsletterTickedByDefault;
        }

        //countries and states
        if (_customerSettings.CountryEnabled || !model.IsB2BUser)
        {
            model.AvailableCountries.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectCountry"), Value = "0" });
            var currentLanguage = await _workContext.GetWorkingLanguageAsync();
            foreach (var c in await _countryService.GetAllCountriesAsync(currentLanguage.Id))
            {
                model.AvailableCountries.Add(new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedAsync(c, x => x.Name),
                    Value = c.Id.ToString(),
                    Selected = c.Id == model.CountryId
                });
            }

            if (_customerSettings.StateProvinceEnabled)
            {
                //states
                var states = (await _stateProvinceService.GetStateProvincesByCountryIdAsync(model.CountryId, currentLanguage.Id)).ToList();
                if (states.Count != 0)
                {
                    model.AvailableStates.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectState"), Value = "0" });

                    foreach (var s in states)
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = await _localizationService.GetLocalizedAsync(s, x => x.Name), Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
                    }
                }
                else
                {
                    var anyCountrySelected = model.AvailableCountries.Any(x => x.Selected);

                    model.AvailableStates.Add(new SelectListItem
                    {
                        Text = await _localizationService.GetResourceAsync(anyCountrySelected ? "Address.Other" : "Address.SelectState"),
                        Value = "0"
                    });
                }

            }
        }

        //custom customer attributes
        var customAttributes = await PrepareCustomCustomerAttributesAsync(customer, overrideCustomCustomerAttributesXml);
        foreach (var attribute in customAttributes)
            model.CustomerAttributes.Add(attribute);

        //GDPR
        if (_gdprSettings.GdprEnabled)
        {
            var consents = (await _gdprService.GetAllConsentsAsync()).Where(consent => consent.DisplayDuringRegistration).ToList();
            foreach (var consent in consents)
            {
                model.GdprConsents.Add(await PrepareGdprConsentModelAsync(consent, false));
            }
        }

        // B2B - Prepare available sales orgs
        await PrepareB2BSalesOrganisationsAsync(model);

        if (!model.IsB2BUser)
            SetCustomerAddressFieldsAsRequired(model);

        return model;
    }

    public virtual async Task<IList<CustomerAttributeModel>> PrepareCustomCustomerAttributesAsync(Customer customer, string overrideAttributesXml = "")
    {
        ArgumentNullException.ThrowIfNull(customer);

        var result = new List<CustomerAttributeModel>();

        var customerAttributes = await _customerAttributeService.GetAllAttributesAsync();

        foreach (var attribute in customerAttributes)
        {
            var attributeModel = new CustomerAttributeModel
            {
                Id = attribute.Id,
                Name = await _localizationService.GetLocalizedAsync(attribute, x => x.Name),
                IsRequired = attribute.IsRequired,
                AttributeControlType = attribute.AttributeControlType,
            };

            if (attribute.ShouldHaveValues)
            {
                //values

                var attributeValues = await _customerAttributeService.GetAttributeValuesAsync(attribute.Id);

                foreach (var attributeValue in attributeValues)
                {
                    var valueModel = new CustomerAttributeValueModel
                    {
                        Id = attributeValue.Id,
                        Name = await _localizationService.GetLocalizedAsync(attributeValue, x => x.Name),
                        IsPreSelected = attributeValue.IsPreSelected
                    };
                    attributeModel.Values.Add(valueModel);
                }
            }

            //set already selected attributes
            var selectedAttributesXml = !string.IsNullOrEmpty(overrideAttributesXml) ?
                overrideAttributesXml :
                customer.CustomCustomerAttributesXML;
            switch (attribute.AttributeControlType)
            {
                case AttributeControlType.DropdownList:
                case AttributeControlType.RadioList:
                case AttributeControlType.Checkboxes:
                    {
                        if (!string.IsNullOrEmpty(selectedAttributesXml))
                        {
                            if (!_customerAttributeParser.ParseValues(selectedAttributesXml, attribute.Id).Any())
                                break;

                            //clear default selection                                
                            foreach (var item in attributeModel.Values)
                                item.IsPreSelected = false;

                            //select new values
                            var selectedValues = await _customerAttributeParser.ParseAttributeValuesAsync(selectedAttributesXml);
                            foreach (var attributeValue in selectedValues)
                                foreach (var item in attributeModel.Values)
                                    if (attributeValue.Id == item.Id)
                                        item.IsPreSelected = true;
                        }
                    }
                    break;
                case AttributeControlType.ReadonlyCheckboxes:
                    {
                        //do nothing
                        //values are already pre-set
                    }
                    break;
                case AttributeControlType.TextBox:
                case AttributeControlType.MultilineTextbox:
                    {
                        if (!string.IsNullOrEmpty(selectedAttributesXml))
                        {
                            var enteredText = _customerAttributeParser.ParseValues(selectedAttributesXml, attribute.Id);
                            if (enteredText.Any())
                                attributeModel.DefaultValue = enteredText[0];
                        }
                    }
                    break;
                case AttributeControlType.ColorSquares:
                case AttributeControlType.ImageSquares:
                case AttributeControlType.Datepicker:
                case AttributeControlType.FileUpload:
                default:
                    //not supported attribute control types
                    break;
            }

            result.Add(attributeModel);
        }

        return result;
    }

    public virtual async Task<ErpAccountCustomerRegistrationFormModel> PrepareErpAccountCustomerRegistrationFormModelAsync(ErpAccountCustomerRegistrationFormModel model, bool setDefaultValues = false)
    {
        ArgumentNullException.ThrowIfNull(model);
        await PrepareROAddressModelAsync(model.RegisteredOfficeAddress, null, false, _addressSettings, loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id));
        await PreparePTAddressModelAsync(model.PhysicalTradingAddressModel.PhysicalTradingAddress, null, false, _addressSettings, loadCountries: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id));
        return model;
    }

    public virtual async Task PrepareROAddressModelAsync(RegisteredOfficeAddressModel model,
        Address address, bool excludeProperties,
        AddressSettings addressSettings,
        Func<Task<IList<Country>>> loadCountries = null,
        bool prePopulateWithCustomerFields = false,
        Customer customer = null,
        string overrideAttributesXml = "")
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(addressSettings);

        if (!excludeProperties && address != null)
        {
            model.Id = address.Id;
            model.FirstName_ROA = address.FirstName;
            model.LastName_ROA = address.LastName;
            model.Email_ROA = address.Email;
            model.Company_ROA = address.Company;
            model.CountryId_ROA = address.CountryId;
            model.CountryName_ROA = await _countryService.GetCountryByAddressAsync(address) is Country country ? await _localizationService.GetLocalizedAsync(country, x => x.Name) : null;
            model.StateProvinceId_ROA = address.StateProvinceId;
            model.StateProvinceName_ROA = await _stateProvinceService.GetStateProvinceByAddressAsync(address) is StateProvince stateProvince ? await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name) : null;
            model.County_ROA = address.County;
            model.City_ROA = address.City;
            model.Address1_ROA = address.Address1;
            model.Address2_ROA = address.Address2;
            model.ZipPostalCode_ROA = address.ZipPostalCode;
            model.PhoneNumber_ROA = address.PhoneNumber;
            model.FaxNumber_ROA = address.FaxNumber;
        }
        if (address == null && prePopulateWithCustomerFields)
        {
            if (customer == null)
                throw new Exception("Customer cannot be null when prepopulating an address");
            model.Email_ROA = customer.Email;
            model.FirstName_ROA = customer.FirstName;
            model.LastName_ROA = customer.LastName;
            model.Company_ROA = customer.Company;
            model.Address1_ROA = customer.StreetAddress;
            model.Address2_ROA = customer.StreetAddress2;
            model.ZipPostalCode_ROA = customer.ZipPostalCode;
            model.City_ROA = customer.City;
            model.County_ROA = customer.County;
            model.PhoneNumber_ROA = customer.Phone;
            model.FaxNumber_ROA = customer.Fax;
        }
        //countries and states
        if (addressSettings.CountryEnabled && loadCountries != null)
        {
            var countries = await loadCountries();
            if (_addressSettings.PreselectCountryIfOnlyOne && countries.Count == 1)
            {
                model.CountryId_ROA = countries[0].Id;
            }
            else
            {
                model.AvailableCountries_ROA.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectCountry"), Value = "0" });
            }
            foreach (var c in countries)
            {
                model.AvailableCountries_ROA.Add(new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedAsync(c, x => x.Name),
                    Value = c.Id.ToString(),
                    Selected = c.Id == model.CountryId_ROA
                });
            }
            if (addressSettings.StateProvinceEnabled)
            {
                var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
                var states = (await _stateProvinceService
                    .GetStateProvincesByCountryIdAsync(model.CountryId_ROA ?? 0, languageId))
                    .ToList();
                if (states.Any())
                {
                    model.AvailableStates_ROA.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectState"), Value = "0" });
                    foreach (var s in states)
                    {
                        model.AvailableStates_ROA.Add(new SelectListItem
                        {
                            Text = await _localizationService.GetLocalizedAsync(s, x => x.Name),
                            Value = s.Id.ToString(),
                            Selected = (s.Id == model.StateProvinceId_ROA)
                        });
                    }
                }
                else
                {
                    var anyCountrySelected = model.AvailableCountries_ROA.Any(x => x.Selected);
                    model.AvailableStates_ROA.Add(new SelectListItem
                    {
                        Text = await _localizationService.GetResourceAsync(anyCountrySelected ? "Address.Other" : "Address.SelectState"),
                        Value = "0"
                    });
                }
            }
        }
        //form fields
        model.CompanyEnabled_ROA = addressSettings.CompanyEnabled;
        model.CompanyRequired_ROA = addressSettings.CompanyRequired;
        model.StreetAddressEnabled_ROA = addressSettings.StreetAddressEnabled;
        model.StreetAddressRequired_ROA = addressSettings.StreetAddressRequired;
        model.StreetAddress2Enabled_ROA = addressSettings.StreetAddress2Enabled;
        model.StreetAddress2Required_ROA = addressSettings.StreetAddress2Required;
        model.ZipPostalCodeEnabled_ROA = addressSettings.ZipPostalCodeEnabled;
        model.ZipPostalCodeRequired_ROA = addressSettings.ZipPostalCodeRequired;
        model.CityEnabled_ROA = addressSettings.CityEnabled;
        model.CityRequired_ROA = addressSettings.CityRequired;
        model.CountyEnabled_ROA = addressSettings.CountyEnabled;
        model.CountyRequired_ROA = addressSettings.CountyRequired;
        model.CountryEnabled_ROA = addressSettings.CountryEnabled;
        model.StateProvinceEnabled_ROA = addressSettings.StateProvinceEnabled;
        model.PhoneEnabled_ROA = addressSettings.PhoneEnabled;
        model.PhoneRequired_ROA = addressSettings.PhoneRequired;
        model.FaxEnabled_ROA = addressSettings.FaxEnabled;
        model.FaxRequired_ROA = addressSettings.FaxRequired;
    }

    public virtual async Task PreparePTAddressModelAsync(PhysicalTradingAddressModel model,
        Address address, bool excludeProperties,
        AddressSettings addressSettings,
        Func<Task<IList<Country>>> loadCountries = null,
        bool prePopulateWithCustomerFields = false,
        Customer customer = null,
        string overrideAttributesXml = "")
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(addressSettings);

        if (!excludeProperties && address != null)
        {
            model.Id = address.Id;
            model.FirstName_PTA = address.FirstName;
            model.LastName_PTA = address.LastName;
            model.Email_PTA = address.Email;
            model.Company_PTA = address.Company;
            model.CountryId_PTA = address.CountryId;
            model.CountryName_PTA = await _countryService.GetCountryByAddressAsync(address) is Country country ? await _localizationService.GetLocalizedAsync(country, x => x.Name) : null;
            model.StateProvinceId_PTA = address.StateProvinceId;
            model.StateProvinceName_PTA = await _stateProvinceService.GetStateProvinceByAddressAsync(address) is StateProvince stateProvince ? await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name) : null;
            model.County_PTA = address.County;
            model.City_PTA = address.City;
            model.Address1_PTA = address.Address1;
            model.Address2_PTA = address.Address2;
            model.ZipPostalCode_PTA = address.ZipPostalCode;
            model.PhoneNumber_PTA = address.PhoneNumber;
            model.FaxNumber_PTA = address.FaxNumber;
        }
        if (address == null && prePopulateWithCustomerFields)
        {
            if (customer == null)
                throw new Exception("Customer cannot be null when prepopulating an address");
            model.Email_PTA = customer.Email;
            model.FirstName_PTA = customer.FirstName;
            model.LastName_PTA = customer.LastName;
            model.Company_PTA = customer.Company;
            model.Address1_PTA = customer.StreetAddress;
            model.Address2_PTA = customer.StreetAddress2;
            model.ZipPostalCode_PTA = customer.ZipPostalCode;
            model.City_PTA = customer.City;
            model.County_PTA = customer.County;
            model.PhoneNumber_PTA = customer.Phone;
            model.FaxNumber_PTA = customer.Fax;
        }
        //countries and states
        if (addressSettings.CountryEnabled && loadCountries != null)
        {
            var countries = await loadCountries();
            if (_addressSettings.PreselectCountryIfOnlyOne && countries.Count == 1)
            {
                model.CountryId_PTA = countries[0].Id;
            }
            else
            {
                model.AvailableCountries_PTA.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectCountry"), Value = "0" });
            }
            foreach (var c in countries)
            {
                model.AvailableCountries_PTA.Add(new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedAsync(c, x => x.Name),
                    Value = c.Id.ToString(),
                    Selected = c.Id == model.CountryId_PTA
                });
            }
            if (addressSettings.StateProvinceEnabled)
            {
                var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
                var states = (await _stateProvinceService
                    .GetStateProvincesByCountryIdAsync(model.CountryId_PTA ?? 0, languageId))
                    .ToList();
                if (states.Any())
                {
                    model.AvailableStates_PTA.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectState"), Value = "0" });
                    foreach (var s in states)
                    {
                        model.AvailableStates_PTA.Add(new SelectListItem
                        {
                            Text = await _localizationService.GetLocalizedAsync(s, x => x.Name),
                            Value = s.Id.ToString(),
                            Selected = (s.Id == model.StateProvinceId_PTA)
                        });
                    }
                }
                else
                {
                    var anyCountrySelected = model.AvailableCountries_PTA.Any(x => x.Selected);
                    model.AvailableStates_PTA.Add(new SelectListItem
                    {
                        Text = await _localizationService.GetResourceAsync(anyCountrySelected ? "Address.Other" : "Address.SelectState"),
                        Value = "0"
                    });
                }
            }
        }
        //form fields
        model.CompanyEnabled_PTA = addressSettings.CompanyEnabled;
        model.CompanyRequired_PTA = addressSettings.CompanyRequired;
        model.StreetAddressEnabled_PTA = addressSettings.StreetAddressEnabled;
        model.StreetAddressRequired_PTA = addressSettings.StreetAddressRequired;
        model.StreetAddress2Enabled_PTA = addressSettings.StreetAddress2Enabled;
        model.StreetAddress2Required_PTA = addressSettings.StreetAddress2Required;
        model.ZipPostalCodeEnabled_PTA = addressSettings.ZipPostalCodeEnabled;
        model.ZipPostalCodeRequired_PTA = addressSettings.ZipPostalCodeRequired;
        model.CityEnabled_PTA = addressSettings.CityEnabled;
        model.CityRequired_PTA = addressSettings.CityRequired;
        model.CountyEnabled_PTA = addressSettings.CountyEnabled;
        model.CountyRequired_PTA = addressSettings.CountyRequired;
        model.CountryEnabled_PTA = addressSettings.CountryEnabled;
        model.StateProvinceEnabled_PTA = addressSettings.StateProvinceEnabled;
        model.PhoneEnabled_PTA = addressSettings.PhoneEnabled;
        model.PhoneRequired_PTA = addressSettings.PhoneRequired;
        model.FaxEnabled_PTA = addressSettings.FaxEnabled;
        model.FaxRequired_PTA = addressSettings.FaxRequired;
    }

    #endregion
}