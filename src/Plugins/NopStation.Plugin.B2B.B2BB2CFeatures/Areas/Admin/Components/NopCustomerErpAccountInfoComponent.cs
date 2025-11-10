using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Common;
using Nop.Services.Attributes;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Components;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Components;

public class NopCustomerErpAccountInfoComponent : NopViewComponent
{
    #region Fields

    private readonly IErpNopUserService _erpNopUserService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly ICustomerService _customerService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly AddressSettings _addressSettings;
    private readonly IAttributeFormatter<AddressAttribute, AddressAttributeValue> _addressAttributeFormatter;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly IAddressService _addressService;
    private readonly ICountryService _countryService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly ILocalizationService _localizationService;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;

    #endregion

    #region Ctor

    public NopCustomerErpAccountInfoComponent(IErpNopUserService erpNopUserService,
        IErpAccountService erpAccountService,
        IDateTimeHelper dateTimeHelper,
        ICustomerService customerService,
        IErpSalesOrgService erpSalesOrgService,
        AddressSettings addressSettings,
        IAttributeFormatter<AddressAttribute, AddressAttributeValue> addressAttributeFormatter,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IErpShipToAddressService erpShipToAddressService,
        IAddressService addressService,
        ICountryService countryService,
        IStateProvinceService stateProvinceService,
        ILocalizationService localizationService,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService)
    {
        _erpNopUserService = erpNopUserService;
        _erpAccountService = erpAccountService;
        _dateTimeHelper = dateTimeHelper;
        _customerService = customerService;
        _erpSalesOrgService = erpSalesOrgService;
        _addressSettings = addressSettings;
        _addressAttributeFormatter = addressAttributeFormatter;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpShipToAddressService = erpShipToAddressService;
        _addressService = addressService;
        _countryService = countryService;
        _stateProvinceService = stateProvinceService;
        _localizationService = localizationService;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
    }

    #endregion

    #region Utilities

