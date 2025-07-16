using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Services.Attributes;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.ExportImport;
using Nop.Services.Forums;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.Customers;
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers;

public partial class OverridenCustomerController : CustomerController
{
    #region Fields

    private readonly IErpAccountService _erpAccountService;
    private readonly IErpNopUserService _erpNopUserService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly ICommonHelperService _commonHelperService;

    #endregion

    #region Ctor

    public OverridenCustomerController(CustomerSettings customerSettings,
        DateTimeSettings dateTimeSettings,
        EmailAccountSettings emailAccountSettings,
        ForumSettings forumSettings,
        GdprSettings gdprSettings,
        IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
        IAddressService addressService,
        ICustomerActivityService customerActivityService,
        IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
        IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService,
        ICustomerModelFactory customerModelFactory,
        ICustomerRegistrationService customerRegistrationService,
        ICustomerService customerService,
        IDateTimeHelper dateTimeHelper,
        IEmailAccountService emailAccountService,
        IEventPublisher eventPublisher,
        IExportManager exportManager,
        IForumService forumService,
        IGdprService gdprService,
        IGenericAttributeService genericAttributeService,
        ILocalizationService localizationService,
        INewsLetterSubscriptionService newsLetterSubscriptionService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IQueuedEmailService queuedEmailService,
        IRewardPointService rewardPointService,
        IStoreContext storeContext,
        IStoreService storeService,
        ITaxService taxService,
        IWorkContext workContext,
        IWorkflowMessageService workflowMessageService,
        TaxSettings taxSettings,
        IErpAccountService erpAccountService,
        IErpNopUserService erpNopUserService,
        IImportManager importManager,
        IStaticCacheManager staticCacheManager,
        ICommonHelperService commonHelperService) : base(customerSettings,
            dateTimeSettings,
            emailAccountSettings,
            forumSettings,
            gdprSettings,
            addressService,
            addressAttributeParser,
            customerAttributeParser,
            customerAttributeService,
            customerActivityService,
            customerModelFactory,
            customerRegistrationService,
            customerService,
            dateTimeHelper,
            emailAccountService,
            eventPublisher,
            exportManager,
            forumService,
            gdprService,
            genericAttributeService,
            importManager,
            localizationService,
            newsLetterSubscriptionService,
            notificationService,
            permissionService,
            queuedEmailService,
            rewardPointService,
            storeContext,
            storeService,
            taxService,
            workContext,
            workflowMessageService,
            taxSettings)
    {
        _erpAccountService = erpAccountService;
        _erpNopUserService = erpNopUserService;
        _staticCacheManager = staticCacheManager;
        _commonHelperService = commonHelperService;
    }

    #endregion

    #region Customers

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [FormValueRequired("save", "save-continue")]
    public override async Task<IActionResult> Edit(CustomerModel model, bool continueEditing, IFormCollection form)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
            return AccessDeniedView();

        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(model.Id);
        if (customer == null || customer.Deleted)
            return RedirectToAction("List");

        //validate customer roles
        var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
        var newCustomerRoles = (from customerRole in allCustomerRoles
                                where model.SelectedCustomerRoleIds.Contains(customerRole.Id)
                                select customerRole).ToList();

        var customerRolesError = await ValidateCustomerRolesAsync(newCustomerRoles, await _customerService.GetCustomerRolesAsync(customer));

        if (!string.IsNullOrEmpty(customerRolesError))
        {
            ModelState.AddModelError(string.Empty, customerRolesError);
            _notificationService.ErrorNotification(customerRolesError);
        }

        // Ensure that valid email address is entered if Registered role is checked to avoid registered customers with empty email address
        if (newCustomerRoles.Count != 0 && newCustomerRoles.Find(c => c.SystemName == NopCustomerDefaults.RegisteredRoleName) != null &&
            !CommonHelper.IsValidEmail(model.Email))
        {
            ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Admin.Customers.Customers.ValidEmailRequiredRegisteredRole"));
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.ValidEmailRequiredRegisteredRole"));
        }

        #region Erp

