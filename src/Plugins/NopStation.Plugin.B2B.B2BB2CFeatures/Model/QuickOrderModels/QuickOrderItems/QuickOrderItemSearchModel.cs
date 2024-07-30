using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.QuickOrderModels.QuickOrderItems
{
    public record QuickOrderItemSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("NopStation.B2BB2CFeatures.QuickOrderItemSearch.Fields.QuickOrderTemplate")]
        public int QuickOrderTemplateId { get; set; }

        [NopResourceDisplayName("NopStation.B2BB2CFeatures.QuickOrderItemSearch.Fields.ProductSku")]
        public string ProductSku { get; set; }

        public bool Validate { get; set; }
    }
}
