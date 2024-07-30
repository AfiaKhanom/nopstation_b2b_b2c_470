using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
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
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories
{
    public class ErpNopUserModelFactory : IErpNopUserModelFactory
    {
        #region Fields

        private readonly IWorkContext _workContext;
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

        #endregion

        #region Ctor

        public ErpNopUserModelFactory(IWorkContext workContext,
            ILocalizationService localizationService,
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
            IErpShipToAddressService erpShipToAddressService)
        {
            _workContext = workContext;
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
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare HTML string address
        /// </summary>
        /// <param name="model">Address model</param>
        /// <param name="address">Address</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected async Task<string> PrepareModelAddressHtmlAsync(AddressModel model, Address address, bool singleLine = true)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

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

        public async Task<List<SelectListItem>> PrepareShipToAddressDropdownAsync(int accountId)
        {
            var availableErpShipToAddresses = new List<SelectListItem>();
            if (accountId > 0)
            {
                var shipToAddresses = await _erpShipToAddressService.GetErpShipToAddressesByErpAccountIdAsync(accountId);
                foreach (var shipToAddress in shipToAddresses)
                {
                    var address = await _addressService.GetAddressByIdAsync(shipToAddress.AddressId);

                    //fill in model values from the entity        
                    var addressModel = address.ToModel<AddressModel>();

                    addressModel.CountryName = (await _countryService.GetCountryByAddressAsync(address))?.Name;
                    addressModel.StateProvinceName = (await _stateProvinceService.GetStateProvinceByAddressAsync(address))?.Name;

                    var selectListItem = new SelectListItem
                    {
                        Value = shipToAddress.Id.ToString(),
                        Text = shipToAddress.ShipToName + " - " + await PrepareModelAddressHtmlAsync(addressModel, address)
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

        protected async Task<string> PrepareSelectedRolesAsync(List<int> selectedRoleIds)
        {
            var selectedRoleNames = new List<string>();
            foreach (var roleId in selectedRoleIds)
            {
                var role = await _customerService.GetCustomerRoleByIdAsync(roleId);
                selectedRoleNames.Add(role.Name);
            }
            var result = string.Join(", ", selectedRoleNames);

            return result;
        }

        #endregion

        #region Method

        public async Task<ErpNopUserSearchModel> PrepareErpNopUserSearchModelAsync(ErpNopUserSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            // Prepare AvailableErpUserTypes dropdown options
            var availableErpUserTypes = await ErpUserType.B2BUser.ToSelectListAsync(false);
            foreach (var types in availableErpUserTypes)
            {
                searchModel.AvailableErpUserTypes.Add(types);
            }
            searchModel.AvailableErpUserTypes.Insert(0, new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Select")
            });

            // Prepare ErpSalesOrgs dropdown options
            searchModel.AvailableErpSalesOrgs = (await _erpSalesOrgService.GetAllErpSalesOrgAsync(showHidden: false))
                .Select(erpSalesOrg => new SelectListItem
                {
                    Value = erpSalesOrg.Id.ToString(),
                    Text = erpSalesOrg.Name.ToString()
                }).ToList();

            searchModel.AvailableErpSalesOrgs.Insert(0, new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Select")
            });

            // Prepare ErpAccounts dropdown options
            var erpAccounts = await _erpAccountService.GetAllErpAccountsAsync();
            searchModel.AvailableErpAccounts = new List<SelectListItem>();

            foreach (var erpAccount in erpAccounts)
            {
                var erpSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId);
                var selectListItem = new SelectListItem
                {
                    Value = erpAccount.Id.ToString(),
                    Text = erpAccount.AccountNumber + " (" + erpSalesOrg?.Name + ")"
                };
                searchModel.AvailableErpAccounts.Add(selectListItem);
            }

            searchModel.AvailableErpAccounts.Insert(0, new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Select")
            });

            // Prepare ErpAccounts dropdown options for a nop user
            if (searchModel.NopUserId > 0)
            {
                var mappedAccounts = await _erpNopUserAccountMapService.GetAllErpNopUserAccountMapsByUserIdAsync(searchModel.NopUserId);

                if (mappedAccounts.Any())
                {
                    var mappedAccountIds = mappedAccounts.Select(x => x.ErpAccountId);
                    var mappedAccountss = erpAccounts.Where(w => mappedAccountIds.Contains(w.Id)).ToList();
                    foreach (var erpAccount in mappedAccountss)
                    {
                        var erpSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId);
                        var selectListItem = new SelectListItem
                        {
                            Value = erpAccount.Id.ToString(),
                            Text = erpAccount.AccountNumber + " (" + erpSalesOrg?.Name + ")"
                        };
                        searchModel.AvailableErpAccountsForAUser.Add(selectListItem);
                    }
                }
            }

            searchModel.AvailableErpAccountsForAUser.Insert(0, new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Select")
            });

            //Todo Available ship to addresses
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

            //prepare available customer roles
            var availableRoles = await _customerService.GetAllCustomerRolesAsync(showHidden: true);
            searchModel.AvailableCustomerRoles = availableRoles.Select(role => new SelectListItem
            {
                Text = role.Name,
                Value = role.Id.ToString(),
                Selected = searchModel.CustomerRoleIds.Contains(role.Id)
            }).ToList();

            //prepare grid
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public async Task<ErpNopUserListModel> PrepareErpNopUserListModelAsync(ErpNopUserSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get ERP Sales Orgs
            var erpNopUsers = await _erpNopUserService.GetAllErpNopUsersAsync(pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize,
                showHidden: searchModel.ShowInActive == 0 ? null : (bool?)(searchModel.ShowInActive == 2),
                email: searchModel.Email,
                accountId: searchModel.AccountId,
                name: searchModel.Name,
                erpShipToAddressId: searchModel.ErpShipToAddressId,
                userType: searchModel.ErpUserTypeId,
                salesOrgId: searchModel.SalesOrgId);

            //prepare list model
            var model = await new ErpNopUserListModel().PrepareToGridAsync(searchModel, erpNopUsers, () =>
            {
                //fill in model values from the entity
                return erpNopUsers.SelectAwait(async erpNopUser =>
                {
                    var erpNopUserModel = new ErpNopUserModel();
                    //Get addionalInfos
                    //var erpNopUserInfo = await _erpNopUserService.GetErpNopUserByIdAsync(erpNopUser.Id);

                    var currentCulture = (await _workContext.GetWorkingLanguageAsync()).LanguageCulture;
                    var dtfi = new CultureInfo(currentCulture, false).DateTimeFormat;

                    //Additional Infos
                    if (erpNopUser != null)
                    {
                        //get customer
                        var nopCustomer = await _customerService.GetCustomerByIdAsync(erpNopUser.NopCustomerId);

                        //get Account
                        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpNopUser.ErpAccountId);

                        //get SalesOrg
                        var erpSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount?.ErpSalesOrgId ?? 0);

                        //prepare ErpShipToAddress model
                        var addressOfShipToAddress = await _addressService.GetAddressByIdAsync((await _erpShipToAddressService.GetErpShipToAddressByIdAsync(erpNopUser.ErpShipToAddressId))?.AddressId ?? 0);
                        var addressModelOfShipToAddress = new AddressModel();

                        if (addressOfShipToAddress != null)
                            addressModelOfShipToAddress = addressOfShipToAddress.ToModel(addressModelOfShipToAddress);
                        await _addressModelFactory.PrepareAddressModelAsync(addressModelOfShipToAddress, addressOfShipToAddress);
                        addressModelOfShipToAddress.AddressHtml = await PrepareModelAddressHtmlAsync(addressModelOfShipToAddress, addressOfShipToAddress, false);

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
                        var selectedCustomerRoleIds = await _erpNopUserAccountMapService.GetErpNopUserRolesByAsync(erpNopUser);

                        erpNopUserModel = new ErpNopUserModel
                        {
                            Id = erpNopUser.Id,
                            NopCustomerId = erpNopUser.NopCustomerId,
                            NopCustomer = nopCustomer.FirstName + " " + nopCustomer.LastName,
                            NopCustomerEmail = nopCustomer.Email,
                            ErpAccountId = erpNopUser.ErpAccountId,
                            ErpAccount = erpAccount.AccountName + "(" + erpAccount.AccountNumber + ")",
                            ErpSalesOrg = erpSalesOrg != null ? erpSalesOrg.Name : "",
                            ErpShipToAddressId = erpNopUser.ErpShipToAddressId,
                            ErpShipToAddress = addressModelOfShipToAddress,
                            BillingErpShipToAddressId = erpNopUser.BillingErpShipToAddressId,
                            BillingErpShipToAddress = billingErpShipToAddressModel,
                            ShippingErpShipToAddressId = erpNopUser.ShippingErpShipToAddressId,
                            ShippingErpShipToAddress = shippingErpShipToAddressModel,
                            ErpUserTypeId = erpNopUser.ErpUserTypeId,
                            ErpUserType = ((ErpUserType)erpNopUser.ErpUserTypeId).ToString(),
                            CreatedBy = erpNopUser.CreatedById.ToString(),//todo
                            UpdatedBy = erpNopUser.UpdatedById.ToString(),//todo
                            IsActive = erpNopUser.IsActive,
                            CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpNopUser.CreatedOnUtc, DateTimeKind.Utc),
                            UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpNopUser.UpdatedOnUtc, DateTimeKind.Utc),
                            SelectedCustomerRoleIds = selectedCustomerRoleIds,
                            SelectedCustomerRoles = await PrepareSelectedRolesAsync(selectedCustomerRoleIds.ToList()),
                        };
                    }

                    return erpNopUserModel;
                });
            });

            return model;
        }

        public async Task<ErpNopUserModel> PrepareErpNopUserModelAsync(ErpNopUserModel model, ErpNopUser erpNopUser)
        {
            if (erpNopUser != null)
            {
                //get customer
                var nopCustomer = await _customerService.GetCustomerByIdAsync(erpNopUser.NopCustomerId);

                //get Account
                var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpNopUser.ErpAccountId);

                //fill in model values from the entity
                model ??= new ErpNopUserModel();

                model.Id = erpNopUser.Id;
                model.NopCustomerId = erpNopUser.NopCustomerId;
                model.NopCustomer = nopCustomer?.FirstName + " " + nopCustomer?.LastName;
                model.NopCustomerEmail = nopCustomer?.Email;
                model.ErpAccountId = erpNopUser.ErpAccountId;
                model.ErpAccount = erpAccount?.AccountName + "(" + erpAccount.AccountNumber + ")";
                model.ErpShipToAddressId = erpNopUser.ErpShipToAddressId;
                model.BillingErpShipToAddressId = erpNopUser.BillingErpShipToAddressId;
                model.ShippingErpShipToAddressId = erpNopUser.ShippingErpShipToAddressId;
                model.ErpUserTypeId = erpNopUser.ErpUserTypeId;
                model.ErpUserType = ((ErpUserType)erpNopUser.ErpUserTypeId).ToString();
                model.CreatedBy = erpNopUser.CreatedById.ToString();//todo
                model.UpdatedBy = erpNopUser.UpdatedById.ToString();//todo
                model.IsActive = erpNopUser.IsActive;
                model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpNopUser.CreatedOnUtc, DateTimeKind.Utc);
                model.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpNopUser.UpdatedOnUtc, DateTimeKind.Utc);

                var selectedCustomerRoleIds = await _erpNopUserAccountMapService.GetErpNopUserRolesByAsync(erpNopUser);

                model.SelectedCustomerRoleIds = selectedCustomerRoleIds;
                model.SelectedCustomerRoles = await PrepareSelectedRolesAsync(selectedCustomerRoleIds.ToList());

                //prepare nested search model
                model.ErpNopUserSearchModel.NopUserId = erpNopUser.Id;
                await PrepareErpNopUserSearchModelAsync(model.ErpNopUserSearchModel);
            }

            #region Dropdowns

            // Prepare AvailableErpUserTypes dropdown options
            var availableErpUserTypes = await ErpUserType.B2BUser.ToSelectListAsync(false);
            foreach (var types in availableErpUserTypes)
            {
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

            var existingNopCustomer = (await _erpNopUserService.GetAllErpNopUsersAsync()).Select(s => s.NopCustomerId)?.ToList();
            if (customerIdsWithOnlyRegisteredRole.Count > 0)
            {
                model.AvailableNopCustomers = (await _customerService.GetCustomersByIdsAsync(customerIdsWithOnlyRegisteredRole.ToArray())).Where(w => !existingNopCustomer.Contains(w.Id))
                .Select(nopCustomer => new SelectListItem
                {
                    Value = nopCustomer.Id.ToString(),
                    Text = nopCustomer.FirstName + " " + nopCustomer.LastName + " (" + nopCustomer.Email + ")"
                }).ToList();
            }

            model.AvailableNopCustomers.Insert(0, new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Select")
            });

            if (erpNopUser != null && erpNopUser.NopCustomerId > 0)
            {
                var currentErpUsersNopCustomer = await _customerService.GetCustomerByIdAsync(erpNopUser.NopCustomerId);
                model.AvailableNopCustomers.Insert(1, new SelectListItem
                {
                    Value = currentErpUsersNopCustomer?.Id.ToString(),
                    Text = currentErpUsersNopCustomer?.FirstName + " " + currentErpUsersNopCustomer?.LastName,
                });
            }

            // Prepare ErpAccounts dropdown options
            model.AvailableErpAccounts = (await _erpAccountService.GetAllErpAccountsAsync())
                .Select(erpAccounts => new SelectListItem
                {
                    Value = erpAccounts.Id.ToString(),
                    Text = erpAccounts.AccountNumber.ToString()
                }).ToList();

            model.AvailableErpAccounts.Insert(0, new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Select")
            });

            //prepare available customer roles
            var availableRoles = (await _customerService.GetAllCustomerRolesAsync(showHidden: true))?.ToList();

            if (availableRoles != null)
            {
                foreach (var role in availableRoles)
                {
                    if (!role.IsSystemRole && role.SystemName != ERPIntegrationCoreDefaults.B2BSalesRepRoleSystemName)
                    {
                        model.AvailableCustomerRoles.Add(new SelectListItem
                        {
                            Text = role.Name,
                            Value = role.Id.ToString(),
                            Selected = model.SelectedCustomerRoleIds.Contains(role.Id)
                        });
                    }
                }
            }

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
            model.IsActive = erpNopUser is null ? true : erpNopUser.IsActive;

            return model;
        }

        public async Task<ErpNopUserAccountListModel> PrepareErpNopUserAccountListModelAsync(int nopUserId, ErpNopUserSearchModel searchModel)
        {
            if (nopUserId <= 0)
                throw new ArgumentNullException();

            var mappedAccounts = await _erpNopUserAccountMapService.GetAllErpNopUserAccountMapsByUserIdAsync(nopUserId);
            var mappedErpAccountIds = mappedAccounts.Select(s => s.ErpAccountId);
            var accountIds = new List<int>();

            var defaultErpAccountId = (await _erpNopUserService.GetErpNopUserByIdAsync(nopUserId)).ErpAccountId;

            if (searchModel.CustomerRoleIds.Any())
            {
                foreach (var r in searchModel.CustomerRoleIds)
                {
                    foreach (var ma in mappedAccounts)
                    {
                        var roles = ma.CustomerRolesIds.Split(',');
                        if (roles.Any(a => a.Equals(r.ToString())))
                            accountIds.Add(ma.ErpAccountId);
                    }
                }
            }

            if (searchModel.AccountId > 0)
            {
                if (searchModel.CustomerRoleIds.Any() && accountIds.Contains(searchModel.AccountId))
                {
                    accountIds = new List<int>
                    {
                        searchModel.AccountId
                    };
                }
                else if (!searchModel.CustomerRoleIds.Any() && mappedErpAccountIds.Contains(searchModel.AccountId))
                    accountIds.Add(searchModel.AccountId);
                else
                    accountIds = new List<int>();
            }
            else if (!searchModel.CustomerRoleIds.Any())
                accountIds = mappedErpAccountIds.ToList();

            var erpAccounts = await _erpAccountService.GetAllErpAccountsByIdsAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize, showHidden: true, getOnlyTotalCount: false, accountIds, email: searchModel.Email);

            //prepare list model
            var model = await new ErpNopUserAccountListModel().PrepareToGridAsync(searchModel, erpAccounts, () =>
            {
                //fill in model values from the entity
                return erpAccounts.SelectAwait(async erpAccount =>
                {
                    //prepare address model
                    var address = await _addressService.GetAddressByIdAsync(erpAccount?.BillingAddressId ?? 0);
                    var addressModel = new AddressModel();
                    if (address != null)
                        addressModel = address.ToModel(addressModel);
                    await _addressModelFactory.PrepareAddressModelAsync(addressModel, address);

                    var selectedCustomerRoles = mappedAccounts.Where(w => w.ErpAccountId == erpAccount.Id);
                    var selectedCustomerRoleIds = selectedCustomerRoles.Any() ? selectedCustomerRoles.FirstOrDefault().CustomerRolesIds : "";

                    var listOfErpNopUserRoleIds = new List<int>();
                    var erpNopUserRoleIds = selectedCustomerRoleIds.Split(",");
                    foreach (var roleId in erpNopUserRoleIds)
                    {
                        if (!string.IsNullOrEmpty(roleId))
                            listOfErpNopUserRoleIds.Add(Convert.ToInt32(roleId));
                    }

                    //fill in model values from the entity
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
                        SelectedCustomerRoles = await PrepareSelectedRolesAsync(listOfErpNopUserRoleIds.ToList()),
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

        public async Task<ErpNopUserAccountMapModel> PrepareErpNopUserModelAsync(ErpNopUserAccountMapModel model, ErpNopUserAccountMap erpNopUserAccountMap)
        {
            if (erpNopUserAccountMap != null)
            {
                //get user
                var erpNopUser = await _erpNopUserService.GetErpNopUserByIdAsync(erpNopUserAccountMap.ErpUserId);

                //get Account
                var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpNopUserAccountMap.ErpAccountId);

                //fill in model values from the entity
                model ??= new ErpNopUserAccountMapModel();

                model.Id = erpNopUserAccountMap.Id;
                model.ErpUserId = erpNopUserAccountMap.ErpUserId;
                model.ErpAccountId = erpNopUserAccountMap.ErpAccountId;
                model.CustomerRolesIds = erpNopUserAccountMap.CustomerRolesIds;
                if (erpAccount != null)
                    model.ErpAccountNumber = erpAccount.AccountNumber;
            }
            //prepare available customer roles
            var availableRoles = await _customerService.GetAllCustomerRolesAsync(showHidden: true);
            model.AvailableCustomerRoles = availableRoles.Select(role => new SelectListItem
            {
                Text = role.Name,
                Value = role.Id.ToString(),
                Selected = model.SelectedCustomerRoleIds.Contains(role.Id)
            }).ToList();
            return model;
        }

        #endregion

    }
}
