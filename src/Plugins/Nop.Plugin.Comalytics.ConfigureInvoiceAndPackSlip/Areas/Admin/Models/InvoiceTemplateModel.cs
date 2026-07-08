using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Areas.Admin.Models
{
    /// <summary>
    /// Represents an invoice template model
    /// </summary>
    public record InvoiceTemplateModel : BaseNopEntityModel
    {
        public InvoiceTemplateModel()
        {
            AvailableStores = new List<SelectListItem>();
            AvailableLanguages = new List<SelectListItem>();
            AvailableRenderModes = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Comalytics.ConfigureInvoiceAndPackSlip.InvoiceTemplates.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Plugins.Comalytics.ConfigureInvoiceAndPackSlip.InvoiceTemplates.Store")]
        public int StoreId { get; set; }

        [NopResourceDisplayName("Plugins.Comalytics.ConfigureInvoiceAndPackSlip.InvoiceTemplates.Language")]
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Plugins.Comalytics.ConfigureInvoiceAndPackSlip.InvoiceTemplates.IsDefault")]
        public bool IsDefault { get; set; }

        [NopResourceDisplayName("Plugins.Comalytics.ConfigureInvoiceAndPackSlip.InvoiceTemplates.RenderMode")]
        public int RenderModeId { get; set; }

        [NopResourceDisplayName("Plugins.Comalytics.ConfigureInvoiceAndPackSlip.InvoiceTemplates.TemplateHtml")]
        public string TemplateHtml { get; set; }

        public string CreatedOn { get; set; }
        public string UpdatedOn { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<SelectListItem> AvailableLanguages { get; set; }
        public IList<SelectListItem> AvailableRenderModes { get; set; }
    }
}
