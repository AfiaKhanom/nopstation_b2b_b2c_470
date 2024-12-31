using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Orders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Debtor;
using NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Invoice;
using NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Stock;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Service;

public class ErpNopMapperService : IErpNopMapperService
{
    #region Fields

    private readonly IOrderService _orderService;
    private readonly IProductService _productService;
    private readonly IAddressService _addressService;
    private readonly ICountryService _countryService;
    private readonly ICustomerService _customerService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpNopUserService _userService;
    private readonly IQRetailIntegrationSettings _iQRetailIntegrationSettings;
    private const string FIRST_SHIP_TO_CODE = "_1";
    private const string SECOND_SHIP_TO_CODE = "_2";
    private const string SELLPRICE1 = "SellPrice1";
    private const string SELLPRICE2 = "SellPrice2";
    private const string SELLPRICE3 = "SellPrice3";
    private const string SELLPRICE4 = "SellPrice4";
    private const string SELLPRICE5 = "SellPrice5";
    private const string SELLPRICE6 = "SellPrice6";
    private const string SELLPRICE7 = "SellPrice7";
    private const string SELLPRICE8 = "SellPrice8";
    private const string SELLPRICE9 = "SellPrice9";
    private const string SELLPRICE10 = "SellPrice10";

    #endregion

    #region Ctor
    public ErpNopMapperService(
        IOrderService orderService,
        IAddressService addressService,
        ICountryService countryService,
        ICustomerService customerService,
        IProductService productService,
        IStateProvinceService stateProvinceService,
        IErpAccountService erpAccountService,
        IQRetailIntegrationSettings iQRetailIntegrationSettings,
        IErpNopUserService userService)
    {
        _orderService = orderService;
        _addressService = addressService;
        _countryService = countryService;
        _customerService = customerService;
        _productService = productService;
        _stateProvinceService = stateProvinceService;
        _erpAccountService = erpAccountService;
        _iQRetailIntegrationSettings = iQRetailIntegrationSettings;
        _userService = userService;
    }

    #endregion

    #region Method

    public async Task<IList<ErpAccountDataModel>> ErpAccountMapNop(IList<DebtorsMasterModel> debtorsMasters)
    {
        var erpAccounts = debtorsMasters.Select(account => new ErpAccountDataModel
        {
            AccountNumber = account.DebtorAccount ?? string.Empty,
            AccountName = account.DebtorName ?? string.Empty,
            Address1 = account.AdditionalAddresses?[0].Address1 ?? string.Empty,
            Address2 = account.AdditionalAddresses?[0].Address2 ?? string.Empty,
            Address3 = account.AdditionalAddresses?[0].Address3 ?? string.Empty,
            StateProvince = account.AdditionalAddresses?[0].Address4 ?? string.Empty,
            Country = account.Area ?? string.Empty,
            ZipPostalCode = account.PostalAddressDetails?[0] ?? string.Empty,
            PhoneNumber = account.TelephoneNumbers?[0] ?? string.Empty,
            Email = account.EmailAddress ?? string.Empty,
            DeliveryRoute = account.DeliveryRoute ?? string.Empty,
            CompanyNo = account.CompanyRegistrationNumber ?? string.Empty,
            PreFilterFacets = string.Empty,
            VatNumber = account.TaxNumber ?? string.Empty,
            PriceGroupCode = account.PreferredSellPrice ?? string.Empty,
            CreditLimit = account.CreditLimit,
            CreditLimitUsed = account.CreditLimitInsured,
            CreditLimitAvailable = account.CreditLimitReserved,
            CurrentBalance = account.BalanceCurrent
        }).ToList();


        return erpAccounts;
    }

