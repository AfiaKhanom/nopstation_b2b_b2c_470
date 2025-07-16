using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.ExportImport;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Controllers;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers;

public partial class OverridenOrderController : OrderController
{
    #region Fields

    protected readonly IAddressService _addressService;
    protected readonly IAttributeParser<AddressAttribute, AddressAttributeValue> _addressAttributeParser;
    protected readonly ICustomerActivityService _customerActivityService;
    protected readonly ICustomerService _customerService;
    protected readonly IDateTimeHelper _dateTimeHelper;
    protected readonly IEncryptionService _encryptionService;
    protected readonly IEventPublisher _eventPublisher;
    protected readonly IExportManager _exportManager;
    protected readonly IGiftCardService _giftCardService;
    protected readonly IImportManager _importManager;
    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly IOrderModelFactory _orderModelFactory;
    protected readonly IOrderProcessingService _orderProcessingService;
    protected readonly IOrderService _orderService;
    protected readonly IPaymentService _paymentService;
    protected readonly IPdfService _pdfService;
    protected readonly IPermissionService _permissionService;
    protected readonly IPriceCalculationService _priceCalculationService;
    protected readonly IProductAttributeFormatter _productAttributeFormatter;
    protected readonly IProductAttributeParser _productAttributeParser;
    protected readonly IProductAttributeService _productAttributeService;
    protected readonly IProductService _productService;
    protected readonly IShipmentService _shipmentService;
    protected readonly IShippingService _shippingService;
    protected readonly IShoppingCartService _shoppingCartService;
    protected readonly IStoreContext _storeContext;
    protected readonly IWorkContext _workContext;
    protected readonly IWorkflowMessageService _workflowMessageService;
    protected readonly OrderSettings _orderSettings;
    protected readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
    private readonly IErpOrderItemAdditionalDataService _erpOrderItemAdditionalDataService;
    private static readonly char[] _separator = [','];

    #endregion

    #region Ctor

    public OverridenOrderController(IAddressService addressService,
        IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
        ICustomerActivityService customerActivityService,
        ICustomerService customerService,
        IDateTimeHelper dateTimeHelper,
        IEncryptionService encryptionService,
        IEventPublisher eventPublisher,
        IExportManager exportManager,
        IGiftCardService giftCardService,
        IImportManager importManager,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IOrderModelFactory orderModelFactory,
        IOrderProcessingService orderProcessingService,
        IOrderService orderService,
        IPaymentService paymentService,
        IPdfService pdfService,
        IPermissionService permissionService,
        IPriceCalculationService priceCalculationService,
        IProductAttributeFormatter productAttributeFormatter,
        IProductAttributeParser productAttributeParser,
        IProductAttributeService productAttributeService,
        IProductService productService,
        IShipmentService shipmentService,
        IShippingService shippingService,
        IShoppingCartService shoppingCartService,
        IStoreContext storeContext,
        IWorkContext workContext,
        IWorkflowMessageService workflowMessageService,
        OrderSettings orderSettings,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        IErpOrderItemAdditionalDataService erpOrderItemAdditionalDataService) : base(addressService,
            addressAttributeParser,
            customerActivityService,
            customerService,
            dateTimeHelper,
            encryptionService,
            eventPublisher,
            exportManager,
            giftCardService,
            importManager,
            localizationService,
            notificationService,
            orderModelFactory,
            orderProcessingService,
            orderService,
            paymentService,
            pdfService,
            permissionService,
            priceCalculationService,
            productAttributeFormatter,
            productAttributeParser,
            productAttributeService,
            productService,
            shipmentService,
            shippingService,
            shoppingCartService,
            storeContext,
            workContext,
            workflowMessageService,
            orderSettings)
    {
        _addressService = addressService;
        _addressAttributeParser = addressAttributeParser;
        _customerActivityService = customerActivityService;
        _customerService = customerService;
        _dateTimeHelper = dateTimeHelper;
        _encryptionService = encryptionService;
        _eventPublisher = eventPublisher;
        _exportManager = exportManager;
        _giftCardService = giftCardService;
        _importManager = importManager;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _orderModelFactory = orderModelFactory;
        _orderProcessingService = orderProcessingService;
        _orderService = orderService;
        _paymentService = paymentService;
        _pdfService = pdfService;
        _permissionService = permissionService;
        _priceCalculationService = priceCalculationService;
        _productAttributeFormatter = productAttributeFormatter;
        _productAttributeParser = productAttributeParser;
        _productAttributeService = productAttributeService;
        _productService = productService;
        _shipmentService = shipmentService;
        _shippingService = shippingService;
        _shoppingCartService = shoppingCartService;
        _storeContext = storeContext;
        _workContext = workContext;
        _workflowMessageService = workflowMessageService;
        _orderSettings = orderSettings;
        _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
        _erpOrderItemAdditionalDataService = erpOrderItemAdditionalDataService;
    }

    #endregion

    #region Order details

    #region Edit, delete

    [HttpPost]
    public override async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AccessDeniedView();

        //try to get an order with the specified id
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return RedirectToAction("List");

        //a vendor does not have access to this functionality
        if (await _workContext.GetCurrentVendorAsync() != null)
            return RedirectToAction("Edit", new { id = order.Id });

