namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Domain
{
    /// <summary>
    /// Represents the PDF job type
    /// </summary>
    public enum JobType
    {
        /// <summary>
        /// Single invoice generation
        /// </summary>
        SingleInvoice = 1,

        /// <summary>
        /// Batch invoice generation
        /// </summary>
        BatchInvoice = 2,

        /// <summary>
        /// Single packing slip generation
        /// </summary>
        SinglePackingSlip = 3,

        /// <summary>
        /// Batch packing slip generation
        /// </summary>
        BatchPackingSlip = 4
    }
}
