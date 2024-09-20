using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Models.Media;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.OrderSummary;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories.ErpOrderDetails;

public class ErpOrderDetailsModelFactory : IErpOrderDetailsModelFactory
{
    #region Fields

    private readonly IOrderService _orderService;
    private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
    private readonly IErpOrderItemAdditionalDataService _erpOrderItemAdditionalDataService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly ILocalizationService _localizationService;
    private readonly IProductService _productService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IMeasureService _measureService;
    private readonly MeasureSettings _measureSettings;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IVendorService _vendorService;
    private readonly IPictureService _pictureService;
    private readonly IAddressService _addressService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly IAddressModelFactory _addressModelFactory;
    private readonly AddressSettings _addressSettings;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IShipmentService _shipmentService;
    private readonly ICustomerService _customerService;
    private readonly IRewardPointService _rewardPointService;
    private readonly IGiftCardService _giftCardService;
    private readonly ICurrencyService _currencyService;
    private readonly TaxSettings _taxSettings;
    private readonly IPaymentService _paymentService;
    private readonly IPaymentPluginManager _paymentPluginManager;
    private readonly CatalogSettings _catalogSettings;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly ICountryService _countryService;
    private readonly IProductAttributeFormatter _productAttributeFormatter;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IWorkContext _workContext;


    #endregion

    #region Ctor 
    public ErpOrderDetailsModelFactory(IOrderService orderService,
        IErpOrderItemAdditionalDataService erpOrderItemAdditionalDataService,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        ILocalizationService localizationService,
        IErpAccountService erpAccountService,
        IMeasureService measureService,
        MeasureSettings measureSettings,
        IPriceFormatter priceFormatter,
        IProductService productService,
        IPictureService pictureService,
        IVendorService vendorService,
        IAddressService addressService,
        IStateProvinceService stateProvinceService,
        IAddressModelFactory addressModelFactory,
        AddressSettings addressSettings,
        IErpShipToAddressService erpShipToAddressService,
        IDateTimeHelper dateTimeHelper,
        IShipmentService shipmentService,
        ICustomerService customerService,
        IPaymentPluginManager paymentPluginManager,
        IPaymentService paymentService,
        TaxSettings taxSettings,
        ICurrencyService currencyService,
        IGiftCardService giftCardService,
        IRewardPointService rewardPointService,
        CatalogSettings catalogSettings,
        IErpSalesOrgService erpSalesOrgService,
        ICountryService countryService,
        IProductAttributeFormatter productAttributeFormatter,
        IUrlRecordService urlRecordService,
        IWorkContext workContext
        )
    {
        _orderService = orderService;
        _erpOrderItemAdditionalDataService = erpOrderItemAdditionalDataService;
        _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _localizationService = localizationService;
        _erpAccountService = erpAccountService;
        _measureService = measureService;
        _measureSettings = measureSettings;
        _priceFormatter = priceFormatter;
        _productService = productService;
        _pictureService = pictureService;
        _vendorService = vendorService;
        _addressService = addressService;
        _stateProvinceService = stateProvinceService;
        _addressModelFactory = addressModelFactory;
        _addressSettings = addressSettings;
        _erpShipToAddressService = erpShipToAddressService;
        _dateTimeHelper = dateTimeHelper;
        _shipmentService = shipmentService;
        _customerService = customerService;
        _paymentPluginManager = paymentPluginManager;
        _paymentService = paymentService;
        _taxSettings = taxSettings;
        _currencyService = currencyService;
        _giftCardService = giftCardService;
        _rewardPointService = rewardPointService;
        _catalogSettings = catalogSettings;
        _erpSalesOrgService = erpSalesOrgService;
        _countryService = countryService;
        _productAttributeFormatter = productAttributeFormatter;
        _urlRecordService = urlRecordService;
        _workContext = workContext;
    }

    #endregion

    #region Utilities 

