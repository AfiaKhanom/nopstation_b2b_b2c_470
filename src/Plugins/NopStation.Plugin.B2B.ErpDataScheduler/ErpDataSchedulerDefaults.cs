namespace NopStation.Plugin.B2B.ErpDataScheduler;

public static class ErpDataSchedulerDefaults
{
    public static string ErpAccountSyncTask => "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpAccountSyncTask";

    public static string ErpAccountSyncTaskName => "Erp Account Synchronization";

    public static string ErpInvoiceSyncTask => "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpInvoiceSyncTask";

    public static string ErpInvoiceSyncTaskName => "Erp Invoice Synchronization";

    public static string ErpProductSyncTask => "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpProductSyncTask";

    public static string ErpProductSyncTaskName => "Erp Product Synchronization";

    public static string ErpStockSyncTask => "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpStockSyncTask";

    public static string ErpStockSyncTaskName => "Erp Stock Synchronization";

    public static string ErpOrderSyncTask => "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpOrderSyncTask";

    public static string ErpOrderSyncTaskName => "Erp Order Synchronization";

    public static string ErpSpecialPriceSyncTask => "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpSpecialPriceSyncTask";

    public static string ErpSpecialPriceSyncTaskName => "Erp Special Price Synchronization";

    public static string ErpGroupPriceSyncTask => "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpGroupPriceSyncTask";

    public static string ErpGroupPriceSyncTaskName => "Erp Group Price Synchronization";

    public static string ErpShipToAddressSyncTask => "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpShipToAddressSyncTask";

    public static string ErpShipToAddressSyncTaskName => "Erp Ship To Address Synchronization";

    public static string SyncLogFileSaveDefaultPath => "SyncLogs\\";

    public static string SyncLogFileExtension => "txt";

    public static string SyncLogFileDeleteTask => "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices.SyncLogFileDeleteTask";

    public static string SyncLogFileDeleteTaskName => "Sync Log Files Delete";

    public static int DefaultSyncLogFileDeleteTaskInverval => 3600;

    public static string ErpAccountSyncTaskIdentity => "ErpAccountSync";

    public static string ErpGroupPriceSyncTaskIdentity => "ErpGroupPriceSync";

    public static string ErpInvoiceSyncTaskIdentity => "ErpInvoiceSync";

    public static string ErpOrderSyncTaskIdentity => "ErpOrderSync";

    public static string ErpProductSyncTaskIdentity => "ErpProductSync";

    public static string ErpShipToAddressSyncTaskIdentity => "ErpShipToAddressSync";

    public static string ErpSpecialPriceSyncTaskIdentity => "ErpSpecialPriceSync";

    public static string ErpStockSyncTaskIdentity => "ErpStockSync";

    public static string JobShouldExecute => "JobShouldExecute";

    public static string IsManualTrigger => "ManualTrigger";

    public static string IsIncrementalSync => "IncrementalSync";

    public static string SyncFailedNotificationMessageTemplate => "SyncFailedNotificationMessageTemplate";
}
