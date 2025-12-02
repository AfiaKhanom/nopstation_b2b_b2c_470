using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Areas.Admin.Models
{
    /// <summary>
    /// Configuration model
    /// </summary>
    public record ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
            AvailableRenderers = new List<SelectListItem>();
            AvailablePageOrientations = new List<SelectListItem>();
            AvailablePageSizes = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Comalytics.ConfigureInvoiceAndPackSlip.Configure.PdfRenderer")]
        public string PdfRendererProvider { get; set; }

        [NopResourceDisplayName("Plugins.Comalytics.ConfigureInvoiceAndPackSlip.Configure.PageOrientation")]
        public string PageOrientation { get; set; }

        [NopResourceDisplayName("Plugins.Comalytics.ConfigureInvoiceAndPackSlip.Configure.PageSize")]
        public string PageSize { get; set; }

        [NopResourceDisplayName("Plugins.Comalytics.ConfigureInvoiceAndPackSlip.Configure.Margins")]
        public int TopMargin { get; set; }

        public int BottomMargin { get; set; }

        public int LeftMargin { get; set; }

        public int RightMargin { get; set; }

        [NopResourceDisplayName("Plugins.Comalytics.ConfigureInvoiceAndPackSlip.Configure.FontSettings")]
        public string FontFamily { get; set; }

        public int FontSize { get; set; }

        public bool EnableRtl { get; set; }

        public int MaxPictureWidth { get; set; }

        public int MaxPictureHeight { get; set; }

        public bool EmbedFonts { get; set; }

        public int NumberOfCopies { get; set; }

        public bool EnableBackgroundProcessing { get; set; }

        public IList<SelectListItem> AvailableRenderers { get; set; }

        public IList<SelectListItem> AvailablePageOrientations { get; set; }

        public IList<SelectListItem> AvailablePageSizes { get; set; }
    }
}
