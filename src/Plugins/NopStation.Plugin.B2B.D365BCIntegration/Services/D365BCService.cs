using Newtonsoft.Json;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using System.Text;
using NopStation.Plugin.B2B.D365BCIntegration.Models;

namespace NopStation.Plugin.B2B.D365BCIntegration.Services;

public class D365BCService : ID365BCService
{
    #region Fields
    private readonly D365BCIntegrationSettings _settings;
    private readonly ID365BCHttpService _d365BCHttpService;
    #endregion

    #region Ctor

    public D365BCService(D365BCIntegrationSettings settings,
        ID365BCHttpService d365BCHttpService,
        D365BCHttpClient d365HttpClient)
    {
        _settings = settings;
        _d365BCHttpService = d365BCHttpService;
    }

    #endregion

    #region Utilities

    private string PrepareODataUrl(string bcObjectName, string? filter = null)
    {
        var baseUrl = _settings.BaseApiUrl ?? D365BCIntegrationDefaults.DefaultBaseApiUrl;
        var apiUrl = string.Join("/", 
            baseUrl, 
            _settings.TenantId, 
            _settings.Environment, 
            D365BCIntegrationDefaults.DefaultApiProtocol, 
            bcObjectName);

        return string.IsNullOrEmpty(filter) ? apiUrl : string.Join("?", apiUrl, filter);
    }

    private List<ErpPlaceOrderDataModel> MapD365OrdersToErpOrders(List<D365BCOrderModel> d365Orders)
    {
        return d365Orders.Select(o => new ErpPlaceOrderDataModel
        {
            AccountNumber = o.Sell_to_Customer_No,
            CustomOrderNumber = o.No,
            CustomerName = o.Sell_to_Customer_Name,
            OrderDate = DateTime.Parse(o.Order_Date),
            CustomerEmail = o.Sell_to_E_Mail,
            CustomerPhoneNumber = o.Sell_to_Phone_No,
            ShippingAddress = new ErpAddressModel
            {
                Name = o.Ship_to_Name,
                Address1 = o.Ship_to_Address,
                Address2 = o.Ship_to_Address_2,
                City = o.Ship_to_City,
                StateProvince = o.Ship_to_County,
                ZipPostalCode = o.Ship_to_Post_Code,
                Country = o.Ship_to_Country_Region_Code,
                PhoneNumber = o.Ship_to_Phone_No
            },
            BillingAddress = new ErpAddressModel
            {
                Name = o.Bill_to_Name,
                Address1 = o.Bill_to_Address,
                Address2 = o.Bill_to_Address_2,
                City = o.Bill_to_City,
                StateProvince = o.Bill_to_County,
                ZipPostalCode = o.Bill_to_Post_Code,
                Country = o.Bill_to_Country_Region_Code,
                PhoneNumber = o.BillToContactPhoneNo
            }
            // Add other mappings as needed
        }).ToList();
    }

    #endregion

    #region Methods

    public bool IsValidD365BCIntegrationSettings(D365BCIntegrationSettings settings)
    {
        if (string.IsNullOrEmpty(settings.ClientId) || string.IsNullOrEmpty(settings.ClientSecret) ||
            string.IsNullOrEmpty(settings.BaseApiUrl) || string.IsNullOrEmpty(settings.CompanyName))
            return false;

        if (!Uri.IsWellFormedUriString(settings.BaseApiUrl, UriKind.Absolute))
            return false;

        return true;
    }

    public async Task<ErpResponseData<IList<ErpAccountDataModel>>> GetCustomersFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<IList<ErpAccountDataModel>>();
        var responseContent = string.Empty;
        var allCustomers = new List<D365Customer>();

