using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Data;
using Nop.Services.Attributes;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Localization;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.Overriden;

/// <summary>
/// Address service
/// </summary>
public partial class OverridenAddressService : AddressService
{
    #region Fields

    private readonly IWorkContext _workContext;
    private readonly IGenericAttributeService _genericAttributeService;

    #endregion

    #region Ctor

    public OverridenAddressService(AddressSettings addressSettings,
        IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
        IAttributeService<AddressAttribute, AddressAttributeValue> addressAttributeService,
        ICountryService countryService,
        IRepository<Address> addressRepository,
        IStateProvinceService stateProvinceService,
        IWorkContext workContext,
        ILocalizationService localizationService,
        IGenericAttributeService genericAttributeService) : base(addressSettings,
            addressAttributeParser,
            addressAttributeService,
            countryService,
            localizationService,
            addressRepository,
            stateProvinceService)
    {
        _workContext = workContext;
        _genericAttributeService = genericAttributeService;
    }

    #endregion

    #region Methods        

    /// <summary>
    /// Gets a value indicating whether address is valid (can be saved)
    /// </summary>
    /// <param name="address">Address to validate</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the result
    /// </returns>
    public override async Task<bool> IsAddressValidAsync(Address address)
    {
        ArgumentNullException.ThrowIfNull(address);

        if (string.IsNullOrWhiteSpace(address.FirstName))
            return false;

        if (string.IsNullOrWhiteSpace(address.LastName))
            return false;

        if (string.IsNullOrWhiteSpace(address.Email))
            return false;

        if (_addressSettings.CompanyEnabled &&
            _addressSettings.CompanyRequired &&
            string.IsNullOrWhiteSpace(address.Company))
            return false;

        if (_addressSettings.StreetAddressEnabled &&
            _addressSettings.StreetAddressRequired &&
            string.IsNullOrWhiteSpace(address.Address1))
            return false;

        if (_addressSettings.StreetAddress2Enabled &&
            _addressSettings.StreetAddress2Required &&
            string.IsNullOrWhiteSpace(address.Address2))
            return false;

        if (_addressSettings.ZipPostalCodeEnabled &&
            _addressSettings.ZipPostalCodeRequired &&
            string.IsNullOrWhiteSpace(address.ZipPostalCode))
            return false;

        if (_addressSettings.CountryEnabled)
        {
            var country = await _countryService.GetCountryByAddressAsync(address);
            if (country == null)
                return false;

            if (_addressSettings.StateProvinceEnabled)
            {
                var states = await _stateProvinceService.GetStateProvincesByCountryIdAsync(country.Id);
                if (states.Any())
                {
                    if (address.StateProvinceId == null || address.StateProvinceId.Value == 0)
                        return false;

                    var state = states.FirstOrDefault(x => x.Id == address.StateProvinceId.Value);
                    if (state == null)
                        return false;
                }
            }
        }

        if (_addressSettings.CountyEnabled &&
            _addressSettings.CountyRequired &&
            string.IsNullOrWhiteSpace(address.County))
            return false;

        if (_addressSettings.CityEnabled &&
            _addressSettings.CityRequired &&
            string.IsNullOrWhiteSpace(address.City))
            return false;

        if (_addressSettings.PhoneEnabled &&
            _addressSettings.PhoneRequired &&
            string.IsNullOrWhiteSpace(address.PhoneNumber))
            return false;

        if (_addressSettings.FaxEnabled &&
            _addressSettings.FaxRequired &&
            string.IsNullOrWhiteSpace(address.FaxNumber))
            return false;

        var customer = await _workContext.GetCurrentCustomerAsync();

        var registeringCustomerErpType = await _genericAttributeService.GetAttributeAsync<string?>(customer, nameof(ErpUserType));

        if (registeringCustomerErpType == ErpUserType.B2CUser.ToString())
        {
            var requiredAttributes = (await _addressAttributeService.GetAllAttributesAsync()).Where(x => x.IsRequired);

            foreach (var requiredAttribute in requiredAttributes)
            {
                var value = _addressAttributeParser.ParseValues(address.CustomAttributes, requiredAttribute.Id);

                if (!value.Any() || string.IsNullOrEmpty(value[0]))
                    return false;
            }
        }

        return true;
    }

    #endregion
}