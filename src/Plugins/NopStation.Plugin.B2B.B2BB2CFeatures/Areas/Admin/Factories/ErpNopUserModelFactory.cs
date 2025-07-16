using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Services;
using Nop.Services.Attributes;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public class ErpNopUserModelFactory : IErpNopUserModelFactory
{
    #region Fields

    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IAddressService _addressService;
    private readonly ICustomerService _customerService;
    private readonly ICountryService _countryService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly IAddressModelFactory _addressModelFactory;
    private readonly AddressSettings _addressSettings;
    private readonly ILocalizationService _localizationService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpNopUserService _erpNopUserService;
    private readonly IAttributeFormatter<AddressAttribute, AddressAttributeValue> _addressAttributeFormatter;
    private readonly IErpNopUserAccountMapService _erpNopUserAccountMapService;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;

    #endregion

    #region Ctor

    public ErpNopUserModelFactory(ILocalizationService localizationService,
        IDateTimeHelper dateTimeHelper,
        IAddressService addressService,
        ICustomerService customerService,
        ICountryService countryService,
        IStateProvinceService stateProvinceService,
        IAddressModelFactory addressModelFactory,
        AddressSettings addressSettings,
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService,
        IErpNopUserService erpNopUserService,
        IAttributeFormatter<AddressAttribute, AddressAttributeValue> addressAttributeFormatter,
        IErpNopUserAccountMapService erpNopUserAccountMapService,
        IErpShipToAddressService erpShipToAddressService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService)
    {
        _localizationService = localizationService;
        _dateTimeHelper = dateTimeHelper;
        _addressService = addressService;
        _customerService = customerService;
        _countryService = countryService;
        _stateProvinceService = stateProvinceService;
        _addressModelFactory = addressModelFactory;
        _addressSettings = addressSettings;
        _erpAccountService = erpAccountService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpNopUserService = erpNopUserService;
        _addressAttributeFormatter = addressAttributeFormatter;
        _erpNopUserAccountMapService = erpNopUserAccountMapService;
        _erpShipToAddressService = erpShipToAddressService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
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

    public async Task<List<SelectListItem>> PrepareShipToAddressDropdownAsync(int accountId, int customerId = 0)
    {
        var availableErpShipToAddresses = new List<SelectListItem>();
        if (accountId > 0)
        {
            var shipToAddresses = new List<ErpShipToAddress>();

            if (_b2BB2CFeaturesSettings.UseDefaultAccountForB2CUser && customerId > 0)
            {
                var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customerId);

                if (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2CUser)
                    shipToAddresses = await _erpShipToAddressService.GetErpShipToAddressesByCustomerAddressesAsync(customerId: customerId, erpAccountId: accountId);
                else
                    shipToAddresses = (List<ErpShipToAddress>)await _erpShipToAddressService.GetErpShipToAddressesByErpAccountIdAsync(accountId);
            }
            else
            {
                shipToAddresses = (List<ErpShipToAddress>)await _erpShipToAddressService.GetErpShipToAddressesByErpAccountIdAsync(accountId);
            }

            foreach (var shipToAddress in shipToAddresses)
            {
                var address = await _addressService.GetAddressByIdAsync(shipToAddress.AddressId);
    
                var addressModel = address.ToModel<AddressModel>();

                addressModel.CountryName = (await _countryService.GetCountryByAddressAsync(address))?.Name;
                addressModel.StateProvinceName = (await _stateProvinceService.GetStateProvinceByAddressAsync(address))?.Name;

                var selectListItem = new SelectListItem
                {
                    Value = $"{shipToAddress.Id}",
                    Text = $"{shipToAddress.ShipToName} - {await PrepareModelAddressHtmlAsync(addressModel, address)}"
                };
                availableErpShipToAddresses.Add(selectListItem);
            }
        }

        availableErpShipToAddresses.Insert(0, new SelectListItem
        {
            Value = "0",
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Select")
        });

        return availableErpShipToAddresses;
    }

    #endregion

    #region Method

    public async Task<ErpNopUserSearchModel> PrepareErpNopUserSearchModelAsync(ErpNopUserSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var availableErpUserTypes = await ErpUserType.B2BUser.ToSelectListAsync(false);
        foreach (var types in availableErpUserTypes)
        {
            var enumValue = Enum.Parse(typeof(ErpUserType), types.Value);
            var resourceKey = $"Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.{enumValue}";
            types.Text = await _localizationService.GetResourceAsync(resourceKey);
            searchModel.AvailableErpUserTypes.Add(types);
        }
        searchModel.AvailableErpUserTypes.Insert(0, new SelectListItem
        {
            Value = "0",
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Select")
        });

        searchModel.AvailableErpSalesOrgs = (await _erpSalesOrgService.GetAllErpSalesOrgAsync(showHidden: false))
            .Select(erpSalesOrg => new SelectListItem
            {
                Value = $"{erpSalesOrg.Id}",
                Text = $"{erpSalesOrg.Name}"
            }).ToList();

        searchModel.AvailableErpSalesOrgs.Insert(0, new SelectListItem
        {
            Value = "0",
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Select")
        });

        var erpAccounts = await _erpAccountService.GetAllErpAccountsAsync();
        searchModel.AvailableErpAccounts = new List<SelectListItem>();

        foreach (var erpAccount in erpAccounts)
        {
            var erpSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId);
            var selectListItem = new SelectListItem
            {
                Value = $"{erpAccount.Id}",
                Text = $"{erpAccount.AccountNumber} ({erpSalesOrg?.Name})"
            };
            searchModel.AvailableErpAccounts.Add(selectListItem);
        }

        searchModel.AvailableErpAccounts.Insert(0, new SelectListItem
        {
            Value = "0",
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Select")
        });

        if (searchModel.NopUserId > 0)
        {
            var mappedAccounts = await _erpNopUserAccountMapService.GetAllErpNopUserAccountMapsByUserIdAsync(searchModel.NopUserId);

            if (mappedAccounts.Any())
            {
                var mappedAccountIds = mappedAccounts.Select(x => x.ErpAccountId);
                foreach (var erpAccount in erpAccounts.Where(w => mappedAccountIds.Contains(w.Id)))
                {
                    var erpSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId);
                    var selectListItem = new SelectListItem
                    {
                        Value = $"{erpAccount.Id}",
                        Text = $"{erpAccount.AccountNumber} ({erpSalesOrg?.Name})"
                    };
                    searchModel.AvailableErpAccountsForAUser.Add(selectListItem);
                }
            }

            //prepare available customer roles
            var availableRoles = await _customerService.GetAllCustomerRolesAsync(showHidden: true);
            var customerRoleIds = Array.Empty<int>();

            var erpNopUser = await _erpNopUserService.GetErpNopUserByIdAsync(searchModel.NopUserId);
            var customer = await _customerService.GetCustomerByIdAsync(erpNopUser.NopCustomerId);
            customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);

            searchModel.AvailableCustomerRoles = availableRoles.Select(role => new SelectListItem
            {
                Text = role.Name,
                Value = $"{role.Id}",
                Selected = customerRoleIds.Contains(role.Id)
            }).ToList();
        }

        searchModel.AvailableErpAccountsForAUser.Insert(0, new SelectListItem
        {
            Value = "0",
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Select")
        });

        searchModel.AvailableErpShipToAddresses = await PrepareShipToAddressDropdownAsync(searchModel.AccountId);

        //prepare "active" filter (0 - all; 1 - active only; 2 - inactive only)
        searchModel.ShowInActiveOption.Add(new SelectListItem
        {
            Value = "0",
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserSearchModel.ShowAll"),
        });
        searchModel.ShowInActiveOption.Add(new SelectListItem
        {
            Value = "1",
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserSearchModel.ShowOnlyActive"),
        });
        searchModel.ShowInActiveOption.Add(new SelectListItem
        {
            Value = "2",
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserSearchModel.ShowOnlyInactive"),
        });

        //prepare grid
        searchModel.SetGridPageSize();

        return searchModel;
    }

    public async Task<ErpNopUserListModel> PrepareErpNopUserListModelAsync(ErpNopUserSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var erpNopUsers = await _erpNopUserService.GetAllErpNopUsersAsync(pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize,
            showHidden: searchModel.ShowInActive == 0 ? null : (searchModel.ShowInActive == 2),
            email: searchModel.Email,
            accountId: searchModel.AccountId,
            name: searchModel.Name,
            erpShipToAddressId: searchModel.ErpShipToAddressId,
            userType: searchModel.ErpUserTypeId,
            salesOrgId: searchModel.SalesOrgId);

        var model = await new ErpNopUserListModel().PrepareToGridAsync(searchModel, erpNopUsers, () =>
        {
            return erpNopUsers.SelectAwait(async erpNopUser =>
            {
                var erpNopUserModel = new ErpNopUserModel();

                if (erpNopUser != null)
                {
                    var nopCustomer = await _customerService.GetCustomerByIdAsync(erpNopUser.NopCustomerId);

                    var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpNopUser.ErpAccountId, filterOutDeleted: false);

                    if (erpAccount == null)
                        return null;

                    var erpSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId, filterOutDeleted: false);

                    if (erpSalesOrg == null)
                        return null;

                    var erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(erpNopUser.ErpShipToAddressId);

                    if (erpShipToAddress == null)
                        return null;

                    var addressOfShipToAddress = await _addressService.GetAddressByIdAsync(erpShipToAddress.AddressId);

                    var addressModelOfShipToAddress = new AddressModel();
                    if (addressOfShipToAddress != null)
                        addressModelOfShipToAddress = addressOfShipToAddress.ToModel(addressModelOfShipToAddress);
                    await _addressModelFactory.PrepareAddressModelAsync(addressModelOfShipToAddress, addressOfShipToAddress);
                    addressModelOfShipToAddress.AddressHtml = await PrepareModelAddressHtmlAsync(addressModelOfShipToAddress, addressOfShipToAddress, false);

                    var billingErpShipToAddress = await _addressService.GetAddressByIdAsync(erpNopUser.BillingErpShipToAddressId);
                    var billingErpShipToAddressModel = new AddressModel();

                    if (billingErpShipToAddress != null)
                        billingErpShipToAddressModel = billingErpShipToAddress.ToModel(billingErpShipToAddressModel);
                    await _addressModelFactory.PrepareAddressModelAsync(billingErpShipToAddressModel, billingErpShipToAddress);

                    var shippingErpShipToAddress = await _addressService.GetAddressByIdAsync(erpNopUser.ShippingErpShipToAddressId);
                    var shippingErpShipToAddressModel = new AddressModel();

                    if (shippingErpShipToAddress != null)
                        shippingErpShipToAddressModel = shippingErpShipToAddress.ToModel(shippingErpShipToAddressModel);
                    await _addressModelFactory.PrepareAddressModelAsync(shippingErpShipToAddressModel, shippingErpShipToAddress);

                    var selectedCustomerRoles = await _customerService.GetCustomerRolesAsync(nopCustomer);


                    erpNopUserModel = new ErpNopUserModel
                    {
                        Id = erpNopUser.Id,
                        NopCustomerId = erpNopUser.NopCustomerId,
                        NopCustomer = $"{nopCustomer.FirstName} {nopCustomer.LastName}",
                        NopCustomerEmail = nopCustomer.Email,
                        ErpAccountId = erpNopUser.ErpAccountId,
                        ErpAccountInfo = $"{erpAccount.AccountName} ({erpAccount.AccountNumber})",
                        ErpSalesOrg = erpSalesOrg.Name,
                        ErpShipToAddressId = erpNopUser.ErpShipToAddressId,
                        ErpShipToAddress = addressModelOfShipToAddress,
                        BillingErpShipToAddressId = erpNopUser.BillingErpShipToAddressId,
                        BillingErpShipToAddress = billingErpShipToAddressModel,
                        ShippingErpShipToAddressId = erpNopUser.ShippingErpShipToAddressId,
                        ShippingErpShipToAddress = shippingErpShipToAddressModel,
                        ErpUserTypeId = erpNopUser.ErpUserTypeId,
                        ErpUserType = ((ErpUserType)erpNopUser.ErpUserTypeId).ToString(),
                        CreatedBy = erpNopUser.CreatedById.ToString(),
                        UpdatedBy = erpNopUser.UpdatedById.ToString(),
                        IsActive = erpNopUser.IsActive,
                        CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpNopUser.CreatedOnUtc, DateTimeKind.Utc),
                        UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpNopUser.UpdatedOnUtc, DateTimeKind.Utc),
                        SelectedCustomerRoleIds = selectedCustomerRoles.Select(x => x.Id).ToList(),
                        SelectedCustomerRoles = string.Join(", ", selectedCustomerRoles.Select(x => x.Name))
                    };
                }

                return erpNopUserModel;
            }).Where(x => x != null);
        });

        return model;
    }
    
    public async Task<ErpNopUserAccountListModel> PrepareErpNopUserAccountListModelAsync(int nopUserId, ErpNopUserSearchModel searchModel)
    {
        if (nopUserId <= 0)
            throw new ArgumentNullException();

        var mappedAccounts = await _erpNopUserAccountMapService.GetAllErpNopUserAccountMapsByUserIdAsync(nopUserId);
        var mappedAccountIds = mappedAccounts.Select(x => x.ErpAccountId).ToList();
        var defaultErpAccountId = (await _erpNopUserService.GetErpNopUserByIdAsync(nopUserId)).ErpAccountId;

        var erpAccounts = await _erpAccountService.GetAllErpAccountsByIdsAsync(
            pageIndex: searchModel.Page - 1, 
            pageSize: searchModel.PageSize, 
            showHidden: true, 
            getOnlyTotalCount: false,
            mappedAccountIds.Contains(searchModel.AccountId) ? [searchModel.AccountId] : mappedAccountIds, 
            email: searchModel.Email);

        var model = await new ErpNopUserAccountListModel().PrepareToGridAsync(searchModel, erpAccounts, () =>
        {
            return erpAccounts.SelectAwait(async erpAccount =>
            {
                var address = await _addressService.GetAddressByIdAsync(erpAccount.BillingAddressId ?? 0);
                var addressModel = new AddressModel();
                if (address != null)
                    addressModel = address.ToModel(addressModel);
                await _addressModelFactory.PrepareAddressModelAsync(addressModel, address);

                var erpNopUserAccountModel = new ErpNopUserAccountModel
                {
                    Id = erpAccount.Id,
                    AccountNumber = erpAccount.AccountNumber,
                    AccountName = erpAccount.AccountName,
                    VatNumber = erpAccount.VatNumber,
                    CurrentBalance = erpAccount.CurrentBalance,
                    ErpSalesOrgId = erpAccount.ErpSalesOrgId,
                    BillingAddressId = erpAccount.BillingAddressId,
                    BillingAddress = addressModel,
                    BillingSuburb = erpAccount.BillingSuburb,
                    CreditLimit = erpAccount.CreditLimit,
                    CreditLimitAvailable = erpAccount.CreditLimitAvailable,
                    LastPaymentAmount = erpAccount.LastPaymentAmount,
                    LastPaymentDate = erpAccount.LastPaymentDate,
                    AllowOverspend = erpAccount.AllowOverspend,
                    PreFilterFacets = erpAccount.PreFilterFacets,
                    PaymentTypeCode = erpAccount.PaymentTypeCode,
                    OverrideAddressEditOnCheckoutConfigSetting = erpAccount.OverrideAddressEditOnCheckoutConfigSetting,
                    OverrideBackOrderingConfigSetting = erpAccount.OverrideBackOrderingConfigSetting,
                    AllowAccountsAddressEditOnCheckout = erpAccount.AllowAccountsAddressEditOnCheckout,
                    AllowAccountsBackOrdering = erpAccount.AllowAccountsBackOrdering,
                    OverrideStockDisplayFormatConfigSetting = erpAccount.OverrideStockDisplayFormatConfigSetting,
                    ErpAccountStatusTypeId = erpAccount.ErpAccountStatusTypeId,
                    ErpAccountStatusType = ((ErpAccountStatusType)erpAccount.ErpAccountStatusTypeId).ToString(),
                    LastErpAccountSyncDate = erpAccount.LastErpAccountSyncDate,
                    B2BPriceGroupCodeId = erpAccount.B2BPriceGroupCodeId,
                    TotalSavingsForthisYear = erpAccount.TotalSavingsForthisYear ?? 0,
                    TotalSavingsForAllTime = erpAccount.TotalSavingsForAllTime ?? 0,
                    TotalSavingsForAllTimeUpdatedOnUtc = erpAccount.TotalSavingsForAllTimeUpdatedOnUtc,
                    TotalSavingsForthisYearUpdatedOnUtc = erpAccount.TotalSavingsForthisYearUpdatedOnUtc,
                    LastTimeOrderSyncOnUtc = erpAccount.LastTimeOrderSyncOnUtc,
                    CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpAccount.CreatedOnUtc, DateTimeKind.Utc),
                    UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpAccount.UpdatedOnUtc, DateTimeKind.Utc),
                    IsActive = erpAccount.IsActive,
                    IsDefault = erpAccount.Id == defaultErpAccountId
                };

                return erpNopUserAccountModel;
            });
        });

        return model;
    }

    public async Task<ErpNopUserModel> PrepareErpNopUserModelAsync(ErpNopUserModel model, ErpNopUser erpNopUser)
    {
        if (erpNopUser != null)
        {
            var nopCustomer = await _customerService.GetCustomerByIdAsync(erpNopUser.NopCustomerId);

            var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpNopUser.ErpAccountId, filterOutDeleted: false);

            model ??= new ErpNopUserModel();

            model.Id = erpNopUser.Id;
            model.NopCustomerId = erpNopUser.NopCustomerId;
            model.NopCustomer = $"{nopCustomer?.FirstName} {nopCustomer?.LastName}";
            model.NopCustomerEmail = nopCustomer?.Email;
            model.ErpAccountId = erpNopUser.ErpAccountId;
            model.ErpAccountInfo = $"{erpAccount?.AccountName} ({erpAccount?.AccountNumber})";
            model.ErpShipToAddressId = erpNopUser.ErpShipToAddressId;
            model.BillingErpShipToAddressId = erpNopUser.BillingErpShipToAddressId;
            model.ShippingErpShipToAddressId = erpNopUser.ShippingErpShipToAddressId;
            model.ErpUserTypeId = erpNopUser.ErpUserTypeId;
            model.ErpUserType = ((ErpUserType)erpNopUser.ErpUserTypeId).ToString();
            model.CreatedBy = $"{erpNopUser.CreatedById}";
            model.UpdatedBy = $"{erpNopUser.UpdatedById}";
            model.IsActive = erpNopUser.IsActive;
            model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpNopUser.CreatedOnUtc, DateTimeKind.Utc);
            model.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpNopUser.UpdatedOnUtc, DateTimeKind.Utc);

            var selectedCustomerRoles = await _customerService.GetCustomerRolesAsync(nopCustomer);

            model.SelectedCustomerRoleIds = selectedCustomerRoles.Select(x => x.Id).ToList();
            model.SelectedCustomerRoles = string.Join(", ", selectedCustomerRoles.Select(x => x.Name));

            //prepare nested search model
            model.ErpNopUserSearchModel.NopUserId = erpNopUser.Id;
            await PrepareErpNopUserSearchModelAsync(model.ErpNopUserSearchModel);
        }

        #region Dropdowns

        // Prepare AvailableErpUserTypes dropdown options
        var availableErpUserTypes = await ErpUserType.B2BUser.ToSelectListAsync(false);
        foreach (var types in availableErpUserTypes)
        {
            var enumValue = Enum.Parse(typeof(ErpUserType), types.Value);
            var resourceKey = $"Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.{enumValue}";
            types.Text = await _localizationService.GetResourceAsync(resourceKey);
            model.AvailableErpUserTypes.Add(types);
        }
        model.AvailableErpUserTypes.Insert(0, new SelectListItem
        {
            Value = "0",
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Select")
        });

        // Prepare NopCustomers dropdown options
        var customerRoleId = (await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName)).Id;
        var customerIdsWithOnlyRegisteredRole = await _erpNopUserService.GetAllCustomersByOnlyTheseRoleIdsAsync(customerRoleId);

        var existingErpNopUsersNopCustomerIds = await _erpNopUserService.GetAllErpNopUsersCustomerIds();
        if (customerIdsWithOnlyRegisteredRole.Count > 0)
        {
            model.AvailableNopCustomers = (await _customerService.GetCustomersByIdsAsync(customerIdsWithOnlyRegisteredRole.ToArray()))
            .Where(w => !existingErpNopUsersNopCustomerIds.Contains(w.Id))
            .Select(nopCustomer => new SelectListItem
            {
                Value = $"{nopCustomer.Id}",
                Text = $"{nopCustomer.FirstName} {nopCustomer.LastName} ({nopCustomer.Email})"
            }).ToList();
        }
        model.AvailableNopCustomers.Insert(0, new SelectListItem
        {
            Value = "0",
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Select")
        });

        var customerRoleIds = Array.Empty<int>();
        if (erpNopUser != null && erpNopUser.NopCustomerId > 0)
        {
            var currentErpUsersNopCustomer = await _customerService.GetCustomerByIdAsync(erpNopUser.NopCustomerId);
            model.AvailableNopCustomers.Insert(1, new SelectListItem
            {
                Value = $"{currentErpUsersNopCustomer?.Id}",
                Text = $"{currentErpUsersNopCustomer?.FirstName} {currentErpUsersNopCustomer?.LastName}"
            });
            customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(currentErpUsersNopCustomer);
        }

        //prepare available customer roles
        var availableRoles = await _customerService.GetAllCustomerRolesAsync(showHidden: true);
        model.AvailableCustomerRoles = availableRoles.Select(role => new SelectListItem
        {
            Text = role.Name,
            Value = $"{role.Id}",
            Selected = customerRoleIds.Contains(role.Id)
        }).ToList();

        #endregion

        //prepare ErpShipToAddress model
        var erpShipToAddress = await _addressService.GetAddressByIdAsync(erpNopUser?.ErpShipToAddressId ?? 0);
        var erpShipToAddressModel = new AddressModel();
        if (erpShipToAddress != null)
            erpShipToAddressModel = erpShipToAddress.ToModel(erpShipToAddressModel);
        await _addressModelFactory.PrepareAddressModelAsync(erpShipToAddressModel, erpShipToAddress);

        //prepare BillingErpShipToAddress model
        var billingErpShipToAddress = await _addressService.GetAddressByIdAsync(erpNopUser?.BillingErpShipToAddressId ?? 0);
        var billingErpShipToAddressModel = new AddressModel();
        if (billingErpShipToAddress != null)
            billingErpShipToAddressModel = billingErpShipToAddress.ToModel(billingErpShipToAddressModel);
        await _addressModelFactory.PrepareAddressModelAsync(billingErpShipToAddressModel, billingErpShipToAddress);

        //prepare ShippingErpShipToAddress model
        var shippingErpShipToAddress = await _addressService.GetAddressByIdAsync(erpNopUser?.ShippingErpShipToAddressId ?? 0);
        var shippingErpShipToAddressModel = new AddressModel();
        if (shippingErpShipToAddress != null)
            shippingErpShipToAddressModel = shippingErpShipToAddress.ToModel(shippingErpShipToAddressModel);
        await _addressModelFactory.PrepareAddressModelAsync(shippingErpShipToAddressModel, shippingErpShipToAddress);

        model.ErpShipToAddress = erpShipToAddressModel;
        model.BillingErpShipToAddress = billingErpShipToAddressModel;
        model.ShippingErpShipToAddress = shippingErpShipToAddressModel;
        model.IsActive = erpNopUser is null || erpNopUser.IsActive;

        return model;
    }

    public async Task<ErpNopUserAccountMapModel> PrepareErpNopUserModelAsync(ErpNopUserAccountMapModel model, ErpNopUserAccountMap erpNopUserAccountMap)
    {
        if (erpNopUserAccountMap != null)
        {
            var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpNopUserAccountMap.ErpAccountId);

            //fill in model values from the entity
            model ??= new ErpNopUserAccountMapModel();

            model.Id = erpNopUserAccountMap.Id;
            model.ErpUserId = erpNopUserAccountMap.ErpUserId;
            model.ErpAccountId = erpNopUserAccountMap.ErpAccountId;
            if (erpAccount != null)
                model.ErpAccountNumber = erpAccount.AccountNumber;
        }

        return model;
    }

    #endregion
}
