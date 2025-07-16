using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;

public record ErpOrderAdditionalDataSearchModel : BaseSearchModel
{
    public ErpOrderAdditionalDataSearchModel()
    {
        AvailableErpOrderTypeOptions = new List<SelectListItem>();
        AvailableIntegrationStatusTypeOptions = new List<SelectListItem>();
        AvailableErpOrderOriginTypeOptions = new List<SelectListItem>();
        AvailableCustomerTypes = new List<SelectListItem>();
    }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.SearchOrderNumber")]
    public string SearchOrderNumber { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.SearchErpAccount")]
    public int SearchErpAccountId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.SearchERPOrderNumber")]
    public string SearchERPOrderNumber { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.SearchErpOrderType")]
    public int SearchErpOrderTypeId { get; set; }

    public IList<SelectListItem> AvailableErpOrderTypeOptions { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.SearchIntegrationStatusType")]
    public int SearchIntegrationStatusTypeId { get; set; }

    public IList<SelectListItem> AvailableIntegrationStatusTypeOptions { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.SearchErpOrderOriginType")]
    public int SearchErpOrderOriginTypeId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.SearchErpOrderPlaceByCustomerTypeId")]
    public int SearchErpOrderPlaceByCustomerTypeId { get; set; }

    public IList<SelectListItem> AvailableErpOrderOriginTypeOptions { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ERPIntegrationCore.ErpOrder.Fields.SearchOrderPlacedOn")]
    [UIHint("DateNullable")]
    public DateTime? SearchOrderPlacedOn { get; set; }

    public IList<SelectListItem> AvailableCustomerTypes { get; set; }
}
