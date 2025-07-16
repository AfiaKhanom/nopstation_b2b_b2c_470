using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Common;
using Nop.Services.Common;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpShipToAddress;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public class ErpShipToAddressModelFactory : IErpShipToAddressModelFactory
{
    #region Fields

    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly IAddressService _addressService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IAddressModelFactory _addressModelFactory;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly AddressSettings _addressSettings;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public ErpShipToAddressModelFactory(IErpShipToAddressService erpShipToAddressService,
        IAddressService addressService,
        IErpAccountService erpAccountService,
        IAddressModelFactory addressModelFactory,
        IDateTimeHelper dateTimeHelper,
        AddressSettings addressSettings,
        IErpSalesOrgService erpSalesOrgService,
        ILocalizationService localizationService)
    {
        _erpShipToAddressService = erpShipToAddressService;
        _addressService = addressService;
        _erpAccountService = erpAccountService;
        _addressModelFactory = addressModelFactory;
        _dateTimeHelper = dateTimeHelper;
        _addressSettings = addressSettings;
        _erpSalesOrgService = erpSalesOrgService;
        _localizationService = localizationService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Prepare erpShipToAddress search model
    /// </summary>
    /// <param name="searchModel">ErpShipToAddress search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the erpShipToAddress search model
    /// </returns>
    public virtual async Task<ErpShipToAddressSearchModel> PrepareErpShipToAddressSearchModelAsync(ErpShipToAddressSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

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

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare paged erpShipToAddress list model
    /// </summary>
    /// <param name="searchModel">ErpShipToAddress search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the erpShipToAddress list model
    /// </returns>
    public virtual async Task<ErpShipToAddressListModel> PrepareErpShipToAddressListModelAsync(ErpShipToAddressSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //get erpShipToAddresses
        var erpShipToAddresses = await _erpShipToAddressService.GetAllErpShipToAddressesAsync(shipToCode: searchModel.SearchShipToCode,
            shipToName: searchModel.SearchShipToName,
            erpAccountId: searchModel.SearchErpAccountId,
            repNum: searchModel.SearchRepNumber,
            repFullName: searchModel.SearchRepFullName,
            repEmail: searchModel.SearchRepEmail,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize,
            showHidden: searchModel.ShowInActive == 0 ? null : searchModel.ShowInActive == 2,
            emailAddresses: searchModel.SearchEmailAddresses);

        //prepare list model
        var model = await new ErpShipToAddressListModel().PrepareToGridAsync(searchModel, erpShipToAddresses, () =>
        {
            //fill in model values from the entity
            return erpShipToAddresses.SelectAwait(async erpShipToAddress =>
            {

                var data = erpShipToAddress.ToModel<ErpShipToAddressModel>();
                var erpAccount = await _erpAccountService.GetErpAccountByErpShipToAddressAsync(erpShipToAddress);

                if (erpAccount != null)
                {
                    data.ErpAccount = $"{erpAccount.AccountName} ({erpAccount.AccountNumber})";
                    var erpAccountSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId);
                    if (erpAccountSalesOrg != null)
                    {
                        data.ErpAccountSalesOrgId = erpAccountSalesOrg.Id;
                        if (!string.IsNullOrEmpty(erpAccountSalesOrg.Code))
                            data.ErpAccountSalesOrgName = $"{erpAccountSalesOrg.Name} ({erpAccountSalesOrg.Code})";
                        else
                            data.ErpAccountSalesOrgName = erpAccountSalesOrg.Name;
                    }
                }
                else
                {
                    data.IsDeletedErpAccount = true;
                }

                return data;

            });
        });

        return model;
    }

    /// <summary>
    /// Prepare erpShipToAddress model
    /// </summary>
    /// <param name="model">ErpShipToAddress model</param>
    /// <param name="erpShipToAddress">ErpShipToAddress</param>
    /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the erpShipToAddress model
    /// </returns>
    public virtual async Task<ErpShipToAddressModel> PrepareErpShipToAddressModelAsync(ErpShipToAddressModel model, ErpShipToAddress erpShipToAddress, bool excludeProperties = false)
    {
        var address = new Address();
        var addressModel = new AddressModel();

        if (erpShipToAddress != null)
        {
            //fill in model values from the entity
            model ??= erpShipToAddress.ToModel<ErpShipToAddressModel>();
            var erpShipToAddressAccountMap = await _erpShipToAddressService.GetErpShipToAddressErpAccountMapByErpShipToAddressIdAsync(erpShipToAddress.Id);
            model.ErpAccountId = erpShipToAddressAccountMap?.ErpAccountId ?? 0;

            var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(model.ErpAccountId);
            model.ErpAccount = erpAccount != null ? $"{erpAccount.AccountName} ({erpAccount.AccountNumber})" : "";
            model.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(erpShipToAddress.CreatedOnUtc, DateTimeKind.Utc);
            model.UpdatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(erpShipToAddress.UpdatedOnUtc, DateTimeKind.Utc);
            model.LastShipToAddressSyncDate = await _dateTimeHelper.ConvertToUserTimeAsync(erpShipToAddress.LastShipToAddressSyncDate ?? DateTime.MinValue, DateTimeKind.Utc);
            //Address model field requirements add
            if (model.AddressId > 0)
            {
                address = await _addressService.GetAddressByIdAsync(model.AddressId);
                //prepare address model
                if (address != null)
                    addressModel = address.ToModel(addressModel);
            }
            else
            {
                addressModel = address.ToModel<AddressModel>();
            }
        }

        await _addressModelFactory.PrepareAddressModelAsync(addressModel, address);

        addressModel.FirstNameRequired = true;
        addressModel.EmailRequired = true;
        addressModel.CountryRequired = true;
        addressModel.CityRequired = true;
        addressModel.PhoneRequired = true;
        addressModel.ZipPostalCodeRequired = true;

        addressModel.CompanyRequired = _addressSettings.CompanyRequired;
        addressModel.CountyRequired = _addressSettings.CountyRequired;
        addressModel.StreetAddressRequired = _addressSettings.StreetAddressRequired;
        addressModel.StreetAddress2Required = _addressSettings.StreetAddress2Required;
        addressModel.FaxRequired = _addressSettings.FaxRequired;

        model.AddressModel = addressModel;
        model.IsActive = erpShipToAddress is null || erpShipToAddress.IsActive;

        return model;
    }

    #endregion
}
