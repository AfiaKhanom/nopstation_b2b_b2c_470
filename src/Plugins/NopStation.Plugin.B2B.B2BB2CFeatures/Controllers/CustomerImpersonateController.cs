using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using Nop.Core.Http;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Web.Controllers;
using Nop.Web.Framework;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.SalesRepUser;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.Customers;
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Controllers;

public class CustomerImpersonateController : BasePublicController
{
    #region Fields

    private readonly IWorkContext _workContext;
    private readonly IErpSalesRepService _erpSalesRepService;
    private readonly ISalesRepUserModelFactory _salesRepUserModelFactory;
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;
    private readonly ICustomerService _customerService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IAuthenticationService _authenticationService;
    private readonly IEventPublisher _eventPublisher;
    private readonly StoreInformationSettings _storeInformationSettings;
    private readonly IErpLogsService _erpLogsService;
    private readonly IErpActivityLogsService _erpActivityLogsService;
    private readonly ICommonHelperService _commonHelperService;
    private readonly IErpAccountService _erpAccountService;

    #endregion

    #region Ctor

    public CustomerImpersonateController(IWorkContext workContext,
        IErpSalesRepService erpSalesRepService,
        ISalesRepUserModelFactory salesRepUserModelFactory,
        INotificationService notificationService,
        ILocalizationService localizationService,
        ICustomerService customerService,
        ICustomerActivityService customerActivityService,
        IGenericAttributeService genericAttributeService,
        IAuthenticationService authenticationService,
        IEventPublisher eventPublisher,
        StoreInformationSettings storeInformationSettings,
        IErpLogsService erpLogsService,
        IErpActivityLogsService erpActivityLogsService,
        ICommonHelperService commonHelperService,
        IErpAccountService erpAccountService)
    {
        _workContext = workContext;
        _erpSalesRepService = erpSalesRepService;
        _salesRepUserModelFactory = salesRepUserModelFactory;
        _notificationService = notificationService;
        _localizationService = localizationService;
        _customerService = customerService;
        _customerActivityService = customerActivityService;
        _genericAttributeService = genericAttributeService;
        _authenticationService = authenticationService;
        _eventPublisher = eventPublisher;
        _storeInformationSettings = storeInformationSettings;
        _erpLogsService = erpLogsService;
        _erpActivityLogsService = erpActivityLogsService;
        _commonHelperService = commonHelperService;
        _erpAccountService = erpAccountService;
    }

    #endregion

    #region Utilities

    protected async Task<bool> HasB2BSalesRepRoleAsync()
    {
        var salesRepRoles = await _customerService.GetCustomerRolesAsync((await _workContext.GetCurrentCustomerAsync()));
        if (!salesRepRoles.Any())
        {
            return false;
        }
        var salesRepRole = salesRepRoles.FirstOrDefault(r => r.SystemName == ERPIntegrationCoreDefaults.B2BSalesRepRoleSystemName);

        if (salesRepRole == null && !salesRepRoles.Any(r => r.SystemName == NopCustomerDefaults.AdministratorsRoleName))
            return false;

        return true;
    }

    #endregion

    #region B2B User For Sales Rep

    public virtual async Task<IActionResult> List()
    {
        if (!await HasB2BSalesRepRoleAsync())
            return AccessDeniedView();

        //prepare model
        var model = await _salesRepUserModelFactory.PrepareSalesRepUserSearchModelAsync(new SalesRepUserSearchModel());

        return View(model);
    }

    [HttpPost]
    public async virtual Task<IActionResult> SalesRepUsersList(SalesRepUserSearchModel searchModel)
    {
        if (!await HasB2BSalesRepRoleAsync())
            return await AccessDeniedDataTablesJson();

        var customer = await _workContext.GetCurrentCustomerAsync();

        var salesRep = (await _erpSalesRepService.GetErpSalesRepsByNopCustomerIdAsync(customer.Id)).FirstOrDefault();

        if (salesRep == null)
            salesRep = new ErpSalesRep();

        if (salesRep.SalesRepTypeId == (int)SalesRepType.MultiBuyers)
        {
            var multiBuyermodel = await _salesRepUserModelFactory.PreparePublicSalesRepUserListModelForSalesRep(searchModel, salesRep);
            return Json(multiBuyermodel);
        }

        var regularModel = await _salesRepUserModelFactory.PrepareSalesRepUserListModelForSalesRep(searchModel, salesRep);
        return Json(regularModel);
    }