        var erpOrder = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(order.Id);
        if (erpOrder != null)
        {
            var erpOrderItemAdditionalDatas = await _erpOrderItemAdditionalDataService.GetAllErpOrderItemAdditionalDataByErpOrderIdAsync(erpOrder.Id);

            foreach (var erpOrderItemAdditionalData in erpOrderItemAdditionalDatas)
                await _erpOrderItemAdditionalDataService.DeleteErpOrderItemAdditionalDataByIdAsync(erpOrderItemAdditionalData.Id);

            await _erpOrderAdditionalDataService.DeleteErpOrderAdditionalDataByIdAsync(erpOrder.Id);
        }

        await _orderProcessingService.DeleteOrderAsync(order);

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteOrder",
            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteOrder"), order.Id), order);

        return RedirectToAction("List");
    }

    [HttpPost, ActionName("Edit")]
    [FormValueRequired(FormValueRequirement.StartsWith, "btnDeleteOrderItem")]
    public override async Task<IActionResult> DeleteOrderItem(int id, IFormCollection form)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
            return AccessDeniedView();

        //try to get an order with the specified id
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
            return RedirectToAction("List");

        //a vendor does not have access to this functionality
        if (await _workContext.GetCurrentVendorAsync() != null)
            return RedirectToAction("Edit", new { id = order.Id });

        //get order item identifier
        var orderItemId = 0;
        foreach (var formValue in form.Keys)
            if (formValue.StartsWith("btnDeleteOrderItem", StringComparison.InvariantCultureIgnoreCase))
                orderItemId = Convert.ToInt32(formValue["btnDeleteOrderItem".Length..]);

        var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId)
            ?? throw new ArgumentException("No order item found with the specified id");

        var erpOrderItemAdditionalData = await _erpOrderItemAdditionalDataService
            .GetErpOrderItemAdditionalDataByNopOrderItemIdAsync(orderItemId);

        if (erpOrderItemAdditionalData != null)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Orders.OrderItem.DeleteAssociatedErpOrderItemAdditionalDataRecordError"));
            return RedirectToAction("Edit", new { id = order.Id });
        }

        if ((await _giftCardService.GetGiftCardsByPurchasedWithOrderItemIdAsync(orderItem.Id)).Any())
        {
            //we cannot delete an order item with associated gift cards
            //a store owner should delete them first

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Orders.OrderItem.DeleteAssociatedGiftCardRecordError"));
        }
        else
        {
            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

            //adjust inventory
            await _productService.AdjustInventoryAsync(product, orderItem.Quantity, orderItem.AttributesXml,
                string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.DeleteOrderItem"), order.Id));

            //delete item
            await _orderService.DeleteOrderItemAsync(orderItem);

            //update order totals
            var updateOrderParameters = new UpdateOrderParameters(order, orderItem);
            await _orderProcessingService.UpdateOrderTotalsAsync(updateOrderParameters);

            //add a note
            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = order.Id,
                Note = "Order item has been deleted",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            await LogEditOrderAsync(order.Id);

            foreach (var warning in updateOrderParameters.Warnings)
                _notificationService.WarningNotification(warning);
        }

        //selected card
        SaveSelectedCardName("order-products");

        return RedirectToAction("Edit", new { id = order.Id });
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteB2BOrderItem(int id, int orderItemId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.Admin.Orders.ErpOrderItem.AccessDenied"));
            return Json(new
            {
                success = false,
                redirect = Url.Action("List", "Order")
            });
        }

        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.Admin.Orders.ErpOrderItem.NoOrderFound"));
            return Json(new
            {
                success = false,
                redirect = Url.Action("List", "Order")
            });
        }

        if (await _workContext.GetCurrentVendorAsync() != null)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.Admin.Orders.ErpOrderItem.VendorAccessDenied"));
            return Json(new
            {
                success = false,
                redirect = Url.Action("Edit", "Order", new { id = order.Id })
            });
        }

        var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId);
        if (orderItem == null)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.Admin.Orders.ErpOrderItem.NoOrderItemFound"));
            return Json(new
            {
                success = false,
                redirect = Url.Action("Edit", "Order", new { id = order.Id })
            });
        }

        var erpOrderItemAdditionalData = await _erpOrderItemAdditionalDataService
            .GetErpOrderItemAdditionalDataByNopOrderItemIdAsync(orderItemId);
        if (erpOrderItemAdditionalData == null)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.Admin.Orders.ErpOrderItem.NoOrderItemFound"));
            return Json(new
            {
                success = false,
                redirect = Url.Action("Edit", "Order", new { id = order.Id })
            });
        }

        try
        {
            await _erpOrderItemAdditionalDataService.DeleteErpOrderItemAdditionalDataByIdAsync(erpOrderItemAdditionalData.Id);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteB2BOrderItem",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteB2BOrderItem"), orderItemId), order);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.B2B.OrderItem.Deleted"));

            return Json(new
            {
                success = true,
                redirect = Url.Action("Edit", "Order", new { id = order.Id })
            });
        }
        catch (Exception ex)
        {
            _notificationService.ErrorNotification(ex.Message);
            return Json(new
            {
                success = false,
                redirect = Url.Action("Edit", "Order", new { id = order.Id })
            });
        }
    }

    #endregion

    #endregion
}
