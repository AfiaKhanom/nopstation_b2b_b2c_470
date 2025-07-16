
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Checkout;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.OrderSummary;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories
{
    public class ErpOrderItemModelFactory : IErpOrderItemModelFactory
    {
        private readonly ILocalizationService _localizationService;
        private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IB2BB2CWorkContext _b2BB2CWorkContext;
        private readonly IErpAccountService _erpAccountService;
        private readonly IOrderService _orderService;
        private readonly IErpOrderItemAdditionalDataService _erpOrderItemAdditionalDataService;
        private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
        private readonly IMeasureService _measureService;
        private readonly MeasureSettings _measureSettings;

        public ErpOrderItemModelFactory(
            IOrderService orderService,
            IErpOrderItemAdditionalDataService erpOrderItemAdditionalDataService,
            IErpOrderAdditionalDataService erpOrderAdditionalDataService,
            IMeasureService measureService,
            MeasureSettings measureSettings,
            ILocalizationService localizationService,
            B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
            IPriceFormatter priceFormatter,
            IB2BB2CWorkContext b2BB2CWorkContext,
            IErpAccountService erpAccountService)
        {
            _orderService = orderService;
            _erpOrderItemAdditionalDataService = erpOrderItemAdditionalDataService;
            _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
            _measureService = measureService;
            _measureSettings = measureSettings;
            _localizationService = localizationService;
            _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
            _priceFormatter = priceFormatter;
            _b2BB2CWorkContext = b2BB2CWorkContext;
            _erpAccountService = erpAccountService;
        }

        public async Task<ErpOrderDetailsModel> PrepareB2BOrderItemDataModelListModelAsync(int nopOrderId, List<string> itemIds)
        {
            var currentCustomer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();
            var b2BOrderDetailsModel = new ErpOrderDetailsModel();
            var b2BAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(currentCustomer.Id);
            var erpOrder = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(nopOrderId);
            var b2bOrder = erpOrder.ErpOrderType == ErpOrderType.B2BSalesOrder || erpOrder.ErpOrderType == ErpOrderType.B2BQuote ? erpOrder : null;
            var b2COrder = erpOrder.ErpOrderType == ErpOrderType.B2CSalesOrder || erpOrder.ErpOrderType == ErpOrderType.B2CQuote ? erpOrder : null;

            var baseWeight = "";
            var totalPriceWithOutSavingsExcTax = decimal.Zero;
            var b2BOnlineOrderDiscountExcTax = decimal.Zero;

            if (_b2BB2CFeaturesSettings.DisplayWeightInformation)
                baseWeight = (await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId))?.Name;

            if (b2BAccount != null && (b2bOrder != null || b2COrder != null) && itemIds != null)
            {
                foreach (var id in itemIds)
                {
                    if (int.TryParse(id, out var itemId))
                    {
                        var nopOrderItem = await _orderService.GetOrderItemByIdAsync(itemId);
                        var erpOrderItem = await _erpOrderItemAdditionalDataService.GetErpOrderItemAdditionalDataByNopOrderItemIdAsync(itemId);

                        if (erpOrderItem == null)
                            continue;

                        //var adjustmentValue = erpOrderItem?.AdjustmentValueOnOnlineSavingsForCashSaleRounding ?? 0;
                        var adjustmentValue = 0;
                        var discountPerUnitExcTax = decimal.Zero;
                        if (nopOrderItem.DiscountAmountExclTax > 0 && nopOrderItem.Quantity > 0)
                            discountPerUnitExcTax = nopOrderItem.DiscountAmountExclTax / nopOrderItem.Quantity;
                        var unitPriceWithOutDiscountExcTax = discountPerUnitExcTax + nopOrderItem.UnitPriceExclTax;
                        totalPriceWithOutSavingsExcTax += nopOrderItem.PriceExclTax + nopOrderItem.DiscountAmountExclTax - adjustmentValue;
                        b2BOnlineOrderDiscountExcTax += nopOrderItem.DiscountAmountExclTax;

                        var orderItemDataModel = new ErpOrderDetailsModel.ErpOrderItemDataModel
                        {
                            NopOrderItemId = itemId,
                            Quantity = nopOrderItem.Quantity,
                            UnitPriceWithOutDiscountExcTax = await _priceFormatter.FormatPriceAsync(unitPriceWithOutDiscountExcTax),
                            DiscountForPerUnitProductExcTax = await _priceFormatter.FormatPriceAsync(discountPerUnitExcTax),
                            ItemWeight = nopOrderItem.ItemWeight ?? decimal.Zero,
                            ItemWeightValue = _b2BB2CFeaturesSettings.DisplayWeightInformation && nopOrderItem.ItemWeight.HasValue ? $"{nopOrderItem.ItemWeight:F2} {baseWeight}" : "",
                        };

                        if (erpOrderItem != null && b2bOrder != null)
                        {
                            orderItemDataModel.Id = erpOrderItem.Id;
                            orderItemDataModel.ERPSalesUoM = erpOrderItem.ErpSalesUoM ?? string.Empty;
                            orderItemDataModel.ERPOrderLineStatus = erpOrderItem.ErpOrderLineStatus ?? string.Empty;
                            orderItemDataModel.ERPDateRequired = erpOrderItem.ErpDateRequired.HasValue ? erpOrderItem.ErpDateRequired.Value.ToShortDateString() : string.Empty;
                            orderItemDataModel.ERPDateExpected = erpOrderItem.ErpDateExpected.HasValue ? erpOrderItem.ErpDateExpected.Value.ToShortDateString() : string.Empty;
                            orderItemDataModel.ERPDeliveryMethod = erpOrderItem.ErpDeliveryMethod ?? string.Empty;
                            orderItemDataModel.ERPInvoiceNumber = erpOrderItem.ErpInvoiceNumber ?? string.Empty;
                        }

                        else if (erpOrderItem != null && b2COrder != null)
                        {
                            orderItemDataModel.Id = erpOrderItem.Id;
                            orderItemDataModel.ERPSalesUoM = erpOrderItem.ErpSalesUoM ?? string.Empty;
                            orderItemDataModel.ERPOrderLineStatus = erpOrderItem.ErpOrderLineStatus ?? string.Empty;
                            orderItemDataModel.ERPOrderLineNumber = erpOrderItem.ErpOrderLineNumber ?? string.Empty;
                            orderItemDataModel.ERPDateRequired = erpOrderItem.ErpDateRequired.HasValue ? erpOrderItem.ErpDateRequired.Value.ToShortDateString() : string.Empty;
                            orderItemDataModel.ERPDateExpected = erpOrderItem.ErpDateExpected.HasValue ? erpOrderItem.ErpDateExpected.Value.ToShortDateString() : string.Empty;
                            //orderItemDataModel.DeliveryDate = erpOrderItem.DeliveryDate.HasValue ? erpOrderItem.DeliveryDate.Value.ToShortDateString() : string.Empty;
                            orderItemDataModel.DeliveryDate = erpOrder.DeliveryDate.HasValue ? erpOrder.DeliveryDate.Value.ToShortDateString() : string.Empty;
                            orderItemDataModel.ERPDeliveryMethod = erpOrderItem.ErpDeliveryMethod ?? string.Empty;
                            orderItemDataModel.ERPInvoiceNumber = erpOrderItem.ErpInvoiceNumber ?? string.Empty;
                            //orderItemDataModel.SpecialInstructions = erpOrderItem.SpecialInstructions ?? string.Empty;
                            orderItemDataModel.SpecialInstructions = erpOrder.SpecialInstructions ?? string.Empty;
                            //orderItemDataModel.WarehouseName = await _shippingService.GetWarehouseByIdAsync(erpOrderItem.NopWarehouseId)?.Name ?? erpOrderItem.WarehouseCode ?? string.Empty;
                            orderItemDataModel.WarehouseName = string.Empty;
                        }

                        b2BOrderDetailsModel.Items.Add(orderItemDataModel);
                    }
                }

                if (b2bOrder != null)
                {
                    b2BOrderDetailsModel.ERPOrderNumber = b2bOrder.ErpOrderNumber ?? b2bOrder.ErpOrderNumber;
                    b2BOrderDetailsModel.ERPOrderStatus = b2bOrder.ERPOrderStatus ?? string.Empty;
                    b2BOrderDetailsModel.IsQuoteOrder = b2bOrder.ErpOrderType == ErpOrderType.B2BSalesOrder ? false : true;
                }
                else if (b2COrder != null)
                {
                    b2BOrderDetailsModel.ERPOrderNumber = b2COrder.ErpOrderNumber ?? b2COrder.ErpOrderNumber;
                    b2BOrderDetailsModel.ERPOrderStatus = b2COrder.ERPOrderStatus ?? string.Empty;
                    b2BOrderDetailsModel.IsQuoteOrder = b2COrder.ErpOrderType == ErpOrderType.B2CSalesOrder ? false : true;
                }

                b2BOrderDetailsModel.TotalPriceWithOutSavingsExcTax = await _priceFormatter.FormatPriceAsync(totalPriceWithOutSavingsExcTax);
                b2BOrderDetailsModel.ErpOnlineOrderDiscountExcTax = await _priceFormatter.FormatPriceAsync(b2BOnlineOrderDiscountExcTax);

                if (_b2BB2CFeaturesSettings.DisplayWeightInformation)
                {
                    baseWeight = await _localizationService.GetResourceAsync("B2B.TotalWeight.Custom.BaseWeight");
                    b2BOrderDetailsModel.TotalWeight = b2BOrderDetailsModel.Items.Sum(x => x.Quantity * x.ItemWeight) ?? decimal.Zero;
                    b2BOrderDetailsModel.TotalWeightValue = $"{b2BOrderDetailsModel.TotalWeight:F2} {baseWeight}";
                }
            }

            return b2BOrderDetailsModel;
        }

        public async Task<ErpCheckoutCompletedModel> PrepareB2BCheckoutCompletedModelAsync(int nopOrderId)
        {
            var b2BCheckoutCompleted = new ErpCheckoutCompletedModel();
            var currentCustomer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();
            var b2BOrderDetailsModel = new ErpOrderDetailsModel();
            var b2BAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(currentCustomer.Id);
            var erpOrder = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(nopOrderId);
            var b2bOrder = erpOrder.ErpOrderType == ErpOrderType.B2BSalesOrder || erpOrder.ErpOrderType == ErpOrderType.B2BQuote ? erpOrder : null;
            var b2COrder = erpOrder.ErpOrderType == ErpOrderType.B2CSalesOrder || erpOrder.ErpOrderType == ErpOrderType.B2CQuote ? erpOrder : null;

            if (b2BAccount != null && (b2bOrder != null || b2COrder != null))
            {
                b2BCheckoutCompleted.ERPOrderNumber = b2bOrder?.ErpOrderNumber ?? b2bOrder?.ErpOrderNumber ?? b2COrder?.ErpOrderNumber ?? b2COrder?.ErpOrderNumber;
                b2BCheckoutCompleted.IsErpIntegrationSuccess = (b2bOrder?.IntegrationStatusType == IntegrationStatusType.Confirmed ||
                    b2COrder?.IntegrationStatusType == IntegrationStatusType.Confirmed) ? true : false;

                if (b2bOrder?.ErpOrderType == ErpOrderType.B2BSalesOrder)
                {
                    if (b2BAccount.AllowOverspend && b2BAccount.CreditLimit < b2BAccount.CurrentBalance)
                    {
                        if (_b2BB2CFeaturesSettings.IsShowOverSpendWarningText)
                        {
                            b2BCheckoutCompleted.DisplayOverSpendWarningText = true;
                            b2BCheckoutCompleted.OverSpendWarningText = _b2BB2CFeaturesSettings.OverSpendWarningText;
                        }
                    }
                }

                else if (b2bOrder?.ErpOrderType == ErpOrderType.B2BQuote || b2COrder?.ErpOrderType == ErpOrderType.B2CQuote)
                    b2BCheckoutCompleted.IsQuoteOrder = true;

                if (!b2BCheckoutCompleted.IsErpIntegrationSuccess)
                {
                    //b2BCheckoutCompleted.IntegrationError = _b2BSAPErrorMsgTranslationService.GetTranslatedAndCompleteIntegrationErrorMsg
                    //                        ((ErpOrderType)(b2bOrder?.ErpOrderType ?? b2COrder?.ErpOrderType), b2bOrder?.IntegrationError ?? b2COrder?.IntegrationError);
                    var error = b2bOrder?.ErpOrderType ?? b2COrder?.ErpOrderType;
                    b2BCheckoutCompleted.IntegrationError = await _localizationService.GetLocalizedEnumAsync(error ?? ErpOrderType.B2CSalesOrder) + b2bOrder?.IntegrationError ?? b2COrder?.IntegrationError;
                }

            }

            return b2BCheckoutCompleted;
        }
    }
}