        try
        {
            // First get total count of modified customers
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");


            var countFilterTerm = $"$filter=Last_Date_Modified ge {sevenDaysAgo}&$count=true&$top=0";
            var countEndpoint = PrepareODataUrl(string.Format(D365BCIntegrationDefaults.CustomersEndpoint,
                _settings.CompanyName), countFilterTerm);

            var countResponse = await _d365BCHttpService.GetAsync(countEndpoint, ErpSyncLevel.Account);

            var countContent = await countResponse.Content.ReadAsStringAsync();
            var countObject = JsonConvert.DeserializeObject<D365CountResponse>(countContent);
            var totalCount = countObject.Count;

            if (totalCount == 0 || erpRequest.Start == "-1")
            {
                erpResponseData.Data = null;
                return erpResponseData;
            }

            // Get customers in batches
            var batchSize = _settings.CustomerSyncLimit > 0 ? _settings.CustomerSyncLimit : 100;
            var currentSkip = 0;

            while (currentSkip < totalCount)
            {
                var apiFilterTerm = $"$filter=Last_Date_Modified ge {sevenDaysAgo}&$top={batchSize}&$skip={currentSkip}";
                var apiEndPoint = PrepareODataUrl(string.Format(D365BCIntegrationDefaults.CustomersEndpoint, _settings.CompanyName),
                    apiFilterTerm);

                var response = await _d365BCHttpService.GetAsync(apiEndPoint, ErpSyncLevel.Account);
                responseContent = await response.Content.ReadAsStringAsync();
                var dynamicsResponse = JsonConvert.DeserializeObject<D365CustomersResponse>(responseContent);

                if (dynamicsResponse?.Value != null)
                    allCustomers.AddRange(dynamicsResponse.Value);

                currentSkip += batchSize;
            }

            erpResponseData.Data = allCustomers.Select(c => new ErpAccountDataModel
            {
                AccountNumber = c.No,
                AccountName = c.Name,
                Address1 = c.Address,
                Address2 = c.Address_2,
                City = c.City,
                StateProvince = c.County,
                ZipPostalCode = c.Post_Code,
                Country = c.Country_Region_Code,
                PhoneNumber = c.Phone_No,
                Email = c.E_Mail,
                CompanyNo = c.Registration_Number,
                VatNumber = c.VAT_Registration_No,
                CreditLimit = c.Credit_Limit_LCY
            }).ToList();
        }
        catch (Exception ex)
        {
            erpResponseData.ErpResponseModel.IsError = true;
            erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
            erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
        }
        erpResponseData.ErpResponseModel.Next = "-1";
        return erpResponseData;
    }

    public async Task<ErpResponseData<ErpAccountDataModel>> GetCustomerByAccountNumberFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<ErpAccountDataModel>();
        var responseContent = string.Empty;

        try
        {
            var apiFilterTerm = $"$filter=No eq '{erpRequest.AccountNumber}'";

            var endpoint = PrepareODataUrl($"{string.Format(D365BCIntegrationDefaults.CustomersEndpoint, _settings.CompanyName)}", apiFilterTerm);

            var response = await _d365BCHttpService.GetAsync(endpoint, ErpSyncLevel.Account);

            responseContent = await response.Content.ReadAsStringAsync();
            var dynamicsResponse = JsonConvert.DeserializeObject<D365CustomersResponse>(responseContent);

            if (dynamicsResponse?.Value == null || !dynamicsResponse.Value.Any())
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = $"Customer not found with account number {erpRequest.AccountNumber}";
                return erpResponseData;
            }

            var customer = dynamicsResponse.Value.First();
            erpResponseData.Data = new ErpAccountDataModel
            {
                AccountNumber = customer.No,
                AccountName = customer.Name,
                Address1 = customer.Address,
                Address2 = customer.Address_2,
                City = customer.City,
                StateProvince = customer.County,
                ZipPostalCode = customer.Post_Code,
                Country = customer.Country_Region_Code,
                PhoneNumber = customer.Phone_No,
                Email = customer.E_Mail,
                CompanyNo = customer.Registration_Number,
                VatNumber = customer.VAT_Registration_No,
                CreditLimit = customer.Credit_Limit_LCY
            };
        }
        catch (Exception ex)
        {
            erpResponseData.ErpResponseModel.IsError = true;
            erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
            erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
        }
        erpResponseData.ErpResponseModel.Next = "-1";
        return erpResponseData;
    }

    public async Task<ErpResponseData<IList<ErpProductDataModel>>> GetProductsFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<IList<ErpProductDataModel>>();
        var responseContent = string.Empty;
        var allProducts = new List<D365BCProductModel>();

        try
        {
            // Get total count of modified products
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");

            var countFilterTerm = $"$filter=Last_Date_Modified gt {sevenDaysAgo}";
            var countEndpoint = PrepareODataUrl(string.Format(D365BCIntegrationDefaults.ProductCountEndpoint, _settings.CompanyName), 
                countFilterTerm);

            var countResponse = await _d365BCHttpService.GetAsync(countEndpoint, ErpSyncLevel.Product);
            var countContent = await countResponse.Content.ReadAsStringAsync();
            var totalCount = int.Parse(countContent);

            if (totalCount == 0 || erpRequest.Start == "-1")
            {
                erpResponseData.Data = null;
                return erpResponseData;
            }

            // Get products in batches
            var batchSize = _settings.ProductSyncLimit > 0 ? _settings.ProductSyncLimit : 100;
            var currentSkip = 0;

            while (currentSkip < totalCount)
            {
                var apiFilterTerm = $"$filter=Last_Date_Modified ge {sevenDaysAgo}&$top={batchSize}&$skip={currentSkip}";
                var apiEndPoint = PrepareODataUrl(string.Format(D365BCIntegrationDefaults.ProductEndpoint, _settings.CompanyName), apiFilterTerm);

                var response = await _d365BCHttpService.GetAsync(apiEndPoint, ErpSyncLevel.Product);
                responseContent = await response.Content.ReadAsStringAsync();
                var dynamicsResponse = JsonConvert.DeserializeObject<D365ProductsResponse>(responseContent);

                if (dynamicsResponse?.Value != null)
                    allProducts.AddRange(dynamicsResponse.Value);

                currentSkip += batchSize;
            }

            erpResponseData.Data = allProducts.Select(p => new ErpProductDataModel
            {
                Name = p.Description,
                Sku = p.No,
                ManufacturerPartNumber = p.Vendor_Item_No,
                ShortDescription = p.Description,
                FullDescription = string.IsNullOrEmpty(p.Description_2)
                    ? p.Description
                    : $"{p.Description} - {p.Description_2}",
                Price = p.Unit_Price,
                StockQuantity = p.InventoryField,
                ProductAttributes = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("Base_Unit_of_Measure", p.Base_Unit_of_Measure),
            new KeyValuePair<string, string>("Costing_Method", p.Costing_Method),
            new KeyValuePair<string, string>("Unit_Cost", p.Unit_Cost.ToString())
        },
                Published = (!p.Blocked),
                VendorCode = p.Vendor_No,
                LastChangedDate = DateTime.Parse(p.Last_Date_Modified),
                WarehouseNameOrCode = p.Shelf_No
            }).ToList();
        }
        catch (Exception ex)
        {
            erpResponseData.ErpResponseModel = new ErpResponseModel
            {
                IsError = true,
                ErrorShortMessage = ex.Message,
                ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace
            };
        }
        erpResponseData.ErpResponseModel.Next = "-1";
        return erpResponseData;
    }

    public async Task<ErpResponseData<ErpProductDataModel>> GetProductByItemNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<ErpProductDataModel>();
        var responseContent = string.Empty;

        try
        {
            var apiFilterTerm = $"$filter=No eq '{erpRequest.ProductSku}'";
            var apiEndPoint = PrepareODataUrl(string.Format(D365BCIntegrationDefaults.ProductEndpoint, 
                _settings.CompanyName), apiFilterTerm);

            var response = await _d365BCHttpService.GetAsync(apiEndPoint, ErpSyncLevel.Product);

            responseContent = await response.Content.ReadAsStringAsync();
            var dynamicsResponse = JsonConvert.DeserializeObject<D365ProductsResponse>(responseContent);

            if (dynamicsResponse?.Value == null || !dynamicsResponse.Value.Any())
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = $"Product not found with item number {erpRequest.ProductSku}";
                return erpResponseData;
            }

            var product = dynamicsResponse.Value.First();
            erpResponseData.Data = new ErpProductDataModel
            {
                Name = product.Description ?? string.Empty,
                Sku = product.No ?? string.Empty,
                ManufacturerPartNumber = product.Vendor_Item_No ?? string.Empty,
                ShortDescription = product.Description ?? string.Empty,
                FullDescription = product.Description_2 ?? string.Empty,
                Price = product.Unit_Price,
                StockQuantity = product.InventoryField,
                ProductAttributes = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Base_Unit_of_Measure", product.Base_Unit_of_Measure ?? string.Empty),
                    new KeyValuePair<string, string>("Shelf_No", product.Shelf_No ?? string.Empty),
                    new KeyValuePair<string, string>("Costing_Method", product.Costing_Method ?? string.Empty)
                },
                TaxCategoryName = product.VAT_Prod_Posting_Group ?? string.Empty,
                Published = (!product.Blocked),
                VendorCode = product.Vendor_No ?? string.Empty,
                VendorName = string.Empty, // You might want to add vendor name lookup if needed
                LastChangedDate = DateTime.TryParse(product.Last_Date_Modified, out var date) ? date : null,
                WarehouseNameOrCode = product.Location_Filter ?? string.Empty
            };

            erpResponseData.ErpResponseModel.IsError = false;
        }
        catch (Exception ex)
        {
            erpResponseData.ErpResponseModel.IsError = true;
            erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
            erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent)
                ? responseContent
                : ex.StackTrace;
        }
        erpResponseData.ErpResponseModel.Next = "-1";
        return erpResponseData;
    }

    public async Task<ErpResponseData<IList<ErpStockDataModel>>> GetStocksFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<IList<ErpStockDataModel>>();
        var responseContent = string.Empty;

        try
        {
            // Get total count of modified products
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");

            var countFilterTerm = $"$filter=Last_Date_Modified gt {sevenDaysAgo}";
            var countEndpoint = PrepareODataUrl(string.Format(D365BCIntegrationDefaults.ProductCountEndpoint, 
                _settings.CompanyName), countFilterTerm);
                
            var countResponse = await _d365BCHttpService.GetAsync(countEndpoint, ErpSyncLevel.Stock);
            var totalCount = int.Parse(await countResponse.Content.ReadAsStringAsync());

            if (totalCount == 0 || erpRequest.Start == "-1")
            {
                erpResponseData.Data = null;
                return erpResponseData;
            }

            // Get stocks in batches
            var batchSize = _settings.StockSyncLimit > 0 ? _settings.StockSyncLimit : 100;
            var currentSkip = 0;
            var allStocks = new List<ErpStockDataModel>();

            while (currentSkip < totalCount)
            {
                var apiFilterTerm = $"$filter=Last_Date_Modified ge {sevenDaysAgo}&$top={batchSize}&$skip={currentSkip}";
                var apiEndpoint = PrepareODataUrl(string.Format(D365BCIntegrationDefaults.ProductEndpoint, 
                    _settings.CompanyName), apiFilterTerm);
                    
                var response = await _d365BCHttpService.GetAsync(apiEndpoint, ErpSyncLevel.Stock);
                responseContent = await response.Content.ReadAsStringAsync();
                var dynamicsResponse = JsonConvert.DeserializeObject<D365ProductsResponse>(responseContent);

                if (dynamicsResponse?.Value != null)
                {
                    var stocks = dynamicsResponse.Value.Select(p => new ErpStockDataModel
                    {
                        WarehouseNameOrCode = p.Shelf_No,
                        Sku = p.No,
                        SalesOrgCode = string.Empty, // Set as needed
                        QuantityOnHand = p.InventoryField,
                        LastChangedDate = DateTime.Parse(p.Last_Date_Modified)
                    });
                    allStocks.AddRange(stocks);
                }

                currentSkip += batchSize;
            }

            erpResponseData.Data = allStocks;
        }
        catch (Exception ex)
        {
            erpResponseData.ErpResponseModel.IsError = true;
            erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
            erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
        }
        erpResponseData.ErpResponseModel.Next = "-1";
        return erpResponseData;
    }

    public async Task<ErpResponseData<ErpStockDataModel>> GetStockByItemNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<ErpStockDataModel>();
        var responseContent = string.Empty;

        try
        {
            var apiFilterTerm = $"$filter=No eq '{erpRequest.ProductSku}'";
            var apiEndpoint = PrepareODataUrl(string.Format(D365BCIntegrationDefaults.ProductEndpoint), apiFilterTerm);
                
            var response = await _d365BCHttpService.GetAsync(apiEndpoint, ErpSyncLevel.Stock);

            responseContent = await response.Content.ReadAsStringAsync();
            var dynamicsResponse = JsonConvert.DeserializeObject<D365ProductsResponse>(responseContent);

            if (dynamicsResponse?.Value == null || !dynamicsResponse.Value.Any())
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = $"Stock not found for item number {erpRequest.ProductSku}";
                return erpResponseData;
            }

            var product = dynamicsResponse.Value.First();
            erpResponseData.Data = new ErpStockDataModel
            {
                WarehouseNameOrCode = product.Shelf_No,
                Sku = product.No,
                SalesOrgCode = string.Empty, // Set as needed
                QuantityOnHand = product.InventoryField,
                LastChangedDate = DateTime.Parse(product.Last_Date_Modified)
            };
        }
        catch (Exception ex)
        {
            erpResponseData.ErpResponseModel.IsError = true;
            erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
            erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
        }
        erpResponseData.ErpResponseModel.Next = "-1";
        return erpResponseData;
    }

    public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrdersByAccountFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<IList<ErpPlaceOrderDataModel>>();
        var responseContent = string.Empty;
        var allOrders = new List<ErpPlaceOrderDataModel>();

        try
        {
            // Get total count of orders for the customer in last 7 days
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");

            var countFilterTerm = $"$filter=Sell_to_Customer_No eq '{erpRequest.AccountNumber}' and Order_Date gt {sevenDaysAgo}";
            var countEndpoint = PrepareODataUrl(string.Format(D365BCIntegrationDefaults.OrderCountEndpoint, 
                _settings.CompanyName), countFilterTerm);

            var countResponse = await _d365BCHttpService.GetAsync(countEndpoint, ErpSyncLevel.Order);
            var totalCount = int.Parse(await countResponse.Content.ReadAsStringAsync());

            if (totalCount == 0 || erpRequest.Start == "-1")
            {
                erpResponseData.Data = null;
                return erpResponseData;
            }

            // Get orders in batches
            var batchSize = _settings.OrderSyncLimit > 0 ? _settings.OrderSyncLimit : 100;
            var currentSkip = 0;

            while (currentSkip < totalCount)
            {
                var apiFilterTerm = $"$filter=Sell_to_Customer_No eq '{erpRequest.AccountNumber}' and Order_Date gt {sevenDaysAgo}&$top={batchSize}&$skip={currentSkip}";
                var apiEndpoint = PrepareODataUrl(string.Format(D365BCIntegrationDefaults.OrderEndpoint, _settings.CompanyName), apiFilterTerm);

                var response = await _d365BCHttpService.GetAsync(apiEndpoint, ErpSyncLevel.Order);
                responseContent = await response.Content.ReadAsStringAsync();
                var dynamicsResponse = JsonConvert.DeserializeObject<D365OrdersResponse>(responseContent);

                if (dynamicsResponse?.Value != null)
                {
                    var orders = MapD365OrdersToErpOrders(dynamicsResponse.Value);
                    allOrders.AddRange(orders);
                }

                currentSkip += batchSize;
            }

            erpResponseData.Data = allOrders;
        }
        catch (Exception ex)
        {
            erpResponseData.ErpResponseModel.IsError = true;
            erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
            erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
        }
        erpResponseData.ErpResponseModel.Next = "-1";
        return erpResponseData;
    }

    public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrderByOrderNumberFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<IList<ErpPlaceOrderDataModel>>();
        var responseContent = string.Empty;

        try
        {
            var apiEndpoint = PrepareODataUrl($"Company('{_settings.CompanyName}')/salesOrders", 
                $"$filter=No eq '{erpRequest.OrderNumber}'");
            var response = await _d365BCHttpService.GetAsync(apiEndpoint, ErpSyncLevel.Order);

            responseContent = await response.Content.ReadAsStringAsync();
            var dynamicsResponse = JsonConvert.DeserializeObject<D365OrdersResponse>(responseContent);

            if (dynamicsResponse?.Value == null || !dynamicsResponse.Value.Any())
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = $"Order not found with number {erpRequest.OrderNumber}";
                return erpResponseData;
            }

            erpResponseData.Data = MapD365OrdersToErpOrders(dynamicsResponse.Value);
        }
        catch (Exception ex)
        {
            erpResponseData.ErpResponseModel.IsError = true;
            erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
            erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
        }
        erpResponseData.ErpResponseModel.Next = "-1";
        return erpResponseData;
    }

    public async Task<ErpResponseData<D365BCOrderModel>> CreateOrderAsync(ErpPlaceOrderDataModel orderData)
    {
        var erpResponseData = new ErpResponseData<D365BCOrderModel>();
        var responseContent = string.Empty;

        try
        {
            var orderRequest = new D365BCOrderRequestModel
            {
                Document_Type = "Order",
                Sell_to_Customer_No = orderData.AccountNumber,
                Posting_Description = $"Order {orderData.CustomOrderNumber}",
                Document_Date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                Posting_Date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                Order_Date = orderData.OrderDate?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.ToString("yyyy-MM-dd"),
                Due_Date = orderData.DateRequired?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd"),
                Requested_Delivery_Date = orderData.DeliveryDate?.ToString("yyyy-MM-dd") ?? DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd"),
                Your_Reference = orderData.CustomerReference ?? "OPEN",
                Salesperson_Code = orderData.RepCode ?? "JO",
                Payment_Terms_Code = "1M(8D)", // Default payment terms
                Shipment_Method_Code = "EXW",  // Default shipment method
                Shipping_Agent_Code = "FEDEX",  // Default shipping agent
                Shipping_Agent_Service_Code = "NEXT DAY",
                Package_Tracking_No = "", // Can be updated later
                Location_Code = orderData.Location ?? "",
                Shipping_Advice = "Partial",
                Shipping_Time = "1D",
                Language_Code = "ENG"
            };

            var jsonContent = JsonConvert.SerializeObject(orderRequest);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var endpoint = $"ODataV4/Company('{_settings.CompanyName}')/salesOrders";
            var response = await _d365BCHttpService.PostAsync(endpoint, content, ErpSyncLevel.Order);

            responseContent = await response.Content.ReadAsStringAsync();
            var createdOrder = JsonConvert.DeserializeObject<D365BCOrderModel>(responseContent);

            if (createdOrder != null)
            {
                erpResponseData.Data = createdOrder;
                erpResponseData.ErpResponseModel.OrderNumber = createdOrder.No;
            }
            else
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "Failed to create order";
            }
        }
        catch (Exception ex)
        {
            erpResponseData.ErpResponseModel.IsError = true;
            erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
            erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
        }
        return erpResponseData;
    }

    #endregion
}