    public async Task<IList<ErpShipToAddressDataModel>> ErpShipToAddressMapNop(IList<DebtorsMasterModel> erpShipToAddressResponse)
    {

        var erpShipToAddresses =new List<ErpShipToAddressDataModel>();
         foreach (var item in erpShipToAddressResponse)
        {
            if ((bool)item.PostalAddressDetails?.Any(a => !string.IsNullOrEmpty(a)))
            {
                var erpShiptoAddress = new ErpShipToAddressDataModel
                {
                    AccountNumber = item.DebtorAccount ?? string.Empty,
                    ShipToCode = $"{item.DebtorAccount ?? string.Empty}{FIRST_SHIP_TO_CODE}",
                    ShipToName = item.DebtorName ?? string.Empty,
                    Company = item.CompanyRegistrationNumber ?? string.Empty,
                    Address1 = item.PostalAddressDetails?[0] ?? string.Empty,
                    Address2 = item.PostalAddressDetails?[1] ?? string.Empty,
                    City = item.PostalAddressDetails?[2] ?? string.Empty,
                    StateProvince = item.PostalAddressDetails?[3] ?? string.Empty,
                    ZipPostalCode = item.PostalAddressDetails?[4] ?? string.Empty,
                    County = item.Area ?? string.Empty,
                    Suburb = item.Area ?? string.Empty,
                    PhoneNumber = item.TelephoneNumbers?[0] ?? string.Empty,
                    FaxNumber = item.FaxNumber ?? string.Empty,
                    CustomAttributes = string.Empty,
                    EmailAddress = item.EmailAddress ?? string.Empty,
                    RepNumber = item.NormalRepresentative.ToString() ?? string.Empty,
                    RepFullName = string.Empty,
                    RepPhoneNumber = string.Empty,
                    RepEmail = string.Empty
                };
                erpShipToAddresses.Add(erpShiptoAddress);
            }

            if ((bool)item.DeliveryAddressDetails?.Any(a => !string.IsNullOrEmpty(a)))
            {
                var erpShiptoAddress = new ErpShipToAddressDataModel
                {
                    AccountNumber = item.DebtorAccount ?? string.Empty,
                    ShipToCode = $"{item.DebtorAccount ?? string.Empty}{SECOND_SHIP_TO_CODE}",
                    ShipToName = item.DebtorName ?? string.Empty,
                    Company = item.CompanyRegistrationNumber ?? string.Empty,
                    Address1 = item.DeliveryAddressDetails?[0] ?? string.Empty,
                    Address2 = item.DeliveryAddressDetails?[1] ?? string.Empty,
                    City = item.DeliveryAddressDetails?[2] ?? string.Empty,
                    StateProvince = item.DeliveryAddressDetails?[3] ?? string.Empty,
                    ZipPostalCode = item.DeliveryAddressDetails?[4] ?? string.Empty,
                    County = item.Area ?? string.Empty,
                    Suburb = item.Area ?? string.Empty,
                    PhoneNumber = item.TelephoneNumbers?[0] ?? string.Empty,
                    FaxNumber = item.FaxNumber ?? string.Empty,
                    CustomAttributes = string.Empty,
                    EmailAddress = item.EmailAddress ?? string.Empty,
                    RepNumber = item.NormalRepresentative.ToString() ?? string.Empty,
                    RepFullName = string.Empty,
                    RepPhoneNumber = string.Empty,
                    RepEmail = string.Empty
                };
                erpShipToAddresses.Add(erpShiptoAddress);
            }
        }
          
        return erpShipToAddresses;
    }

    public async Task<IList<ErpInvoiceDataModel>> ErpInvoiceMapNop(IList<ProcessingDocumentModel> erpInvoicesResponse)
    {
        var erpInvoiceByAccount = new List<ErpInvoiceDataModel>();

        foreach (var invoice in erpInvoicesResponse)
        {

            var erpInvoiceDataModel = new ErpInvoiceDataModel
            {
                RelatedDocumentNo = invoice.Document.DocumentReference ?? string.Empty,
                ErpDocumentNumber = invoice.Document.DocumentNumber ?? string.Empty,
                DocumentDateUtc = invoice.Document.InvoiceDate,
                DocumentType = ErpDocumentType.Invoice.ToString(),
                AmountExclVat = invoice.Document.DocumentTotal,
                ErpOrderNumber = invoice.Document.OrderNumber ?? string.Empty,
                Items = new List<ErpOrderItemAdditionalData>()
            };

            var nopOrder = await _orderService.GetOrderByCustomOrderNumberAsync(invoice.Document.OrderNumber) ?? new Order();
            var nopOrderItems = await _orderService.GetOrderItemsAsync(orderId: nopOrder?.Id ?? 0) ?? new List<OrderItem>();

            erpInvoiceDataModel.Items = (await Task.WhenAll(invoice.Items.Select(async item =>
            {
                var nopProduct = await _productService.GetProductBySkuAsync(item.StockCode) ?? new Product();
                var nopOderItem = nopOrderItems.FirstOrDefault(a => a.ProductId == nopProduct.Id);

                return new ErpOrderItemAdditionalData()
                {
                    ErpInvoiceNumber = invoice.Document.DocumentNumber ?? string.Empty,
                    NopOrderItemId = nopOderItem?.Id ?? 0,
                    ErpOrderLineNotes = invoice.Document.DeliveryNoteNumber ?? string.Empty,
                    LastErpUpdateUtc = DateTime.UtcNow,
                    ChangedOnUtc = DateTime.UtcNow,
                    ChangedBy = 1
                };
            }))).ToList();

            erpInvoiceByAccount.Add(erpInvoiceDataModel);
        }

        return erpInvoiceByAccount;
    }