    public async Task<ErpShipToAddressDataModel> PreapreErpShippingAddress(ErpShipToAddress erpShipToAddress)
    {
        var model = new ErpShipToAddressDataModel();

        if (erpShipToAddress != null)
        {
            var address = await _addressService.GetAddressByIdAsync(erpShipToAddress.AddressId);
            if (address != null)
            {
                model.Company = address.Company;
                model.Address1 = address.Address1;
                model.Address2 = address.Address2;
                model.County = address.County;
                model.ZipPostalCode = address.ZipPostalCode;
                model.PhoneNumber = address.PhoneNumber;
                model.FaxNumber = address.FaxNumber;
                model.City = address.City;
                model.StateProvince = (await _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId ?? 0))?.Name ?? string.Empty;
            }

            model.Suburb = erpShipToAddress.Suburb;
            model.ProvinceCode = erpShipToAddress.ProvinceCode;
            model.ShipToCode = erpShipToAddress.ShipToCode;
            model.ShipToName = erpShipToAddress.ShipToName;
            model.DeliveryNotes = erpShipToAddress.DeliveryNotes;
            model.EmailAddress = erpShipToAddress.EmailAddresses;
            model.RepNumber = erpShipToAddress.RepNumber;
            model.RepFullName = erpShipToAddress.RepFullName;
            model.RepPhoneNumber = erpShipToAddress.RepPhoneNumber;
            model.RepEmail = erpShipToAddress.RepEmail;
        }
        return model;
    }

    #endregion

    #region Methods

    public async Task<ErpOrderDetailsModel> PrepareErpOrderDetailsModelFactoryAsync(ErpOrderAdditionalData erpOrderPerAccount)
    {
        var order = await _orderService.GetOrderByIdAsync(erpOrderPerAccount.NopOrderId);
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var b2BOrderDetailsModel = new ErpOrderDetailsModel();
        var b2BAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(currentCustomer.Id);
        var erpOrder = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(erpOrderPerAccount.NopOrderId);
        var b2bOrder = erpOrder.ErpOrderType == ErpOrderType.B2BSalesOrder || erpOrder.ErpOrderType == ErpOrderType.B2BQuote ? erpOrder : null;
        var baseWeight = "";
        var totalPriceWithOutSavingsExcTax = decimal.Zero;
        var b2BOnlineOrderDiscountExcTax = decimal.Zero;
        var language = await _workContext.GetWorkingLanguageAsync();
        var languageId = language.Id;
        var erpBillingAddress = await _addressService.GetAddressByIdAsync(b2BAccount.BillingAddressId ?? 0);
        var salesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(b2BAccount.ErpSalesOrgId);
        var erpStateProvidenAddress = await _stateProvinceService.GetStateProvinceByAddressAsync(erpBillingAddress);
        var erpCountry = await _countryService.GetCountryByIdAsync(erpBillingAddress.CountryId ?? 0);

        b2BOrderDetailsModel.ErpAccountDataModel = new ErpAccountDataModel()
        {
            AccountNumber = b2BAccount.AccountNumber,
            AccountName = b2BAccount.AccountName,
            PaymentTypeCode = b2BAccount.PaymentTypeCode,
            Address1 = erpBillingAddress?.City + "," + erpBillingAddress?.County + "," + erpStateProvidenAddress?.Name + erpBillingAddress?.ZipPostalCode + "," + erpCountry?.Name,
            ErpSalesOrgCode = salesOrg.Code,
            IsActive = b2BAccount.IsActive,
            CreditLimit = b2BAccount.CreditLimit,
            CreditLimitUsed = b2BAccount.CreditLimit - b2BAccount.CreditLimitAvailable,
            CreditLimitAvailable = b2BAccount.CreditLimitAvailable,
            CurrentBalance = b2BAccount.CurrentBalance
        };

        b2BOrderDetailsModel.ErpAccountDataModel.CurrentBalanceStr = await _priceFormatter.FormatPriceAsync(b2BAccount.CurrentBalance);
        b2BOrderDetailsModel.ErpAccountDataModel.CreditLimitAvailableStr = await _priceFormatter.FormatPriceAsync(b2BAccount.CreditLimitAvailable);
        b2BOrderDetailsModel.ErpAccountDataModel.CreditLimitUsedStr = await _priceFormatter.FormatPriceAsync(b2BAccount.CreditLimit - b2BAccount.CreditLimitAvailable);
        b2BOrderDetailsModel.ErpAccountDataModel.CreditLimitStr = await _priceFormatter.FormatPriceAsync(b2BAccount.CreditLimit);

        var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
        await _addressModelFactory.PrepareAddressModelAsync(b2BOrderDetailsModel.ErpBillingAddressModel, billingAddress, false, _addressSettings);
        b2BOrderDetailsModel.Id = order.Id;

        if (order.PickupInStore)
        {
            b2BOrderDetailsModel.IsPicUpAddressActive = order.PickupInStore;
            var pickupAddress = await _addressService.GetAddressByIdAsync(order.PickupAddressId ?? 0);
            if (pickupAddress != null)
                await _addressModelFactory.PrepareAddressModelAsync(b2BOrderDetailsModel.ErpPickUpAddressModel, pickupAddress, false, _addressSettings);
        }
        else
        {
            var erpShipToAddressId = erpOrder.ErpShipToAddressId ?? 0;

            var erpShippingAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(erpShipToAddressId);// erpOrder.ErpShipToAddress;
            b2BOrderDetailsModel.ErpShippingAddressModel = await PreapreErpShippingAddress(erpShippingAddress);
            b2BOrderDetailsModel.ErpShippingAddressModel.AccountNumber = b2BAccount.AccountNumber;
        }

        b2BOrderDetailsModel.VatNumber = order.VatNumber;

        var shipments = (await _shipmentService.GetShipmentsByOrderIdAsync(order.Id, !order.PickupInStore, order.PickupInStore)).OrderBy(x => x.CreatedOnUtc).ToList();
        foreach (var shipment in shipments)
        {
            var shipmentModel = new ErpOrderDetailsModel.ShipmentBriefModel
            {
                Id = shipment.Id,
                TrackingNumber = shipment.TrackingNumber,
            };
            if (shipment.ShippedDateUtc.HasValue)
                shipmentModel.ShippedDate = await _dateTimeHelper.ConvertToUserTimeAsync(shipment.ShippedDateUtc.Value, DateTimeKind.Utc);
            if (shipment.ReadyForPickupDateUtc.HasValue)
                shipmentModel.ReadyForPickupDate = await _dateTimeHelper.ConvertToUserTimeAsync(shipment.ReadyForPickupDateUtc.Value, DateTimeKind.Utc);
            if (shipment.DeliveryDateUtc.HasValue)
                shipmentModel.DeliveryDate = await _dateTimeHelper.ConvertToUserTimeAsync(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc);
            b2BOrderDetailsModel.ShipmentBriefModels.Add(shipmentModel);
        }
        // billing address coverstion
        if (_b2BB2CFeaturesSettings.DisplayWeightInformation)
            baseWeight = (await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId))?.Name;

        var orderItems = await _orderService.GetOrderItemsAsync(erpOrderPerAccount.NopOrderId);

        //payment method
        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
        var paymentMethod = await _paymentPluginManager
            .LoadPluginBySystemNameAsync(order.PaymentMethodSystemName, customer, order.StoreId);
        b2BOrderDetailsModel.PaymentMethod = paymentMethod != null ? await _localizationService.GetLocalizedFriendlyNameAsync(paymentMethod, language.Id) : order.PaymentMethodSystemName;
        b2BOrderDetailsModel.PaymentMethodStatus = await _localizationService.GetLocalizedEnumAsync(order.PaymentStatus);
        b2BOrderDetailsModel.CanRePostProcessPayment = await _paymentService.CanRePostProcessPaymentAsync(order);
        //custom values
        b2BOrderDetailsModel.CustomValues = _paymentService.DeserializeCustomValues(order);

        //order subtotal
        if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
        {
            //including tax

            //order subtotal
            var orderSubtotalInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
            b2BOrderDetailsModel.OrderSubtotal = await _priceFormatter.FormatPriceAsync(orderSubtotalInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
            b2BOrderDetailsModel.OrderSubtotalValue = orderSubtotalInclTaxInCustomerCurrency;
            //discount (applied to order subtotal)
            var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
            if (orderSubTotalDiscountInclTaxInCustomerCurrency > decimal.Zero)
            {
                b2BOrderDetailsModel.OrderSubTotalDiscount = await _priceFormatter.FormatPriceAsync(-orderSubTotalDiscountInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                b2BOrderDetailsModel.OrderSubTotalDiscountValue = orderSubTotalDiscountInclTaxInCustomerCurrency;
            }
        }
        else
        {
            //excluding tax

            //order subtotal
            var orderSubtotalExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
            b2BOrderDetailsModel.OrderSubtotal = await _priceFormatter.FormatPriceAsync(orderSubtotalExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
            b2BOrderDetailsModel.OrderSubtotalValue = orderSubtotalExclTaxInCustomerCurrency;
            //discount (applied to order subtotal)
            var orderSubTotalDiscountExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
            if (orderSubTotalDiscountExclTaxInCustomerCurrency > decimal.Zero)
            {
                b2BOrderDetailsModel.OrderSubTotalDiscount = await _priceFormatter.FormatPriceAsync(-orderSubTotalDiscountExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                b2BOrderDetailsModel.OrderSubTotalDiscountValue = orderSubTotalDiscountExclTaxInCustomerCurrency;
            }
        }

        if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
        {
            //including tax

            //order shipping
            var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
            b2BOrderDetailsModel.OrderShipping = await _priceFormatter.FormatShippingPriceAsync(orderShippingInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
            b2BOrderDetailsModel.OrderShippingValue = orderShippingInclTaxInCustomerCurrency;
            //payment method additional fee
            var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
            if (paymentMethodAdditionalFeeInclTaxInCustomerCurrency > decimal.Zero)
            {
                b2BOrderDetailsModel.PaymentMethodAdditionalFee = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                b2BOrderDetailsModel.PaymentMethodAdditionalFeeValue = paymentMethodAdditionalFeeInclTaxInCustomerCurrency;
            }
        }
        else
        {
            //excluding tax

            //order shipping
            var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
            b2BOrderDetailsModel.OrderShipping = await _priceFormatter.FormatShippingPriceAsync(orderShippingExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
            b2BOrderDetailsModel.OrderShippingValue = orderShippingExclTaxInCustomerCurrency;
            //payment method additional fee
            var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
            if (paymentMethodAdditionalFeeExclTaxInCustomerCurrency > decimal.Zero)
            {
                b2BOrderDetailsModel.PaymentMethodAdditionalFee = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                b2BOrderDetailsModel.PaymentMethodAdditionalFeeValue = paymentMethodAdditionalFeeExclTaxInCustomerCurrency;
            }
        }

        //tax
        var displayTax = true;
        var displayTaxRates = true;
        if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
        {
            displayTax = false;
            displayTaxRates = false;
        }
        else
        {
            if (order.OrderTax == 0 && _taxSettings.HideZeroTax)
            {
                displayTax = false;
                displayTaxRates = false;
            }
            else
            {
                var taxRates = _orderService.ParseTaxRates(order, order.TaxRates);
                displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                displayTax = !displayTaxRates;

                var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                b2BOrderDetailsModel.Tax = await _priceFormatter.FormatPriceAsync(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode, false, languageId);

                foreach (var tr in taxRates)
                {
                    b2BOrderDetailsModel.TaxRates.Add(new ErpOrderDetailsModel.TaxRate
                    {
                        Rate = _priceFormatter.FormatTaxRate(tr.Key),
                        Value = await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(tr.Value, order.CurrencyRate), true, order.CustomerCurrencyCode, false, languageId),
                    });
                }
            }
        }
        b2BOrderDetailsModel.DisplayTaxRates = displayTaxRates;
        b2BOrderDetailsModel.DisplayTax = displayTax;
        b2BOrderDetailsModel.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoOrderDetailsPage;

        //discount (applied to order total)
        var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
        if (orderDiscountInCustomerCurrency > decimal.Zero)
        {
            b2BOrderDetailsModel.OrderTotalDiscount = await _priceFormatter.FormatPriceAsync(-orderDiscountInCustomerCurrency, true, order.CustomerCurrencyCode, false, languageId);
            b2BOrderDetailsModel.OrderTotalDiscountValue = orderDiscountInCustomerCurrency;
        }

        //gift cards
        foreach (var gcuh in await _giftCardService.GetGiftCardUsageHistoryAsync(order))
        {
            b2BOrderDetailsModel.GiftCards.Add(new ErpOrderDetailsModel.GiftCard
            {
                CouponCode = (await _giftCardService.GetGiftCardByIdAsync(gcuh.GiftCardId)).GiftCardCouponCode,
                Amount = await _priceFormatter.FormatPriceAsync(-(_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, languageId),
            });
        }

        //reward points           
        if (order.RedeemedRewardPointsEntryId.HasValue && await _rewardPointService.GetRewardPointsHistoryEntryByIdAsync(order.RedeemedRewardPointsEntryId.Value) is RewardPointsHistory redeemedRewardPointsEntry)
        {
            b2BOrderDetailsModel.RedeemedRewardPoints = -redeemedRewardPointsEntry.Points;
            b2BOrderDetailsModel.RedeemedRewardPointsAmount = await _priceFormatter.FormatPriceAsync(-(_currencyService.ConvertCurrency(redeemedRewardPointsEntry.UsedAmount, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, languageId);
        }

        //total
        var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
        b2BOrderDetailsModel.OrderTotal = await _priceFormatter.FormatPriceAsync(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, languageId);
        b2BOrderDetailsModel.OrderTotalValue = orderTotalInCustomerCurrency;

        //checkout attributes
        b2BOrderDetailsModel.CheckoutAttributeInfo = order.CheckoutAttributeDescription;

        //order notes
        foreach (var orderNote in (await _orderService.GetOrderNotesByOrderIdAsync(order.Id, true))
            .OrderByDescending(on => on.CreatedOnUtc)
            .ToList())
        {
            b2BOrderDetailsModel.OrderNotes.Add(new ErpOrderDetailsModel.OrderNote
            {
                Id = orderNote.Id,
                HasDownload = orderNote.DownloadId > 0,
                Note = _orderService.FormatOrderNoteText(orderNote),
                CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(orderNote.CreatedOnUtc, DateTimeKind.Utc)
            });
        }

        foreach (var nopOrderItem in orderItems)
        {
            var itemId = nopOrderItem.Id;
            var erpOrderItem = await _erpOrderItemAdditionalDataService.GetErpOrderItemAdditionalDataByNopOrderItemIdAsync(itemId);

            if (erpOrderItem == null)
                continue;

            var adjustmentValue = 0;
            var discountPerUnitExcTax = decimal.Zero;
            if (nopOrderItem.DiscountAmountExclTax > 0 && nopOrderItem.Quantity > 0)
                discountPerUnitExcTax = nopOrderItem.DiscountAmountExclTax / nopOrderItem.Quantity;
            var unitPriceWithOutDiscountExcTax = discountPerUnitExcTax + nopOrderItem.UnitPriceExclTax;
            totalPriceWithOutSavingsExcTax += nopOrderItem.PriceExclTax + nopOrderItem.DiscountAmountExclTax - adjustmentValue;
            b2BOnlineOrderDiscountExcTax += nopOrderItem.DiscountAmountExclTax;
            var product = await _productService.GetProductByIdAsync(nopOrderItem.ProductId);

            if (product == null)
                continue;
            var vendor = await _vendorService.GetVendorByProductIdAsync(product.Id);
            var pictures = await _pictureService.GetPicturesByProductIdAsync(product.Id);
            var picture = await _pictureService.GetProductPictureAsync(product, nopOrderItem.AttributesXml);

            var pictureModel = new PictureModel
            {
                ImageUrl = await _pictureService.GetPictureUrlAsync(product.Id),
                ThumbImageUrl = await _pictureService.GetThumbLocalPathAsync(picture),
                FullSizeImageUrl = string.Empty,
                Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), product.Name),
                AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), product.Name),
            };

            var orderItemDataModel = new ErpOrderDetailsModel.ErpOrderItemDataModel
            {
                NopOrderItemId = itemId,
                Quantity = nopOrderItem.Quantity,
                UnitPriceWithOutDiscountExcTax = await _priceFormatter.FormatPriceAsync(unitPriceWithOutDiscountExcTax),
                DiscountForPerUnitProductExcTax = await _priceFormatter.FormatPriceAsync(discountPerUnitExcTax),
                ItemWeight = nopOrderItem.ItemWeight ?? decimal.Zero,
                ItemWeightValue = _b2BB2CFeaturesSettings.DisplayWeightInformation && nopOrderItem.ItemWeight.HasValue ? $"{nopOrderItem.ItemWeight:F2} {baseWeight}" : "",
                Picture = pictureModel,
                TotalUnitPriceWithOutDiscountExcTax = await _priceFormatter.FormatPriceAsync(unitPriceWithOutDiscountExcTax * nopOrderItem.Quantity),
                Sku = product.Sku,
                AttributeInfo = await _productAttributeFormatter.FormatAttributesAsync(product, nopOrderItem.AttributesXml),
                ProductName = product.Name,
                ProductSeName = await _urlRecordService.GetSeNameAsync(product),
            };
            b2BOrderDetailsModel.PricesIncludeTax = order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax;
            b2BOrderDetailsModel.ShippingStatus = await _localizationService.GetLocalizedEnumAsync(order.ShippingStatus);
            b2BOrderDetailsModel.ShippingMethod = order.ShippingMethod;
            if (vendor != null)
                orderItemDataModel.VendorName = vendor.Name;

            if (erpOrderItem != null && b2bOrder != null)
            {
                orderItemDataModel.Id = erpOrderItem.Id;
                orderItemDataModel.ERPSalesUoM = erpOrderItem.ErpSalesUoM ?? string.Empty;
                orderItemDataModel.ERPOrderLineStatus = erpOrderItem.ErpOrderLineStatus ?? string.Empty;
                orderItemDataModel.ERPDateRequired = erpOrderItem.ErpDateRequired.HasValue ? erpOrderItem.ErpDateRequired.Value.ToShortDateString() : string.Empty;
                orderItemDataModel.ERPDateExpected = erpOrderItem.ErpDateExpected.HasValue ? erpOrderItem.ErpDateExpected.Value.ToShortDateString() : string.Empty;
                orderItemDataModel.ERPDeliveryMethod = erpOrderItem.ErpDeliveryMethod ?? string.Empty;
                orderItemDataModel.ERPInvoiceNumber = erpOrderItem.ErpInvoiceNumber ?? string.Empty;
                orderItemDataModel.ERPOrderLineNumber = erpOrderItem.ErpOrderLineNumber ?? string.Empty;
            }

            b2BOrderDetailsModel.Items.Add(orderItemDataModel);
        }

        if (b2bOrder != null)
        {
            b2BOrderDetailsModel.ERPOrderNumber = b2bOrder.ErpOrderNumber ?? b2bOrder.ErpOrderNumber;
            b2BOrderDetailsModel.ERPOrderStatus = b2bOrder.ERPOrderStatus ?? string.Empty;
            b2BOrderDetailsModel.IsQuoteOrder = b2bOrder.ErpOrderType == ErpOrderType.B2BSalesOrder ? false : true;
        }

        b2BOrderDetailsModel.CreatedOn = order.CreatedOnUtc;
        b2BOrderDetailsModel.TotalPriceWithOutSavingsExcTax = await _priceFormatter.FormatPriceAsync(totalPriceWithOutSavingsExcTax);
        b2BOrderDetailsModel.ErpOnlineOrderDiscountExcTax = await _priceFormatter.FormatPriceAsync(b2BOnlineOrderDiscountExcTax);

        if (_b2BB2CFeaturesSettings.DisplayWeightInformation)
        {
            baseWeight = await _localizationService.GetResourceAsync("B2B.TotalWeight.Custom.BaseWeight");
            b2BOrderDetailsModel.TotalWeight = b2BOrderDetailsModel.Items.Sum(x => x.Quantity * x.ItemWeight) ?? decimal.Zero;
            b2BOrderDetailsModel.TotalWeightValue = $"{b2BOrderDetailsModel.TotalWeight:F2} {baseWeight}";
        }

        return b2BOrderDetailsModel;
    }

    #endregion
}