        var isErpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id) != null;
        var erpUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer.Id);
        var customerAttributesXml = await ParseCustomCustomerAttributesAsync(form);

        if (isErpAccount &&
            erpUser != null &&
            erpUser.ErpUserType == ErpUserType.B2CUser && 
            newCustomerRoles.Count != 0 && 
            newCustomerRoles.Find(c => c.SystemName == NopCustomerDefaults.RegisteredRoleName) != null)
        {
            var customerAttributeWarnings = await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError(string.Empty, error);
            }            
        }

        #endregion            

        if (ModelState.IsValid)
        {
            try
            {
                var wasCustomerActive = customer.Active;
                customer.AdminComment = model.AdminComment;
                customer.IsTaxExempt = model.IsTaxExempt;

                //prevent deactivation of the last active administrator
                if (!await _customerService.IsAdminAsync(customer) || model.Active || await SecondAdminAccountExistsAsync(customer))
                    customer.Active = model.Active;
                else
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.AdminAccountShouldExists.Deactivate"));

                //email
                if (!string.IsNullOrWhiteSpace(model.Email))
                    await _customerRegistrationService.SetEmailAsync(customer, model.Email, false);
                else
                    customer.Email = model.Email;

                //username
                if (_customerSettings.UsernamesEnabled)
                {
                    if (!string.IsNullOrWhiteSpace(model.Username))
                        await _customerRegistrationService.SetUsernameAsync(customer, model.Username);
                    else
                        customer.Username = model.Username;
                }

                //VAT number
                if (_taxSettings.EuVatEnabled)
                {
                    var prevVatNumber = customer.VatNumber;

                    customer.VatNumber = model.VatNumber;
                    //set VAT number status
                    if (!string.IsNullOrEmpty(model.VatNumber))
                    {
                        if (!model.VatNumber.Equals(prevVatNumber, StringComparison.InvariantCultureIgnoreCase))
                        {
                            customer.VatNumberStatusId = (int)(await _taxService.GetVatNumberStatusAsync(model.VatNumber)).vatNumberStatus;
                        }
                    }
                    else
                        customer.VatNumberStatusId = (int)VatNumberStatus.Empty;
                }

                //vendor
                customer.VendorId = model.VendorId;

                //form fields
                if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                    customer.TimeZoneId = model.TimeZoneId;
                if (_customerSettings.GenderEnabled)
                    customer.Gender = model.Gender;
                if (_customerSettings.FirstNameEnabled)
                    customer.FirstName = model.FirstName;
                if (_customerSettings.LastNameEnabled)
                    customer.LastName = model.LastName;
                if (_customerSettings.DateOfBirthEnabled)
                    customer.DateOfBirth = model.DateOfBirth;
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

                //custom customer attributes
                customer.CustomCustomerAttributesXML = customerAttributesXml;

                //newsletter subscriptions
                if (!string.IsNullOrEmpty(customer.Email))
                {
                    var allStores = await _storeService.GetAllStoresAsync();
                    foreach (var store in allStores)
                    {
                        var newsletterSubscription = await _newsLetterSubscriptionService
                            .GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customer.Email, store.Id);
                        if (model.SelectedNewsletterSubscriptionStoreIds != null &&
                            model.SelectedNewsletterSubscriptionStoreIds.Contains(store.Id))
                        {
                            //subscribed
                            if (newsletterSubscription == null)
                            {
                                await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(new NewsLetterSubscription
                                {
                                    NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                    Email = customer.Email,
                                    Active = true,
                                    StoreId = store.Id,
                                    CreatedOnUtc = DateTime.UtcNow
                                });
                            }
                        }
                        else
                        {
                            //not subscribed
                            if (newsletterSubscription != null)
                            {
                                await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(newsletterSubscription);
                            }
                        }
                    }
                }

                var currentCustomerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer, true);

                //customer roles
                foreach (var customerRole in allCustomerRoles)
                {
                    //ensure that the current customer cannot add/remove to/from "Administrators" system role
                    //if he's not an admin himself
                    if (customerRole.SystemName == NopCustomerDefaults.AdministratorsRoleName &&
                        !await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
                        continue;

                    if (model.SelectedCustomerRoleIds.Contains(customerRole.Id))
                    {
                        //new role
                        if (currentCustomerRoleIds.All(roleId => roleId != customerRole.Id))
                            await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = customerRole.Id });
                    }
                    else
                    {
                        //prevent attempts to delete the administrator role from the user, if the user is the last active administrator
                        if (customerRole.SystemName == NopCustomerDefaults.AdministratorsRoleName && !await SecondAdminAccountExistsAsync(customer))
                        {
                            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.AdminAccountShouldExists.DeleteRole"));
                            continue;
                        }

                        //remove role
                        if (currentCustomerRoleIds.Any(roleId => roleId == customerRole.Id))
                            await _customerService.RemoveCustomerRoleMappingAsync(customer, customerRole);
                    }
                }

                await _customerService.UpdateCustomerAsync(customer);

                if (isErpAccount && erpUser != null && erpUser.ErpUserType == ErpUserType.B2BUser && !wasCustomerActive && model.Active)
                {
                    var isThisANewB2BCustomerNeedsApproval = await _genericAttributeService.GetAttributeAsync<bool?>(customer,
                        ERPIntegrationCoreDefaults.NewB2BCustomerNeedsApproval);

                    if (isThisANewB2BCustomerNeedsApproval.HasValue && isThisANewB2BCustomerNeedsApproval.Value)
                    {
                        //send customer welcome message
                        await _workflowMessageService.SendCustomerWelcomeMessageAsync(customer, (await _workContext.GetWorkingLanguageAsync()).Id);
                        await _genericAttributeService.SaveAttributeAsync(customer, ERPIntegrationCoreDefaults.NewB2BCustomerNeedsApproval, false);
                    }
                }

                //ensure that a customer with a vendor associated is not in "Administrators" role
                //otherwise, he won't have access to the other functionality in admin area
                if (await _customerService.IsAdminAsync(customer) && customer.VendorId > 0)
                {
                    customer.VendorId = 0;
                    await _customerService.UpdateCustomerAsync(customer);
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.AdminCouldNotbeVendor"));
                }

                //ensure that a customer in the Vendors role has a vendor account associated.
                //otherwise, he will have access to ALL products
                if (await _customerService.IsVendorAsync(customer) && customer.VendorId == 0)
                {
                    var vendorRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.VendorsRoleName);
                    await _customerService.RemoveCustomerRoleMappingAsync(customer, vendorRole);

                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.CannotBeInVendoRoleWithoutVendorAssociated"));
                }

                //activity log
                await _customerActivityService.InsertActivityAsync("EditCustomer",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditCustomer"), customer.Id), customer);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = customer.Id });
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc.Message);
            }
        }

        //prepare model
        model = await _customerModelFactory.PrepareCustomerModelAsync(model, customer, true);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    [HttpPost, ActionName("Edit")]
    [FormValueRequired("impersonate")]
    public override async Task<IActionResult> Impersonate(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AllowCustomerImpersonation))
            return AccessDeniedView();

        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null)
            return RedirectToAction("List");

        var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);
        if (erpAccount == null)
        {
            _notificationService.WarningNotification(
                await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.Admin.Customers.Impersonate.ErpAccountNotAvailable"));

            return RedirectToAction("Edit", customer.Id);
        }

        var key = _staticCacheManager.PrepareKeyForDefaultCache(ERPIntegrationCoreDefaults.ErpNopUserByCustomerCacheKey, customer.Id);
        await _staticCacheManager.RemoveAsync(key);

        if (!customer.Active)
        {
            _notificationService.WarningNotification(
                await _localizationService.GetResourceAsync("Admin.Customers.Customers.Impersonate.Inactive"));
            return RedirectToAction("Edit", customer.Id);
        }

        //ensure that a non-admin user cannot impersonate as an administrator
        //otherwise, that user can simply impersonate as an administrator and gain additional administrative privileges
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsAdminAsync(currentCustomer) && await _customerService.IsAdminAsync(customer))
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.NonAdminNotImpersonateAsAdminError"));
            return RedirectToAction("Edit", customer.Id);
        }

        //activity log
        await _customerActivityService.InsertActivityAsync("Impersonation.Started",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Started.StoreOwner"), customer.Email, customer.Id), customer);
        await _customerActivityService.InsertActivityAsync(customer, "Impersonation.Started",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Started.Customer"), currentCustomer.Email, currentCustomer.Id), currentCustomer);

        //ensure login is not required
        customer.RequireReLogin = false;
        await _customerService.UpdateCustomerAsync(customer);
        await _genericAttributeService.SaveAttributeAsync<int?>(currentCustomer, NopCustomerDefaults.ImpersonatedCustomerIdAttribute, customer.Id);

        await _commonHelperService.ClearUnavailableShoppingCartAndWishlistItemsBeforeImpersonation(customer: customer);

        return RedirectToAction("Index", "Home", new { area = string.Empty });
    }

    #endregion
}