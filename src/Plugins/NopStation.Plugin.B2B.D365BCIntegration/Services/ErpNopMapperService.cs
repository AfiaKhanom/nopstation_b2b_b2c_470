using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Orders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.D365BCIntegration.Models;

namespace NopStation.Plugin.B2B.D365BCIntegration.Services;

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
    private readonly D365BCIntegrationSettings _d365Settings;

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
        D365BCIntegrationSettings d365Settings,
        IErpNopUserService userService)
    {
        _orderService = orderService;
        _addressService = addressService;
        _countryService = countryService;
        _customerService = customerService;
        _productService = productService;
        _stateProvinceService = stateProvinceService;
        _erpAccountService = erpAccountService;
        _d365Settings = d365Settings;
        _userService = userService;
    }

    #endregion

    #region Methods

    public async Task<IList<ErpAccountDataModel>> ErpAccountMapNop(IList<D365Customer> customers)
    {
        var erpAccounts = customers.Select(customer => new ErpAccountDataModel
        {
            AccountNumber = customer.No ?? string.Empty,
            AccountName = customer.Name ?? string.Empty,
            Address1 = customer.Address ?? string.Empty,
            Address2 = customer.Address_2 ?? string.Empty,
            City = customer.City ?? string.Empty,
            StateProvince = customer.County ?? string.Empty,
            Country = customer.Country_Region_Code ?? string.Empty,
            ZipPostalCode = customer.Post_Code ?? string.Empty,
            PhoneNumber = customer.Phone_No ?? string.Empty,
            Email = customer.E_Mail ?? string.Empty,
            CompanyNo = customer.Registration_Number ?? string.Empty,
            VatNumber = customer.VAT_Registration_No ?? string.Empty,
            CreditLimit = customer.Credit_Limit_LCY
        }).ToList();

        return erpAccounts;
    }

    public Task<IList<ErpShipToAddressDataModel>> ErpShipToAddressMapNop(IList<D365Customer> erpShipToAddressResponse)
    {
        // Not implemented for this plugin as D365 doesn't provide separate shipping addresses in this implementation
        throw new NotImplementedException();
    }

    public Task<IList<ErpInvoiceDataModel>> ErpInvoiceMapNop(IList<object> erpInvoicesResponse)
    {
        // Not implemented for this plugin as we're only handling customer sync
        throw new NotImplementedException();
    }

    public async Task<IList<ErpProductDataModel>> ErpProductMapNop(IList<object> erpStockResponses)
    {
        var products = erpStockResponses.Cast<D365BCProductModel>().ToList();

        var erpProducts = products.Select(product => new ErpProductDataModel
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
                new KeyValuePair<string, string>("Shelf_No", product.Shelf_No ?? string.Empty)
            },
            TaxCategoryName = product.VAT_Prod_Posting_Group ?? string.Empty,
            Published = (!product.Blocked),
            VendorCode = product.Vendor_No ?? string.Empty,
            LastChangedDate = DateTime.TryParse(product.Last_Date_Modified, out var date) ? date : null,
            WarehouseNameOrCode = product.Location_Filter ?? string.Empty
        }).ToList();

        return await Task.FromResult<IList<ErpProductDataModel>>(erpProducts);
    }




    public Task<IList<ErpStockDataModel>> ErpStockMapNop(IList<object> erpStockResponses)
    {
        // Not implemented for this plugin as we're only handling customer sync
        throw new NotImplementedException();
    }

    public Task<IList<ErpPriceSpecialPricingDataModel>> ErpSpecialPriceMapNop(IList<object> erpSpecialPriceResponses)
    {
        // Not implemented for this plugin as we're only handling customer sync
        throw new NotImplementedException();
    }

    public Task<IList<ErpPriceGroupPricingDataModel>> ErpGroupPriceMapNop(IList<object> erpGroupPriceResponses)
    {
        // Not implemented for this plugin as we're only handling customer sync
        throw new NotImplementedException();
    }

    public Task<IList<ErpPlaceOrderDataModel>> ErpOrderMapNop(IList<object> erpOrdersResponse)
    {
        throw new NotImplementedException();
    }

    #endregion
}