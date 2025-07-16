using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpRegistrationApplication;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public class ErpRegistrationApplicationModelFactory : IErpRegistrationApplicationModelFactory
{
    #region Fields

    private readonly IWorkContext _workContext;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly ILocalizationService _localizationService;
    private readonly IErpAccountCustomerRegistrationFormService _erpAccountCustomerRegistrationFormService;
    private readonly IErpAccountCustomerRegistrationBankingDetailsService _erpAccountCustomerRegistrationBankingDetailsService;
    private readonly IErpAccountCustomerRegistrationPhysicalTradingAddressService _erpAccountCustomerRegistrationPhysicalTradingAddressService;
    private readonly IErpAccountCustomerRegistrationTradeReferencesService _erpAccountCustomerRegistrationTradeReferencesService;
    private readonly IErpAccountCustomerRegistrationPremisesService _erpAccountCustomerRegistrationPremisesService;
    private readonly IAddressService _addressService;
    private readonly IAddressModelFactory _addressModelFactory;

    #endregion

    #region ctor

    public ErpRegistrationApplicationModelFactory(IWorkContext workContext,
        IDateTimeHelper dateTimeHelper,
        ILocalizationService localizationService,
        IErpAccountCustomerRegistrationFormService erpAccountCustomerRegistrationFormService,
        IErpAccountCustomerRegistrationBankingDetailsService erpAccountCustomerRegistrationBankingDetailsService,
        IErpAccountCustomerRegistrationPhysicalTradingAddressService erpAccountCustomerRegistrationPhysicalTradingAddressService,
        IErpAccountCustomerRegistrationTradeReferencesService erpAccountCustomerRegistrationTradeReferencesService,
        IErpAccountCustomerRegistrationPremisesService erpAccountCustomerRegistrationPremisesService,
        IAddressService addressService,
        IAddressModelFactory addressModelFactory
        )
    {
        _workContext = workContext;
        _localizationService = localizationService;
        _dateTimeHelper = dateTimeHelper;
        _erpAccountCustomerRegistrationFormService = erpAccountCustomerRegistrationFormService;
        _erpAccountCustomerRegistrationBankingDetailsService = erpAccountCustomerRegistrationBankingDetailsService;
        _erpAccountCustomerRegistrationPhysicalTradingAddressService = erpAccountCustomerRegistrationPhysicalTradingAddressService;
        _erpAccountCustomerRegistrationTradeReferencesService = erpAccountCustomerRegistrationTradeReferencesService;
        _erpAccountCustomerRegistrationPremisesService = erpAccountCustomerRegistrationPremisesService;
        _addressService = addressService;
        _addressModelFactory = addressModelFactory;
    }

    #endregion

    #region Utilities

    #endregion

    #region Method
    public async Task<ErpRegistrationApplicationSearchModel> PrepareErpRegistrationApplicationSearchModelAsync(ErpRegistrationApplicationSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

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

        //prepare "approved" filter (0 - all; 1 - approved only; 2 - pending only)
        searchModel.ShowIsApprovedOption.Add(new SelectListItem
        {
            Value = "0",
            Text = await _localizationService.GetResourceAsync("B2BB2CFeatures.Admin.ErpRegistrationApplication.ErpRegistrationApplicationSearchModel.Option.ShowAll"),
        });
        searchModel.ShowIsApprovedOption.Add(new SelectListItem
        {
            Value = "1",
            Text = await _localizationService.GetResourceAsync("B2BB2CFeatures.Admin.ErpRegistrationApplication.ErpRegistrationApplicationSearchModel.Option.ShowApproved"),
        });
        searchModel.ShowIsApprovedOption.Add(new SelectListItem
        {
            Value = "2",
            Text = await _localizationService.GetResourceAsync("B2BB2CFeatures.Admin.ErpRegistrationApplication.ErpRegistrationApplicationSearchModel.Option.ShowPending"),
        });

        //prepare grid
        searchModel.SetGridPageSize();

        return searchModel;
    }

    public async Task<ErpRegistrationApplicationListModel> PrepareErpRegistrationApplicationListModelAsync(ErpRegistrationApplicationSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get Erp Registration Applications forms
        var erpRegistrationApplicationForms = await _erpAccountCustomerRegistrationFormService.GetAllErpAccountCustomerRegistrationFormAsync(
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize,
            showHidden: searchModel.ShowInActive == 0 ? null : (bool?)(searchModel.ShowInActive == 2),
            formId: Convert.ToInt32(searchModel.FormId),
            fullRegisteredName: searchModel.FullRegisteredName,
            registrationNumber: searchModel.RegistrationNumber,
            accountsEmail: searchModel.AccountsEmail,
            fromDate: searchModel.FromDate,
            toDate: searchModel.ToDate,
            showApproved: searchModel.ShowIsApproved == 0 ? null : (bool?)(searchModel.ShowIsApproved == 1));

        //prepare list model
        var model = await new ErpRegistrationApplicationListModel().PrepareToGridAsync(searchModel, erpRegistrationApplicationForms, () =>
        {
            //fill in model values from the entity
            return erpRegistrationApplicationForms.SelectAwait(async erpRegistrationApplicationForm =>
            {
                var addressModel = new AddressModel();
                var address = await _addressService.GetAddressByIdAsync(erpRegistrationApplicationForm.RegisteredOfficeAddressId);

                if (address != null)
                    addressModel = address.ToModel(addressModel);
                await _addressModelFactory.PrepareAddressModelAsync(addressModel, address);

                //fill in model values from the entity
                var erpRegistrationApplicationFormModel = new ApplicationFormModel
                {
                    Id = erpRegistrationApplicationForm.Id,
                    FullRegisteredName = erpRegistrationApplicationForm.FullRegisteredName,
                    RegistrationNumber = erpRegistrationApplicationForm.RegistrationNumber,
                    VatNumber = erpRegistrationApplicationForm.VatNumber,
                    TelephoneNumber1 = erpRegistrationApplicationForm.TelephoneNumber1,
                    TelephoneNumber2 = erpRegistrationApplicationForm.TelephoneNumber2,
                    TelefaxNumber = erpRegistrationApplicationForm.TelefaxNumber,
                    AccountsContactPersonNameSurname = erpRegistrationApplicationForm.AccountsContactPersonNameSurname,
                    AccountsEmail = erpRegistrationApplicationForm.AccountsEmail,
                    AccountsTelephoneNumber = erpRegistrationApplicationForm.AccountsTelephoneNumber,
                    AccountsCellphoneNumber = erpRegistrationApplicationForm.AccountsCellphoneNumber,
                    BuyerContactPersonNameSurname = erpRegistrationApplicationForm.BuyerContactPersonNameSurname,
                    BuyerEmail = erpRegistrationApplicationForm.BuyerEmail,
                    NatureOfBusiness = erpRegistrationApplicationForm.NatureOfBusiness,
                    RegisteredOfficeAddress = addressModel,
                    TypeOfBusiness = erpRegistrationApplicationForm.TypeOfBusiness,
                    EstimatePurchasesPerMonthZAR = erpRegistrationApplicationForm.EstimatePurchasesPerMonthZAR,
                    CreditLimitRequired = erpRegistrationApplicationForm.CreditLimitRequired,
                    IsApproved = erpRegistrationApplicationForm.IsApproved,
                    CreatedBy = "",//erpRegistrationApplicationForm.CreatedById,
                    UpdatedBy = "",//erpRegistrationApplicationForm.UpdatedById,
                    CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpRegistrationApplicationForm.CreatedOnUtc, DateTimeKind.Utc),
                    UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpRegistrationApplicationForm.UpdatedOnUtc, DateTimeKind.Utc)
                };

                //Additional Info
                var erpAccountCustomerRegistrationBankingDetails = (await _erpAccountCustomerRegistrationBankingDetailsService.GetErpAccountCustomerRegistrationBankingDetailsByFormIdAsync(erpRegistrationApplicationForm.Id))?.FirstOrDefault();
                if (erpAccountCustomerRegistrationBankingDetails != null)
                {
                    var erpBankingDetailsModel = new BankingDetailsModel();
                    erpBankingDetailsModel.FormId = erpAccountCustomerRegistrationBankingDetails.FormId;
                    erpBankingDetailsModel.NameOfBanker = erpAccountCustomerRegistrationBankingDetails.NameOfBanker;
                    erpBankingDetailsModel.AccountNumber = erpAccountCustomerRegistrationBankingDetails.AccountNumber;
                    erpBankingDetailsModel.AccountName = erpAccountCustomerRegistrationBankingDetails.AccountName;
                    erpBankingDetailsModel.BranchCode = erpAccountCustomerRegistrationBankingDetails.BranchCode;
                    erpBankingDetailsModel.Branch = erpAccountCustomerRegistrationBankingDetails.Branch;

                    erpRegistrationApplicationFormModel.BankingDetailsModel = erpBankingDetailsModel;
                }

                var erpAccountCustomerRegistrationPhysicalTradingAddress = (await _erpAccountCustomerRegistrationPhysicalTradingAddressService.GetErpAccountCustomerRegistrationPhysicalTradingAddressByFormIdAsync(erpRegistrationApplicationForm.Id))?.FirstOrDefault();
                if (erpAccountCustomerRegistrationPhysicalTradingAddress != null)
                {
                    var physicalTradingAddressModel = new AddressModel();
                    var physicalTradingAddress = await _addressService.GetAddressByIdAsync(erpAccountCustomerRegistrationPhysicalTradingAddress.PhysicalTradingAddressId);

                    if (physicalTradingAddress != null)
                        physicalTradingAddressModel = physicalTradingAddress.ToModel(physicalTradingAddressModel);

                    await _addressModelFactory.PrepareAddressModelAsync(physicalTradingAddressModel, physicalTradingAddress);

                    var erpPhysicalTradingAddressModel = new PhysicalTradingAddressModel();
                    erpPhysicalTradingAddressModel.FormId = erpAccountCustomerRegistrationPhysicalTradingAddress.FormId;
                    erpPhysicalTradingAddressModel.FullName = erpAccountCustomerRegistrationPhysicalTradingAddress.FullName;
                    erpPhysicalTradingAddressModel.Surname = erpAccountCustomerRegistrationPhysicalTradingAddress.Surname;
                    erpPhysicalTradingAddressModel.PhysicalTradingAddress = physicalTradingAddressModel;

                    erpRegistrationApplicationFormModel.PhysicalTradingAddressModel = erpPhysicalTradingAddressModel;
                }

                var erpAccountCustomerRegistrationPermises = (await _erpAccountCustomerRegistrationPremisesService.GetErpAccountCustomerRegistrationPremisesByFormIdAsync(erpRegistrationApplicationForm.Id))?.FirstOrDefault();
                if (erpAccountCustomerRegistrationPermises != null)
                {
                    var erpPremisesModel = new PremisesModel();
                    erpPremisesModel.FormId = erpAccountCustomerRegistrationPermises.FormId;
                    erpPremisesModel.OwnedOrLeased = erpAccountCustomerRegistrationPermises.OwnedOrLeased.ToString();
                    erpPremisesModel.NameOfLandlord = erpAccountCustomerRegistrationPermises.NameOfLandlord;
                    erpPremisesModel.AddressOfLandlord = erpAccountCustomerRegistrationPermises.AddressOfLandlord;
                    erpPremisesModel.EmailOfLandlord = erpAccountCustomerRegistrationPermises.EmailOfLandlord;
                    erpPremisesModel.TelephoneNumberOfLandlord = erpAccountCustomerRegistrationPermises.TelephoneNumberOfLandlord;

                    erpRegistrationApplicationFormModel.PremisesModel = erpPremisesModel;
                }

                var erpAccountCustomerRegistrationTradeReferences = (await _erpAccountCustomerRegistrationTradeReferencesService.GetErpAccountCustomerRegistrationTradeReferencesByFormIdAsync(erpRegistrationApplicationForm.Id))?.FirstOrDefault();
                if (erpAccountCustomerRegistrationTradeReferences != null)
                {
                    var erpTradeReferencesModel = new TradeReferencesModel();
                    erpTradeReferencesModel.FormId = erpAccountCustomerRegistrationTradeReferences.FormId;
                    erpTradeReferencesModel.Name = erpAccountCustomerRegistrationTradeReferences.Name;
                    erpTradeReferencesModel.Telephone = erpAccountCustomerRegistrationTradeReferences.Telephone;
                    erpTradeReferencesModel.Amount = erpAccountCustomerRegistrationTradeReferences.Amount;
                    erpTradeReferencesModel.Terms = erpAccountCustomerRegistrationTradeReferences.Terms;
                    erpTradeReferencesModel.HowLong = erpAccountCustomerRegistrationTradeReferences.HowLong;

                    erpRegistrationApplicationFormModel.TradeReferencesModel = erpTradeReferencesModel;
                }

                return erpRegistrationApplicationFormModel;
            });
        });

        return model;
    }

    public async Task<ApplicationFormModel> PrepareErpRegistrationApplicationModelAsync(ApplicationFormModel model, ErpAccountCustomerRegistrationForm erpAccountCustomerRegistrationForm)
    {
        if (erpAccountCustomerRegistrationForm == null)
        {
            return model;
        }
        else
        {
            var addressModel = new AddressModel();
            var address = await _addressService.GetAddressByIdAsync(erpAccountCustomerRegistrationForm.RegisteredOfficeAddressId);

            if (address != null)
                addressModel = address.ToModel(addressModel);
            await _addressModelFactory.PrepareAddressModelAsync(addressModel, address);

            //fill in model values from the entity
            model ??= new ApplicationFormModel();

            model.Id = erpAccountCustomerRegistrationForm.Id;
            model.FullRegisteredName = erpAccountCustomerRegistrationForm.FullRegisteredName;
            model.RegistrationNumber = erpAccountCustomerRegistrationForm.RegistrationNumber;
            model.VatNumber = erpAccountCustomerRegistrationForm.VatNumber;
            model.TelephoneNumber1 = erpAccountCustomerRegistrationForm.TelephoneNumber1;
            model.TelephoneNumber2 = erpAccountCustomerRegistrationForm.TelephoneNumber2;
            model.TelefaxNumber = erpAccountCustomerRegistrationForm.TelefaxNumber;
            model.AccountsContactPersonNameSurname = erpAccountCustomerRegistrationForm.AccountsContactPersonNameSurname;
            model.AccountsEmail = erpAccountCustomerRegistrationForm.AccountsEmail;
            model.AccountsTelephoneNumber = erpAccountCustomerRegistrationForm.AccountsTelephoneNumber;
            model.AccountsCellphoneNumber = erpAccountCustomerRegistrationForm.AccountsCellphoneNumber;
            model.BuyerContactPersonNameSurname = erpAccountCustomerRegistrationForm.BuyerContactPersonNameSurname;
            model.BuyerEmail = erpAccountCustomerRegistrationForm.BuyerEmail;
            model.NatureOfBusiness = erpAccountCustomerRegistrationForm.NatureOfBusiness;
            model.RegisteredOfficeAddress = addressModel;
            model.TypeOfBusiness = erpAccountCustomerRegistrationForm.TypeOfBusiness;
            model.EstimatePurchasesPerMonthZAR = erpAccountCustomerRegistrationForm.EstimatePurchasesPerMonthZAR;
            model.CreditLimitRequired = erpAccountCustomerRegistrationForm.CreditLimitRequired;
            model.IsApproved = erpAccountCustomerRegistrationForm.IsApproved;
            model.CreatedBy = "";//erpAccountCustomerRegistrationForm.CreatedById,
            model.UpdatedBy = "";//erpAccountCustomerRegistrationForm.UpdatedById,
            model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpAccountCustomerRegistrationForm.CreatedOnUtc, DateTimeKind.Utc);
            model.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpAccountCustomerRegistrationForm.UpdatedOnUtc, DateTimeKind.Utc);

            //Additional Info
            var erpAccountCustomerRegistrationBankingDetails = (await _erpAccountCustomerRegistrationBankingDetailsService.GetErpAccountCustomerRegistrationBankingDetailsByFormIdAsync(erpAccountCustomerRegistrationForm.Id))?.FirstOrDefault();
            if (erpAccountCustomerRegistrationBankingDetails != null)
            {
                var erpBankingDetailsModel = new BankingDetailsModel();
                erpBankingDetailsModel.FormId = erpAccountCustomerRegistrationBankingDetails.FormId;
                erpBankingDetailsModel.NameOfBanker = erpAccountCustomerRegistrationBankingDetails.NameOfBanker;
                erpBankingDetailsModel.AccountNumber = erpAccountCustomerRegistrationBankingDetails.AccountNumber;
                erpBankingDetailsModel.AccountName = erpAccountCustomerRegistrationBankingDetails.AccountName;
                erpBankingDetailsModel.BranchCode = erpAccountCustomerRegistrationBankingDetails.BranchCode;
                erpBankingDetailsModel.Branch = erpAccountCustomerRegistrationBankingDetails.Branch;

                model.BankingDetailsModel = erpBankingDetailsModel;
            }

            var erpAccountCustomerRegistrationPhysicalTradingAddress = (await _erpAccountCustomerRegistrationPhysicalTradingAddressService.GetErpAccountCustomerRegistrationPhysicalTradingAddressByFormIdAsync(erpAccountCustomerRegistrationForm.Id))?.FirstOrDefault();
            if (erpAccountCustomerRegistrationPhysicalTradingAddress != null)
            {
                var physicalTradingAddressModel = new AddressModel();
                var physicalTradingAddress = await _addressService.GetAddressByIdAsync(erpAccountCustomerRegistrationPhysicalTradingAddress.PhysicalTradingAddressId);

                if (physicalTradingAddress != null)
                    physicalTradingAddressModel = physicalTradingAddress.ToModel(physicalTradingAddressModel);

                await _addressModelFactory.PrepareAddressModelAsync(physicalTradingAddressModel, physicalTradingAddress);

                var erpPhysicalTradingAddressModel = new PhysicalTradingAddressModel();
                erpPhysicalTradingAddressModel.FormId = erpAccountCustomerRegistrationPhysicalTradingAddress.FormId;
                erpPhysicalTradingAddressModel.FullName = erpAccountCustomerRegistrationPhysicalTradingAddress.FullName;
                erpPhysicalTradingAddressModel.Surname = erpAccountCustomerRegistrationPhysicalTradingAddress.Surname;
                erpPhysicalTradingAddressModel.PhysicalTradingAddress = physicalTradingAddressModel;

                model.PhysicalTradingAddressModel = erpPhysicalTradingAddressModel;
            }

            var erpAccountCustomerRegistrationPermises = (await _erpAccountCustomerRegistrationPremisesService.GetErpAccountCustomerRegistrationPremisesByFormIdAsync(erpAccountCustomerRegistrationForm.Id))?.FirstOrDefault();
            if (erpAccountCustomerRegistrationPermises != null)
            {
                var erpPremisesModel = new PremisesModel();
                erpPremisesModel.FormId = erpAccountCustomerRegistrationPermises.FormId;
                erpPremisesModel.OwnedOrLeased = erpAccountCustomerRegistrationPermises.OwnedOrLeased.ToString();
                erpPremisesModel.NameOfLandlord = erpAccountCustomerRegistrationPermises.NameOfLandlord;
                erpPremisesModel.AddressOfLandlord = erpAccountCustomerRegistrationPermises.AddressOfLandlord;
                erpPremisesModel.EmailOfLandlord = erpAccountCustomerRegistrationPermises.EmailOfLandlord;
                erpPremisesModel.TelephoneNumberOfLandlord = erpAccountCustomerRegistrationPermises.TelephoneNumberOfLandlord;

                model.PremisesModel = erpPremisesModel;
            }

            var erpAccountCustomerRegistrationTradeReferences = (await _erpAccountCustomerRegistrationTradeReferencesService.GetErpAccountCustomerRegistrationTradeReferencesByFormIdAsync(erpAccountCustomerRegistrationForm.Id))?.FirstOrDefault();
            if (erpAccountCustomerRegistrationTradeReferences != null)
            {
                var erpTradeReferencesModel = new TradeReferencesModel();
                erpTradeReferencesModel.FormId = erpAccountCustomerRegistrationTradeReferences.FormId;
                erpTradeReferencesModel.Name = erpAccountCustomerRegistrationTradeReferences.Name;
                erpTradeReferencesModel.Telephone = erpAccountCustomerRegistrationTradeReferences.Telephone;
                erpTradeReferencesModel.Amount = erpAccountCustomerRegistrationTradeReferences.Amount;
                erpTradeReferencesModel.Terms = erpAccountCustomerRegistrationTradeReferences.Terms;
                erpTradeReferencesModel.HowLong = erpAccountCustomerRegistrationTradeReferences.HowLong;

                model.TradeReferencesModel = erpTradeReferencesModel;
            }

            return model;
        }
    }

    #endregion
}
