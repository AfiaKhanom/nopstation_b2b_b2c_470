using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Components;

public class ErpOrderInOrderDetailsAdminViewComponent : NopViewComponent
{
    #region Fields

    private readonly IErpOrderModelFactory _erpOrderModelFactory;
    private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;

    #endregion

    #region Ctor

    public ErpOrderInOrderDetailsAdminViewComponent(IErpOrderModelFactory erpOrderModelFactory, IErpOrderAdditionalDataService erpOrderAdditionalDataService)
    {
        _erpOrderModelFactory = erpOrderModelFactory;
        _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
    }

    #endregion

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        if (!(additionalData is int orderId))
            return Content(string.Empty);

        var erpOrderAdditionalData = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(orderId);

        if (erpOrderAdditionalData == null)
            return Content(string.Empty);

        if (erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2BSalesOrder || erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2BQuote)
        {
            var erpOrderAdditionalDataModel = await _erpOrderModelFactory.PrepareErpOrderPerAccountModel(null, erpOrderAdditionalData);
            return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Areas/Admin/Views/Shared/Components/ErpOrderInOrderDetailsAdmin/ErpOrderAdditionalDataForB2B.cshtml", erpOrderAdditionalDataModel);
        }

        if (erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2CSalesOrder || erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2CQuote)
        {
            var erpOrderAdditionalDataModel = await _erpOrderModelFactory.PrepareErpOrderPerAccountModel(null, erpOrderAdditionalData);
            return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Areas/Admin/Views/Shared/Components/ErpOrderInOrderDetailsAdmin/ErpOrderAdditionalDataForB2C.cshtml", erpOrderAdditionalDataModel);
        }

        return Content(string.Empty);
    }

    #endregion
}
