using Nop.Data.Mapping;
using NopStation.Plugin.B2B.ErpDataScheduler.Domain;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new()
        {
            { typeof(SyncTask), "Erp_Data_Sync_Task" }
        };

        public Dictionary<(Type, string), string> ColumnName => new()
        {
            { (typeof(SyncTask),"Name"), "Name" },
            { (typeof(SyncTask),"Type"), "Type" },
            { (typeof(SyncTask),"Seconds"), "Seconds" },
            { (typeof(SyncTask),"LastEnabledUtc"), "LastEnabledUtc" },
            { (typeof(SyncTask),"Enabled"), "Enabled" },
            { (typeof(SyncTask),"StopOnError"), "StopOnError" },
            { (typeof(SyncTask),"LastStartUtc"), "LastStartUtc" },
            { (typeof(SyncTask),"LastEndUtc"), "LastEndUtc" },
            { (typeof(SyncTask),"LastSuccessUtc"), "LastSuccessUtc" },
            { (typeof(SyncTask),"DayTimeSlots"), "DayTimeSlots" }
        };
    }
}