    public async Task<IList<ErpPlaceOrderDataModel>> ErpOrderMapNop(IList<ProcessingDocumentModel> erpOrdersResponse)
    {
        var erpOrders = new List<ErpPlaceOrderDataModel>();

        if (!erpOrdersResponse.Any())
        {
            return erpOrders;
        }
        var orderList = new List<ErpPlaceOrderDataModel>();

        foreach (var order in erpOrdersResponse)
        {
            var erpAccount = await _erpAccountService.GetErpAccountByErpAccountNumberAsync(erpOrdersResponse[0].Document.DebtorAccount);
             var orderType = "";
            var defaultCustomer = await _customerService.GetCustomerByEmailAsync(order.Document?.EmailAddress);
            if (defaultCustomer == null)
            {
                defaultCustomer = await _customerService.GetCustomerByIdAsync(_iQRetailIntegrationSettings.DefaultCustomerId);
                if (order.ExportClass == IQRetailIntegrationDefaults.ExportClassSalesOrder)
                    orderType = ErpOrderType.B2BSalesOrder.ToString();
                if (order.ExportClass == IQRetailIntegrationDefaults.ExportClassQuote)
                    orderType = ErpOrderType.B2BQuote.ToString();
            }
            else
            {

                var user = await _userService.GetErpNopUserByCustomerIdAsync(defaultCustomer.Id);
                if (user == null || user.ErpUserType == ErpUserType.B2BUser)
                {
                    await _customerService.GetCustomerByIdAsync(_iQRetailIntegrationSettings.DefaultCustomerId);
                    if (order.ExportClass == IQRetailIntegrationDefaults.ExportClassSalesOrder)
                        orderType = ErpOrderType.B2BSalesOrder.ToString();
                    if (order.ExportClass == IQRetailIntegrationDefaults.ExportClassQuote)
                        orderType = ErpOrderType.B2BQuote.ToString();
                }
                else
                {
                    await _customerService.GetCustomerByIdAsync(_iQRetailIntegrationSettings.DefaultCustomerId);
                    if (order.ExportClass == IQRetailIntegrationDefaults.ExportClassSalesOrder)
                        orderType = ErpOrderType.B2CSalesOrder.ToString();
                    if (order.ExportClass == IQRetailIntegrationDefaults.ExportClassQuote)
                        orderType = ErpOrderType.B2CQuote.ToString();

                }
            }


            var orderModel = new ErpPlaceOrderDataModel();
            orderModel.AccountNumber = order.Document?.DebtorAccount ?? string.Empty;
            orderModel.Location = order.Document?.DeliveryAddressInformation?[0] ?? string.Empty;
            orderModel.CustomOrderNumber = order.Document?.OrderNumber;
            orderModel.DateRequired = DateTime.Now.AddDays(7);
            orderModel.RepCode = order.Document?.SalesRepresentativeNumber.ToString() ?? string.Empty;
            orderModel.ShippingAddress = new ErpAddressModel
            {
                Address1 = order.Document?.DeliveryAddressInformation?[0] ?? string.Empty,
                Address2 = order.Document?.DeliveryAddressInformation?[0] ?? string.Empty,
                Address3 = order.Document?.DeliveryAddressInformation?[0] ?? string.Empty,
                ZipPostalCode = order.Document?.DeliveryAddressInformation?[0] ?? string.Empty
            };
             
            orderModel.CustomerFirstName = defaultCustomer?.FirstName ?? string.Empty;
            orderModel.CustomerLastName = defaultCustomer?.LastName ?? string.Empty;
            orderModel.CustomerName = $"{defaultCustomer?.FirstName} {defaultCustomer?.LastName}";
            orderModel.CustomerPhoneNumber = defaultCustomer?.Phone ?? string.Empty;
            orderModel.CustomerMobileNumber = defaultCustomer?.Phone ?? string.Empty;
            orderModel.CustomerEmail = defaultCustomer?.Email ?? string.Empty;
            orderModel.CustomerReference = order.Document?.DocumentReference ?? string.Empty;
            orderModel.Notes = order.Document?.DeliveryNoteNumber ?? string.Empty;
            orderModel.DeliveryInstruction = order.Document?.DeliveryRoute ?? string.Empty;
            orderModel.OrderCategory = string.Empty;
            orderModel.DeliveryMethod = order.Document?.DeliveryMethod ?? string.Empty;
            orderModel.VatNumber = order.Document?.VatNumber ?? string.Empty;
            orderModel.OrderType = orderType;
            orderModel.QuoteNumber = order.Document?.DocumentNumber ?? string.Empty;
            orderModel.OrderSubtotalInclTax = order.Document?.TotalVat ?? 0;
            orderModel.OrderSubtotalExclTax = order.Document?.DocumentTotal ?? 0;
            var orderDate = DateTime.Now;
            DateTime.TryParse(order.Document?.OrderInformation?.OrderDate, out orderDate);
            var deliveryDate = DateTime.Now;
            DateTime.TryParse(order.Document?.OrderInformation?.ExpectedDate, out deliveryDate);

            orderModel.OrderDate = orderDate;
            orderModel.DeliveryDate = deliveryDate;
            orderModel.CustomerCurrencyCode = order.Document?.Currency ?? string.Empty;
            orderModel.ErpPlaceOrderItemDatas = order.Items.Select(item => new ErpPlaceOrderItemDataModel
            {
                Sku = item.StockCode ?? string.Empty,
                BatchCode = item.StockCode ?? string.Empty,
                Description = item.StockDescription ?? string.Empty,
                Quantity = item.Quantity,
                UnitOfMeasure = item.Volumetric?.Units.ToString() ?? string.Empty,
                SpecialInstruction = item.Comment ?? string.Empty,
                UnitPriceExclTax = item.ListPrice,
                UnitPriceInclTax = item.DiscountPercentage,
                PriceExclTax = item.LineTotalExclusive,
                PriceInclTax = item.LineTotalInclusive
            }).ToList();
            erpOrders.Add(orderModel);
        }


        return erpOrders;
    }

