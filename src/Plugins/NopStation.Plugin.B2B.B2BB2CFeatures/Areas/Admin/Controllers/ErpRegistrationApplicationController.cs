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
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpRegistrationApplication;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpWorkflowMessage;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers
{
    public class ErpRegistrationApplicationController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IErpAccountModelFactory _erpAccountModelFactory;
        private readonly IErpAccountService _erpAccountService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IAddressService _addressService;
        private readonly IErpSalesOrgService _erpSalesOrgService;
        private readonly IWorkContext _workContext;
        private readonly IErpLogsService _erpLogsService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly IErpIntegrationPluginManager _erpIntegrationPluginManager;
        private readonly IErpNopUserModelFactory _erpNopUserModelFactory;
        private readonly IErpShipToAddressModelFactory _erpShipToAddressModelFactory;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IErpRegistrationApplicationModelFactory _erpRegistrationApplicationModelFactory;
        private readonly IErpAccountCustomerRegistrationFormService _erpAccountCustomerRegistrationFormService;
        private readonly IErpAccountCustomerRegistrationBankingDetailsService _erpAccountCustomerRegistrationBankingDetailsService;
        private readonly IErpAccountCustomerRegistrationPhysicalTradingAddressService _erpAccountCustomerRegistrationPhysicalTradingAddressService;
        private readonly IErpAccountCustomerRegistrationTradeReferencesService _erpAccountCustomerRegistrationTradeReferencesService;
        private readonly IErpAccountCustomerRegistrationPremisesService _erpAccountCustomerRegistrationPremisesService;
        private readonly IErpWorkflowMessageService _erpWorkflowMessageService;

        #endregion

        #region Ctor

        public ErpRegistrationApplicationController(
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IErpAccountModelFactory erpAccountModelFactory,
            IErpAccountService erpAccountService,
            ICustomerActivityService customerActivityService,
            IAddressService addressService,
            IErpSalesOrgService erpSalesOrgService,
            IErpLogsService erpLogsService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IErpIntegrationPluginManager erpIntegrationPluginManager,
            IErpNopUserModelFactory erpNopUserModelFactory,
            IErpShipToAddressModelFactory erpShipToAddressModelFactory,
            ISettingService settingService,
            IStoreContext storeContext,
            IErpRegistrationApplicationModelFactory erpRegistrationApplicationModelFactory,
            IErpAccountCustomerRegistrationFormService erpAccountCustomerRegistrationFormService,
            IErpAccountCustomerRegistrationBankingDetailsService erpAccountCustomerRegistrationBankingDetailsService,
            IErpAccountCustomerRegistrationPhysicalTradingAddressService erpAccountCustomerRegistrationPhysicalTradingAddressService,
            IErpAccountCustomerRegistrationTradeReferencesService erpAccountCustomerRegistrationTradeReferencesService,
            IErpAccountCustomerRegistrationPremisesService erpAccountCustomerRegistrationPremisesService,
            IErpWorkflowMessageService erpWorkflowMessageService,
            IWorkContext workContext)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _erpAccountModelFactory = erpAccountModelFactory;
            _erpAccountService = erpAccountService;
            _customerActivityService = customerActivityService;
            _addressService = addressService;
            _erpSalesOrgService = erpSalesOrgService;
            _erpLogsService = erpLogsService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _erpIntegrationPluginManager = erpIntegrationPluginManager;
            _erpNopUserModelFactory = erpNopUserModelFactory;
            _settingService = settingService;
            _storeContext = storeContext;
            _erpRegistrationApplicationModelFactory = erpRegistrationApplicationModelFactory;
            _erpAccountCustomerRegistrationFormService = erpAccountCustomerRegistrationFormService;
            _erpAccountCustomerRegistrationBankingDetailsService = erpAccountCustomerRegistrationBankingDetailsService;
            _erpAccountCustomerRegistrationPhysicalTradingAddressService = erpAccountCustomerRegistrationPhysicalTradingAddressService;
            _erpAccountCustomerRegistrationTradeReferencesService = erpAccountCustomerRegistrationTradeReferencesService;
            _erpAccountCustomerRegistrationPremisesService = erpAccountCustomerRegistrationPremisesService;
            _erpWorkflowMessageService = erpWorkflowMessageService;
            _erpShipToAddressModelFactory = erpShipToAddressModelFactory;
            _workContext = workContext;
        }

        #endregion

        #region Utilities

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

            var model = new ErpRegistrationApplicationSearchModel();
            model = await _erpRegistrationApplicationModelFactory.PrepareErpRegistrationApplicationSearchModelAsync(searchModel: model);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> List(ErpRegistrationApplicationSearchModel erpRegistrationApplicationSearchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return await AccessDeniedDataTablesJson();

            var model = await _erpRegistrationApplicationModelFactory.PrepareErpRegistrationApplicationListModelAsync(erpRegistrationApplicationSearchModel);

            return Json(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            //prepare model
            var model = await _erpRegistrationApplicationModelFactory.PrepareErpRegistrationApplicationModelAsync(new ApplicationFormModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public async Task<IActionResult> Create(ApplicationFormModel model, bool continueEditing, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var registeredOfficeAddress = model.RegisteredOfficeAddress.ToEntity<Address>();
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
                    RegisteredOfficeAddressId = registeredOfficeAddress?.Id ?? 0,
                    TypeOfBusiness = model.TypeOfBusiness ?? string.Empty,
                    EstimatePurchasesPerMonthZAR = model.EstimatePurchasesPerMonthZAR,
                    CreditLimitRequired = model.CreditLimitRequired,
                    IsActive = true
                };

                applicationForm.CreatedOnUtc = DateTime.UtcNow;
                applicationForm.CreatedById = (await _workContext.GetCurrentCustomerAsync()).Id;

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
                    var PhysicalTradingAddress = model.PhysicalTradingAddressModel.PhysicalTradingAddress.ToEntity<Address>();
                    await _addressService.InsertAddressAsync(PhysicalTradingAddress);

                    var erpPhysicalTradingAddress = new ErpAccountCustomerRegistrationPhysicalTradingAddress
                    {
                        FormId = applicationForm.Id,
                        FullName = model.PhysicalTradingAddressModel.FullName,
                        Surname = model.PhysicalTradingAddressModel.Surname,
                        PhysicalTradingAddressId = PhysicalTradingAddress?.Id ?? 0,
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

                await _erpLogsService.InformationAsync(successMsg + " Erp Account Customer Registration Form Id: " + applicationForm.Id, ErpSyncLevel.Account, customer: await _workContext.GetCurrentCustomerAsync());

                //Send Email to Admin and Customer
                await _erpWorkflowMessageService.SendERPCustomerRegistrationApplicationCreatedNotificationAsync(applicationForm, (await _workContext.GetWorkingLanguageAsync()).Id);

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("ApplicationEdit", new { id = applicationForm.Id });
            }

            //prepare model
            model = await _erpRegistrationApplicationModelFactory.PrepareErpRegistrationApplicationModelAsync(model, null);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> ApplicationEdit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            //try to get application with the specified id
            var erpAccountCustomerRegistrationForm = await _erpAccountCustomerRegistrationFormService.GetErpAccountCustomerRegistrationFormByIdAsync(id);
            if (erpAccountCustomerRegistrationForm == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _erpRegistrationApplicationModelFactory.PrepareErpRegistrationApplicationModelAsync(null, erpAccountCustomerRegistrationForm);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [HttpPost, ParameterBasedOnFormName("approve", "approve")]
        [FormValueRequired("save", "save-continue", "approve")]
        public async Task<IActionResult> ApplicationEdit(ApplicationFormModel model, bool continueEditing, bool approve, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            //try to get a application with the specified id
            var erpAccountCustomerRegistrationForm = await _erpAccountCustomerRegistrationFormService.GetErpAccountCustomerRegistrationFormByIdAsync(model.Id);
            if (erpAccountCustomerRegistrationForm == null)
                return RedirectToAction("List");

            if (approve)
            {
                erpAccountCustomerRegistrationForm.IsApproved = approve;
                await _erpAccountCustomerRegistrationFormService.UpdateErpAccountCustomerRegistrationFormAsync(erpAccountCustomerRegistrationForm);

                //Send Email to Customer
                await _erpWorkflowMessageService.SendERPCustomerRegistrationApplicationApprovedNotificationAsync(erpAccountCustomerRegistrationForm, (await _workContext.GetWorkingLanguageAsync()).Id);

                return RedirectToAction("ApplicationEdit", new { id = model.Id });
            }
            else if (ModelState.IsValid)
            {
                try
                {
                    var registeredOfficeAddress = model.RegisteredOfficeAddress.ToEntity<Address>();

                    if (registeredOfficeAddress.Id == 0)
                        await _addressService.InsertAddressAsync(registeredOfficeAddress);
                    else
                        await _addressService.UpdateAddressAsync(registeredOfficeAddress);

                    #region Form
                    erpAccountCustomerRegistrationForm.FullRegisteredName = model.FullRegisteredName ?? string.Empty;
                    erpAccountCustomerRegistrationForm.RegistrationNumber = model.RegistrationNumber ?? string.Empty;
                    erpAccountCustomerRegistrationForm.VatNumber = model.VatNumber ?? string.Empty;
                    erpAccountCustomerRegistrationForm.TelephoneNumber1 = model.TelephoneNumber1 ?? string.Empty;
                    erpAccountCustomerRegistrationForm.TelephoneNumber2 = model.TelephoneNumber2 ?? string.Empty;
                    erpAccountCustomerRegistrationForm.TelefaxNumber = model.TelefaxNumber ?? string.Empty;
                    erpAccountCustomerRegistrationForm.AccountsContactPersonNameSurname = model.AccountsContactPersonNameSurname ?? string.Empty;
                    erpAccountCustomerRegistrationForm.AccountsEmail = model.AccountsEmail ?? string.Empty;
                    erpAccountCustomerRegistrationForm.AccountsTelephoneNumber = model.AccountsTelephoneNumber ?? string.Empty;
                    erpAccountCustomerRegistrationForm.AccountsCellphoneNumber = model.AccountsCellphoneNumber ?? string.Empty;
                    erpAccountCustomerRegistrationForm.BuyerContactPersonNameSurname = model.BuyerContactPersonNameSurname ?? string.Empty;
                    erpAccountCustomerRegistrationForm.BuyerEmail = model.BuyerEmail ?? string.Empty;
                    erpAccountCustomerRegistrationForm.NatureOfBusiness = model.NatureOfBusiness ?? string.Empty;
                    erpAccountCustomerRegistrationForm.RegisteredOfficeAddressId = registeredOfficeAddress.Id;
                    erpAccountCustomerRegistrationForm.TypeOfBusiness = model.TypeOfBusiness ?? string.Empty;
                    erpAccountCustomerRegistrationForm.EstimatePurchasesPerMonthZAR = model.EstimatePurchasesPerMonthZAR;
                    erpAccountCustomerRegistrationForm.CreditLimitRequired = model.CreditLimitRequired;
                    erpAccountCustomerRegistrationForm.UpdatedOnUtc = DateTime.UtcNow;
                    erpAccountCustomerRegistrationForm.UpdatedById = (await _workContext.GetCurrentCustomerAsync()).Id;

                    await _erpAccountCustomerRegistrationFormService.UpdateErpAccountCustomerRegistrationFormAsync(erpAccountCustomerRegistrationForm);
                    #endregion

                    #region Banking Details
                    var erpAccountCustomerRegistrationBankingDetails = (await _erpAccountCustomerRegistrationBankingDetailsService.GetErpAccountCustomerRegistrationBankingDetailsByFormIdAsync(model.Id))?.FirstOrDefault();

                    if (model.BankingDetailsModel != null && erpAccountCustomerRegistrationBankingDetails != null)
                    {
                        erpAccountCustomerRegistrationBankingDetails.NameOfBanker = model.BankingDetailsModel.NameOfBanker;
                        erpAccountCustomerRegistrationBankingDetails.AccountNumber = model.BankingDetailsModel.AccountNumber;
                        erpAccountCustomerRegistrationBankingDetails.AccountName = model.BankingDetailsModel.AccountName;
                        erpAccountCustomerRegistrationBankingDetails.BranchCode = model.BankingDetailsModel.BranchCode;
                        erpAccountCustomerRegistrationBankingDetails.Branch = model.BankingDetailsModel.Branch;
                        erpAccountCustomerRegistrationBankingDetails.IsActive = true;

                        await _erpAccountCustomerRegistrationBankingDetailsService.UpdateErpAccountCustomerRegistrationBankingDetailsAsync(erpAccountCustomerRegistrationBankingDetails);
                    }
                    else if (model.BankingDetailsModel != null)
                    {
                        var erpBankingDetails = new ErpAccountCustomerRegistrationBankingDetails
                        {
                            FormId = model.Id,
                            NameOfBanker = model.BankingDetailsModel.NameOfBanker,
                            AccountNumber = model.BankingDetailsModel.AccountNumber,
                            AccountName = model.BankingDetailsModel.AccountName,
                            BranchCode = model.BankingDetailsModel.BranchCode,
                            Branch = model.BankingDetailsModel.Branch,
                            IsActive = true,
                            CreatedOnUtc = DateTime.UtcNow,
                            CreatedById = (await _workContext.GetCurrentCustomerAsync()).Id
                        };

                        await _erpAccountCustomerRegistrationBankingDetailsService.InsertErpAccountCustomerRegistrationBankingDetailsAsync(erpBankingDetails);
                    }
                    #endregion

                    #region Physical Trading Address
                    var erpAccountCustomerRegistrationPhysicalTradingAddress = (await _erpAccountCustomerRegistrationPhysicalTradingAddressService.GetErpAccountCustomerRegistrationPhysicalTradingAddressByFormIdAsync(model.Id))?.FirstOrDefault();

                    if (model.PhysicalTradingAddressModel != null && erpAccountCustomerRegistrationPhysicalTradingAddress != null)
                    {
                        var physicalTradingAddress = model.PhysicalTradingAddressModel.PhysicalTradingAddress.ToEntity<Address>();

                        if (physicalTradingAddress.Id == 0)
                            await _addressService.InsertAddressAsync(physicalTradingAddress);
                        else
                            await _addressService.UpdateAddressAsync(physicalTradingAddress);

                        erpAccountCustomerRegistrationPhysicalTradingAddress.FullName = model.PhysicalTradingAddressModel.FullName;
                        erpAccountCustomerRegistrationPhysicalTradingAddress.Surname = model.PhysicalTradingAddressModel.Surname;
                        erpAccountCustomerRegistrationPhysicalTradingAddress.PhysicalTradingAddressId = physicalTradingAddress.Id;

                        await _erpAccountCustomerRegistrationPhysicalTradingAddressService.UpdateErpAccountCustomerRegistrationPhysicalTradingAddressAsync(erpAccountCustomerRegistrationPhysicalTradingAddress);
                    }
                    else if (model.PhysicalTradingAddressModel != null)
                    {
                        var newPhysicalTradingAddress = model.PhysicalTradingAddressModel.PhysicalTradingAddress.ToEntity<Address>();

                        if (newPhysicalTradingAddress.Id == 0)
                            await _addressService.InsertAddressAsync(newPhysicalTradingAddress);
                        else
                            await _addressService.UpdateAddressAsync(newPhysicalTradingAddress);

                        var erpPhysicalTradingAddress = new ErpAccountCustomerRegistrationPhysicalTradingAddress
                        {
                            FormId = model.Id,
                            FullName = model.PhysicalTradingAddressModel.FullName,
                            Surname = model.PhysicalTradingAddressModel.Surname,
                            PhysicalTradingAddressId = newPhysicalTradingAddress.Id,
                            IsActive = true,
                            CreatedOnUtc = DateTime.UtcNow,
                            CreatedById = (await _workContext.GetCurrentCustomerAsync()).Id
                        };

                        await _erpAccountCustomerRegistrationPhysicalTradingAddressService.InsertErpAccountCustomerRegistrationPhysicalTradingAddressAsync(erpPhysicalTradingAddress);
                    }
                    #endregion

                    #region Permises
                    var erpAccountCustomerRegistrationPermises = (await _erpAccountCustomerRegistrationPremisesService.GetErpAccountCustomerRegistrationPremisesByFormIdAsync(model.Id))?.FirstOrDefault();

                    if (model.PremisesModel != null && erpAccountCustomerRegistrationPermises != null)
                    {
                        erpAccountCustomerRegistrationPermises.OwnedOrLeased = model.PremisesModel.OwnedOrLeased?.Trim().ToLower() == "true";
                        erpAccountCustomerRegistrationPermises.NameOfLandlord = model.PremisesModel.NameOfLandlord;
                        erpAccountCustomerRegistrationPermises.AddressOfLandlord = model.PremisesModel.AddressOfLandlord;
                        erpAccountCustomerRegistrationPermises.EmailOfLandlord = model.PremisesModel.EmailOfLandlord;
                        erpAccountCustomerRegistrationPermises.TelephoneNumberOfLandlord = model.PremisesModel.TelephoneNumberOfLandlord;

                        await _erpAccountCustomerRegistrationPremisesService.UpdateErpAccountCustomerRegistrationPremisesAsync(erpAccountCustomerRegistrationPermises);
                    }
                    else if (model.PremisesModel != null)
                    {
                        var erpPremises = new ErpAccountCustomerRegistrationPremises
                        {
                            FormId = model.Id,
                            OwnedOrLeased = model.PremisesModel.OwnedOrLeased?.Trim().ToLower() == "true",
                            NameOfLandlord = model.PremisesModel.NameOfLandlord,
                            AddressOfLandlord = model.PremisesModel.AddressOfLandlord,
                            EmailOfLandlord = model.PremisesModel.EmailOfLandlord,
                            TelephoneNumberOfLandlord = model.PremisesModel.TelephoneNumberOfLandlord,
                            IsActive = true,
                            CreatedOnUtc = DateTime.UtcNow,
                            CreatedById = (await _workContext.GetCurrentCustomerAsync()).Id
                        };

                        await _erpAccountCustomerRegistrationPremisesService.InsertErpAccountCustomerRegistrationPremisesAsync(erpPremises);
                    }
                    #endregion

                    #region Trade References
                    var erpAccountCustomerRegistrationTradeReferences = (await _erpAccountCustomerRegistrationTradeReferencesService.GetErpAccountCustomerRegistrationTradeReferencesByFormIdAsync(model.Id))?.FirstOrDefault();

                    if (model.TradeReferencesModel != null && erpAccountCustomerRegistrationTradeReferences != null)
                    {
                        erpAccountCustomerRegistrationTradeReferences.Name = model.TradeReferencesModel.Name;
                        erpAccountCustomerRegistrationTradeReferences.Telephone = model.TradeReferencesModel.Telephone;
                        erpAccountCustomerRegistrationTradeReferences.Amount = model.TradeReferencesModel.Amount;
                        erpAccountCustomerRegistrationTradeReferences.Terms = model.TradeReferencesModel.Terms;
                        erpAccountCustomerRegistrationTradeReferences.HowLong = model.TradeReferencesModel.HowLong;

                        await _erpAccountCustomerRegistrationTradeReferencesService.UpdateErpAccountCustomerRegistrationTradeReferencesAsync(erpAccountCustomerRegistrationTradeReferences);
                    }
                    else if (model.TradeReferencesModel != null)
                    {
                        var erpTradeReferences = new ErpAccountCustomerRegistrationTradeReferences
                        {
                            FormId = model.Id,
                            Name = model.TradeReferencesModel.Name,
                            Telephone = model.TradeReferencesModel.Telephone,
                            Amount = model.TradeReferencesModel.Amount,
                            Terms = model.TradeReferencesModel.Terms,
                            HowLong = model.TradeReferencesModel.HowLong,
                            IsActive = true,
                            CreatedOnUtc = DateTime.UtcNow,
                            CreatedById = (await _workContext.GetCurrentCustomerAsync()).Id
                        };

                        await _erpAccountCustomerRegistrationTradeReferencesService.InsertErpAccountCustomerRegistrationTradeReferencesAsync(erpTradeReferences);
                    }
                    #endregion

                    var successMsg = await _localizationService.GetResourceAsync("B2BB2CFeatures.ErpAccountCustomerRegistrationForm.Updated");
                    _notificationService.SuccessNotification(successMsg);

                    await _erpLogsService.InformationAsync(successMsg + "  " + model.Id, ErpSyncLevel.Account, customer: await _workContext.GetCurrentCustomerAsync());

                    if (!continueEditing)
                        return RedirectToAction("List");

                    return RedirectToAction("ApplicationEdit", new { id = model.Id });
                }
                catch (Exception exc)
                {
                    _notificationService.ErrorNotification(exc.Message);
                }
            }

            //prepare model
            model = await _erpRegistrationApplicationModelFactory.PrepareErpRegistrationApplicationModelAsync(model, erpAccountCustomerRegistrationForm);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //try to get a erpSalesOrg with the specified id
            var erpAccountCustomerRegistrationForm = await _erpAccountCustomerRegistrationFormService.GetErpAccountCustomerRegistrationFormByIdAsync(id);
            if (erpAccountCustomerRegistrationForm == null)
                return RedirectToAction("List");

            //delete a erpShipToAddress
            await _erpAccountCustomerRegistrationFormService.DeleteErpAccountCustomerRegistrationFormByIdAsync(id);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteErpAccountCustomerRegistrationApplication",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteErpAccountCustomerRegistrationApplication"), id), erpAccountCustomerRegistrationForm);

            var successMsg = await _localizationService.GetResourceAsync("B2BB2CFeatures.ErpAccountCustomerRegistrationForm.Deleted");

            _notificationService.SuccessNotification(successMsg);

            await _erpLogsService.InformationAsync(successMsg + " Erp Account Customer Registration Form Id: " + id, ErpSyncLevel.Account, customer: await _workContext.GetCurrentCustomerAsync());

            return RedirectToAction("List");
        }

        #endregion
    }
}