    protected async Task<string> PrepareModelAddressHtmlAsync(AddressModel model, Address address, bool singleLine = true)
    {
        ArgumentNullException.ThrowIfNull(model);

        var addressHtmlSb = new StringBuilder();

        if (singleLine)
        {
            if (_addressSettings.CompanyEnabled && !string.IsNullOrEmpty(model.Company))
                addressHtmlSb.Append(model.Company);

            if (_addressSettings.StreetAddressEnabled && !string.IsNullOrEmpty(model.Address1))
                addressHtmlSb.Append(", " + model.Address1);

            if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(model.Address2))
                addressHtmlSb.Append(" " + model.Address2);

            if (_addressSettings.CityEnabled && !string.IsNullOrEmpty(model.City))
                addressHtmlSb.Append(", " + model.City);

            if (_addressSettings.CountyEnabled && !string.IsNullOrEmpty(model.County))
                addressHtmlSb.Append(", " + model.County);

            if (_addressSettings.StateProvinceEnabled && !string.IsNullOrEmpty(model.StateProvinceName))
                addressHtmlSb.Append(", " + model.StateProvinceName);

            if (_addressSettings.ZipPostalCodeEnabled && !string.IsNullOrEmpty(model.ZipPostalCode))
                addressHtmlSb.Append(", " + model.ZipPostalCode);

            if (_addressSettings.CountryEnabled && !string.IsNullOrEmpty(model.CountryName))
                addressHtmlSb.Append(", " + model.CountryName);
        }
        else
        {
            addressHtmlSb = new StringBuilder("<div>");

            if (_addressSettings.CompanyEnabled && !string.IsNullOrEmpty(model.Company))
                addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.Company));

            if (_addressSettings.StreetAddressEnabled && !string.IsNullOrEmpty(model.Address1))
                addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.Address1));

            if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(model.Address2))
                addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.Address2));

            if (_addressSettings.CityEnabled && !string.IsNullOrEmpty(model.City))
                addressHtmlSb.AppendFormat("{0},", WebUtility.HtmlEncode(model.City));

            if (_addressSettings.CountyEnabled && !string.IsNullOrEmpty(model.County))
                addressHtmlSb.AppendFormat("{0},", WebUtility.HtmlEncode(model.County));

            if (_addressSettings.StateProvinceEnabled && !string.IsNullOrEmpty(model.StateProvinceName))
                addressHtmlSb.AppendFormat("{0},", WebUtility.HtmlEncode(model.StateProvinceName));

            if (_addressSettings.ZipPostalCodeEnabled && !string.IsNullOrEmpty(model.ZipPostalCode))
                addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.ZipPostalCode));

            if (_addressSettings.CountryEnabled && !string.IsNullOrEmpty(model.CountryName))
                addressHtmlSb.AppendFormat("{0}", WebUtility.HtmlEncode(model.CountryName));

            var customAttributesFormatted = await _addressAttributeFormatter.FormatAttributesAsync(address?.CustomAttributes);
            if (!string.IsNullOrEmpty(customAttributesFormatted))
            {
                //already encoded
                addressHtmlSb.AppendFormat("<br />{0}", customAttributesFormatted);
            }

            addressHtmlSb.Append("</div>");
        }

        return addressHtmlSb.ToString();
    }

    #endregion

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        try
        {
            var customerModel = additionalData as CustomerModel;

            var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customerModel.Id);

            if (erpNopUser == null)
                return View(new CustomerErpAccountInfoModel());

            var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpNopUser.ErpAccountId, filterOutDeleted: false);

            var model = new CustomerErpAccountInfoModel();
            model.Id = erpNopUser.Id;
            model.NopCustomerId = erpNopUser.NopCustomerId;
            model.NopCustomer = $"{customerModel.FirstName} {customerModel.LastName} ({customerModel.Email})";
            model.ErpAccountId = erpNopUser.ErpAccountId;
            model.ErpAccountInfo = $"{erpAccount?.AccountName} ({erpAccount?.AccountNumber})";
            model.ErpSalesOrgId = erpAccount?.ErpSalesOrgId ?? 0;
            model.ErpSalesOrg = (await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount?.ErpSalesOrgId ?? 0, filterOutDeleted: false))?.Name ?? "";
            model.ErpShipToAddressId = erpNopUser.ErpShipToAddressId;
            model.CreatedBy = (await _customerService.GetCustomerByIdAsync(erpNopUser.CreatedById))?.Email ?? $"{erpNopUser.CreatedById}";
            model.UpdatedBy = (await _customerService.GetCustomerByIdAsync(erpNopUser.UpdatedById))?.Email ?? $"{erpNopUser.UpdatedById}";
            model.IsActive = erpNopUser.IsActive;
            model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpNopUser.CreatedOnUtc, DateTimeKind.Utc);
            model.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpNopUser.UpdatedOnUtc, DateTimeKind.Utc);
            model.ErpShipToAddressInfo = "";

            var enumValue = Enum.Parse(typeof(ErpUserType), $"{erpNopUser.ErpUserType}");
            model.ErpUserType = await _localizationService.GetResourceAsync($"Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.{enumValue}");

            ErpShipToAddress erpShipToAddress = null;
            if (erpAccount?.Id > 0)
            {
                if (_b2BB2CFeaturesSettings.UseDefaultAccountForB2CUser && 
                    erpNopUser.ErpUserType == ErpUserType.B2CUser)
                {
                    erpShipToAddress = (await _erpShipToAddressService.
                        GetErpShipToAddressesByCustomerAddressesAsync(
                        customerId: erpNopUser.NopCustomerId, 
                        erpAccountId: erpAccount.Id, 
                        erpShipToAddressCreatedByTypeId: (int)ErpShipToAddressCreatedByType.Admin,
                        showHidden: false))
                        .FirstOrDefault();
                }
                else
                {
                    erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(erpNopUser.ErpShipToAddressId, filterOutDeleted: false);
                }

                if (erpShipToAddress != null)
                {
                    var address = await _addressService.GetAddressByIdAsync(erpShipToAddress.AddressId);

                    if (address != null)
                    {
                        var addressModel = address.ToModel<AddressModel>();
                        addressModel.CountryName = (await _countryService.GetCountryByAddressAsync(address))?.Name;
                        addressModel.StateProvinceName = (await _stateProvinceService.GetStateProvinceByAddressAsync(address))?.Name;
                        model.ErpShipToAddressInfo = $"{erpShipToAddress.ShipToName} - ({erpShipToAddress.ShipToCode}) - {await PrepareModelAddressHtmlAsync(addressModel, address)}";
                    }
                }
            }

            return View(model);
        }
        catch
        {
            return View(new CustomerErpAccountInfoModel());
        }
    }

    #endregion
}
