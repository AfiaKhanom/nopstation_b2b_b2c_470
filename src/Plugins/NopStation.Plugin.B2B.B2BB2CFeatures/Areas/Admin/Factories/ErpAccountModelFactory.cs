using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Common;
using Nop.Services;
using Nop.Services.Common;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public class ErpAccountModelFactory : IErpAccountModelFactory
{
    #region Fields

    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IAddressService _addressService;
    private readonly IAddressModelFactory _addressModelFactory;
    private readonly AddressSettings _addressSettings;
    private readonly ILocalizationService _localizationService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpSalesOrgModelFactory _erpSalesOrgModelFactory;
    private readonly IErpGroupPriceCodeService _erpGroupPriceCodeService;
    private readonly IErpNopUserModelFactory _erpNopUserModelFactory;
    private readonly IErpShipToAddressModelFactory _erpShipToAddressModelFactory;
    private readonly IErpNopUserAccountMapService _erpNopUserAccountMapService;
    private readonly IErpShipToAddressService _erpShipToAddressService;

    #endregion

    #region Ctor

    public ErpAccountModelFactory(IDateTimeHelper dateTimeHelper,
        IAddressService addressService,
        IAddressModelFactory addressModelFactory,
        AddressSettings addressSettings,
        ILocalizationService localizationService,
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService,
        IErpSalesOrgModelFactory erpSalesOrgModelFactory,
        IErpGroupPriceCodeService erpGroupPriceCodeService,
        IErpNopUserModelFactory erpNopUserModelFactory,
        IErpShipToAddressModelFactory erpShipToAddressModelFactory,
        IErpNopUserAccountMapService erpNopUserAccountMapService,
        IErpShipToAddressService erpShipToAddressService)
    {
        _localizationService = localizationService;
        _dateTimeHelper = dateTimeHelper;
        _addressService = addressService;
        _addressModelFactory = addressModelFactory;
        _addressSettings = addressSettings;
        _erpAccountService = erpAccountService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpSalesOrgModelFactory = erpSalesOrgModelFactory;
        _erpGroupPriceCodeService = erpGroupPriceCodeService;
        _erpNopUserModelFactory = erpNopUserModelFactory;
        _erpShipToAddressModelFactory = erpShipToAddressModelFactory;
        _erpNopUserAccountMapService = erpNopUserAccountMapService;
        _erpShipToAddressService = erpShipToAddressService;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Set some address fields as required
    /// </summary>
    /// <param name="model">Address model</param>
    protected virtual void SetAddressFieldsAsRequired(AddressModel model)
    {
        model.FirstNameRequired = true;
        model.EmailRequired = true;
        model.CountryRequired = true;
        model.CityRequired = true;
        model.PhoneRequired = true;
        model.ZipPostalCodeRequired = true;

        model.CompanyRequired = _addressSettings.CompanyRequired;
        model.CountyRequired = _addressSettings.CountyRequired;
        model.StreetAddressRequired = _addressSettings.StreetAddressRequired;
        model.StreetAddress2Required = _addressSettings.StreetAddress2Required;
        model.FaxRequired = _addressSettings.FaxRequired;
    }

    #endregion

    #region Method

    public async Task<ErpAccountSearchModel> PrepareErpAccountSearchModelAsync(ErpAccountSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        // Prepare ErpAccountStatusTypes dropdown options
        var availableErpAccountStatusTypes = await ErpAccountStatusType.Normal.ToSelectListAsync(false);
        foreach (var types in availableErpAccountStatusTypes)
        {
            searchModel.ErpAccountStatusTypes.Add(types);
        }
        searchModel.ErpAccountStatusTypes.Insert(0, new SelectListItem
        {
            Value = "0",
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Select")
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
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Select")
        });

        //prepare "active" filter (0 - all; 1 - active only; 2 - inactive only)
        searchModel.ShowInActiveOption.Add(new SelectListItem
        {
            Value = "0",
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccountSearchModel.ShowAll"),
        });
        searchModel.ShowInActiveOption.Add(new SelectListItem
        {
            Value = "1",
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccountSearchModel.ShowOnlyActive"),
        });
        searchModel.ShowInActiveOption.Add(new SelectListItem
        {
            Value = "2",
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccountSearchModel.ShowOnlyInactive"),
        });

        //prepare grid
        searchModel.SetGridPageSize();

        return searchModel;
    }

    public async Task<ErpAccountListModel> PrepareErpAccountListModelAsync(ErpAccountSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var erpAccounts = await _erpAccountService.GetAllErpAccountsAsync(
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize,
            showHidden: searchModel.ShowInActive == 0 ? null : (searchModel.ShowInActive == 2),
            erpAccountNo: searchModel.AccountNumber,
            salesOrgId: searchModel.ErpSalesOrgId,
            email: searchModel.Email,
            accountName: searchModel.AccountName,
            erpAccountStatusTypeId: searchModel.ErpAccountStatusTypeId);

        //prepare list model
        var model = await new ErpAccountListModel().PrepareToGridAsync(searchModel, erpAccounts, () =>
        {
            //fill in model values from the entity
            return erpAccounts.SelectAwait(async erpAccount =>
            {
                //prepare address model
                var address = await _addressService.GetAddressByIdAsync(erpAccount.BillingAddressId ?? 0);

                var addressModel = new AddressModel();
                if (address != null)
                    addressModel = address.ToModel(addressModel);
                await _addressModelFactory.PrepareAddressModelAsync(addressModel, address);

                //fill in model values from the entity
                var erpAccountModel = new ErpAccountModel
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
                    IsActive = erpAccount.IsActive
                };

                var erpAccountSalesOrgInfo = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId);
                if (erpAccountSalesOrgInfo != null)
                {
                    var erpAccountSalesOrgInfoModel = new ErpSalesOrgModel();

                    erpAccountModel.ErpSalesOrgModel = await _erpSalesOrgModelFactory.PrepareErpSalesOrgModelAsync(erpAccountSalesOrgInfoModel, erpAccountSalesOrgInfo);
                    erpAccountModel.ErpSalesOrgName = erpAccountSalesOrgInfo.Name;
                }
                erpAccountModel.ERPNopUserCount = (await _erpNopUserAccountMapService.GetAllErpNopUserAccountMapsByAccountIdAsync(erpAccount.Id)).Count;
                erpAccountModel.ShipToAddressCount = (await _erpShipToAddressService.GetErpShipToAddressesByErpAccountIdAsync(erpAccount.Id)).Count;
                return erpAccountModel;
            });
        });

        return model;
    }

    public async Task<ErpAccountModel> PrepareErpAccountModelAsync(ErpAccountModel model, ErpAccount erpAccount)
    {

        if (erpAccount == null)
        {
            model.AvailableErpSalesOrgs = (await _erpSalesOrgService.GetAllErpSalesOrgAsync(showHidden: false))
              .Select(erpSalesOrg => new SelectListItem
              {
                  Value = erpSalesOrg.Id.ToString(),
                  Text = erpSalesOrg.Name.ToString(),
              }).ToList();

            // Prepare B2BPriceGroupCodes dropdown options
            model.AvailableB2BPriceGroupCodes = (await _erpGroupPriceCodeService.GetAllErpGroupPriceCodesAsync())
                .Select(erpGroupPrice => new SelectListItem
                {
                    Value = erpGroupPrice.Id.ToString(),
                    Text = erpGroupPrice.Code.ToString()
                }).ToList();

            model.AvailableB2BPriceGroupCodes.Insert(0, new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Select")
            });

            // Prepare ErpAccountStatusTypes dropdown options
            var availableErpAccountStatusTypes = await ErpAccountStatusType.Normal.ToSelectListAsync(false);
            foreach (var types in availableErpAccountStatusTypes)
            {
                model.ErpAccountStatusTypes.Add(types);
            }
            model.ErpAccountStatusTypes.Insert(0, new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Select")
            });

            model.IsActive = true;

            await _addressModelFactory.PrepareAddressModelAsync(model.BillingAddress, null);
            return model;
        }
        else
        {
            //fill in model values from the entity
            model ??= new ErpAccountModel();

            model.Id = erpAccount.Id;
            model.AccountNumber = erpAccount.AccountNumber;
            model.AccountName = erpAccount.AccountName;
            model.VatNumber = erpAccount.VatNumber;
            model.CurrentBalance = erpAccount.CurrentBalance;
            model.ErpSalesOrgId = erpAccount.ErpSalesOrgId;
            model.BillingAddressId = erpAccount.BillingAddressId;
            model.BillingSuburb = erpAccount.BillingSuburb;
            model.CreditLimit = erpAccount.CreditLimit;
            model.CreditLimitAvailable = erpAccount.CreditLimitAvailable;
            model.LastPaymentAmount = erpAccount.LastPaymentAmount;
            model.LastPaymentDate = erpAccount.LastPaymentDate;
            model.AllowOverspend = erpAccount.AllowOverspend;
            model.PreFilterFacets = erpAccount.PreFilterFacets;
            model.PaymentTypeCode = erpAccount.PaymentTypeCode;
            model.OverrideAddressEditOnCheckoutConfigSetting = erpAccount.OverrideAddressEditOnCheckoutConfigSetting;
            model.OverrideBackOrderingConfigSetting = erpAccount.OverrideBackOrderingConfigSetting;
            model.AllowAccountsAddressEditOnCheckout = erpAccount.AllowAccountsAddressEditOnCheckout;
            model.AllowAccountsBackOrdering = erpAccount.AllowAccountsBackOrdering;
            model.OverrideStockDisplayFormatConfigSetting = erpAccount.OverrideStockDisplayFormatConfigSetting;
            model.ErpAccountStatusTypeId = erpAccount.ErpAccountStatusTypeId;
            model.ErpAccountStatusType = ((ErpAccountStatusType)erpAccount.ErpAccountStatusTypeId).ToString();
            model.LastErpAccountSyncDate = erpAccount.LastErpAccountSyncDate;
            model.B2BPriceGroupCodeId = erpAccount.B2BPriceGroupCodeId;
            model.TotalSavingsForthisYear = erpAccount.TotalSavingsForthisYear ?? 0;
            model.TotalSavingsForAllTime = erpAccount.TotalSavingsForAllTime ?? 0;
            model.TotalSavingsForAllTimeUpdatedOnUtc = erpAccount.TotalSavingsForAllTimeUpdatedOnUtc;
            model.TotalSavingsForthisYearUpdatedOnUtc = erpAccount.TotalSavingsForthisYearUpdatedOnUtc;
            model.LastTimeOrderSyncOnUtc = erpAccount.LastTimeOrderSyncOnUtc;
            model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpAccount.CreatedOnUtc, DateTimeKind.Utc);
            model.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpAccount.UpdatedOnUtc, DateTimeKind.Utc);
            model.IsActive = erpAccount.IsActive;

            //Additional Info
            var erpAccountSalesOrgInfo = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId);
            if (erpAccountSalesOrgInfo != null)
            {
                var erpAccountSalesOrgInfoModel = new ErpSalesOrgModel();

                model.ErpSalesOrgModel = await _erpSalesOrgModelFactory.PrepareErpSalesOrgModelAsync(erpAccountSalesOrgInfoModel, erpAccountSalesOrgInfo);
                model.ErpSalesOrgName = model.ErpSalesOrgModel.Name;
            }

            // Prepare ErpSalesOrgs dropdown options
            model.AvailableErpSalesOrgs = (await _erpSalesOrgService.GetAllErpSalesOrgAsync(showHidden: false))
            .Select(erpSalesOrg => new SelectListItem
            {
                Value = erpSalesOrg.Id.ToString(),
                Text = erpSalesOrg.Name.ToString()
            }).ToList();

            // Prepare B2BPriceGroupCodes dropdown options
            model.AvailableB2BPriceGroupCodes = (await _erpGroupPriceCodeService.GetAllErpGroupPriceCodesAsync())
            .Select(erpGroupPrice => new SelectListItem
            {
                Value = erpGroupPrice.Id.ToString(),
                Text = erpGroupPrice.Code.ToString()
            }).ToList();

            model.AvailableB2BPriceGroupCodes.Insert(0, new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Select")
            });

            // Prepare ErpAccountStatusTypes dropdown options
            var availableErpAccountStatusTypes = await ErpAccountStatusType.Normal.ToSelectListAsync(false);
            foreach (var types in availableErpAccountStatusTypes)
            {
                model.ErpAccountStatusTypes.Add(types);
            }
            model.ErpAccountStatusTypes.Insert(0, new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Select")
            });

            //prepare address model
            var address = await _addressService.GetAddressByIdAsync(erpAccount.BillingAddressId ?? 0);
            var addressModel = new AddressModel();
            if (address != null)
                addressModel = address.ToModel(addressModel);

            await _addressModelFactory.PrepareAddressModelAsync(addressModel, address);
            SetAddressFieldsAsRequired(addressModel);
            model.BillingAddress = addressModel;

            //prepare nop user search Model
            model.ErpNopUserSearchModel.AccountId = model.Id;
            model.ErpNopUserSearchModel = await _erpNopUserModelFactory.PrepareErpNopUserSearchModelAsync(searchModel: model.ErpNopUserSearchModel);

            //prepare erp ship to address search model
            model.ErpShipToAddressSearchModel.SearchErpAccountId = model.Id;
            model.ErpShipToAddressSearchModel = await _erpShipToAddressModelFactory.PrepareErpShipToAddressSearchModelAsync(searchModel: model.ErpShipToAddressSearchModel);

            return model;
        }
    }

    #endregion
}
