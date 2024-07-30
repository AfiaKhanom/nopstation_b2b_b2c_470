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

namespace NopStation.Plugin.B2B.IQRetailIntegration.Service
{
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
                AccNo = account.DebtorAccount ?? string.Empty,
                Name = account.DebtorName ?? string.Empty,
                Branch = account.AdditionalAddresses?[0].BranchNumber ?? string.Empty,
                Notes = account.Terms ?? string.Empty,
                Address1 = account.AdditionalAddresses?[0].Address1 ?? string.Empty,
                Address2 = account.AdditionalAddresses?[0].Address2 ?? string.Empty,
                Address3 = account.AdditionalAddresses?[0].Address3 ?? string.Empty,
                Province = account.AdditionalAddresses?[0].Address4 ?? string.Empty,
                Country = account.Area ?? string.Empty,
                PostalCode = account.PostalAddressDetails?[0] ?? string.Empty,
                TelNo = account.TelephoneNumbers?[0] ?? string.Empty,
                EMail = account.EmailAddress ?? string.Empty,
                EMail1 = account.AdditionalAddresses?[0].EmailAddress ?? string.Empty,
                DelName = account.DeliveryRoute ?? string.Empty,
                DelInstruc1 = account.DeliveryAddressDetails?[0] ?? string.Empty,
                DelInstruc2 = account.DeliveryAddressDetails?[0] ?? string.Empty,
                DelInstruc3 = account.DeliveryAddressDetails?[0] ?? string.Empty,
                CompanyNo = account.CompanyRegistrationNumber ?? string.Empty,
                PrefilterFacets = string.Empty,
                VatNumber = account.TaxNumber ?? string.Empty,
                PriceGroupCode = account.PreferredSellPrice ?? string.Empty,
                CreditLimit = account.CreditLimit,
                CreditLimitUsed = account.CreditLimitInsured,
                CreditLimitAvailable = account.CreditLimitReserved,
                Balance = account.BalanceCurrent,
                CreditRepresentativeGroup = account.DebtorSubGroup ?? string.Empty
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
                        AccNo = item.DebtorAccount ?? string.Empty,
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
                        AccNo = item.DebtorAccount ?? string.Empty,
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
                    CustomerReference = invoice.Document.DocumentReference ?? string.Empty,
                    InvoiceNumber = invoice.Document.DocumentNumber ?? string.Empty,
                    InvoiceDate = invoice.Document.InvoiceDate,
                    DocumentNo = invoice.Document.DocumentNumber ?? string.Empty,
                    DocumentType = ErpDocumentType.Invoice.ToString(),
                    Balance = invoice.Document.DocumentTotal,
                    Reference = invoice.Document.DocumentNumber ?? string.Empty,
                    BranchNo = invoice.Document.StoreDepartment ?? string.Empty,
                    BranchName = invoice.Document.DeliveryAddressInformation?[0] ?? string.Empty,
                    BranchShort = invoice.Document.DeliveryAddressInformation?[1] ?? string.Empty,
                    OrderNo = invoice.Document.OrderNumber ?? string.Empty,
                    Status = string.Empty,
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
                orderModel.AccNo = order.Document?.DebtorAccount ?? string.Empty;
                orderModel.Location = order.Document?.DeliveryAddressInformation?[0] ?? string.Empty;
                orderModel.User = order.Document?.DebtorAccount ?? string.Empty;
                orderModel.Reference = order.Document?.OrderNumber;
                orderModel.DateRequired = DateTime.Now.AddDays(7);
                orderModel.RepCode = order.Document?.SalesRepresentativeNumber.ToString() ?? string.Empty;
                orderModel.ShippingAddress = new ErpAddressModel
                {
                    AddressLine1 = order.Document?.DeliveryAddressInformation?[0] ?? string.Empty,
                    AddressLine2 = order.Document?.DeliveryAddressInformation?[0] ?? string.Empty,
                    AddressLine3 = order.Document?.DeliveryAddressInformation?[0] ?? string.Empty,
                    PostalCode = order.Document?.DeliveryAddressInformation?[0] ?? string.Empty
                };
                 
                orderModel.CustomerFirstName = defaultCustomer?.FirstName ?? string.Empty;
                orderModel.CustomerLastName = defaultCustomer?.LastName ?? string.Empty;
                orderModel.CustomerName = $"{defaultCustomer?.FirstName} {defaultCustomer?.LastName}";
                orderModel.CustomerNumber = order.Document?.DebtorAccount ?? string.Empty;
                orderModel.CustomerPhoneNumber = defaultCustomer?.Phone ?? string.Empty;
                orderModel.CustomerMobileNumber = defaultCustomer?.Phone ?? string.Empty;
                orderModel.CustomerEmail = defaultCustomer?.Email ?? string.Empty;
                orderModel.CustomerReference = order.Document?.DocumentReference ?? string.Empty;
                orderModel.Notes = order.Document?.DeliveryNoteNumber ?? string.Empty;
                orderModel.DelInstruction1 = order.Document?.DeliveryRoute ?? string.Empty;
                orderModel.OrderCategory = string.Empty;
                orderModel.DelMethod = order.Document?.DeliveryMethod ?? string.Empty;
                orderModel.TaxNumber = order.Document?.VatNumber ?? string.Empty;
                orderModel.OrderType = orderType;
                orderModel.QuoteNumber = order.Document?.DocumentNumber ?? string.Empty;
                orderModel.Total_Vat = order.Document?.TotalVat ?? 0;
                orderModel.Total_Excl = order.Document?.DocumentTotal ?? 0;
                var orderDate = DateTime.Now;
                DateTime.TryParse(order.Document?.OrderInformation?.OrderDate, out orderDate);
                var deliveryDate = DateTime.Now;
                DateTime.TryParse(order.Document?.OrderInformation?.ExpectedDate, out deliveryDate);

