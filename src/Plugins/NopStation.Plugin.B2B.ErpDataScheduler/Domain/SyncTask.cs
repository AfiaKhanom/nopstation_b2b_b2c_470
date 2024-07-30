using Nop.Core;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Domain
{
    /// <summary>
    /// Erp Data Scheduler SyncTask
    /// </summary>
    public partial class SyncTask : BaseEntity
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of appropriate ISyncTask class
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the run period (in seconds)
        /// </summary>
        public int Seconds { get; set; } = 1800;

        /// <summary>
        /// Gets or sets the datetime when task was enabled last time
        /// </summary>
        public DateTime? LastEnabledUtc { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether a task is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether a task should be stopped on some error
        /// </summary>
        public bool StopOnError { get; set; }

        /// <summary>
        /// Gets or sets the datetime when it was started last time
        /// </summary>
        public DateTime? LastStartUtc { get; set; }

        /// <summary>
        /// Gets or sets the datetime when it was finished last time (no matter failed or success)
        /// </summary>
        public DateTime? LastEndUtc { get; set; }

        /// <summary>
        /// Gets or sets the datetime when it was successfully finished last time
        /// </summary>
        public DateTime? LastSuccessUtc { get; set; }

        /// <summary>
        /// Gets or sets the day of the week and time slots in a string for the scheduler
        /// </summary>
        public string DayTimeSlots { get; set; }
    }
}