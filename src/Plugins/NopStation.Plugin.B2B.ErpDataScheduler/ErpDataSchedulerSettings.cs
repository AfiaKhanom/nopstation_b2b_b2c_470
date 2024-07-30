using Nop.Core.Configuration;

namespace NopStation.Plugin.B2B.ErpDataScheduler
{
    public class ErpDataSchedulerSettings : ISettings
    {
        public DateTime? SyncFromDate { get; set; }
        public bool NeedQuoteOrderCall { get; set; }
        public bool StartProductSyncAfterLastSyncedProduct { get; set; }
    }
}