                orderModel.OrderDate = orderDate;
                orderModel.DeliveryDate = deliveryDate;
                orderModel.Currency = order.Document?.Currency ?? string.Empty;
                orderModel.ErpPlaceOrderItemDatas = order.Items.Select(item => new ErpPlaceOrderItemDataModel
                {
                    ItemNo = item.StockCode ?? string.Empty,
                    LineNo = 0,
                    BatchCode = item.StockCode ?? string.Empty,
                    Description = item.StockDescription ?? string.Empty,
                    Quantity = item.Quantity,
                    UOM = item.Volumetric?.Units.ToString() ?? string.Empty,
                    SpecInstruct = item.Comment ?? string.Empty,
                    UnitPrice = item.ListPrice,
                    Discount = item.DiscountPercentage,
                    LineTotalExcl = item.LineTotalExclusive,
                    LineTotalIncl = item.LineTotalInclusive
                }).ToList();
                erpOrders.Add(orderModel);
            }


            return erpOrders;
        }

        public async Task<IList<ErpProductDataModel>> ErpStockMapNop(IList<ErpStockRecordModel> erpStockResponses)
        {
            var erpStocks = (await Task.WhenAll(erpStockResponses.Select(async stocks =>
            {
                return new ErpProductDataModel
                {
                    ItemNo = stocks.Code ?? string.Empty,
                    MasterCode = stocks.Code ?? string.Empty,
                    Description = stocks.Description ?? string.Empty,
                    IsSpecial = false,
                    FullDescription = stocks.Description ?? string.Empty,
                    SellingPriceA = stocks.SellPrice1,
                    UnitOfMeasure = string.Empty,
                    VatRate = stocks.VatRate ?? string.Empty,
                    Active = string.Empty,
                    VendorName = string.Empty,
                    Brand = stocks.BrandName,
                    BrandDesc = stocks.AlternativeDescription ?? string.Empty,
                    Categories = new List<ErpProductCategory>()
                    {
                        new ()
                        {
                            CategoryCode =  string.Empty,
                            CategoryName = stocks.DepartmentName ?? string.Empty
                        },
                        new ()
                        {
                            CategoryCode =  string.Empty,
                            CategoryName = stocks.GroupName ?? string.Empty
                        },
                        new ()
                        {
                            CategoryCode =  string.Empty,
                            CategoryName = stocks.CategoryName ?? string.Empty
                        }
                    },
                    Attributes = new List<KeyValuePair<string, string>>()
                    {
                        new (nameof(ErpStockRecordModel.Colour), stocks.Colour),
                        new (nameof(ErpStockRecordModel.Size), stocks.Size)
                    }
                };
            }))).ToList();

            return erpStocks;
        }

        public async Task<IList<ErpPriceSpecialPricingDataModel>> ErpSpecialPriceMapNop(IList<ErpStockRecordModel> erpSpecialPriceResponses)
        {
            var erpSpecialPrices = erpSpecialPriceResponses.Select(stocks => new ErpPriceSpecialPricingDataModel
            {
                AccNo = stocks.Code ?? string.Empty,
                ItemNo = stocks.Code ?? string.Empty,
                Branch = string.Empty,
                SellingPrice = stocks.SellPrice1,
                AccountPrice = stocks.SellPrice2,
                PromoPrice = stocks.SellPrice3,
                ListPrice = stocks.SellPrice4,
                RetailPrice = stocks.SellPrice5,
                DiscountPerc = stocks.MaximumDiscount,
                PricingNotes = stocks.Notes ?? string.Empty
            }).ToList();

            return erpSpecialPrices;
        }

        public async Task<IList<ErpPriceGroupPricingDataModel>> ErpGroupPriceMapNop(IList<ErpStockRecordModel> erpGroupPriceResponses)
        {
            var erpGroupPrices = erpGroupPriceResponses.Select(stocks => new ErpPriceGroupPricingDataModel
            {
                ItemNo = stocks.Code ?? string.Empty,
                DiscountPerc = stocks.OnHand,
                SellingPrice = stocks.SellPrice1,
                Prices = new Dictionary<string, decimal?>
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
}