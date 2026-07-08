using System;
using Nop.Core;

namespace Nop.Plugin.Comalytics.ConfigureInvoiceAndPackSlip.Domain
{
    /// <summary>
    /// Represents a PDF generation job
    /// </summary>
    public class PdfJob : BaseEntity
    {
        /// <summary>
        /// Gets or sets the job type identifier
        /// </summary>
        public int JobTypeId { get; set; }

        /// <summary>
        /// Gets or sets the job type
        /// </summary>
        public JobType JobType
        {
            get => (JobType)JobTypeId;
            set => JobTypeId = (int)value;
        }

        /// <summary>
        /// Gets or sets the status identifier
        /// </summary>
        public int StatusId { get; set; }

        /// <summary>
        /// Gets or sets the status
        /// </summary>
        public JobStatus Status
        {
            get => (JobStatus)StatusId;
            set => StatusId = (int)value;
        }

        /// <summary>
        /// Gets or sets the requested by customer identifier
        /// </summary>
        public int RequestedByCustomerId { get; set; }

        /// <summary>
        /// Gets or sets the parameters JSON
        /// </summary>
        public string ParametersJson { get; set; }

        /// <summary>
        /// Gets or sets the date and time of job creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of job completion
        /// </summary>
        public DateTime? CompletedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
