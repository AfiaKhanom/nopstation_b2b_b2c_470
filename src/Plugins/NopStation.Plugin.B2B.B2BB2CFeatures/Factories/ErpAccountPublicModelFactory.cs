using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.ErpAccountPublic;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories
{
    public class ErpAccountPublicModelFactory : IErpAccountPublicModelFactory
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IAddressService _addressService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICustomerService _customerService;
        private readonly ICurrencyService _currencyService;
        private readonly IOrderService _orderService;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly AddressSettings _addressSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IErpAccountService _erpAccountService;
        private readonly IErpSalesOrgService _erpSalesOrgService;
        private readonly IErpInvoiceService _erpInvoiceService;
        private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
        private readonly IShippingService _shippingService;
        private readonly IErpWarehouseAdditionalDataService _erpWarehouseAdditionalDataService;
        private readonly IErpWarehouseSalesOrgMapService _erpWarehouseSalesOrgMapService;
        private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
        private readonly IB2BB2CWorkContext _b2BB2CWorkContext;
        private readonly IErpNopUserService _erpNopUserService;

        #endregion

        #region Ctor

        public ErpAccountPublicModelFactory(IWorkContext workContext,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            IAddressService addressService,
            IPriceFormatter priceFormatter,
            ICustomerService customerService,
            ICurrencyService currencyService,
            IOrderService orderService,
            IAddressModelFactory addressModelFactory,
            AddressSettings addressSettings,
            IStoreContext storeContext,
            IErpAccountService erpAccountService,
            IErpSalesOrgService erpSalesOrgService,
            IErpInvoiceService erpInvoiceService,
            IErpOrderAdditionalDataService erpOrderAdditionalDataService,
            IShippingService shippingService,
            IErpWarehouseAdditionalDataService erpWarehouseAdditionalDataService,
            IErpWarehouseSalesOrgMapService erpWarehouseSalesOrgMapService,
            IErpCustomerFunctionalityService erpCustomerFunctionalityService,
            IB2BB2CWorkContext b2BB2CWorkContext,
            IErpNopUserService erpNopUserService
            )
        {
            _workContext = workContext;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _addressService = addressService;
            _priceFormatter = priceFormatter;
            _customerService = customerService;
            _currencyService = currencyService;
            _orderService = orderService;
            _addressModelFactory = addressModelFactory;
            _addressSettings = addressSettings;
            _storeContext = storeContext;
            _erpAccountService = erpAccountService;
            _erpSalesOrgService = erpSalesOrgService;
            _erpInvoiceService = erpInvoiceService;
            _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
            _shippingService = shippingService;
            _erpWarehouseAdditionalDataService = erpWarehouseAdditionalDataService;
            _erpWarehouseSalesOrgMapService = erpWarehouseSalesOrgMapService;
            _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
            _b2BB2CWorkContext = b2BB2CWorkContext;
            _erpNopUserService = erpNopUserService;
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
            model.LastNameRequired = true;
            model.EmailRequired = true;
            model.CompanyRequired = _addressSettings.CompanyRequired;
            model.CountyRequired = _addressSettings.CountyRequired;
            model.CityRequired = _addressSettings.CityRequired;
            model.StreetAddressRequired = _addressSettings.StreetAddressRequired;
            model.StreetAddress2Required = _addressSettings.StreetAddress2Required;
            model.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
            model.PhoneRequired = _addressSettings.PhoneRequired;
            model.FaxRequired = _addressSettings.FaxRequired;
        }

        protected decimal GetErpCustomerAccountSavingsForCurrentYear(ErpAccount b2BAccount)
        {
            if (b2BAccount.TotalSavingsForthisYearUpdatedOnUtc.HasValue && b2BAccount.TotalSavingsForthisYearUpdatedOnUtc.Value.Date == DateTime.UtcNow.Date && b2BAccount.TotalSavingsForthisYear.HasValue)
                return b2BAccount.TotalSavingsForthisYear.Value;

            var totalSavings = decimal.Zero;
            return totalSavings;
        }

        protected decimal GetCustomerAccountSavingsForAllTime(ErpAccount b2BAccount)
        {
            if (b2BAccount.TotalSavingsForAllTimeUpdatedOnUtc.HasValue && b2BAccount.TotalSavingsForAllTimeUpdatedOnUtc.Value.Date == DateTime.UtcNow.Date && b2BAccount.TotalSavingsForAllTime.HasValue)
                return b2BAccount.TotalSavingsForAllTime.Value;

            var totalSavings = decimal.Zero;
            return totalSavings;
        }

        #endregion

        #region Method

        public async Task<ErpAccountPublicSearchModel> PrepareErpAccountSearchModelAsync(ErpAccountPublicSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));


            ////prepare "active" filter (0 - all; 1 - active only; 2 - inactive only)
            //searchModel.AvailableDeclineOptions.Add(new SelectListItem
            //{
            //    Value = "0",
            //    Text = await _localizationService.GetResourceAsync("Plugins.Misc.CustomOrderManager.Admin.SearchOrder.All"),
            //});
            //searchModel.AvailableDeclineOptions.Add(new SelectListItem
            //{
            //    Value = "1",
            //    Text = await _localizationService.GetResourceAsync("Plugins.Misc.CustomOrderManager.Admin.SearchOrder.Decliend"),
            //});
            //searchModel.AvailableDeclineOptions.Add(new SelectListItem
            //{
            //    Value = "2",
            //    Text = await _localizationService.GetResourceAsync("Plugins.Misc.CustomOrderManager.Admin.SearchOrder.Accepted"),
            //});

            //// preselect (0 - all;)
            //searchModel.IsDeclinedId = 0;


            ////prepare available stores
            //await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            ////prepare available vendors
            //await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

            ////prepare available warehouses
            //await _baseAdminModelFactory.PrepareWarehousesAsync(searchModel.AvailableWarehouses);

            //prepare grid
            searchModel.SetGridPageSize();

            //searchModel.HideStoresList = _catalogSettings.IgnoreStoreLimitations || searchModel.AvailableStores.SelectionIsNotPossible();
            return searchModel;
        }

        public async Task<ErpAccountPublicListModel> PrepareErpAccountListModelAsync(ErpAccountPublicSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get ERP Accounts
            var erpAccounts = await _erpAccountService.GetAllErpAccountsAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize, showHidden: false);

            //prepare list model
            var model = await new ErpAccountPublicListModel().PrepareToGridAsync(searchModel, erpAccounts, () =>
            {
                //fill in model values from the entity
                return erpAccounts.SelectAwait(async erpAccount =>
                {
                    //Get addionalInfos
                    var erpAccountSalesOrgInfo = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId);

                    //fill in model values from the entity
                    var erpAccountModel = new ErpAccountPublicModel
                    {
                        Id = erpAccount.Id,
                        AccountNumber = erpAccount.AccountNumber,
                        AccountName = erpAccount.AccountName,
                        VatNumber = erpAccount.VatNumber,
                        CurrentBalance = erpAccount.CurrentBalance,
                    };

                    var currentCulture = (await _workContext.GetWorkingLanguageAsync()).LanguageCulture;
                    var dtfi = new CultureInfo(currentCulture, false).DateTimeFormat;

                    //Additional Infos
                    if (erpAccountSalesOrgInfo != null)
                    {
                        var address = await _addressService.GetAddressByIdAsync(erpAccountSalesOrgInfo.AddressId);
                        var addressModel = new AddressModel();
                        await _addressModelFactory.PrepareAddressModelAsync(addressModel, address);
                        var erpAccountSalesOrgModel = new ErpSalesOrgModel
                        {
                            Name = erpAccountSalesOrgInfo.Name,
                            Code = erpAccountSalesOrgInfo.Code,
                            Email = erpAccountSalesOrgInfo.Email,
                            Address = addressModel,
                            IntegrationClientId = erpAccountSalesOrgInfo.IntegrationClientId,
                            AuthenticationKey = erpAccountSalesOrgInfo.AuthenticationKey,
                        };

                        erpAccountModel.ErpSalesOrgModel = erpAccountSalesOrgModel;
                    }

                    return erpAccountModel;
                });
            });

            return model;
        }

        public async Task<RecentTransactionListModel> PrepareRecentTransactionListAsync(ErpAccountInfoModel erpAccountInfoModel)
        {
            var currCustomer = await _workContext.GetCurrentCustomerAsync();
            var currencyTmp = await _currencyService.GetCurrencyByIdAsync(currCustomer.CurrencyId ?? 0);
            var customerCurrency = currencyTmp != null && currencyTmp.Published ? currencyTmp : await _workContext.GetWorkingCurrencyAsync();
            var customerCurrencyCode = customerCurrency.CurrencyCode;

            var transactionFromDate = erpAccountInfoModel.SearchTransactionFromDate;
            var transactionToDate = erpAccountInfoModel.SearchTransactionToDate;

            if (erpAccountInfoModel.SearchTransactionFromDate.HasValue)
            {
                transactionFromDate = erpAccountInfoModel.SearchTransactionFromDate.Value;
            }

            if (erpAccountInfoModel.SearchTransactionToDate.HasValue)
            {
                transactionToDate = erpAccountInfoModel.SearchTransactionToDate.Value.AddHours(23).AddMinutes(59).AddSeconds(59);
            }

            IPagedList<ErpInvoice> transactionPerAccounts = null;

            if (erpAccountInfoModel.ErpAccountId > 0)
            {
                transactionPerAccounts = await _erpInvoiceService.GetAllErpInvoiceAsync(
                    pageIndex: erpAccountInfoModel.Page - 1,
                    pageSize: erpAccountInfoModel.PageSize,
                    getOnlyTotalCount: false,
                    erpOrderNumber: erpAccountInfoModel.SearchOrderNumberOrName,
                    documentName: erpAccountInfoModel.SearchDocumentNumberOrName,
                    documentDateUtc: transactionFromDate,
                    erpAccountId: erpAccountInfoModel.ErpAccountId,
                    erpDocumentNumber: erpAccountInfoModel.SearchDocumentNumberOrName,
                    customOrderNumber: erpAccountInfoModel.SearchOrderNumberOrName,
                    postingFromDateUtc: transactionFromDate,
                    postingToDateUtc: transactionToDate
                );
            }

            var language = await _workContext.GetWorkingLanguageAsync();
            //prepare list model
            var model = await new RecentTransactionListModel().PrepareToGridAsync(erpAccountInfoModel, transactionPerAccounts, () =>
            {
                return transactionPerAccounts.SelectAwait(async transaction =>
                {
                    var isDocumentTypeInvoice = !string.IsNullOrEmpty(transaction.DocumentDisplayName) && transaction.DocumentDisplayName == "Invoice";
                    var isDocumentTypeDownloadable = transaction.DocumentTypeId == (int)ErpDocumentType.Invoice;
                    var nopOrder = await _erpOrderAdditionalDataService.GetNopOrderByErpOrderNumberAsync(transaction.ErpOrderNumber);
                    var accountModel = new RecentTransactionModel
                    {
                        PostingDate = await _dateTimeHelper.ConvertToUserTimeAsync(transaction.PostingDateUtc, DateTimeKind.Utc),
                        DocumentDisplayName = transaction.DocumentDisplayName,
                        DocumentNo = transaction.ErpDocumentNumber,
                        Status = "", // NO Data in Image
                        Remaining = decimal.Zero, // Zero in Image
                        AmountExVat = await _priceFormatter.FormatPriceAsync(price: transaction.AmountExclVat, showCurrency: true, currencyCode: customerCurrencyCode, showTax: true, languageId: language.Id),
                        IsDocumentTypeInvoice = isDocumentTypeInvoice,
                        IsDocumentTypeDownloadable = isDocumentTypeDownloadable,
                        ERPOrderNumber = transaction.ErpOrderNumber,
                        CustomerOrder = nopOrder?.CustomOrderNumber ?? string.Empty,
                        NopOrderId = nopOrder?.Id ?? 0
                    };
                    return accountModel;
                });
            });

            //var erpOrderNumbers = model.Data.Where(rtm => !string.IsNullOrEmpty(rtm.ERPOrderNumber)).Select(x => x.ERPOrderNumber).ToList();
            //IDictionary<string, string> customerReferences = null;
            //if (erpAccountInfoModel.ErpAccountId > 0)
            //    customerReferences = await _erpOrderAdditionalDataService.GetAllCustomerReferencesByERPOrderNumbersAsync(erpOrderNumbers);

            //if (customerReferences != null)
            //{
            //    foreach (var recentTransactionModel in model.Data)
            //    {
            //        if (!string.IsNullOrEmpty(recentTransactionModel.ERPOrderNumber))
            //            recentTransactionModel.CustomerOrder = customerReferences.ContainsKey(recentTransactionModel.ERPOrderNumber) ?
            //                        customerReferences[recentTransactionModel.ERPOrderNumber] : string.Empty;
            //    }
            //}
            return model;
        }

        public async Task<ErpAccountInfoModel> PrepareErpAccountInfoModelAsync(ErpAccount b2BAccount, ErpAccountInfoModel model, bool enableErpAccountUpdate = false)
        {
            if (b2BAccount == null)
                throw new ArgumentNullException(nameof(b2BAccount));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var currCustomer = await _workContext.GetCurrentCustomerAsync();

            //customer currency
            var currencyTmp = await _currencyService.GetCurrencyByIdAsync(currCustomer.Id);
            var customerCurrency = currencyTmp != null && currencyTmp.Published ? currencyTmp : await _workContext.GetWorkingCurrencyAsync();
            var customerCurrencyCode = customerCurrency.CurrencyCode;

            // ERP Account Update
            //if (enableErpAccountUpdate)
            //b2BAccount = _b2BERPIntegrationService.AccountBalanceUpdate(b2BAccount);

            model.ErpAccountId = b2BAccount.Id;
            model.AccountNumber = b2BAccount.AccountNumber;
            model.AccountName = b2BAccount.AccountName;
            model.HasErpQuoteAssistantRole = false; //_b2bCustomerFunctionality.IsCurrentCustomerInB2BQuoteAssistantRole();
            model.HasErpOrderAssistantRole = false; //_b2bCustomerFunctionality.IsCurrentCustomerInB2BOrderAssistantRole();

            var availableBlanace = b2BAccount.CreditLimitAvailable;

            if (!model.HasErpQuoteAssistantRole && !model.HasErpOrderAssistantRole)
            {
                var storeScope = await _storeContext.GetCurrentStoreAsync();
                //var b2BCustomerAccountSettings = await _settingService.LoadSetting<ErpCusAccountSettings>(storeScope.Id);

                model.IsShowYearlySavings = true;//b2BCustomerAccountSettings.IsShowYearlySavings;
                model.IsShowAllTimeSavings = true;//b2BCustomerAccountSettings.IsShowAllTimeSavings;
                model.IsShowAccountStatementDownloadEnabled = false;//b2BCustomerAccountSettings.EnableAccountStatementDownload;

                var currentYearSavings = string.Empty;
                var allTimeSavings = string.Empty;

                var language = await _workContext.GetWorkingLanguageAsync();
                if (model.IsShowYearlySavings)
                {
                    currentYearSavings = await _priceFormatter.FormatPriceAsync(GetErpCustomerAccountSavingsForCurrentYear(b2BAccount), true, customerCurrencyCode, true, language.Id);
                }
                if (model.IsShowAllTimeSavings)
                {
                    allTimeSavings = await _priceFormatter.FormatPriceAsync(GetCustomerAccountSavingsForAllTime(b2BAccount), true, customerCurrencyCode, true, language.Id);
                }

                model.CurrentYearOnlineSavings = currentYearSavings;
                model.AllTimeOnlineSavings = allTimeSavings;
                model.CreditLimit = await _priceFormatter.FormatPriceAsync(b2BAccount.CreditLimit, true, customerCurrencyCode, true, language.Id);
                model.CurrentBalance = await _priceFormatter.FormatPriceAsync(b2BAccount.CurrentBalance, true, customerCurrencyCode, true, language.Id);
                model.AvailableCredit = await _priceFormatter.FormatPriceAsync(availableBlanace, true, customerCurrencyCode, true, language.Id);
                model.LastPaymentAmount = b2BAccount.LastPaymentAmount.HasValue ?
                    await _priceFormatter.FormatPriceAsync(b2BAccount.LastPaymentAmount.Value, true, customerCurrencyCode, true, language.Id) : string.Empty;
                model.LastPaymentDate = b2BAccount.LastPaymentDate.HasValue ? b2BAccount.LastPaymentDate.Value.ToShortDateString() : string.Empty;
            }

            //prepare page parameters
            if (!enableErpAccountUpdate)
            {
                model.SetGridPageSize();

            }

            return model;
        }

        public async Task<ErpAccountOrderSearchModel> PrepareErpAccountOrderSearchModelAsync(ErpAccount erpAccount, ErpNopUser erpNopUser, ErpAccountOrderSearchModel model)
        {
            if (erpAccount == null)
                throw new ArgumentNullException(nameof(erpAccount));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.ErpAccountId = erpAccount?.Id ?? 0;
            model.ErpAccountNumber = erpAccount?.AccountNumber;
            model.ErpNopUserId = erpNopUser?.Id ?? 0;

            //prepare page parameters
            model.SetGridPageSize();

            return model;
        }

        public async Task<ErpAccountOrderListModel> PrepareErpOrderListModelAsync(ErpAccountOrderSearchModel searchModel)
        {
            var currCustomer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            var orderPlacedOnDateFrom = searchModel.SearchOrderDateFrom;
            var orderPlacedOnDateTo = searchModel.SearchOrderDateTo;

            if (searchModel.SearchOrderDateFrom.HasValue)
            {
                orderPlacedOnDateFrom = searchModel.SearchOrderDateFrom.Value; 
            }

            if (searchModel.SearchOrderDateTo.HasValue)
            {
                orderPlacedOnDateTo = searchModel.SearchOrderDateTo.Value.AddHours(23).AddMinutes(59).AddSeconds(59);  
            }

            var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(currCustomer.Id);
            var erpOrderTypeId = erpNopUser != null ? erpNopUser.ErpUserType == ErpUserType.B2BUser ? (int)ErpOrderType.B2BSalesOrder : erpNopUser.ErpUserType == ErpUserType.B2CUser ? (int)ErpOrderType.B2CSalesOrder : 0 : 0;

            var erpOrderPerUsers = await _erpOrderAdditionalDataService.GetAllErpOrderAdditionalDataAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize, accountId: searchModel.ErpAccountId, nopCustomerId: searchModel.NopCustomerId,
                erpOrderNumber: searchModel.SearchOrderNumberOrName, nopOrderNumber: searchModel.SearchOrderNumberOrName, erpOrderTypeId: erpOrderTypeId, searchOrderDateFrom: orderPlacedOnDateFrom, searchOrderDateTo: orderPlacedOnDateTo);

            //customer currency
            var currencyTmp = await _currencyService.GetCurrencyByIdAsync(currCustomer.CurrencyId ?? 0);
            var customerCurrency = currencyTmp != null && currencyTmp.Published ? currencyTmp : await _workContext.GetWorkingCurrencyAsync();
            var customerCurrencyCode = customerCurrency.CurrencyCode;

            //prepare list model
            var model = await new ErpAccountOrderListModel().PrepareToGridAsync(searchModel, erpOrderPerUsers, () =>
            {
                return erpOrderPerUsers.SelectAwait(async b2COrder =>
                {
                    var nopOrder = await _orderService.GetOrderByIdAsync(b2COrder.NopOrderId);
                    var orderItems = await _orderService.GetOrderItemsAsync(nopOrder.Id);
                    var totalOrderItems = orderItems?.Sum(x => x.Quantity) ?? 0;

                    (var invoiceCount, var totalShipped) = _erpInvoiceService
                    .GetTotalNumberOfInvoicesAndNumOfShippedItemsByErpAccountIdERPOrderNumber(searchModel.ErpAccountId, b2COrder.ErpOrderNumber);
                    var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(b2COrder.ErpAccountId);
                    var salesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount?.ErpSalesOrgId ?? 0);
                    var warehouse = await _erpWarehouseAdditionalDataService.GetErpWarehouseAdditionalDataBySalesOrgIdAsync(salesOrg?.Id ?? 0);
                    var map = (await _erpWarehouseSalesOrgMapService.GetWarehouseSalesOrgMapByNopWarehouseIdAsync(warehouse?.Id ?? 0)).FirstOrDefault();

                    //fill in model values from the entity
                    var b2BAccountOrder = new ErpAccountOrderDetailsModel
                    {
                        ErpOrderOriginType = await _localizationService.GetLocalizedEnumAsync(b2COrder.ErpOrderOriginType),
                        ERPOrderNumber = string.IsNullOrEmpty(b2COrder.ErpOrderNumber) ? nopOrder?.CustomOrderNumber : b2COrder.ErpOrderNumber,
                        CustomerOrder = b2COrder.CustomerReference,
                        PaygateReferenceNumber = string.Empty,
                        ErpAccountSalesOrgId = salesOrg?.Id ?? 0,
                        ERPOrderStatus = b2COrder.ERPOrderStatus,
                        ExpectedDelivery = b2COrder.DeliveryDate,
                        WarehouseName = (await _shippingService.GetWarehouseByIdAsync(map?.NopWarehouseId ?? 0))?.Name ?? "",
                        Invoices = invoiceCount,
                        CustomerReference = b2COrder.CustomerReference,
                        SpecialInstructions = b2COrder.SpecialInstructions,
                    };

                    if (nopOrder is not null)
                    {
                        b2BAccountOrder.NopOrderId = nopOrder.Id;
                        b2BAccountOrder.NopOrderNumber = nopOrder.CustomOrderNumber;
                        b2BAccountOrder.PlacedByCustomer = b2COrder.ErpOrderOriginType == ErpOrderOriginType.ERPOrder ? (await _customerService.GetCustomerByIdAsync(b2COrder?.ChangedById ?? 0))?.Email : (await _customerService.GetCustomerByIdAsync(nopOrder?.CustomerId ?? 0))?.Email;
                        b2BAccountOrder.TotalOrderItems = totalOrderItems;
                        b2BAccountOrder.Unshipped = int.Max(totalOrderItems - totalShipped, 0);
                        b2BAccountOrder.OrderPlacedOn = await _dateTimeHelper.ConvertToUserTimeAsync(nopOrder.CreatedOnUtc, DateTimeKind.Utc);
                        b2BAccountOrder.OrderTotalAmount = await _priceFormatter.FormatPriceAsync(nopOrder.OrderTotal, true, customerCurrencyCode, true, (await _workContext.GetWorkingLanguageAsync()).Id);
                    }

                    return b2BAccountOrder;
                });
            });

            return model;
        }

        public async Task<ErpAccountQuoteOrderSearchModel> PrepareErpAccountQuoteOrderSearchModelAsync(ErpAccount erpAccount, ErpNopUser erpNopUser, ErpAccountQuoteOrderSearchModel model)
        {
            if (erpAccount == null)
                throw new ArgumentNullException(nameof(erpAccount));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.ErpAccountId = erpAccount?.Id ?? 0;
            model.ErpAccountNumber = erpAccount.AccountNumber;
            model.ErpNopUserId = erpNopUser?.Id ?? 0;

            model.SetGridPageSize();
            return model;
        }

        public async Task<ErpQuoteOrderListModel> PrepareErpQuoteOrderListModelAsync(ErpAccountQuoteOrderSearchModel searchModel)
        {
            var currCustomer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            var orderPlacedOnDateFrom = searchModel.SearchOrderDateFrom;
            var orderPlacedOnDateTo = searchModel.SearchOrderDateTo;

            if (searchModel.SearchOrderDateFrom.HasValue)
            {

                DateTime selectedDate = searchModel.SearchOrderDateFrom.Value;
                orderPlacedOnDateFrom = selectedDate;// searchModel.SearchOrderDateFrom;
            }

            if (searchModel.SearchOrderDateTo.HasValue)
            {

                DateTime selectedDate = searchModel.SearchOrderDateTo.Value.AddHours(23).AddMinutes(59).AddSeconds(59);
                orderPlacedOnDateTo = selectedDate;// searchModel.SearchOrderDateTo;
            }

            var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(currCustomer.Id);
            var erpOrderTypeId = erpNopUser != null ? erpNopUser.ErpUserType == ErpUserType.B2BUser ? (int)ErpOrderType.B2BQuote : erpNopUser.ErpUserType == ErpUserType.B2CUser ? (int)ErpOrderType.B2CQuote : 0 : 0;

            var erpQuoteOrders = await _erpOrderAdditionalDataService.GetAllErpOrderAdditionalDataAsync(accountId: searchModel.ErpAccountId, nopOrderNumber: searchModel.SearchQuoteNumberOrName, erpOrderTypeId: (int)ErpOrderType.B2BQuote,/* orderPlacedOn: orderPlacedOnDate ,*/ pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize, searchOrderDateTo: orderPlacedOnDateTo, searchOrderDateFrom: orderPlacedOnDateFrom);

            //customer currency
            var currencyTmp = await _currencyService.GetCurrencyByIdAsync(currCustomer.CurrencyId ?? 0);
            var customerCurrency = currencyTmp != null && currencyTmp.Published ? currencyTmp : await _workContext.GetWorkingCurrencyAsync();
            var customerCurrencyCode = customerCurrency.CurrencyCode;

            //prepare list model
            var model = await new ErpQuoteOrderListModel().PrepareToGridAsync(searchModel, erpQuoteOrders, () =>
            {
                return erpQuoteOrders.SelectAwait(async erpOrder =>
                {
                    var nopOrder = await _orderService.GetOrderByIdAsync(erpOrder.NopOrderId);
                    var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpOrder?.ErpAccountId ?? 0);
                    var salesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount?.ErpSalesOrgId ?? 0);
                    var warehouse = await _erpWarehouseAdditionalDataService.GetErpWarehouseAdditionalDataBySalesOrgIdAsync(salesOrg?.Id ?? 0);
                    var map = (await _erpWarehouseSalesOrgMapService.GetWarehouseSalesOrgMapByNopWarehouseIdAsync(warehouse?.Id ?? 0))?.FirstOrDefault();

                    var quoteOrderModel = new ErpQuoteOrderModel
                    {
                        ErpOrderOriginType = await _localizationService.GetLocalizedEnumAsync(erpOrder.ErpOrderOriginType),
                        QuoteNumber = string.IsNullOrEmpty(erpOrder.ErpOrderNumber) ? nopOrder?.CustomOrderNumber : erpOrder.ErpOrderNumber,
                        ErpAccountSalesOrganisationId = salesOrg?.Id ?? 0,
                        ERPOrderStatus = erpOrder.ERPOrderStatus,
                        WarehouseName = (await _shippingService.GetWarehouseByIdAsync(map?.NopWarehouseId ?? 0))?.Name ?? "",
                        CustomerOrder = erpOrder.CustomerReference
                    };

                    if (erpOrder.QuoteExpiryDate.HasValue)
                    {
                        quoteOrderModel.ExpiryDate = await _dateTimeHelper.ConvertToUserTimeAsync(erpOrder.QuoteExpiryDate.Value, DateTimeKind.Utc);
                        quoteOrderModel.IsQuoteExpired = erpOrder.QuoteExpiryDate.Value.Date < DateTime.UtcNow.Date ? true : false;
                        quoteOrderModel.IsQuoteActive = await _erpCustomerFunctionalityService.CheckQuoteOrderStatusAsync(erpOrder);
                        quoteOrderModel.IsQuoteConvertedToOrder = erpOrder.QuoteSalesOrderId.HasValue && erpOrder.QuoteSalesOrderId.Value > 0;
                    }

                    if (nopOrder != null)
                    {
                        quoteOrderModel.NopOrderId = nopOrder.Id;
                        quoteOrderModel.PlacedByCustomerEmail = erpOrder.ErpOrderOriginType == ErpOrderOriginType.ERPOrder ? (await _customerService.GetCustomerByIdAsync(erpOrder?.ChangedById ?? 0))?.Email : (await _customerService.GetCustomerByIdAsync(nopOrder?.CustomerId ?? 0))?.Email;
                        quoteOrderModel.QuoteDate = await _dateTimeHelper.ConvertToUserTimeAsync(nopOrder.CreatedOnUtc, DateTimeKind.Utc);
                        quoteOrderModel.TotalAmount = await _priceFormatter.FormatPriceAsync(nopOrder.OrderTotal, true, customerCurrencyCode, true, (await _workContext.GetWorkingLanguageAsync()).Id);
                    }

                    return quoteOrderModel;
                });
            });
            return model;
        }

        //public async Task<ErpAccountInfoAjaxLoadModel> PrepareB2BAccountInfoAjaxLoadModelAsync(ErpAccount erpAccount, bool enableErpAccountUpdate = false)
        //{
        //    if (erpAccount == null)
        //        throw new ArgumentNullException(nameof(erpAccount));

        //    var currCustomer = await _workContext.GetCurrentCustomerAsync();
        //    var model = new ErpAccountInfoAjaxLoadModel();

        //    //customer currency
        //    var currencyTmp = await _currencyService.GetCurrencyByIdAsync(currCustomer.Id);
        //    var customerCurrency = currencyTmp != null && currencyTmp.Published ? currencyTmp : await _workContext.GetWorkingCurrencyAsync();
        //    var customerCurrencyCode = customerCurrency.CurrencyCode;

        //    // ERP Account Update
        //    if (enableErpAccountUpdate)
        //        erpAccount = _erpIntegrationService.AccountBalanceUpdate(erpAccount);

        //    model.ErpAccountId = erpAccount.Id;
        //    model.HasErpQuoteAssistantRole = _b2bCustomerFunctionality.IsCurrentCustomerInB2BQuoteAssistantRole();
        //    model.HasErpOrderAssistantRole = _b2bCustomerFunctionality.IsCurrentCustomerInB2BOrderAssistantRole();
        //    var availableBlanace = b2BAccount.CreditLimitAvailable;

        //    if (!model.HasB2BQuoteAssistantRole && !model.HasB2BOrderAssistantRole)
        //    {
        //        model.CreditLimit = _priceFormatter.FormatPrice(b2BAccount.CreditLimit, true, customerCurrencyCode, _workContext.WorkingLanguage, true);
        //        model.CurrentBalance = _priceFormatter.FormatPrice(b2BAccount.CurrentBalance, true, customerCurrencyCode, _workContext.WorkingLanguage, true);
        //        model.AvailableCredit = _priceFormatter.FormatPrice(availableBlanace, true, customerCurrencyCode, _workContext.WorkingLanguage, true);
        //        model.LastPaymentAmount = b2BAccount.LastPaymentAmount.HasValue ?
        //            _priceFormatter.FormatPrice(b2BAccount.LastPaymentAmount.Value, true, customerCurrencyCode, _workContext.WorkingLanguage, true) : string.Empty;
        //        model.LastPaymentDate = b2BAccount.LastPaymentDate.HasValue ? b2BAccount.LastPaymentDate.Value.ToShortDateString() : string.Empty;
        //    }

        //    var allowOverSpend = b2BAccount.AllowOverspend;
        //    model.AllowOverSpend = allowOverSpend;
        //    if (!allowOverSpend)
        //    {
        //        var shoppingCartItems = _workContext.CurrentCustomer.ShoppingCartItems?.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();
        //        var shoppingCartTotalBase = _orderTotalCalculationService.GetShoppingCartTotal(shoppingCartItems, out var orderTotalDiscountAmountBase, out var _, out var appliedGiftCards, out var redeemedRewardPoints, out var redeemedRewardPointsAmount);
        //        var orderTotal = decimal.Zero;

        //        if (shoppingCartTotalBase.HasValue)
        //        {
        //            orderTotal = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTotalBase.Value, _workContext.WorkingCurrency);
        //        }

        //        model.CurrentBalanceWithCurrentOrderTotal = _priceFormatter.FormatPrice((b2BAccount.CurrentBalance + orderTotal), true, customerCurrencyCode, _workContext.WorkingLanguage, true);
        //        model.CurrentOrderTotal = _priceFormatter.FormatPrice(orderTotal, true, customerCurrencyCode, _workContext.WorkingLanguage, true);
        //        model.IsOverSpend = orderTotal > availableBlanace;
        //        if (model.HasB2BOrderAssistantRole || model.HasB2BQuoteAssistantRole)
        //        {
        //            model.CreditWarningMessage = string.Format(_localizationService.GetResource("Plugins.Payments.B2BCustomerAccount.B2BQouteOrder.CreditLimitExceed"));
        //        }
        //        else
        //        {
        //            model.CreditWarningMessage = string.Format(_localizationService.GetResource("Plugins.Payments.B2BCustomerAccount.B2BQouteOrder.CreditLimitExceedWithValue"), model.AvailableCredit, model.CurrentOrderTotal);
        //        }
        //    }
        //    return model;
        //}

        #endregion

    }
}