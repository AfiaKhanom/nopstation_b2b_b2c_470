using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpShipToAddress;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers;

public class ErpAccountController : NopStationAdminController
{
    #region Fields

    private readonly IStoreContext _storeContext;
    private readonly IAddressService _addressService;
    private readonly ICountryService _countryService;
    private readonly ISettingService _settingService;
    private readonly IPermissionService _permissionService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly IErpLogsService _erpLogsService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpAccountModelFactory _erpAccountModelFactory;
    private readonly IErpNopUserModelFactory _erpNopUserModelFactory;
    private readonly IErpActivityLogsService _erpActivityLogsService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginManager;
    private readonly IErpShipToAddressModelFactory _erpShipToAddressModelFactory;
    private readonly IB2BB2CWorkContext _b2BB2CWorkContext;

    #endregion

    #region Ctor

    public ErpAccountController(
        IStoreContext storeContext,
        IAddressService addressService,
        ICountryService countryService,
        ISettingService settingService,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IStateProvinceService stateProvinceService,
        IErpLogsService erpLogsService,
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService,
        IErpAccountModelFactory erpAccountModelFactory,
        IErpNopUserModelFactory erpNopUserModelFactory,
        IErpActivityLogsService erpActivityLogsService,
        IErpIntegrationPluginManager erpIntegrationPluginManager,
        IErpShipToAddressModelFactory erpShipToAddressModelFactory,
        IB2BB2CWorkContext b2BB2CWorkContext)
    {
        _storeContext = storeContext;
        _addressService = addressService;
        _countryService = countryService;
        _settingService = settingService;
        _permissionService = permissionService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _stateProvinceService = stateProvinceService;
        _erpLogsService = erpLogsService;
        _erpAccountService = erpAccountService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpAccountModelFactory = erpAccountModelFactory;
        _erpNopUserModelFactory = erpNopUserModelFactory;
        _erpActivityLogsService = erpActivityLogsService;
        _erpIntegrationPluginManager = erpIntegrationPluginManager;
        _erpShipToAddressModelFactory = erpShipToAddressModelFactory;
        _b2BB2CWorkContext = b2BB2CWorkContext;
    }

    #endregion

    #region Utilities

