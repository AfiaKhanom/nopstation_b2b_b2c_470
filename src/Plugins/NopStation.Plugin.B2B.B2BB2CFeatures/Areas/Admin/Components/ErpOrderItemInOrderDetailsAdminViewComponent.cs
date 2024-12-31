using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Components;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Components;

public class ErpOrderItemInOrderDetailsAdminViewComponent : NopViewComponent
{
    #region Fields

    private readonly IErpOrderModelFactory _erpOrderModelFactory;
    private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;

    #endregion

    #region Ctor

    public ErpOrderItemInOrderDetailsAdminViewComponent(IErpOrderModelFactory erpOrderModelFactory,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService)
    {
        _erpOrderModelFactory = erpOrderModelFactory;
        _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
    }

    #endregion

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        if (!(additionalData is OrderModel orderModel))
            return Content(string.Empty);

        var erpOrderAdditionalData = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(orderModel.Id);

        if (erpOrderAdditionalData == null)
            return Content(string.Empty);

        if (erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2BSalesOrder || erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2BQuote)
        {
            var erpOrderModel = await _erpOrderModelFactory.PrepareErpOrderModel(new ErpOrderModel(), orderModel);
            return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Areas/Admin/Views/Shared/Components/ErpOrderItemInOrderDetailsAdmin/ErpOrderItemAdditionalDataForB2B.cshtml", erpOrderModel);
        }

        if (erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2CSalesOrder || erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2CQuote)
        {
            var erpOrderModel = await _erpOrderModelFactory.PrepareErpOrderModel(new ErpOrderModel(), orderModel);
            return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Areas/Admin/Views/Shared/Components/ErpOrderItemInOrderDetailsAdmin/ErpOrderItemAdditionalDataForB2C.cshtml", erpOrderModel);
        }

        return Content(string.Empty);
    }

    #endregion
}
