using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.ErpAccountPublic;

public record ErpAccountOrderSearchModel : BaseSearchModel
{
    public int ErpAccountId { get; set; }
    public string ErpAccountNumber { get; set; }
    public int ErpNopUserId { get; set; }
    public int NopCustomerId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccountOrderDetails.Fields.SearchOrderNumberOrName")]
    public string SearchOrderNumberOrName { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccountOrderDetails.Fields.SearchOrderDateFrom")]
    [UIHint("DateNullable")]
    [DataType(DataType.Date)]
    public DateTime? SearchOrderDateFrom { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.B2BB2CFeatures.ErpAccountOrderDetails.Fields.SearchOrderDateTo")]
    [UIHint("DateNullable")]
    [DataType(DataType.Date)]
    public DateTime? SearchOrderDateTo { get; set; }
}