    public async virtual Task<IActionResult> Impersonate(int id)
    {
        if (!await HasB2BSalesRepRoleAsync())
            return AccessDeniedView();

        //try to get a customer with the specified id
        var customer = await _customerService.GetCustomerByIdAsync(id);
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null || !customer.Active || customer.Deleted)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeaturesErpNopUser.NoCustomerAssociated"));
            return RedirectToAction("List");
        }

        if (!customer.Active)
        {
            _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.Impersonate.Inactive"));
            return RedirectToAction("List");
        }

        var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);
        if (erpAccount == null)
        {
            _notificationService.WarningNotification(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.Admin.Customers.Impersonate.ErpAccountNotAvailable"));
            return RedirectToAction("List");
        }

        //ensure that a non-admin user cannot impersonate as an administrator
        //otherwise, that user can simply impersonate as an administrator and gain additional administrative privileges
        if (await _customerService.IsAdminAsync(customer))
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.NonAdminNotImpersonateAsAdminError"));
            return RedirectToAction("List");
        }

        await _workContext.SetCurrentCustomerAsync(customer);

        await _commonHelperService.ClearUnavailableShoppingCartAndWishlistItemsBeforeImpersonation(customer: customer);

        var successMsg = await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Started.Customer");
        await _erpLogsService.InformationAsync($"{successMsg}. Impersonated Customer: {customer.Email}, Id: {customer.Id}. Original Customer Email: {currentCustomer.Email}, Id: {currentCustomer.Id}", ErpSyncLevel.LoginLogout, customer: customer);

        //erp activity log  
        await _erpActivityLogsService.InsertErpActivityAsync(customer, "Erp_CustomerImpersonationStart",
            string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.CustomerImpersonationStarted"),
            customer.Email, customer.Id, currentCustomer.Email, currentCustomer.Id), customer);

        //activity log
        await _customerActivityService.InsertActivityAsync("Impersonation.Started",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Started.Customer"), customer.Email, customer.Id), customer);

        //ensure login is not required
        customer.RequireReLogin = false;
        await _customerService.UpdateCustomerAsync(customer);
        await _genericAttributeService.SaveAttributeAsync<int?>(currentCustomer, NopCustomerDefaults.ImpersonatedCustomerIdAttribute, customer.Id);

        return RedirectToAction("Index", "Home", new { area = string.Empty });
    }

    public virtual async Task<IActionResult> Logout()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (_workContext.OriginalCustomerIfImpersonated != null)
        {
            await _erpLogsService.InformationAsync($"Customer impersonation finished as: {customer.Email}, Customer Id: {customer.Id}. Original Customer Email: {_workContext.OriginalCustomerIfImpersonated.Email}, Id: {_workContext.OriginalCustomerIfImpersonated.Id}", ErpSyncLevel.LoginLogout, customer: customer);

            //erp activity log
            await _erpActivityLogsService.InsertErpActivityAsync(_workContext.OriginalCustomerIfImpersonated, "Erp_CustomerImpersonationEnd",
                string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.CustomerImpersonationEnded"),
            customer.Email, customer.Id, _workContext.OriginalCustomerIfImpersonated.Email, _workContext.OriginalCustomerIfImpersonated.Id), customer);

            //activity log
            await _customerActivityService.InsertActivityAsync(_workContext.OriginalCustomerIfImpersonated, "Impersonation.Finished",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Finished.StoreOwner"),
                    customer.Email, customer.Id),
                customer);

            await _customerActivityService.InsertActivityAsync("Impersonation.Finished",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Finished.Customer"),
                    _workContext.OriginalCustomerIfImpersonated.Email, _workContext.OriginalCustomerIfImpersonated.Id),
                _workContext.OriginalCustomerIfImpersonated);

            //logout impersonated customer
            await _genericAttributeService
                .SaveAttributeAsync<int?>(_workContext.OriginalCustomerIfImpersonated, NopCustomerDefaults.ImpersonatedCustomerIdAttribute, null);

            if (await _customerService.IsAdminAsync(_workContext.OriginalCustomerIfImpersonated) || await _customerService.IsInCustomerRoleAsync(_workContext.OriginalCustomerIfImpersonated, ERPIntegrationCoreDefaults.B2BB2CAdminRoleSystemName))
            {
                var adminLastVisitedPageBeforeImpersonating = await _genericAttributeService.GetAttributeAsync<string?>(_workContext.OriginalCustomerIfImpersonated, NopCustomerDefaults.LastVisitedPageAttribute);
                await _genericAttributeService.SaveAttributeAsync<string?>(_workContext.OriginalCustomerIfImpersonated, NopCustomerDefaults.LastVisitedPageAttribute, null);

                if (!string.IsNullOrEmpty(adminLastVisitedPageBeforeImpersonating))
                    return Redirect(adminLastVisitedPageBeforeImpersonating);

                //redirect back to customer details page (admin area)
                return RedirectToAction("Edit", "Customer", new { id = customer.Id, area = AreaNames.ADMIN });
            }

            //redirect back to customer details page (admin area)
            return RedirectToAction(nameof(List));
        }
        //activity log
        await _erpLogsService.InformationAsync($"Customer Logged out as: {customer.Email}, Customer Id: {customer.Id}", ErpSyncLevel.LoginLogout, customer: customer);

        if (await _customerService.IsAdminAsync(customer))
        {
            //erp activity log
            await _erpActivityLogsService.InsertErpActivityAsync(customer, "Erp_ErpCustomerPublicStoreLogOut",
            string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.CustomerPublicStoreLogOut"),
            customer.Email, customer.Id), customer);
        }

        //activity log
        await _customerActivityService.InsertActivityAsync(customer, "PublicStore.Logout",
            await _localizationService.GetResourceAsync("ActivityLog.PublicStore.Logout"), customer);

        //standard logout 
        await _authenticationService.SignOutAsync();

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
}