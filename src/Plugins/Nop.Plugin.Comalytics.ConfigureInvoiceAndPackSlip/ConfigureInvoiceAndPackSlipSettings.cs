using Nop.Core.Configuration;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip
{
    /// <summary>
    /// Represents settings for invoice and packing slip configuration
    /// </summary>
    public class ConfigureInvoiceAndPackSlipSettings : ISettings
    {
        /// <summary>
        /// Gets or sets the default PDF renderer provider (QuestPdf or Chromium)
        /// </summary>
        public string PdfRendererProvider { get; set; } = "QuestPdf";

        /// <summary>
        /// Gets or sets the default page orientation (Portrait or Landscape)
        /// </summary>
        public string PageOrientation { get; set; } = "Portrait";

        /// <summary>
        /// Gets or sets the default page size (A4, Letter, etc.)
        /// </summary>
        public string PageSize { get; set; } = "A4";

        /// <summary>
        /// Gets or sets the top margin in millimeters
        /// </summary>
        public int TopMargin { get; set; } = 20;

        /// <summary>
        /// Gets or sets the bottom margin in millimeters
        /// </summary>
        public int BottomMargin { get; set; } = 20;

        /// <summary>
        /// Gets or sets the left margin in millimeters
        /// </summary>
        public int LeftMargin { get; set; } = 20;

        /// <summary>
        /// Gets or sets the right margin in millimeters
        /// </summary>
        public int RightMargin { get; set; } = 20;

        /// <summary>
        /// Gets or sets the default font family
        /// </summary>
        public string FontFamily { get; set; } = "Arial";

        /// <summary>
        /// Gets or sets the default font size
        /// </summary>
        public int FontSize { get; set; } = 10;

        /// <summary>
        /// Gets or sets the maximum picture width
        /// </summary>
        public int MaxPictureWidth { get; set; } = 200;

        /// <summary>
        /// Gets or sets the maximum picture height
        /// </summary>
        public int MaxPictureHeight { get; set; } = 200;

        /// <summary>
        /// Gets or sets a value indicating whether to enable RTL support
        /// </summary>
        public bool EnableRtl { get; set; } = false;

        /// <summary>
        /// Gets or sets the file storage path
        /// </summary>
        public string FileStoragePath { get; set; } = "~/App_Data/Plugins/Comalytics.ConfigureInvoiceAndPackSlip/Pdfs";

        /// <summary>
        /// Gets or sets the number of copies to print
        /// </summary>
        public int NumberOfCopies { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether to embed fonts
        /// </summary>
        public bool EmbedFonts { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to enable background processing for batch jobs
        /// </summary>
        public bool EnableBackgroundProcessing { get; set; } = true;
    }
}
