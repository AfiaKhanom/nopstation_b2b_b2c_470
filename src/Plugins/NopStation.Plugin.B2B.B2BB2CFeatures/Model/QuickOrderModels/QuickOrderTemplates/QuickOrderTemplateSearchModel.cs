using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.QuickOrderModels.QuickOrderTemplates;

public record QuickOrderTemplateSearchModel : BaseSearchModel
{
    [NopResourceDisplayName("NopStation.B2BB2CFeatures.QuickOrderTemplate.Fields.SearchName")]
    public string SearchName { get; set; }
    [UIHint("DateNullable")]
    [NopResourceDisplayName("NopStation.B2BB2CFeatures.QuickOrderTemplate.Fields.SearchCreatedOn")]
    public DateTime? SearchCreatedOn { get; set; }

    public int SearchCustomerId { get; set; }

    public bool LoadTotalPrice { get; set; }
}
