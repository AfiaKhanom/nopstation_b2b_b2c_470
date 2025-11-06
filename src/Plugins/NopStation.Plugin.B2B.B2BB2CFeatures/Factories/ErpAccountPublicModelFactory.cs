using System;
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
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.ErpAccountPublic;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories;

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
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpInvoiceService _erpInvoiceService;
    private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
    private readonly IShippingService _shippingService;
    private readonly IErpWarehouseAdditionalDataService _erpWarehouseAdditionalDataService;
    private readonly IErpWarehouseSalesOrgMapService _erpWarehouseSalesOrgMapService;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly IErpNopUserService _erpNopUserService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;

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
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService,
        IErpInvoiceService erpInvoiceService,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        IShippingService shippingService,
        IErpWarehouseAdditionalDataService erpWarehouseAdditionalDataService,
        IErpWarehouseSalesOrgMapService erpWarehouseSalesOrgMapService,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        IErpNopUserService erpNopUserService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings)
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
        _erpAccountService = erpAccountService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpInvoiceService = erpInvoiceService;
        _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
        _shippingService = shippingService;
        _erpWarehouseAdditionalDataService = erpWarehouseAdditionalDataService;
        _erpWarehouseSalesOrgMapService = erpWarehouseSalesOrgMapService;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _erpNopUserService = erpNopUserService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
    }

    #endregion

    #region Utilities

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
        ArgumentNullException.ThrowIfNull(searchModel);

        //prepare grid
        searchModel.SetGridPageSize();

        return searchModel;
    }

    public async Task<ErpAccountPublicListModel> PrepareErpAccountListModelAsync(ErpAccountPublicSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var erpAccounts = await _erpAccountService.GetAllErpAccountsAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize, showHidden: false);

        var model = await new ErpAccountPublicListModel().PrepareToGridAsync(searchModel, erpAccounts, () =>
        {
            return erpAccounts.SelectAwait(async erpAccount =>
            {
                var erpAccountSalesOrgInfo = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId);

                var erpAccountModel = new ErpAccountPublicModel
                {
                    Id = erpAccount.Id,
                    AccountNumber = erpAccount.AccountNumber,
                    AccountName = erpAccount.AccountName,
                    VatNumber = erpAccount.VatNumber,
                    CurrentBalance = erpAccount.CurrentBalance,
                };

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
                        AuthenticationKey = erpAccountSalesOrgInfo.AuthenticationKey
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
        var model = await new RecentTransactionListModel().PrepareToGridAsync(erpAccountInfoModel, transactionPerAccounts, () =>
        {
            return transactionPerAccounts.SelectAwait(async transaction =>
            {
                var isDocumentTypeInvoice = !string.IsNullOrEmpty(transaction.DocumentDisplayName) && transaction.DocumentDisplayName == "Invoice";
                var isDocumentTypeDownloadable = transaction.DocumentTypeId == (int)ErpDocumentType.Invoice;
                var nopOrder = await _erpOrderAdditionalDataService.GetNopOrderByErpOrderNumberAsync(transaction.ErpOrderNumber);
                var accountModel = new RecentTransactionModel
                {
                    Id = transaction.Id,
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

        return model;
    }

    public async Task<ErpAccountInfoModel> PrepareErpAccountInfoModelAsync(ErpAccount b2BAccount, ErpAccountInfoModel model, bool enableErpAccountUpdate = false)
    {
        ArgumentNullException.ThrowIfNull(b2BAccount);

        ArgumentNullException.ThrowIfNull(model);

        var currCustomer = await _workContext.GetCurrentCustomerAsync();

        var currencyTmp = await _currencyService.GetCurrencyByIdAsync(currCustomer.Id);
        var customerCurrency = currencyTmp != null && currencyTmp.Published ? currencyTmp : await _workContext.GetWorkingCurrencyAsync();
        var customerCurrencyCode = customerCurrency.CurrencyCode;

        model.ErpAccountId = b2BAccount.Id;
        model.AccountNumber = b2BAccount.AccountNumber;
        model.AccountName = b2BAccount.AccountName;
        model.HasErpQuoteAssistantRole = false;
        model.HasErpOrderAssistantRole = false;

        var availableBlanace = b2BAccount.CreditLimitAvailable;

        if (!model.HasErpQuoteAssistantRole && !model.HasErpOrderAssistantRole)
        {
            model.IsShowYearlySavings = _b2BB2CFeaturesSettings.IsShowYearlySavings;
            model.IsShowAllTimeSavings = _b2BB2CFeaturesSettings.IsShowAllTimeSavings;
            model.IsShowAccountStatementDownloadEnabled = _b2BB2CFeaturesSettings.EnableAccountStatementDownload;

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
        ArgumentNullException.ThrowIfNull(erpAccount);
        ArgumentNullException.ThrowIfNull(erpNopUser);
        ArgumentNullException.ThrowIfNull(model);

        model.ErpAccountId = erpAccount.Id;
        model.ErpAccountNumber = erpAccount.AccountNumber;
        model.ErpNopUserId = erpNopUser.Id;

        //prepare page parameters
        model.SetGridPageSize();

        return model;
    }

    public async Task<ErpAccountOrderListModel> PrepareErpOrderListModelAsync(ErpAccountOrderSearchModel searchModel)
    {
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

        var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(searchModel.NopCustomerId);
        var erpOrderTypeId = 0;
        if (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2BUser)
        {
            erpOrderTypeId = (int)ErpOrderType.B2BSalesOrder;
        }
        if (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2CUser)
        {
            erpOrderTypeId = (int)ErpOrderType.B2CSalesOrder;
        }

        if (erpOrderTypeId == 0)
        {
            return new ErpAccountOrderListModel();
        }

        var erpOrderPerUsers = await _erpOrderAdditionalDataService.GetAllErpOrderAdditionalDataAsync(
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize,
            accountId: searchModel.ErpAccountId,
            nopCustomerId: 0,
            erpOrderNumber: searchModel.SearchOrderNumberOrName,
            nopOrderNumber: searchModel.SearchOrderNumberOrName,
            erpOrderTypeId: erpOrderTypeId,
            searchOrderDateFrom: orderPlacedOnDateFrom,
            searchOrderDateTo: orderPlacedOnDateTo);

        var model = await new ErpAccountOrderListModel().PrepareToGridAsync(searchModel, erpOrderPerUsers, () =>
        {
            return erpOrderPerUsers.SelectAwait(async erpOrderAdditionalData =>
            {
                var nopOrder = await _orderService.GetOrderByIdAsync(erpOrderAdditionalData.NopOrderId);
                var orderItems = await _orderService.GetOrderItemsAsync(nopOrder?.Id ?? 0);
                var totalOrderItems = orderItems?.Sum(x => x.Quantity) ?? 0;

                (var invoiceCount, var totalShipped) = _erpInvoiceService
                    .GetTotalNumberOfInvoicesAndNumOfShippedItemsByErpAccountIdERPOrderNumber(searchModel.ErpAccountId, erpOrderAdditionalData.ErpOrderNumber);
                var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpOrderAdditionalData.ErpAccountId);
                var salesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount?.ErpSalesOrgId ?? 0);
                var warehouse = await _erpWarehouseAdditionalDataService.GetErpWarehouseAdditionalDataBySalesOrgIdAsync(salesOrg?.Id ?? 0);
                var map = new ErpWarehouseSalesOrgMap();
                if (warehouse != null && warehouse.Id > 0)
                    map = (await _erpWarehouseSalesOrgMapService.GetWarehouseSalesOrgMapByNopWarehouseIdAsync(warehouse.Id)).FirstOrDefault();

                var b2BAccountOrder = new ErpAccountOrderDetailsModel
                {
                    ErpOrderOriginType = await _localizationService.GetLocalizedEnumAsync(erpOrderAdditionalData.ErpOrderOriginType),
                    ERPOrderNumber = string.IsNullOrEmpty(erpOrderAdditionalData.ErpOrderNumber) ?
                        nopOrder?.CustomOrderNumber : erpOrderAdditionalData.ErpOrderNumber,
                    CustomerOrder = erpOrderAdditionalData.CustomerReference,
                    PaygateReferenceNumber = string.Empty,
                    ErpAccountSalesOrgId = salesOrg?.Id ?? 0,
                    ERPOrderStatus = erpOrderAdditionalData.ERPOrderStatus,
                    ExpectedDelivery = erpOrderAdditionalData.DeliveryDate,
                    WarehouseName = (await _shippingService.GetWarehouseByIdAsync(map?.NopWarehouseId ?? 0))?.Name ?? "",
                    Invoices = invoiceCount,
                    CustomerReference = erpOrderAdditionalData.CustomerReference,
                    SpecialInstructions = erpOrderAdditionalData.SpecialInstructions,
                };

                if (nopOrder != null)
                {
                    b2BAccountOrder.NopOrderId = nopOrder.Id;
                    b2BAccountOrder.NopOrderNumber = nopOrder.CustomOrderNumber;
                    b2BAccountOrder.PlacedByCustomer = erpOrderAdditionalData.ErpOrderOriginType == ErpOrderOriginType.ERPOrder ?
                        (await _customerService.GetCustomerByIdAsync(erpOrderAdditionalData.OrderPlacedByNopCustomerId))?.Email :
                        (await _customerService.GetCustomerByIdAsync(nopOrder.CustomerId))?.Email;
                    b2BAccountOrder.TotalOrderItems = totalOrderItems;
                    b2BAccountOrder.Unshipped = int.Max(totalOrderItems - totalShipped, 0);
                    b2BAccountOrder.OrderPlacedOn = await _dateTimeHelper.ConvertToUserTimeAsync(nopOrder.CreatedOnUtc, DateTimeKind.Utc);
                    b2BAccountOrder.OrderTotalAmount = await _priceFormatter.FormatPriceAsync(
                        nopOrder.OrderTotal,
                        true,
                        (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode,
                        true,
                        (await _workContext.GetWorkingLanguageAsync()).Id);
                }

                return b2BAccountOrder;
            });
        });

        return model;
    }

    public async Task<ErpAccountQuoteOrderSearchModel> PrepareErpAccountQuoteOrderSearchModelAsync(ErpAccount erpAccount, ErpNopUser erpNopUser, ErpAccountQuoteOrderSearchModel model)
    {
        ArgumentNullException.ThrowIfNull(erpAccount);
        ArgumentNullException.ThrowIfNull(erpNopUser);
        ArgumentNullException.ThrowIfNull(model);

        model.ErpAccountId = erpAccount.Id;
        model.ErpAccountNumber = erpAccount.AccountNumber;
        model.ErpNopUserId = erpNopUser.Id;

        model.SetGridPageSize();
        return model;
    }

    public async Task<ErpQuoteOrderListModel> PrepareErpQuoteOrderListModelAsync(ErpAccountQuoteOrderSearchModel searchModel)
    {
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

        var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(searchModel.NopCustomerId);

        var erpOrderTypeId = 0;
        if (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2BUser)
        {
            erpOrderTypeId = (int)ErpOrderType.B2BQuote;
        }
        if (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2CUser)
        {
            erpOrderTypeId = (int)ErpOrderType.B2CQuote;
        }

        if (erpOrderTypeId == 0)
        {
            return new ErpQuoteOrderListModel();
        }

        var erpQuoteOrders = await _erpOrderAdditionalDataService.GetAllErpOrderAdditionalDataAsync(
            accountId: searchModel.ErpAccountId,
            nopOrderNumber: searchModel.SearchQuoteNumberOrName,
            erpOrderTypeId: erpOrderTypeId,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize,
            searchOrderDateTo: orderPlacedOnDateTo,
            searchOrderDateFrom: orderPlacedOnDateFrom);

        var model = await new ErpQuoteOrderListModel().PrepareToGridAsync(searchModel, erpQuoteOrders, () =>
        {
            return erpQuoteOrders.SelectAwait(async erpOrderAdditionalData =>
            {
                var nopOrder = await _orderService.GetOrderByIdAsync(erpOrderAdditionalData.NopOrderId);
                var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpOrderAdditionalData.ErpAccountId);
                var salesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount?.ErpSalesOrgId ?? 0);
                var warehouse = await _erpWarehouseAdditionalDataService.GetErpWarehouseAdditionalDataBySalesOrgIdAsync(salesOrg?.Id ?? 0);
                var map = (await _erpWarehouseSalesOrgMapService
                    .GetWarehouseSalesOrgMapByNopWarehouseIdAsync(warehouse?.Id ?? 0))?.FirstOrDefault();

                var quoteOrderModel = new ErpQuoteOrderModel
                {
                    ErpOrderOriginType = await _localizationService.GetLocalizedEnumAsync(erpOrderAdditionalData.ErpOrderOriginType),
                    QuoteNumber = string.IsNullOrEmpty(erpOrderAdditionalData.ErpOrderNumber) ? nopOrder?.CustomOrderNumber : erpOrderAdditionalData.ErpOrderNumber,
                    ErpAccountSalesOrganisationId = salesOrg?.Id ?? 0,
                    ERPOrderStatus = erpOrderAdditionalData.ERPOrderStatus,
                    WarehouseName = (await _shippingService.GetWarehouseByIdAsync(map?.NopWarehouseId ?? 0))?.Name ?? "",
                    CustomerOrder = erpOrderAdditionalData.CustomerReference
                };

                if (erpOrderAdditionalData.QuoteExpiryDate.HasValue)
                {
                    quoteOrderModel.ExpiryDate = await _dateTimeHelper.ConvertToUserTimeAsync(erpOrderAdditionalData.QuoteExpiryDate.Value, DateTimeKind.Utc);
                    quoteOrderModel.IsQuoteExpired = erpOrderAdditionalData.QuoteExpiryDate.Value.Date < DateTime.UtcNow.Date;
                    quoteOrderModel.IsQuoteActive = await _erpCustomerFunctionalityService.CheckQuoteOrderStatusAsync(erpOrderAdditionalData);
                    quoteOrderModel.IsQuoteConvertedToOrder = erpOrderAdditionalData.QuoteSalesOrderId.HasValue && erpOrderAdditionalData.QuoteSalesOrderId.Value > 0;
                }

                if (nopOrder != null)
                {
                    quoteOrderModel.NopOrderId = nopOrder.Id;
                    quoteOrderModel.PlacedByCustomerEmail = erpOrderAdditionalData.ErpOrderOriginType == ErpOrderOriginType.ERPOrder ?
                        (await _customerService.GetCustomerByIdAsync(erpOrderAdditionalData.ChangedById))?.Email :
                        (await _customerService.GetCustomerByIdAsync(nopOrder.CustomerId))?.Email;
                    quoteOrderModel.QuoteDate = await _dateTimeHelper.ConvertToUserTimeAsync(nopOrder.CreatedOnUtc, DateTimeKind.Utc);
                    quoteOrderModel.TotalAmount = await _priceFormatter.FormatPriceAsync(
                        nopOrder.OrderTotal,
                        true,
                        (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode,
                        true,
                        (await _workContext.GetWorkingLanguageAsync()).Id);
                }

                return quoteOrderModel;
            });
        });

        return model;

        #endregion
    }
}