    public async Task<IList<ErpProductDataModel>> ErpProductMapNop(IList<ErpStockRecordModel> erpStockResponses)
    {
        var erpStocks = (await Task.WhenAll(erpStockResponses.Select(async stocks =>
        {
            return new ErpProductDataModel
            {
                Name = stocks.Description ?? string.Empty,
                Sku = stocks.Code ?? string.Empty,
                ShortDescription = stocks.Description ?? string.Empty,
                FullDescription = stocks.Description ?? string.Empty,
                Price = stocks.SellPrice1,
                ManufacturerName = stocks.BrandName,
                VendorCode = string.Empty,
                VendorName = string.Empty,
                TaxCategoryId = 0,
                Published = "true",
                ProductCategories = new List<ErpCategoryDataModel>()
                {
                    new ()
                    {
                        CategoryName = stocks.DepartmentName ?? string.Empty
                    },
                    new ()
                    {
                        CategoryName = stocks.GroupName ?? string.Empty
                    },
                    new ()
                    {
                        CategoryName = stocks.CategoryName ?? string.Empty
                    }
                },
                ProductAttributes = new List<KeyValuePair<string, string>>()
                {
                    new (nameof(ErpStockRecordModel.Colour), stocks.Colour),
                    new (nameof(ErpStockRecordModel.Size), stocks.Size)
                }
            };
        }))).ToList();

        return erpStocks;
    }

