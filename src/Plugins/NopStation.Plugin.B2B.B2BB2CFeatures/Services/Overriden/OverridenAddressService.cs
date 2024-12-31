using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Services.Attributes;
using Nop.Services.Directory;
using Nop.Services.Localization;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace Nop.Services.Common
{
    /// <summary>
    /// Address service
    /// </summary>
    public partial class OverridenAddressService : AddressService
    {
        #region Fields

        private readonly AddressSettings _addressSettings;
        private readonly IAttributeParser<AddressAttribute,AddressAttributeValue> _addressAttributeParser;
        private readonly IAttributeService<AddressAttribute, AddressAttributeValue> _addressAttributeService;
        private readonly ICountryService _countryService;
        private readonly IRepository<Address> _addressRepository;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public OverridenAddressService(AddressSettings addressSettings,
            IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
            IAttributeService<AddressAttribute, AddressAttributeValue> addressAttributeService,
            ICountryService countryService,
            IRepository<Address> addressRepository,
            IStateProvinceService stateProvinceService,
            IGenericAttributeService genericAttributeService,
            IWorkContext workContext,
            ILocalizationService localizationService) : base(
                addressSettings,
                addressAttributeParser,
                addressAttributeService,
                countryService,
                localizationService,
                addressRepository,
                stateProvinceService)
        {
            _addressSettings = addressSettings;
            _addressAttributeParser = addressAttributeParser;
            _addressAttributeService = addressAttributeService;
            _countryService = countryService;
            _addressRepository = addressRepository;
            _stateProvinceService = stateProvinceService;
            _genericAttributeService = genericAttributeService;
            _workContext = workContext;
            _localizationService = localizationService;
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
            if (address == null)
                throw new ArgumentNullException(nameof(address));

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
}