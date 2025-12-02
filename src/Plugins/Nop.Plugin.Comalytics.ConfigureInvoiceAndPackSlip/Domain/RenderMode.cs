namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Domain
{
    /// <summary>
    /// Represents the PDF rendering mode
    /// </summary>
    public enum RenderMode
    {
        /// <summary>
        /// QuestPDF native rendering
        /// </summary>
        QuestPdf = 1,

        /// <summary>
        /// HTML to PDF using Chromium
        /// </summary>
        Chromium = 2
    }
}
