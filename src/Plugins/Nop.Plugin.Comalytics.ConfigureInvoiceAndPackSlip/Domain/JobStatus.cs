namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Domain
{
    /// <summary>
    /// Represents the PDF job status
    /// </summary>
    public enum JobStatus
    {
        /// <summary>
        /// Job is pending
        /// </summary>
        Pending = 1,

        /// <summary>
        /// Job is processing
        /// </summary>
        Processing = 2,

        /// <summary>
        /// Job completed successfully
        /// </summary>
        Completed = 3,

        /// <summary>
        /// Job failed
        /// </summary>
        Failed = 4
    }
}
