namespace NopStation.Plugin.B2B.ErpDataScheduler
{
    /// <summary>
    /// Represents plugin constants
    /// </summary>
    public static class ErpDataSchedulerDefaults
    {
        /// <summary>
        /// Gets a plugin system name
        /// </summary>
        public static string SystemName => "Misc.NopStation.ErpDataScheduler";

        /// <summary>
        /// Gets a type of the erp account synchronization schedule task
        /// </summary>
        public static string ErpAccountSyncTask => "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpAccountSyncTask";

        /// <summary>
        /// Gets a name of the erp account synchronization schedule task name
        /// </summary>
        public static string ErpAccountSyncTaskName => "Erp Account Synchronization";

        /// <summary>
        /// Gets a type of the erp invoice synchronization schedule task
        /// </summary>
        public static string ErpInvoiceSyncTask => "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpInvoiceSyncTask";

        /// <summary>
        /// Gets a name of the erp invoice synchronization schedule task name
        /// </summary>
        public static string ErpInvoiceSyncTaskName => "Erp Invoice Synchronization";

        /// <summary>
        /// Gets a type of the erp product synchronization schedule task
        /// </summary>
        public static string ErpProductSyncTask => "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpProductSyncTask";

        /// <summary>
        /// Gets a name of the erp product synchronization schedule task name
        /// </summary>
        public static string ErpProductSyncTaskName => "Erp Product Synchronization";

        /// <summary>
        /// Gets a type of the erp stock synchronization schedule task
        /// </summary>
        public static string ErpStockSyncTask => "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpStockSyncTask";

        /// <summary>
        /// Gets a name of the erp stock synchronization schedule task name
        /// </summary>
        public static string ErpStockSyncTaskName => "Erp Stock Synchronization";

        /// <summary>
        /// Gets a type of the erp order synchronization schedule task
        /// </summary>
        public static string ErpOrderSyncTask => "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpOrderSyncTask";

        /// <summary>
        /// Gets a name of the erp order synchronization schedule task name
        /// </summary>
        public static string ErpOrderSyncTaskName => "Erp Order Synchronization";

        /// <summary>
        /// Gets a type of the erp special price synchronization schedule task
        /// </summary>
        public static string ErpSpecialPriceSyncTask => "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpSpecialPriceSyncTask";

        /// <summary>
        /// Gets a name of the erp special price synchronization schedule task name
        /// </summary>
        public static string ErpSpecialPriceSyncTaskName => "Erp Special Price Synchronization";

        /// <summary>
        /// Gets a type of the erp group price synchronization schedule task
        /// </summary>
        public static string ErpGroupPriceSyncTask => "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpGroupPriceSyncTask";

        /// <summary>
        /// Gets a name of the erp group price synchronization schedule task name
        /// </summary>
        public static string ErpGroupPriceSyncTaskName => "Erp Group Price Synchronization";

        /// <summary>
        /// Gets a type of the erp ship to address synchronization schedule task
        /// </summary>
        public static string ErpShipToAddressSyncTask => "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpShipToAddressSyncTask";

        /// <summary>
        /// Gets a name of the erp ship to address synchronization schedule task name
        /// </summary>
        public static string ErpShipToAddressSyncTaskName => "Erp Ship To Address Synchronization";

        /// <summary>
        /// Gets a default sync task time out period in seconds
        /// </summary>
        public static int DefaultSyncTaskTimeOutPeriod => 1800;

        /// <summary>
        /// Gets a default sync task time interval in minutes
        /// </summary>
        public static int DefaultSyncTaskTimeInterval => 30;

        /// <summary>
        /// Gets a default Erp Order Type
        /// </summary>
        public static string ErpOrderType => "ORDER";

        /// <summary>
        /// Gets a default Erp Quote Type
        /// </summary>
        public static string ErpQuoteType => "QUOTE";

        /// <summary>
        /// Gets a default Response empty or null status
        /// </summary>
        public static string ResponseEmptyOrNull => "Server Response empty or null.";

        public static string SyncLogFileSaveDefaultPath => "SyncLogs\\";

        public static string SyncLogFileExtension => "txt";

        /// <summary>
        /// Gets a type of the sync log file delete schedule task
        /// </summary>
        public static string SyncLogFileDeleteTask => "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices.SyncLogFileDeleteTask";

        /// <summary>
        /// Gets a name of the sync log file delete schedule task name
        /// </summary>
        public static string SyncLogFileDeleteTaskName => "Sync Log Files Delete";

        /// <summary>
        /// Gets a default sync log files checker for deleting, interval in seconds
        /// </summary>
        public static int DefaultSyncLogFileDeleteTaskInverval => 3600;

        /// <summary>
        /// Gets the last sync product sku from erp
        /// </summary>
        public static string LastSyncedProductSkuFromErp => "LastSyncedProductSkuFromErp";

        public static string LastSyncStartTimeBeforeDisruption => "LastSyncStartTimeBeforeDisruption";
    }
}