    public async Task<IList<ErpStockDataModel>> ErpStockMapNop(IList<ErpStockRecordModel> erpStockResponses)
    {
        var erpStocks = (await Task.WhenAll(erpStockResponses.Select(async stocks =>
        {
            return new ErpStockDataModel
            {
                WarehouseNameOrCode = string.Empty,
                Sku = stocks.Code ?? string.Empty,
                SalesOrgCode = string.Empty,
                QuantityOnHand = stocks.OnHand,
                LastChangedDate = DateTime.MinValue
            };
        }))).ToList();

        return erpStocks;
    }

    public async Task<IList<ErpPriceSpecialPricingDataModel>> ErpSpecialPriceMapNop(IList<ErpStockRecordModel> erpSpecialPriceResponses)
    {
        var erpSpecialPrices = erpSpecialPriceResponses.Select(stocks => new ErpPriceSpecialPricingDataModel
        {
            AccountNumber = stocks.Code ?? string.Empty,
            Sku = stocks.Code ?? string.Empty,
            Branch = string.Empty,
            SpecialPrice = stocks.SellPrice1,
            SellingPrice = stocks.SellPrice2,
            PromoPrice = stocks.SellPrice3,
            ListPrice = stocks.SellPrice4,
            RetailPrice = stocks.SellPrice5,
            DiscountPercentage = stocks.MaximumDiscount,
            PricingNotes = stocks.Notes ?? string.Empty
        }).ToList();

        return erpSpecialPrices;
    }

    public async Task<IList<ErpPriceGroupPricingDataModel>> ErpGroupPriceMapNop(IList<ErpStockRecordModel> erpGroupPriceResponses)
    {
        var erpGroupPrices = erpGroupPriceResponses.Select(stocks => new ErpPriceGroupPricingDataModel
        {
            Sku = stocks.Code ?? string.Empty,
            Price = stocks.SellPrice1,
            GroupPrices = new Dictionary<string, decimal?>
            {
                { string.IsNullOrEmpty( _iQRetailIntegrationSettings.SellPrice1) ? SELLPRICE1 : _iQRetailIntegrationSettings.SellPrice1, stocks.SellPrice1},
                { string.IsNullOrEmpty( _iQRetailIntegrationSettings.SellPrice2) ? SELLPRICE2 : _iQRetailIntegrationSettings.SellPrice2, stocks.SellPrice2},
                { string.IsNullOrEmpty( _iQRetailIntegrationSettings.SellPrice3) ? SELLPRICE3 : _iQRetailIntegrationSettings.SellPrice3, stocks.SellPrice3},
                { string.IsNullOrEmpty( _iQRetailIntegrationSettings.SellPrice4) ? SELLPRICE4 : _iQRetailIntegrationSettings.SellPrice4, stocks.SellPrice4},
                { string.IsNullOrEmpty( _iQRetailIntegrationSettings.SellPrice5) ? SELLPRICE5 : _iQRetailIntegrationSettings.SellPrice5, stocks.SellPrice5},
                { string.IsNullOrEmpty( _iQRetailIntegrationSettings.SellPrice6) ? SELLPRICE6 : _iQRetailIntegrationSettings.SellPrice6, stocks.SellPrice6},
                { string.IsNullOrEmpty( _iQRetailIntegrationSettings.SellPrice7) ? SELLPRICE7 : _iQRetailIntegrationSettings.SellPrice7, stocks.SellPrice7},
                { string.IsNullOrEmpty( _iQRetailIntegrationSettings.SellPrice8) ? SELLPRICE8 : _iQRetailIntegrationSettings.SellPrice8, stocks.SellPrice8},
                { string.IsNullOrEmpty( _iQRetailIntegrationSettings.SellPrice9) ? SELLPRICE9 : _iQRetailIntegrationSettings.SellPrice9, stocks.SellPrice9},
                { string.IsNullOrEmpty( _iQRetailIntegrationSettings.SellPrice10) ? SELLPRICE10 : _iQRetailIntegrationSettings.SellPrice10, stocks.SellPrice10}
            }
        }).ToList();

        return erpGroupPrices;
    }

    #endregion
}