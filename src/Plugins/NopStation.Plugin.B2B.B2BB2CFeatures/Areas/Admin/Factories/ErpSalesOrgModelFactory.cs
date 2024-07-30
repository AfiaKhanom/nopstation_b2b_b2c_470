using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Services.Common;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories
{
    public class ErpSalesOrgModelFactory : IErpSalesOrgModelFactory
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IAddressService _addressService;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly AddressSettings _addressSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IErpSalesOrgService _erpSalesOrgService;
        private readonly IErpWarehouseSalesOrgMapService _erpWarehouseSalesOrgMapService;
        private readonly IShippingService _shippingService;
        private readonly IErpWarehouseAdditionalDataService _erpWarehouseAdditionalDataService;

        #endregion

        #region Ctor

        public ErpSalesOrgModelFactory(IWorkContext workContext,
            IBaseAdminModelFactory baseAdminModelFactory,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            IAddressService addressService,
            IAddressModelFactory addressModelFactory,
            AddressSettings addressSettings,
            IErpSalesOrgService erpSalesOrgService,
            IErpWarehouseSalesOrgMapService erpWarehouseSalesOrgMapService,
            IShippingService shippingService,
            IErpWarehouseAdditionalDataService erpWarehouseAdditionalDataService
            )
        {
            _workContext = workContext;
            _baseAdminModelFactory = baseAdminModelFactory;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _addressService = addressService;
            _addressModelFactory = addressModelFactory;
            _addressSettings = addressSettings;
            _erpSalesOrgService = erpSalesOrgService;
            _erpWarehouseSalesOrgMapService = erpWarehouseSalesOrgMapService;
            _shippingService = shippingService;
            _erpWarehouseAdditionalDataService = erpWarehouseAdditionalDataService;
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

        public async Task<ErpSalesOrgSearchModel> PrepareErpSalesOrgSearchModelAsync(ErpSalesOrgSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare "active" filter (0 - all; 1 - active only; 2 - inactive only)
            searchModel.ShowInActiveOption.Add(new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrgSearchModel.ShowAll"),
            });
            searchModel.ShowInActiveOption.Add(new SelectListItem
            {
                Value = "1",
                Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrgSearchModel.ShowOnlyActive"),
            });
            searchModel.ShowInActiveOption.Add(new SelectListItem
            {
                Value = "2",
                Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrgSearchModel.ShowOnlyInactive"),
            });

            //prepare grid
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public async Task<ErpSalesOrgListModel> PrepareErpSalesOrgListModelAsync(ErpSalesOrgSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get ERP Sales Orgs
            var erpSalesOrgs = await _erpSalesOrgService.GetAllErpSalesOrgAsync(pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize,
                name: searchModel.Name,
                email: searchModel.Email,
                code: searchModel.Code,
                showHidden: searchModel.ShowInActive == 0 ? null : (bool?)(searchModel.ShowInActive == 2));

            //prepare list model
            var model = await new ErpSalesOrgListModel().PrepareToGridAsync(searchModel, erpSalesOrgs, () =>
            {
                //fill in model values from the entity
                return erpSalesOrgs.SelectAwait(async erpSalesOrg =>
                {
                    var erpSalesOrgModel = new ErpSalesOrgModel();
                    //Get addionalInfos
                    //var erpSalesOrgInfo = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpSalesOrg.Id);

                    var currentCulture = (await _workContext.GetWorkingLanguageAsync()).LanguageCulture;
                    var dtfi = new CultureInfo(currentCulture, false).DateTimeFormat;

                    //Additional Infos
                    if (erpSalesOrg != null)
                    {
                        //prepare address model
                        var address = await _addressService.GetAddressByIdAsync(erpSalesOrg?.AddressId ?? 0);
                        var addressModel = new AddressModel();
                        if (address != null)
                            addressModel = address.ToModel(addressModel);
                        await _addressModelFactory.PrepareAddressModelAsync(addressModel, address);

                        erpSalesOrgModel = new ErpSalesOrgModel
                        {
                            Id = erpSalesOrg.Id,
                            Name = erpSalesOrg.Name,
                            Code = erpSalesOrg.Code,
                            Email = erpSalesOrg.Email,
                            AddressId = erpSalesOrg.AddressId,
                            Address = addressModel,
                            IntegrationClientId = erpSalesOrg.IntegrationClientId,
                            AuthenticationKey = erpSalesOrg.AuthenticationKey,
                            CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpSalesOrg.CreatedOnUtc, DateTimeKind.Utc),
                            UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpSalesOrg.UpdatedOnUtc, DateTimeKind.Utc),
                            IsActive = erpSalesOrg.IsActive,
                        };
                    }

                    return erpSalesOrgModel;
                });
            });

            return model;
        }

        public async Task<ErpSalesOrgModel> PrepareErpSalesOrgModelAsync(ErpSalesOrgModel model, ErpSalesOrg erpSalesOrg)
        {
            if (erpSalesOrg != null)
            {
                //fill in model values from the entity
                model ??= new ErpSalesOrgModel();

                model.Id = erpSalesOrg.Id;
                model.Name = erpSalesOrg.Name;
                model.Code = erpSalesOrg.Code;
                model.Email = erpSalesOrg.Email;
                model.AddressId = erpSalesOrg.AddressId;
                model.IntegrationClientId = erpSalesOrg.IntegrationClientId;
                model.AuthenticationKey = erpSalesOrg.AuthenticationKey;
                model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpSalesOrg.CreatedOnUtc, DateTimeKind.Utc);
                model.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpSalesOrg.UpdatedOnUtc, DateTimeKind.Utc);
                model.IsActive = erpSalesOrg.IsActive;
                model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpSalesOrg.CreatedOnUtc, DateTimeKind.Utc);
                model.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpSalesOrg.UpdatedOnUtc, DateTimeKind.Utc);
                model.IsActive = erpSalesOrg.IsActive;

            }

            //prepare address model
            var address = await _addressService.GetAddressByIdAsync(erpSalesOrg?.AddressId ?? 0);
            var addressModel = new AddressModel();
            if (address != null)
                addressModel = address.ToModel(addressModel);

            await _addressModelFactory.PrepareAddressModelAsync(addressModel, address);
            SetAddressFieldsAsRequired(addressModel);
            model.Address = addressModel;

            // prepare nested model
            await _baseAdminModelFactory.PrepareWarehousesAsync(model.AddErpSalesOrgWarehouseModel.AvailableWarehouses, true, "Select");
            model.AddErpSalesOrgWarehouseModel.Id = model.Id;
            model.AddErpSalesOrgWarehouseModel.ErpSalesOrgId = model.Id;
            model.ErpSalesOrgWarehouseSearchModel.ErpSalesOrgId = model.Id;

            return model;
        }

        #region Warehouse

        public async Task<ErpSalesOrgWarehouseListModel> PrepareErpSalesOrgWarehouseListModel(ErpSalesOrgWarehouseSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var erpSalesOrgWarehouses = await _erpWarehouseSalesOrgMapService.GetAllErpWarehouseSalesOrgMapsAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize, salesOrgId: searchModel.ErpSalesOrgId);
            var model = await new ErpSalesOrgWarehouseListModel().PrepareToGridAsync(searchModel, erpSalesOrgWarehouses, () =>
            {
                return erpSalesOrgWarehouses.SelectAwait(async saleOrgWarehouse =>
                {
                    var warehouse = await _shippingService.GetWarehouseByIdAsync(saleOrgWarehouse.NopWarehouseId);
                    var erpWarehouse = await _erpWarehouseAdditionalDataService.GetErpWarehouseAdditionalDataByIdAsync(saleOrgWarehouse.ErpWarehouseId);
                    //fill in model values from the entity
                    var salesOrgWarehouseModel = new ErpSalesOrgWarehouseModel
                    {
                        Id = saleOrgWarehouse.Id,
                        WarehouseId = saleOrgWarehouse.NopWarehouseId,
                        WarehouseName = warehouse?.Name,
                        ErpSalesOrgId = saleOrgWarehouse.ErpSalesOrgId,
                        ErpWarehouseCode = erpWarehouse.Code,
                        LastUpdateTime = (await _dateTimeHelper.ConvertToUserTimeAsync(erpWarehouse.LastUpdateTime, DateTimeKind.Utc)).ToString(),
                    };
                    return salesOrgWarehouseModel;
                });
            });
            return model;
        }

        #endregion

        #endregion
    }
}
