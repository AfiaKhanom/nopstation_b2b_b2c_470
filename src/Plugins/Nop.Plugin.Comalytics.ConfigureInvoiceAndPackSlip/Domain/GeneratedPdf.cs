using System;
using Nop.Core;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Domain
{
    /// <summary>
    /// Represents a generated PDF file
    /// </summary>
    public class GeneratedPdf : BaseEntity
    {
        /// <summary>
        /// Gets or sets the job identifier
        /// </summary>
        public int JobId { get; set; }

        /// <summary>
        /// Gets or sets the order identifier
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the file name
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the file path or blob URL
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the file size in bytes
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Gets or sets the date and time of entity creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
    }
}
