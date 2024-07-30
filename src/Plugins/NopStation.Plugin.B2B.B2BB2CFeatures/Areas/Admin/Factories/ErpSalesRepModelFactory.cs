using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpSalesRep;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories
{
    public class ErpSalesRepModelFactory : IErpSalesRepModelFactory
    {
        #region Fields

        private readonly IErpSalesRepService _erpSalesRepService;
        private readonly IErpSalesOrgService _erpSalesOrgService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IErpSalesRepSalesOrgMapService _erpSalesRepSalesOrgMapService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IErpNopUserService _erpNopUserService;

        #endregion

        #region ctor

        public ErpSalesRepModelFactory(
            IErpSalesRepService erpSalesRepService,
            IErpSalesOrgService erpSalesOrgService,
            ICustomerService customerService,
            ILocalizationService localizationService,
            IErpSalesRepSalesOrgMapService erpSalesRepSalesOrgMapService,
            IDateTimeHelper dateTimeHelper,
            IErpNopUserService erpNopUserService)
        {
            _erpSalesRepService = erpSalesRepService;
            _erpSalesOrgService = erpSalesOrgService;
            _customerService = customerService;
            _localizationService = localizationService;
            _erpSalesRepSalesOrgMapService = erpSalesRepSalesOrgMapService;
            _dateTimeHelper = dateTimeHelper;
            _erpNopUserService = erpNopUserService;
        }

        #endregion

        #region Utilities

        public async Task PrepareAvailableCustomersAsync(IList<SelectListItem> model, int includeSalesRepCustomerId = 0)
        {
            // Prepare NopCustomers dropdown options
            var customerRoleId = (await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName)).Id;
            var customerIdsWithOnlyRegisteredRole = await _erpNopUserService.GetAllCustomersByOnlyTheseRoleIdsAsync(customerRoleId);

            var existingNopCustomer = (await _erpNopUserService.GetAllErpNopUsersAsync()).Select(s => s.NopCustomerId)?.ToList();
            if (customerIdsWithOnlyRegisteredRole.Count > 0)
            {
                model = (await _customerService.GetCustomersByIdsAsync(customerIdsWithOnlyRegisteredRole.ToArray())).Where(w => !existingNopCustomer.Contains(w.Id))
                .Select(nopCustomer => new SelectListItem
                {
                    Value = nopCustomer.Id.ToString(),
                    Text = nopCustomer.FirstName + " " + nopCustomer.LastName + nopCustomer.Email,
                }).ToList();
            }

            model.Add(new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.Select"),
                Value = "0"
            });
        }

        public async Task PrepareAvailableSalesRepsAsync(IList<SelectListItem> model)
        {
            var customers = await _erpSalesRepService.GetAllSalesRepCustomersAsync();

            model.Add(new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.Select"),
                Value = "0"
            });

            foreach (var customer in customers)
            {
                model.Add(new SelectListItem
                {
                    Text = customer.Email,
                    Value = customer.Id.ToString()
                });
            }
        }

        public async Task PrepareAvailableSalesRepTypeAsync(IList<SelectListItem> model)
        {
            // Prepare SalesRepTypes dropdown options
            var availableSalesRepTypes = await SalesRepType.AllUsers.ToSelectListAsync(false);
            foreach (var types in availableSalesRepTypes)
            {
                model.Add(types);
            }
            model.Insert(0, new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Admin.Common.Select"),
            });
        }

        public async Task PrepareAvailableSalesOrgsAsync(IList<SelectListItem> model)
        {

            var salesOrgs = await _erpSalesOrgService.GetAllErpSalesOrgsAsync();

            foreach (var salesOrg in salesOrgs)
            {
                model.Add(new SelectListItem
                {
                    Text = salesOrg.Name,
                    Value = salesOrg.Id.ToString()
                });
            }
        }

        #endregion

        #region Method

        public async Task<ErpSalesRepModel> PrepareErpSalesRepModelAsync(ErpSalesRepModel model, ErpSalesRep erpSalesRep)
        {
            if (erpSalesRep != null)
            {
                model = erpSalesRep.ToModel<ErpSalesRepModel>();
                model.NopCustomerId = erpSalesRep.NopCustomerId;
                model.SalesRepTypeId = erpSalesRep.SalesRepTypeId;
                model.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(erpSalesRep.CreatedOnUtc, DateTimeKind.Utc);
                model.UpdatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(erpSalesRep.UpdatedOnUtc, DateTimeKind.Utc);

                var salesOrgMaps = await _erpSalesRepSalesOrgMapService.GetErpSalesRepSalesOrgMapsByErpSalesRepIdAsync(erpSalesRep.Id);
                if (salesOrgMaps.Any())
                {
                    model.SalesOrgIds = salesOrgMaps.Select(x => x.ErpSalesOrgId).ToList();
                }
            }
            model.NopCustomer = model.NopCustomerId > 0 ? (await _customerService.GetCustomerByIdAsync(model.NopCustomerId))?.Email ?? "" : "";
            model.IsActive = erpSalesRep == null || erpSalesRep.IsActive;

            #region Prepare NopCustomers dropdown options

            var customerRoleId = (await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName)).Id;
            var customerIdsWithOnlyRegisteredRole = await _erpNopUserService.GetAllCustomersByOnlyTheseRoleIdsAsync(customerRoleId);

            var existingNopCustomer = (await _erpNopUserService.GetAllErpNopUsersAsync()).Select(s => s.NopCustomerId)?.ToList();
            if (customerIdsWithOnlyRegisteredRole.Count > 0)
            {
                model.AvailableCustomers = (await _customerService.GetCustomersByIdsAsync(customerIdsWithOnlyRegisteredRole.ToArray())).Where(w => !existingNopCustomer.Contains(w.Id))
                .Select(nopCustomer => new SelectListItem
                {
                    Value = nopCustomer.Id.ToString(),
                    Text = nopCustomer.FirstName + " " + nopCustomer.LastName + " (" + nopCustomer.Email + ")",
                }).ToList();
            }

            model.AvailableCustomers.Insert(0, new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.Select"),
                Value = "0"
            });

            #endregion

            await PrepareAvailableSalesRepTypeAsync(model.AvailableSalesRepType);
            await PrepareAvailableSalesOrgsAsync(model.AvailableSalesOrgs);

            model.ErpAccountSearchModel.SetGridPageSize();

            return model;
        }

        public async Task<ErpSalesRepSearchModel> PrepareErpSalesRepSearchModelAsync(ErpSalesRepSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            await PrepareAvailableSalesRepsAsync(searchModel.AvailableSalesReps);
            await PrepareAvailableSalesRepTypeAsync(searchModel.AvailableSalesRepType);
            await PrepareAvailableSalesOrgsAsync(searchModel.AvailableSalesOrgs);

            //prepare grid
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public async Task<ErpSalesRepListModel> PrepareErpSalesRepListModelAsync(ErpSalesRepSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get ERP Accounts
            var erpSalesReps = await _erpSalesRepService.GetAllErpSalesRepAsync(
                nopCustomerId: searchModel.NopCustomerId,
                salesRepTypeId: searchModel.SalesRepTypeId,
                erpSalesOrgIds: searchModel.SelectedSalesOrgIds.ToArray(),
                showHidden: true,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new ErpSalesRepListModel().PrepareToGridAsync(searchModel, erpSalesReps, () =>
            {
                //fill in model values from the entity
                return erpSalesReps.SelectAwait(async erpSalesRep =>
                {
                    //fill in model values from the entity
                    var erpSalesRepModel = erpSalesRep.ToModel<ErpSalesRepModel>();
                    var customer = await _customerService.GetCustomerByIdAsync(erpSalesRep.NopCustomerId);

                    erpSalesRepModel.NopCustomer = customer?.Email;

                    erpSalesRepModel.SalesRepType = CommonHelper.ConvertEnum(((SalesRepType)erpSalesRep.SalesRepTypeId).ToString());

                    //fill in additional values (not existing in the entity)
                    erpSalesRepModel.CustomerRoleNames = string.Join(", ",
                        (await _customerService.GetCustomerRolesAsync(customer)).Select(role => role.Name));

                    erpSalesRepModel.CommaSeparatedOrgNames = string.Join(", ",
                        (await _erpSalesRepService.GetSalesRepOrgsAsync(erpSalesRep.Id)).Select(org => org.Name));

                    return erpSalesRepModel;
                });
            });

            return model;
        }

        #endregion
    }
}