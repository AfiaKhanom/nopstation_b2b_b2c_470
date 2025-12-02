using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Services.Barcode;
using Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Services.Models;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Stores;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Services.Token
{
    /// <summary>
    /// Token resolver service implementation
    /// </summary>
    public class TokenResolverService : ITokenResolverService
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IAddressService _addressService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly IStoreService _storeService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IPaymentService _paymentService;
        private readonly IShipmentService _shipmentService;
        private readonly IProductService _productService;
        private readonly IPictureService _pictureService;
        private readonly IBarcodeService _barcodeService;
        private readonly ILocalizationService _localizationService;
        private readonly ConfigureInvoiceAndPackSlipSettings _settings;

        public TokenResolverService(
            IOrderService orderService,
            ICustomerService customerService,
            IAddressService addressService,
            IStateProvinceService stateProvinceService,
            ICountryService countryService,
            IStoreService storeService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            IPaymentService paymentService,
            IShipmentService shipmentService,
            IProductService productService,
            IPictureService pictureService,
            IBarcodeService barcodeService,
            ILocalizationService localizationService,
            ConfigureInvoiceAndPackSlipSettings settings)
        {
            _orderService = orderService;
            _customerService = customerService;
            _addressService = addressService;
            _stateProvinceService = stateProvinceService;
            _countryService = countryService;
            _storeService = storeService;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _paymentService = paymentService;
            _shipmentService = shipmentService;
            _productService = productService;
            _pictureService = pictureService;
            _barcodeService = barcodeService;
            _localizationService = localizationService;
            _settings = settings;
        }

        public virtual async Task<InvoiceRenderModel> ResolveInvoiceTokensAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var model = new InvoiceRenderModel
            {
                FontFamily = _settings.FontFamily,
                FontSize = _settings.FontSize,
                EnableRtl = _settings.EnableRtl
            };

            // Order info
            model.OrderNumber = order.CustomOrderNumber;
            model.OrderDate = order.CreatedOnUtc.ToString("d");

            // Customer info
            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            model.CustomerName = await _customerService.GetCustomerFullNameAsync(customer);
            model.CustomerEmail = customer?.Email ?? "";

            // Billing address
            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
            if (billingAddress != null)
            {
                model.BillingAddress = await FormatAddressAsync(billingAddress);
            }

            // Shipping address
            if (order.ShippingAddressId.HasValue)
            {
                var shippingAddress = await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value);
                if (shippingAddress != null)
                {
                    model.ShippingAddress = await FormatAddressAsync(shippingAddress);
                }
            }

            // Payment method
            model.PaymentMethod = order.PaymentMethodSystemName;

            // Shipping method
            model.ShippingMethod = order.ShippingMethod;

            // Currency
            model.Currency = order.CustomerCurrencyCode ?? "";

            // Totals
            model.SubTotal = await _priceFormatter.FormatPriceAsync(order.OrderSubtotalInclTax, true, order.CustomerCurrencyCode, false, 0);
            model.ShippingTotal = await _priceFormatter.FormatPriceAsync(order.OrderShippingInclTax, true, order.CustomerCurrencyCode, false, 0);
            model.Tax = await _priceFormatter.FormatPriceAsync(order.OrderTax, true, order.CustomerCurrencyCode, false, 0);
            model.Total = await _priceFormatter.FormatPriceAsync(order.OrderTotal, true, order.CustomerCurrencyCode, false, 0);

            // Store info
            var store = await _storeService.GetStoreByIdAsync(order.StoreId);
            model.StoreName = store?.Name ?? "";
            model.StoreUrl = store?.Url ?? "";

            // Barcode
            model.BarcodeImageBase64 = await _barcodeService.GenerateBarcodeBase64Async(order.CustomOrderNumber);

            // Order items
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
            foreach (var orderItem in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                var itemModel = new InvoiceItemModel
                {
                    Sku = product?.Sku ?? "",
                    Name = HttpUtility.HtmlEncode(product?.Name ?? ""),
                    Quantity = orderItem.Quantity.ToString(),
                    UnitPrice = await _priceFormatter.FormatPriceAsync(orderItem.UnitPriceInclTax, true, order.CustomerCurrencyCode, false, 0),
                    Total = await _priceFormatter.FormatPriceAsync(orderItem.PriceInclTax, true, order.CustomerCurrencyCode, false, 0)
                };

                // Get product picture
                var picture = (await _pictureService.GetPicturesByProductIdAsync(product?.Id ?? 0, 1)).FirstOrDefault();
                if (picture != null)
                {
                    itemModel.PictureUrl = (await _pictureService.GetPictureUrlAsync(picture, _settings.MaxPictureWidth)).Url;
                }

                model.Items.Add(itemModel);
            }

            return model;
        }

        public virtual async Task<PackingSlipRenderModel> ResolvePackingSlipTokensAsync(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);
            if (order == null)
                throw new ArgumentException("Order not found for shipment", nameof(shipment));

            var model = new PackingSlipRenderModel
            {
                FontFamily = _settings.FontFamily,
                FontSize = _settings.FontSize,
                EnableRtl = _settings.EnableRtl
            };

            // Order info
            model.OrderNumber = order.CustomOrderNumber;
            model.OrderDate = order.CreatedOnUtc.ToString("d");
            model.ShipmentDate = shipment.ShippedDateUtc?.ToString("d") ?? "";
            model.TrackingNumber = shipment.TrackingNumber ?? "";

            // Customer info
            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            model.CustomerName = await _customerService.GetCustomerFullNameAsync(customer);

            // Shipping address
            if (order.ShippingAddressId.HasValue)
            {
                var shippingAddress = await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value);
                if (shippingAddress != null)
                {
                    model.ShippingAddress = await FormatAddressAsync(shippingAddress);
                }
            }

            // Shipping method
            model.ShippingMethod = order.ShippingMethod;

            // Store info
            var store = await _storeService.GetStoreByIdAsync(order.StoreId);
            model.StoreName = store?.Name ?? "";
            model.StoreUrl = store?.Url ?? "";

            // Barcode
            model.BarcodeImageBase64 = await _barcodeService.GenerateBarcodeBase64Async(order.CustomOrderNumber);

            // Shipment items
            var shipmentItems = await _shipmentService.GetShipmentItemsByShipmentIdAsync(shipment.Id);
            foreach (var shipmentItem in shipmentItems)
            {
                var orderItem = await _orderService.GetOrderItemByIdAsync(shipmentItem.OrderItemId);
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                var itemModel = new PackingSlipItemModel
                {
                    Sku = product?.Sku ?? "",
                    Name = HttpUtility.HtmlEncode(product?.Name ?? ""),
                    Quantity = shipmentItem.Quantity.ToString()
                };

                // Get product picture
                var picture = (await _pictureService.GetPicturesByProductIdAsync(product?.Id ?? 0, 1)).FirstOrDefault();
                if (picture != null)
                {
                    itemModel.PictureUrl = (await _pictureService.GetPictureUrlAsync(picture, _settings.MaxPictureWidth)).Url;
                }

                model.Items.Add(itemModel);
            }

            return model;
        }

        private async Task<string> FormatAddressAsync(Address address)
        {
            var addressLines = new System.Collections.Generic.List<string>();

            if (!string.IsNullOrWhiteSpace(address.FirstName) || !string.IsNullOrWhiteSpace(address.LastName))
                addressLines.Add($"{address.FirstName} {address.LastName}".Trim());

            if (!string.IsNullOrWhiteSpace(address.Company))
                addressLines.Add(address.Company);

            if (!string.IsNullOrWhiteSpace(address.Address1))
                addressLines.Add(address.Address1);

            if (!string.IsNullOrWhiteSpace(address.Address2))
                addressLines.Add(address.Address2);

            var cityStateZip = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrWhiteSpace(address.City))
                cityStateZip.Add(address.City);

            if (address.StateProvinceId.HasValue)
            {
                var stateProvince = await _localizationService.GetLocalizedAsync(
                    await _stateProvinceService.GetStateProvinceByAddressAsync(address),
                    x => x.Name);
                if (!string.IsNullOrWhiteSpace(stateProvince))
                    cityStateZip.Add(stateProvince);
            }

            if (!string.IsNullOrWhiteSpace(address.ZipPostalCode))
                cityStateZip.Add(address.ZipPostalCode);

            if (cityStateZip.Any())
                addressLines.Add(string.Join(", ", cityStateZip));

            if (address.CountryId.HasValue)
            {
                var country = await _localizationService.GetLocalizedAsync(
                    await _countryService.GetCountryByIdAsync(address.CountryId.Value),
                    x => x.Name);
                if (!string.IsNullOrWhiteSpace(country))
                    addressLines.Add(country);
            }

            return HttpUtility.HtmlEncode(string.Join("\n", addressLines));
        }
    }
}
