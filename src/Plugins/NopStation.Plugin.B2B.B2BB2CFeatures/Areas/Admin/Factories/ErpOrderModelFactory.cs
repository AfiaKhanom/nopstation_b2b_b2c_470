using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Helpers;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories
{
    public class ErpOrderModelFactory : IErpOrderModelFactory
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly ILocalizationService _localizationService;
        private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
        private readonly IErpOrderItemAdditionalDataService _erpOrderItemAdditionalDataService;
        private readonly IErpAccountService _erpAccountService;
        private readonly IErpSalesOrgService _erpSalesOrgService;
        private readonly ICommonHelper _commonHelper;
        private readonly IErpShipToAddressService _erpShipToAddressService;

        #endregion

        #region ctor

        public ErpOrderModelFactory(
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            ICustomerService customerService,
            IOrderService orderService,
            IErpOrderAdditionalDataService erpOrderAdditionalDataService,
            IErpOrderItemAdditionalDataService erpOrderItemAdditionalDataService,
            IErpAccountService erpAccountService,
            IErpSalesOrgService erpSalesOrgService,
            ICommonHelper commonHelper,
            IErpShipToAddressService erpShipToAddressService)
        {
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _customerService = customerService;
            _orderService = orderService;
            _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
            _erpOrderItemAdditionalDataService = erpOrderItemAdditionalDataService;
            _erpAccountService = erpAccountService;
            _erpSalesOrgService = erpSalesOrgService;
            _commonHelper = commonHelper;
            _erpShipToAddressService = erpShipToAddressService;
        }

        #endregion

        #region Method

        public async Task<ErpOrderAdditionalDataListModel> PrepareErpOrderPerAccountListModel(ErpOrderAdditionalDataSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var orderPlacedOn = !searchModel.SearchOrderPlacedOn.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.SearchOrderPlacedOn.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());

            var orderPerAccounts = await _erpOrderAdditionalDataService.GetAllErpOrderAdditionalDataAsync(
                nopOrderNumber: searchModel.SearchOrderNumber,
                accountId: searchModel.SearchErpAccountId,
                erpOrderNumber: searchModel.SearchERPOrderNumber,
                erpOrderOriginTypeId: searchModel.SearchErpOrderOriginTypeId,
                erpOrderTypeId: searchModel.SearchErpOrderTypeId,
                integrationStatusTypeId: searchModel.SearchIntegrationStatusTypeId,
                /* orderPlacedOn: orderPlacedOn, */
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            foreach (var item in orderPerAccounts)
            {
                item.ErpAccount = await _erpAccountService.GetErpAccountByIdAsync(item.ErpAccountId);
            }

            var model = await new ErpOrderAdditionalDataListModel().PrepareToGridAsync(searchModel, orderPerAccounts, () =>
            {
                return orderPerAccounts.SelectAwait(async orderPerAccount =>
                {
                    var erpOrderModel = new ErpOrderAdditionalDataModel
                    {
                        Id = orderPerAccount.Id,
                        NopOrderId = orderPerAccount.NopOrderId,
                        ErpOrderNumber = orderPerAccount.ErpOrderNumber,
                        ErpOrderOriginTypeId = orderPerAccount.ErpOrderOriginTypeId,
                        ErpOrderOriginType = await _localizationService.GetLocalizedEnumAsync(orderPerAccount.ErpOrderOriginType),
                        ErpOrderTypeId = orderPerAccount.ErpOrderTypeId,
                        ErpOrderType = await _localizationService.GetLocalizedEnumAsync(orderPerAccount.ErpOrderType),
                        IntegrationStatusTypeId = orderPerAccount.IntegrationStatusTypeId,
                        IntegrationStatusType = await _localizationService.GetLocalizedEnumAsync(orderPerAccount.IntegrationStatusType),
                        ErpAccountId = orderPerAccount.ErpAccountId,
                        ErpAccountName = orderPerAccount.ErpAccount != null ? orderPerAccount.ErpAccount.AccountNumber : "",
                    };

                    if (orderPerAccount.ErpOrderType == ErpOrderType.B2BQuote)
                    {
                        erpOrderModel.ErpOrderType = "B2B Quote";
                    }
                    else if (orderPerAccount.ErpOrderType == ErpOrderType.B2BSalesOrder)
                    {
                        erpOrderModel.ErpOrderType = "B2B Sales Order";
                    }
                    else if (orderPerAccount.ErpOrderType == ErpOrderType.B2CQuote)
                    {
                        erpOrderModel.ErpOrderType = "B2C Quote";
                    }
                    else if (orderPerAccount.ErpOrderType == ErpOrderType.B2CSalesOrder)
                    {
                        erpOrderModel.ErpOrderType = "B2C Sales Order";
                    }

                    var nopOrder = await _orderService.GetOrderByIdAsync(orderPerAccount.NopOrderId);
                    if (nopOrder != null)
                    {
                        erpOrderModel.QouteDate = await _dateTimeHelper.ConvertToUserTimeAsync(nopOrder.CreatedOnUtc, DateTimeKind.Utc);
                        erpOrderModel.OrderNumber = string.IsNullOrEmpty(nopOrder.CustomOrderNumber) ? nopOrder.Id.ToString() : nopOrder.CustomOrderNumber;
                    }

                    return erpOrderModel;
                });
            });
            return model;
        }

        public async Task<ErpOrderAdditionalDataSearchModel> PrepareErpOrderPerAccountSearchModel(ErpOrderAdditionalDataSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available ERP Order Types
            searchModel.AvailableErpOrderTypeOptions = await _commonHelper.PrepareDropdownDataFromEnumAsync<ErpOrderType>();

            // Prepare AvailableIntegrationStatusTypeOptions dropdown options
            searchModel.AvailableIntegrationStatusTypeOptions = await _commonHelper.PrepareDropdownDataFromEnumAsync<IntegrationStatusType>();

            // Prepare AvailableErpOrderOriginTypeOptions dropdown options
            searchModel.AvailableErpOrderOriginTypeOptions = await _commonHelper.PrepareDropdownDataFromEnumAsync<ErpOrderOriginType>();

            // Prepare AvailableCustomerTypes dropdown options
            searchModel.AvailableCustomerTypes = await _commonHelper.PrepareDropdownDataFromEnumAsync<ErpUserType>();

            searchModel.SetGridPageSize();
            return searchModel;
        }

        public async Task<ErpOrderAdditionalDataModel> PrepareErpOrderPerAccountModel(ErpOrderAdditionalDataModel model, ErpOrderAdditionalData erpOrderAdditional)
        {
            if (erpOrderAdditional != null)
            {
                model = model ?? new ErpOrderAdditionalDataModel();
                model.Id = erpOrderAdditional.Id;
                model.NopOrderId = erpOrderAdditional.NopOrderId;
                model.ErpOrderOriginTypeId = erpOrderAdditional.ErpOrderOriginTypeId;
                model.ErpOrderOriginType = await _localizationService.GetLocalizedEnumAsync(erpOrderAdditional.ErpOrderOriginType);
                model.ErpOrderTypeId = erpOrderAdditional.ErpOrderTypeId;
                model.ErpOrderType = await _localizationService.GetLocalizedEnumAsync(erpOrderAdditional.ErpOrderType);
                model.ErpAccountId = erpOrderAdditional.ErpAccountId;
                model.OrderPlacedByNopCustomerEmail = erpOrderAdditional.OrderPlacedByNopCustomerId > 0 ? (await _customerService.GetCustomerByIdAsync(erpOrderAdditional.OrderPlacedByNopCustomerId))?.Email ?? string.Empty : string.Empty;
                if (string.IsNullOrEmpty(model.ErpAccountName) || string.IsNullOrEmpty(erpOrderAdditional.ErpAccount?.AccountNumber))
                {
                    var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpOrderAdditional.ErpAccountId);
                    if (erpAccount != null)
                    {
                        model.ErpAccountName = string.Concat(erpAccount.AccountName, " (", erpAccount.AccountNumber, ")");
                        model.ErpAccountSalesOrganisationName = erpAccount.ErpSalesOrgId > 0 ? (await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId))?.Name ?? string.Empty : string.Empty;
                    }
                }
                else
                {
                    model.ErpAccountName = erpOrderAdditional.ErpAccount?.AccountName + " (" + erpOrderAdditional.ErpAccount?.AccountNumber + ")";
                    model.ErpAccountSalesOrganisationName = erpOrderAdditional.ErpAccount?.ErpSalesOrgId > 0 ? (await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpOrderAdditional.ErpAccount.ErpSalesOrgId))?.Name ?? string.Empty : string.Empty;
                }

                model.ErpShipToAddressId = erpOrderAdditional.ErpShipToAddressId;
                if (string.IsNullOrEmpty(model.ErpShipToName))
                {
                    var erpShipAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(erpOrderAdditional.ErpShipToAddressId ?? 0);
                    if (erpShipAddress != null)
                    {
                        model.ErpShipToName = erpShipAddress.ShipToName;
                    }
                }
                else
                {
                    model.ErpShipToName = erpOrderAdditional.ErpShipToAddress?.ShipToName;
                }

                var nopOrder = await _orderService.GetOrderByIdAsync(model.NopOrderId);
                model.OrderNumber = nopOrder?.CustomOrderNumber ?? erpOrderAdditional.ErpOrderNumber;

                model.ErpOrderNumber = erpOrderAdditional.ErpOrderNumber;
                model.SpecialInstructions = erpOrderAdditional.SpecialInstructions;
                model.CustomerReference = erpOrderAdditional.CustomerReference;
                model.ERPOrderStatus = erpOrderAdditional.ERPOrderStatus;
                model.ExpectedDeliveryDate = erpOrderAdditional.DeliveryDate;
                model.QuoteExpiryDate = erpOrderAdditional.QuoteExpiryDate;
                model.IntegrationStatusTypeId = erpOrderAdditional.IntegrationStatusTypeId;
                model.IntegrationStatusType = await _localizationService.GetLocalizedEnumAsync(erpOrderAdditional.IntegrationStatusType);
                model.IntegrationError = erpOrderAdditional.IntegrationError;
                model.IntegrationErrorDateTime = !erpOrderAdditional.IntegrationErrorDateTimeUtc.HasValue ? model.IntegrationErrorDateTime
                    : await _dateTimeHelper.ConvertToUserTimeAsync(erpOrderAdditional.IntegrationErrorDateTimeUtc.Value, DateTimeKind.Utc);
                model.LastERPUpdate = !erpOrderAdditional.LastERPUpdateUtc.HasValue ? model.LastERPUpdate
                : await _dateTimeHelper.ConvertToUserTimeAsync(erpOrderAdditional.LastERPUpdateUtc.Value, DateTimeKind.Utc);
                if (erpOrderAdditional.ChangedOnUtc.HasValue)
                    model.ChangedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpOrderAdditional.ChangedOnUtc.Value, DateTimeKind.Utc);
                model.ChangedById = erpOrderAdditional.ChangedById;
                var customer = await _customerService.GetCustomerByIdAsync(erpOrderAdditional.ChangedById);
                model.ChangedByCustomerEmail = customer?.Email;

                if (erpOrderAdditional.QuoteSalesOrderId.HasValue && erpOrderAdditional.QuoteSalesOrderId.Value > 0)
                {
                    model.QuoteSalesOrderId = erpOrderAdditional.QuoteSalesOrderId.Value;

                    var originalQuoteOrder = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByIdAsync(erpOrderAdditional.QuoteSalesOrderId.Value);
                    if (originalQuoteOrder != null)
                    {
                        model.QuoteSalesOrderNumber = originalQuoteOrder.ErpOrderNumber;
                        model.QuoteSalesOrderNopOrderId = originalQuoteOrder.NopOrderId;
                    }
                }
            }
            return model;
        }

        public async Task<ErpOrderModel> PrepareErpOrderModel(ErpOrderModel erpOrderModel, OrderModel orderModel)
        {
            erpOrderModel.Id = orderModel.Id;
            erpOrderModel.HasDownloadableProducts = orderModel.HasDownloadableProducts;
            erpOrderModel.IsLoggedInAsVendor = orderModel.IsLoggedInAsVendor;
            erpOrderModel.AllowCustomersToSelectTaxDisplayType = orderModel.AllowCustomersToSelectTaxDisplayType;
            erpOrderModel.TaxDisplayType = orderModel.TaxDisplayType;
            erpOrderModel.CheckoutAttributeInfo = orderModel.CheckoutAttributeInfo;
            erpOrderModel.CustomOrderNumber = orderModel.CustomOrderNumber;

            foreach (var item in orderModel.Items)
            {
                var erpOrderLineItem = await _erpOrderItemAdditionalDataService.GetErpOrderItemAdditionalDataByNopOrderItemIdAsync(item.Id);
                if (erpOrderLineItem == null)
                    continue;

                var erpOrderItemModel = new ErpOrderItemAdditionalDataModel
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    PictureThumbnailUrl = item.PictureThumbnailUrl,
                    AttributeInfo = item.AttributeInfo,
                    RecurringInfo = item.RecurringInfo,
                    RentalInfo = item.RentalInfo,
                    Sku = item.Sku,
                    VendorName = item.VendorName,
                    ReturnRequests = item.ReturnRequests,
                    PurchasedGiftCardIds = item.PurchasedGiftCardIds,
                    IsDownload = item.IsDownload,
                    DownloadCount = item.DownloadCount,
                    DownloadActivationType = item.DownloadActivationType,
                    IsDownloadActivated = item.IsDownloadActivated,
                    LicenseDownloadGuid = item.LicenseDownloadGuid,
                    UnitPriceInclTax = item.UnitPriceInclTax,
                    UnitPriceInclTaxValue = item.UnitPriceInclTaxValue,
                    UnitPriceExclTax = item.UnitPriceExclTax,
                    UnitPriceExclTaxValue = item.UnitPriceExclTaxValue,
                    Quantity = item.Quantity,
                    DiscountInclTax = item.DiscountInclTax,
                    DiscountInclTaxValue = item.DiscountInclTaxValue,
                    DiscountExclTax = item.DiscountExclTax,
                    DiscountExclTaxValue = item.DiscountExclTaxValue,
                    SubTotalInclTax = item.SubTotalInclTax,
                    SubTotalInclTaxValue = item.SubTotalInclTaxValue,
                    SubTotalExclTax = item.SubTotalExclTax,
                    SubTotalExclTaxValue = item.SubTotalExclTaxValue,

                    NopOrderItemId = item.Id,
                    ERPOrderLineNumber = erpOrderLineItem.ErpOrderLineNumber,
                    ERPSalesUoM = erpOrderLineItem.ErpSalesUoM,
                    ERPOrderLineStatus = erpOrderLineItem.ErpOrderLineStatus,
                    ERPDateRequired = erpOrderLineItem.ErpDateRequired,
                    ERPDateExpected = erpOrderLineItem.ErpDateExpected,
                    ERPDeliveryMethod = erpOrderLineItem.ErpDeliveryMethod,
                    ERPInvoiceNumber = erpOrderLineItem.ErpInvoiceNumber,
                    ERPOrderLineNotes = erpOrderLineItem.ErpOrderLineNotes,
                    LastERPUpdateUtc = erpOrderLineItem.LastErpUpdateUtc,
                };

                erpOrderModel.ErpOrderItems.Add(erpOrderItemModel);
            }
            return erpOrderModel;
        }

        //public async Task<byte[]> DownloadTheOrderPDFAsync(ErpAccount erpAccount, string documentNumber, string type)
        //{
        //    if (erpAccount == null || string.IsNullOrEmpty(documentNumber))
        //        return null;

        //    var accountSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdWithActiveAsync(erpAccount.ErpSalesOrgId);
        //    if (accountSalesOrg == null || string.IsNullOrEmpty(accountSalesOrg.IntegrationClientId) || string.IsNullOrEmpty(accountSalesOrg.Code))
        //    {
        //        if (_b2BB2CFeaturesSettings.EnableLogOnErpCall)
        //        {
        //            _logger.Error($"ERP integration: Error occured while downloading the order PDF. Sales org credential is not provided for Account Number: {erpAccount.AccountNumber}");
        //        }

        //        return null;
        //    }

        //    var baseUrl = accountSalesOrg.ServerBaseURL.TrimEnd('/');
        //    var requestUrl = baseUrl + "/getDocumentForOrder?branch_erpsystem_id=" + accountSalesOrg.IntegrationClientId + "&auth_key=" + accountSalesOrg.AuthenticationKey + "&document=" + documentNumber;

        //    if (type == "quote")
        //    {
        //        requestUrl = baseUrl + "/getDocumentForQuote?branch_erpsystem_id=" + accountSalesOrg.IntegrationClientId + "&auth_key=" + accountSalesOrg.AuthenticationKey + "&document=" + documentNumber;
        //    }

        //    try
        //    {
        //        var httpClient = _httpClientFactory.CreateClient(NopHttpDefaults.DefaultHttpClient);
        //        httpClient.Timeout = TimeSpan.FromMilliseconds(_b2BB2CFeaturesSettings.ERPTimeOutMilliseconds);
        //        var response = httpClient.GetStringAsync(requestUrl).Result;
        //        var responseModel = JsonConvert.DeserializeObject<DownloadInvoiceDocumentResponseModel>(response);

        //        if (response != null && _b2BB2CFeaturesSettings.EnableLogOnErpCall)
        //        {
        //            _logger.InsertLog((Core.Domain.Logging.LogLevel)LogLevel.Info, $"ERP integration: Response found while downloading the order PDF for account: {erpAccount.AccountNumber}" +
        //                $", sales org {accountSalesOrg.Code}, request URL: {requestUrl}", $"request URL: {requestUrl} \n\n Response: {response}");
        //        }

        //        if (responseModel == null || responseModel.ImageBase64 == null || string.IsNullOrEmpty(responseModel.ImageBase64.Trim()))
        //        {
        //            if (_b2BB2CFeaturesSettings.EnableLogOnErpCall)
        //            {
        //                _logger.InsertLog((Core.Domain.Logging.LogLevel)LogLevel.Error, $"ERP integration: Error occured while downloading the order PDF. Response contains no data for account: {erpAccount.AccountNumber}" +
        //                $", sales org {accountSalesOrg.Code}, request URL: {requestUrl}", $"Request URL: {requestUrl} \n\n Response: {response}");
        //            }

        //            return null;
        //        }

        //        return Convert.FromBase64String(responseModel.ImageBase64);
        //    }
        //    catch (Exception ex)
        //    {
        //        if (_b2BB2CFeaturesSettings.EnableLogOnErpCall)
        //        {
        //            _logger.Error($"ERP integration: Exception occured while downloading the order PDF for account: {erpAccount.AccountNumber}" +
        //                $", sales org {accountSalesOrg.Code}, Request URL: {requestUrl}", ex);
        //        }
        //    }

        //    return null;
        //}

        #endregion
    }
}