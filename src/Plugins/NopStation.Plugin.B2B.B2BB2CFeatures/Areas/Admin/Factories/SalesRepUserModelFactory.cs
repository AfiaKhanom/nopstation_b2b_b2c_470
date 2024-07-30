using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpNopUser;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories
{
    public class SalesRepUserModelFactory : ISalesRepUserModelFactory
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IErpAccountService _erpAccountService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IErpSalesRepService _erpSalesRepService;
        private readonly IErpSalesOrgService _erpSalesOrgService;
        private readonly IAddressService _addressService;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IErpSalesOrgModelFactory _erpSalesOrgModelFactory;

        #endregion

        #region Ctor

        public SalesRepUserModelFactory(
            ICustomerService customerService,
            IErpAccountService erpAccountService,
            IDateTimeHelper dateTimeHelper,
            IErpSalesRepService erpSalesRepService,
            IErpSalesOrgService erpSalesOrgService,
            IAddressService addressService,
            IAddressModelFactory addressModelFactory,
            IErpSalesOrgModelFactory erpSalesOrgModelFactory)
        {
            _customerService = customerService;
            _erpAccountService = erpAccountService;
            _dateTimeHelper = dateTimeHelper;
            _erpSalesRepService = erpSalesRepService;
            _erpSalesOrgService = erpSalesOrgService;
            _addressService = addressService;
            _addressModelFactory = addressModelFactory;
            _erpSalesOrgModelFactory = erpSalesOrgModelFactory;
        }

        #endregion

        #region Methods

        public async Task<SalesRepUserSearchModel> PrepareSalesRepUserSearchModelAsync(SalesRepUserSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }
        public async Task<SalesRepUserListModel> PrepareSalesRepUserListModelForSalesRep(SalesRepUserSearchModel searchModel, ErpSalesRep erpSalesRep)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var erpNopUsers = await _erpSalesRepService.GetAllSalesRepUsersAsync(salesRepId: (erpSalesRep.SalesRepTypeId == (int)SalesRepType.AllUsers) ? 0 : erpSalesRep.Id,
                erpAccontNo: searchModel.SearchERPAccountNumber,
                accountName: searchModel.SearchERPAccountName, email: searchModel.SearchCustomerEmail, fullName: searchModel.SearchCustomerFullName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            // get ErpNopUsers
            //var erpNopUsers = await _erpNopUserService.GetAllErpNopUsersAsync(accountId: 1, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new SalesRepUserListModel().PrepareToGridAsync(searchModel, erpNopUsers, () =>
            {
                return erpNopUsers.SelectAwait(async user =>
                {
                    var customer = await _customerService.GetCustomerByIdAsync(user.NopCustomerId);
                    var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(user.ErpAccountId);
                    //var erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(user.ErpAccountId);
                    //fill in model values from the entity
                    var userModel = new SalesRepUserModel
                    {
                        Id = user.Id,
                        NopCustomerId = user.NopCustomerId,
                        CustomerFullName = await _customerService.GetCustomerFullNameAsync(customer),
                        CustomerEmail = customer.Email,
                        ErpShipToAddressId = user.ErpShipToAddressId,
                        //convert dates to the user time
                        CreatedOnUtc = user.CreatedOnUtc,
                        IsActive = user.IsActive,
                        ErpUserType = ((ErpUserType)user.ErpUserTypeId).ToString()
                    };

                    if (erpAccount != null)
                    {
                        userModel.ErpAccountId = user.ErpAccountId;
                        userModel.ErpAccountNumber = erpAccount.AccountNumber;
                        userModel.ErpAccountName = erpAccount.AccountName;
                    }

                    return userModel;
                });
            });

            return model;
        }

        public async Task<SalesRepUserListModel> PreparePublicSalesRepUserListModelForSalesRep(SalesRepUserSearchModel searchModel, ErpSalesRep erpSalesRep)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));


            var erpNopUsers = await _erpSalesRepService.GetAllSalesRepUsersBySalesRepIdAsync(salesRepId: (erpSalesRep.SalesRepTypeId == (int)SalesRepType.MultiBuyers) ? erpSalesRep.Id : 0,
                erpAccontNo: searchModel.SearchERPAccountNumber,
                accountName: searchModel.SearchERPAccountName, email: searchModel.SearchCustomerEmail, fullName: searchModel.SearchCustomerFullName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);


            //prepare list model
            var model = await new SalesRepUserListModel().PrepareToGridAsync(searchModel, erpNopUsers, () =>
            {
                return erpNopUsers.SelectAwait(async user =>
                {
                    var customer = await _customerService.GetCustomerByIdAsync(user.NopCustomerId);
                    var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(user.ErpAccountId);
                    //fill in model values from the entity
                    var userModel = new SalesRepUserModel
                    {
                        Id = user.Id,
                        NopCustomerId = user.NopCustomerId,
                        CustomerFullName = await _customerService.GetCustomerFullNameAsync(customer),
                        CustomerEmail = customer.Email,
                        ErpShipToAddressId = user.ErpShipToAddressId,
                        //convert dates to the user time
                        CreatedOnUtc = user.CreatedOnUtc,
                        IsActive = user.IsActive,
                        ErpUserType = ((ErpUserType)user.ErpUserTypeId).ToString()
                    };

                    if (erpAccount != null)
                    {
                        userModel.ErpAccountId = user.ErpAccountId;
                        userModel.ErpAccountNumber = erpAccount.AccountNumber;
                        userModel.ErpAccountName = erpAccount.AccountName;
                    }

                    return userModel;
                });
            });

            return model;
        }

        public async Task<ErpAccountListModel> PrepareSalesRepErpUserListModelForSalesRep(ErpAccountSearchModel searchModel, ErpSalesRep erpSalesRep)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var erpAccountIdMaps = (await _erpAccountService.GetAllErpAccountsBySalesRepIdAsync(erpSalesRepId: searchModel.ErpAccountId)).ToPagedList(searchModel);

            //prepare list model
            var model = await new ErpAccountListModel().PrepareToGridAsync(searchModel, erpAccountIdMaps, () =>
            {
                return erpAccountIdMaps.SelectAwait(async erpIdMap =>
                {
                    var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpIdMap.ErpAccountId);

                    //prepare address model
                    var address = await _addressService.GetAddressByIdAsync(erpAccount?.BillingAddressId ?? 0);
                    var addressModel = new AddressModel();
                    if (address != null)
                        addressModel = address.ToModel(addressModel);
                    await _addressModelFactory.PrepareAddressModelAsync(addressModel, address);

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
                    return erpAccountModel;
                });
            });

            return model;
        }
        #endregion
    }
}