    protected async Task<string> GetErpSalesOrgNameAndCodeById(int salesOrgId)
    {
        if (salesOrgId == 0)
            return null;
        var tmp = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(salesOrgId);
        return tmp.Name + '-' + tmp.Code;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> Index()
    {
        return RedirectToAction("List");
    }

    public async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        var model = new ErpAccountSearchModel();
        model = await _erpAccountModelFactory.PrepareErpAccountSearchModelAsync(searchModel: model);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ErpAccountList(ErpAccountSearchModel erpAccountSearchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return await AccessDeniedDataTablesJson();

        var model = await _erpAccountModelFactory.PrepareErpAccountListModelAsync(erpAccountSearchModel);

        return Json(model);
    }

    public async Task<IActionResult> CreateErpAccount()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        //prepare model
        var model = await _erpAccountModelFactory.PrepareErpAccountModelAsync(new ErpAccountModel(), null);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [FormValueRequired("save", "save-continue")]
    public async Task<IActionResult> CreateErpAccount(ErpAccountModel model, bool continueEditing, IFormCollection form)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var currentCustomer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();
            var stateProvidence = await _stateProvinceService.GetStateProvinceByIdAsync(model.BillingAddress.StateProvinceId ?? 0);
            var country = await _countryService.GetCountryByIdAsync(model.BillingAddress.CountryId ?? 0);

            //If UseERPIntegration is not true, then ERPIntegrations will not be called
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var b2BB2CFeaturesSettings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(storeScope);

            if (b2BB2CFeaturesSettings.UseERPIntegration)
            {
                var erpIntegrationPlugin = await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();
                if (erpIntegrationPlugin == null)
                {
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("B2BB2C.Account.Registration.AccountNotCreated"));
                    await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Error, ErpSyncLevel.Account, "Integration method not found.");
                }
                else
                {
                    var erpAccountInfo = await erpIntegrationPlugin.CreateAccountNoErpAsync(new ErpCreateAccountModel
                    {
                        VatNumber = model.VatNumber ?? string.Empty,
                        StateProvince = stateProvidence is null ? string.Empty : stateProvidence.Name,
                        Country = country is null ? string.Empty : country.Name,
                        County = model.BillingAddress.County ?? string.Empty,
                        City = model.BillingAddress.City ?? string.Empty,
                        ContactName = $"{model.BillingAddress.FirstName} {model.BillingAddress.LastName}",
                        FaxNumber = model.BillingAddress.FaxNumber ?? string.Empty,
                        PhoneNumber = model.BillingAddress.PhoneNumber ?? string.Empty,
                        PostalCode = model.BillingAddress.ZipPostalCode ?? string.Empty,
                        ZipPostalCode = model.BillingAddress.ZipPostalCode ?? string.Empty,
                        Address1 = model.BillingAddress.Address1 ?? string.Empty,
                        Address2 = model.BillingAddress.Address2 ?? string.Empty,
                        Address3 = string.Empty,
                        Email = model.BillingAddress.Email ?? string.Empty,
                        AccountNumber = model.AccountNumber,
                        AccountName = model.AccountName ?? string.Empty,
                    });

                    if (erpAccountInfo.IsError)
                    {
                        _notificationService.ErrorNotification(erpAccountInfo.ErrorShortMessage);
                        await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Error, ErpSyncLevel.Account, erpAccountInfo.ErrorShortMessage, erpAccountInfo.ErrorFullMessage);
                        model = await _erpAccountModelFactory.PrepareErpAccountModelAsync(model, null);
                        return View(model);
                    }
                }
            }

            //fill entity from model
            var erpAccount = model.ToEntity<ErpAccount>();

            erpAccount.CreatedOnUtc = DateTime.UtcNow;
            erpAccount.CreatedById = currentCustomer.Id;

            await _erpAccountService.InsertErpAccountAsync(erpAccount);

            //address
            var address = model.BillingAddress.ToEntity<Address>();
            address.CreatedOnUtc = DateTime.UtcNow;

            //some validation
            if (address.CountryId == 0)
                address.CountryId = null;
            if (address.StateProvinceId == 0)
                address.StateProvinceId = null;
            await _addressService.InsertAddressAsync(address);
            erpAccount.BillingAddressId = address.Id;

            await _erpAccountService.UpdateErpAccountAsync(erpAccount);

            var successMsg = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Added");
            _notificationService.SuccessNotification(successMsg);

            await _erpLogsService.InformationAsync($"{successMsg}. Erp Account Id: {erpAccount.Id}", ErpSyncLevel.Account, customer: currentCustomer);

            //erp activity log
            await _erpActivityLogsService.InsertErpActivityAsync("Erp_AddNewErpAccount",
                string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.AddNewErpAccount"),
                erpAccount.Id),
                erpAccount);

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("ErpAccountEdit", new { id = erpAccount.Id });
        }

        //prepare model
        model = await _erpAccountModelFactory.PrepareErpAccountModelAsync(model, null);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    public async Task<IActionResult> ErpAccountEdit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        //try to get a customer with the specified id
        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(id);
        if (erpAccount == null)
            return RedirectToAction("List");

        //prepare model
        var model = await _erpAccountModelFactory.PrepareErpAccountModelAsync(null, erpAccount);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [FormValueRequired("save", "save-continue")]
    public async Task<IActionResult> ErpAccountEdit(ErpAccountModel model, bool continueEditing, IFormCollection form)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        //try to get a erpAccount with the specified id
        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(model.Id);
        if (erpAccount == null)
            return RedirectToAction("List");

        if (ModelState.IsValid)
        {
            try
            {
                erpAccount.AccountName = model.AccountName;
                erpAccount.AccountNumber = model.AccountNumber;
                erpAccount.VatNumber = model.VatNumber;
                erpAccount.ErpSalesOrgId = model.ErpSalesOrgId;
                erpAccount.IsActive = model.IsActive;
                erpAccount.BillingSuburb = model.BillingSuburb;
                erpAccount.CreditLimit = model.CreditLimit;
                erpAccount.CreditLimitAvailable = model.CreditLimitAvailable;
                erpAccount.CurrentBalance = model.CurrentBalance;
                erpAccount.LastPaymentAmount = model.LastPaymentAmount;
                erpAccount.LastPaymentDate = model.LastPaymentDate;
                erpAccount.AllowOverspend = model.AllowOverspend;
                erpAccount.PreFilterFacets = model.PreFilterFacets;
                erpAccount.PaymentTypeCode = model.PaymentTypeCode;
                erpAccount.OverrideBackOrderingConfigSetting = model.OverrideBackOrderingConfigSetting;
                erpAccount.AllowAccountsBackOrdering = model.AllowAccountsBackOrdering;
                erpAccount.OverrideAddressEditOnCheckoutConfigSetting = model.OverrideAddressEditOnCheckoutConfigSetting;
                erpAccount.AllowAccountsAddressEditOnCheckout = model.AllowAccountsAddressEditOnCheckout;
                erpAccount.OverrideStockDisplayFormatConfigSetting = model.OverrideStockDisplayFormatConfigSetting;
                erpAccount.ErpAccountStatusTypeId = model.ErpAccountStatusTypeId;
                erpAccount.B2BPriceGroupCodeId = model.B2BPriceGroupCodeId;
                erpAccount.LastPriceRefresh = model.LastPriceRefresh;
                erpAccount.UpdatedOnUtc = DateTime.UtcNow;
                erpAccount.UpdatedById = (await _b2BB2CWorkContext.GetCurrentCustomerAsync()).Id;

                await _erpAccountService.UpdateErpAccountAsync(erpAccount);


                //address
                var address = await _addressService.GetAddressByIdAsync(erpAccount.BillingAddressId ?? 0);
                if (address == null)
                {
                    address = model.BillingAddress.ToEntity<Address>();
                    address.CreatedOnUtc = DateTime.UtcNow;

                    //some validation
                    if (address.CountryId == 0)
                        address.CountryId = null;
                    if (address.StateProvinceId == 0)
                        address.StateProvinceId = null;

                    await _addressService.InsertAddressAsync(address);

                    erpAccount.BillingAddressId = address.Id;
                    await _erpAccountService.UpdateErpAccountAsync(erpAccount);
                }
                else
                {
                    address = model.BillingAddress.ToEntity(address);

                    //some validation
                    if (address.CountryId == 0)
                        address.CountryId = null;
                    if (address.StateProvinceId == 0)
                        address.StateProvinceId = null;

                    await _addressService.UpdateAddressAsync(address);
                }

                var successMsg = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Updated");
                _notificationService.SuccessNotification(successMsg);

                await _erpLogsService.InformationAsync($"{successMsg}. Erp Account Id: {erpAccount.Id}", ErpSyncLevel.Account, customer: await _b2BB2CWorkContext.GetCurrentCustomerAsync());

                //erp activity log
                await _erpActivityLogsService.InsertErpActivityAsync("Erp_EditErpAccount",
                    string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.EditErpAccount"),
                    erpAccount.Id),
                    erpAccount);

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("ErpAccountEdit", new { id = erpAccount.Id });
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc.Message);
            }
        }

        //prepare model
        model = await _erpAccountModelFactory.PrepareErpAccountModelAsync(model, erpAccount);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        //try to get a erpSalesOrg with the specified id
        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(id);
        if (erpAccount == null)
            return RedirectToAction("List");

        //delete a erpShipToAddress
        await _erpAccountService.DeleteErpAccountByIdAsync(erpAccount.Id);

        var successMsg = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Deleted");
        _notificationService.SuccessNotification(successMsg);

        await _erpLogsService.InformationAsync($"{successMsg}. Erp Account Id: {erpAccount.Id}", ErpSyncLevel.Account, customer: await _b2BB2CWorkContext.GetCurrentCustomerAsync());

        //erp activity log
        await _erpActivityLogsService.InsertErpActivityAsync("Erp_DeleteErpAccount",
            string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.DeleteErpAccount"), id), erpAccount);

        return RedirectToAction("List");
    }

    public async Task<IActionResult> ErpAccountSearchAutoComplete(string term)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        const int searchTermMinimumLength = 3;
        if (string.IsNullOrWhiteSpace(term) || term.Length < searchTermMinimumLength)
            return Content(string.Empty);

        //b2b accounts
        var accounts = await _erpAccountService.GetAllErpAccountsAsync(erpAccontNo: term, pageSize: 15, showHidden: false);

        var result =
            (from acc in accounts
             select new
             {
                 label = $"{acc.AccountNumber} ({acc.AccountName}),",
                 erpaccountid = acc.Id
             }
            ).ToList();

        return Json(result);
    }

    public async Task<IActionResult> GetDefaultB2CErpAccountInfo(int erpAccountId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpAccountId);

        var erpAccountDetails = $"{erpAccount.AccountNumber} ({erpAccount.AccountName})";

        return Json(erpAccountDetails);
    }

    #endregion

    #region ErpAccount List of Nop User

    [HttpPost]
    public async Task<IActionResult> ErpAccountNopUsersList(ErpNopUserSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return await AccessDeniedDataTablesJson();

        var model = await _erpNopUserModelFactory.PrepareErpNopUserListModelAsync(searchModel);

        return Json(model);
    }

    #endregion

    #region ErpAccount List of Erp ShipToAddresses

    [HttpPost]
    public async Task<IActionResult> ErpAccountShipToAddressesList(ErpShipToAddressSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return await AccessDeniedDataTablesJson();

        var model = await _erpShipToAddressModelFactory.PrepareErpShipToAddressListModelAsync(searchModel);

        return Json(model);
    }

    #endregion
}