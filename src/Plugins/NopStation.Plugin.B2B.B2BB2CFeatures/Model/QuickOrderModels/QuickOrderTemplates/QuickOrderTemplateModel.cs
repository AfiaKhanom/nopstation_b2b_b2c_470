using System;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.QuickOrderModels.QuickOrderItems;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Catalog;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.QuickOrderModels.QuickOrderTemplates
{
    public record QuickOrderTemplateModel : BaseNopEntityModel
    {
        public QuickOrderTemplateModel()
        {
            ProductDetailsModel = new ProductDetailsModel();
        }
        [NopResourceDisplayName("NopStation.B2BB2CFeatures.QuickOrderTemplate.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("NopStation.B2BB2CFeatures.QuickOrderTemplate.Fields.Customer")]
        public int CustomerId { get; set; }

        [NopResourceDisplayName("NopStation.B2BB2CFeatures.QuickOrderTemplate.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
        public string CreatedOnStr { get; set; }

        [NopResourceDisplayName("NopStation.B2BB2CFeatures.QuickOrderTemplate.Fields.EditedOn")]
        public DateTime EditedOn { get; set; }
        public string EditedOnStr { get; set; }

        [NopResourceDisplayName("NopStation.B2BB2CFeatures.QuickOrderTemplate.Fields.LastOrderDate")]
        public DateTime? LastOrderDate { get; set; }
        public string LastOrderDateStr { get; set; }

        public decimal TotalValue { get; set; }

        [NopResourceDisplayName("NopStation.B2BB2CFeatures.QuickOrderTemplate.Fields.TotalValue")]
        public string TotalValueText { get; set; }

        [NopResourceDisplayName("NopStation.B2BB2CFeatures.QuickOrderTemplate.Fields.TotalOrderItems")]
        public string TotalOrderItems { get; set; }

        public string DisplayNameForPrimaryStock { get; set; }

        public bool ShowSecondaryStock { get; set; }

        public string DisplayNameForSecondaryStock { get; set; }

        public QuickOrderItemSearchModel QuickOrderItemSearchModel { get; set; }

        public QuickOrderItemModel AddQuickOrderItem { get; set; }

        public ProductDetailsModel ProductDetailsModel { get; set; }

        //public List<ProductAttributeModel> ProductAttributes { get; set; }
    }





}
