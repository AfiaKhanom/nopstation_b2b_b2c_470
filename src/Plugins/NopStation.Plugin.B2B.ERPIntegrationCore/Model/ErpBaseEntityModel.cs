using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Model;

public record ErpBaseEntityModel : BaseNopEntityModel
{
    [NopResourceDisplayName("B2BB2CFeatures.Common.Fields.IsActive")]
    public bool IsActive { get; set; }

    [NopResourceDisplayName("B2BB2CFeatures.Common.Fields.CreatedOnUtc")]
    public DateTime CreatedOnUtc { get; set; }

    [NopResourceDisplayName("B2BB2CFeatures.Common.Fields.CreatedById")]
    public int CreatedById { get; set; }

    [NopResourceDisplayName("B2BB2CFeatures.Common.Fields.UpdatedOnUtc")]
    public DateTime UpdatedOnUtc { get; set; }

    [NopResourceDisplayName("B2BB2CFeatures.Common.Fields.UpdatedById")]
    public int UpdatedById { get; set; }
    public bool IsDeleted { get; set; }
}
