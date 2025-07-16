using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.B2B.ErpDataScheduler.Domain;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.NopStationSyncServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.B2B.ErpDataScheduler;

public class ErpDataSchedulerPlugin : BasePlugin, IAdminMenuPlugin, IMiscPlugin, INopStationPlugin
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly ISyncTaskService _syncTaskService;
    private readonly IScheduleTaskService _scheduleTaskService;
    private readonly IWebHelper _webHelper;
    private readonly INopStationScheduler _nopStationScheduler;
    private readonly IMessageTemplateService _messageTemplateService;
    private readonly IEmailAccountService _emailAccountService;
    private const string CONFIG_PAGE_URL_EXTENSION = "Admin/ErpDataScheduler/Configure";
    private const string THIRD_PARTY_PLUGINS = "Third party plugins";
    private const string PLUGIN_SYSTEM_NAME = "NopStation.ErpDataScheduler";
    private const string PLUGIN_TITLE = "Erp Data Scheduler";
    private const string PLUGIN_ICON_CLASS = "nav-icon fas fa-cube";
    private const bool PLUGIN_VISIBLE = true;
    private const string PLUGIN_NEXT_VERSION = "4.70.3.20";
    private const string CHILD_NODE_CONFIG_SYSTEM_NAME = "NopStation.ErpDataScheduler.Configuration";
    private const string CHILD_NODE_CONFIG_TITLE = "Configuration";
    private const string CHILD_NODE_CONFIG_CONTROLLER_NAME = "ErpDataScheduler";
    private const string CHILD_NODE_CONFIG_ACTION_NAME = "Configure";
    private const string CHILD_NODE_CONFIG_ICON_CLASS = "nav-icon fas fa-cogs";
    private const bool CHILD_NODE_CONFIG_VISIBLE = true;

    private const string CHILD_NODE_PARTIALSYNC_SYSTEM_NAME = "NopStation.ErpDataScheduler.PartialSync";
    private const string CHILD_NODE_PARTIALSYNC_TITLE = "Partial Sync";
    private const string CHILD_NODE_PARTIALSYNC_CONTROLLER_NAME = "PartialSync";
    private const string CHILD_NODE_PARTIALSYNC_ACTION_NAME = "Index";
    private const string CHILD_NODE_PARTIALSYNC_ICON_CLASS = "nav-icon fas fa-sync-alt";
    private const bool CHILD_NODE_PARTIALSYNC_VISIBLE = true;

    private const string CHILD_NODE_SYNCTASK_SYSTEM_NAME = "NopStation.ErpDataScheduler.SyncTaskList";
    private const string CHILD_NODE_SYNCTASK_TITLE = "Sync Task List";
    private const string CHILD_NODE_SYNCTASK_CONTROLLER_NAME = "SyncTasks";
    private const string CHILD_NODE_SYNCTASK_ACTION_NAME = "List";
    private const string CHILD_NODE_SYNCTASK_ICON_CLASS = "nav-icon fas fa-list";
    private const bool CHILD_NODE_SYNCTASK_VISIBLE = true;

    #endregion

    #region Ctor

    public ErpDataSchedulerPlugin(ILocalizationService localizationService,
        ISyncTaskService taskService,
        IWebHelper webHelper,
        IScheduleTaskService scheduleTaskService,
        INopStationScheduler nopStationScheduler,
        IMessageTemplateService messageTemplateService,
        IEmailAccountService emailAccountService)
    {
        _localizationService = localizationService;
        _webHelper = webHelper;
        _syncTaskService = taskService;
        _scheduleTaskService = scheduleTaskService;
        _nopStationScheduler = nopStationScheduler;
        _messageTemplateService = messageTemplateService;
        _emailAccountService = emailAccountService;
    }

    #endregion

    #region Methods

    public override async Task InstallAsync()
    {
        await this.InstallPluginAsync();

        #region Sync tasks installation

        //install Erp Account Sync task
        if ((await _syncTaskService.GetTaskByTypeAsync(ErpDataSchedulerDefaults.ErpAccountSyncTask)) is null)
        {
            await _syncTaskService.InsertTaskAsync(new SyncTask
            {
                Enabled = false,
                LastEnabledUtc = DateTime.UtcNow,
                Name = ErpDataSchedulerDefaults.ErpAccountSyncTaskName,
                Type = ErpDataSchedulerDefaults.ErpAccountSyncTask
            });
        }

        //install Erp Invoice Sync task
        if ((await _syncTaskService.GetTaskByTypeAsync(ErpDataSchedulerDefaults.ErpInvoiceSyncTask)) is null)
        {
            await _syncTaskService.InsertTaskAsync(new SyncTask
            {
                Enabled = false,
                LastEnabledUtc = DateTime.UtcNow,
                Name = ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName,
                Type = ErpDataSchedulerDefaults.ErpInvoiceSyncTask
            });
        }

        //install Erp Product Sync task
        if ((await _syncTaskService.GetTaskByTypeAsync(ErpDataSchedulerDefaults.ErpProductSyncTask)) is null)
        {
            await _syncTaskService.InsertTaskAsync(new SyncTask
            {
                Enabled = false,
                LastEnabledUtc = DateTime.UtcNow,
                Name = ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                Type = ErpDataSchedulerDefaults.ErpProductSyncTask
            });
        }

        //install Erp Stock Sync task
        if ((await _syncTaskService.GetTaskByTypeAsync(ErpDataSchedulerDefaults.ErpStockSyncTask)) is null)
        {
            await _syncTaskService.InsertTaskAsync(new SyncTask
            {
                Enabled = false,
                LastEnabledUtc = DateTime.UtcNow,
                Name = ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                Type = ErpDataSchedulerDefaults.ErpStockSyncTask
            });
        }

        //install Erp Special Price Sync task
        if ((await _syncTaskService.GetTaskByTypeAsync(ErpDataSchedulerDefaults.ErpSpecialPriceSyncTask)) is null)
        {
            await _syncTaskService.InsertTaskAsync(new SyncTask
            {
                Enabled = false,
                LastEnabledUtc = DateTime.UtcNow,
                Name = ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName,
                Type = ErpDataSchedulerDefaults.ErpSpecialPriceSyncTask
            });
        }

        //install Erp Group Price Sync task
        if ((await _syncTaskService.GetTaskByTypeAsync(ErpDataSchedulerDefaults.ErpGroupPriceSyncTask)) is null)
        {
            await _syncTaskService.InsertTaskAsync(new SyncTask
            {
                Enabled = false,
                LastEnabledUtc = DateTime.UtcNow,
                Name = ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName,
                Type = ErpDataSchedulerDefaults.ErpGroupPriceSyncTask
            });
        }

        //install Erp Ship To Address Sync task
        if ((await _syncTaskService.GetTaskByTypeAsync(ErpDataSchedulerDefaults.ErpShipToAddressSyncTask)) is null)
        {
            await _syncTaskService.InsertTaskAsync(new SyncTask
            {
                Enabled = false,
                LastEnabledUtc = DateTime.UtcNow,
                Name = ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName,
                Type = ErpDataSchedulerDefaults.ErpShipToAddressSyncTask
            });
        }

        //install Erp Order Sync task
        if ((await _syncTaskService.GetTaskByTypeAsync(ErpDataSchedulerDefaults.ErpOrderSyncTask)) is null)
        {
            await _syncTaskService.InsertTaskAsync(new SyncTask
            {
                Enabled = false,
                LastEnabledUtc = DateTime.UtcNow,
                Name = ErpDataSchedulerDefaults.ErpOrderSyncTaskName,
                Type = ErpDataSchedulerDefaults.ErpOrderSyncTask
            });
        }

        //install Sync Log delete task
        if (await _scheduleTaskService.GetTaskByTypeAsync(ErpDataSchedulerDefaults.SyncLogFileDeleteTask) == null)
        {
            await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
            {
                Enabled = true,
                LastEnabledUtc = DateTime.UtcNow,
                Seconds = ErpDataSchedulerDefaults.DefaultSyncLogFileDeleteTaskInverval,
                Name = ErpDataSchedulerDefaults.SyncLogFileDeleteTaskName,
                Type = ErpDataSchedulerDefaults.SyncLogFileDeleteTask,
            });
        }

        #endregion

        await RegisterScheduleJobsAsync();

        await base.InstallAsync();
    }

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}{CONFIG_PAGE_URL_EXTENSION}";
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == THIRD_PARTY_PLUGINS);

        if (pluginNode is null)
        {
            return;
        }

        pluginNode.ChildNodes.Add(new()
        {
            SystemName = PLUGIN_SYSTEM_NAME,
            Title = PLUGIN_TITLE,
            IconClass = PLUGIN_ICON_CLASS,
            Visible = PLUGIN_VISIBLE,
            ChildNodes = new List<SiteMapNode>() {
                new ()
                {
                    SystemName = CHILD_NODE_CONFIG_SYSTEM_NAME,
                    Title = CHILD_NODE_CONFIG_TITLE,
                    ControllerName = CHILD_NODE_CONFIG_CONTROLLER_NAME,
                    ActionName = CHILD_NODE_CONFIG_ACTION_NAME,
                    IconClass = CHILD_NODE_CONFIG_ICON_CLASS,
                    Visible = CHILD_NODE_CONFIG_VISIBLE
                },
                new ()
                {
                    SystemName = CHILD_NODE_PARTIALSYNC_SYSTEM_NAME,
                    Title = CHILD_NODE_PARTIALSYNC_TITLE,
                    ControllerName = CHILD_NODE_PARTIALSYNC_CONTROLLER_NAME,
                    ActionName = CHILD_NODE_PARTIALSYNC_ACTION_NAME,
                    IconClass = CHILD_NODE_PARTIALSYNC_ICON_CLASS,
                    Visible = CHILD_NODE_PARTIALSYNC_VISIBLE
                },
                new ()
                {
                    SystemName = CHILD_NODE_SYNCTASK_SYSTEM_NAME,
                    Title = CHILD_NODE_SYNCTASK_TITLE,
                    ControllerName = CHILD_NODE_SYNCTASK_CONTROLLER_NAME,
                    ActionName = CHILD_NODE_SYNCTASK_ACTION_NAME,
                    IconClass = CHILD_NODE_SYNCTASK_ICON_CLASS,
                    Visible = CHILD_NODE_SYNCTASK_VISIBLE
                }
            }
        });
    }

    public override async Task UninstallAsync()
    {
        var erpDataSchedulerTasks = await _syncTaskService.GetAllTasksAsync();

        if (erpDataSchedulerTasks.Count > 0)
        {
            foreach (var erpDataSchedulerTask in erpDataSchedulerTasks)
            {
                await _syncTaskService.DeleteTaskAsync(erpDataSchedulerTask);
            }
        }

        await this.UninstallPluginAsync();

        await base.UninstallAsync();
    }

    public override async Task UpdateAsync(string currentVersion, string targetVersion)
    {
        if (targetVersion == currentVersion || targetVersion != PLUGIN_NEXT_VERSION)
        {
            return;
        }

        await RegisterScheduleJobsAsync();

        var keyValuePairs = PluginResouces().ToDictionary(kv => kv.Key, kv => kv.Value);
        foreach (var keyValuePair in keyValuePairs)
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync(keyValuePair.Key, keyValuePair.Value);
        }

        await InsertSyncFailedNotificationMessageTemplate();

        await base.UpdateAsync(currentVersion, targetVersion);
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        return new List<KeyValuePair<string, string>>
        {
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure", "Erp Data Scheduler Configuration"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure.Block.General", "General"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure.Fields.NeedQuoteOrderCall", "Need Quote Order Call"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Admin.SyncTaskList", "Erp Data Scheduler Sync Tasks"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure.Fields.EnalbeSendingEmailNotificationToStoreOwnerOnSyncError", "Enable sending email notification to store owner on sync error"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure.Fields.EnalbeSendingEmailNotificationToStoreOwnerOnSyncError.Hint", "Enable sending email notification to store owner on sync error"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure.Fields.AdditionalEmailAddresses", "Additional Email Addresses"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure.Fields.AdditionalEmailAddresses.Hint", "Configure the email addresses (semi-colon separated) to which the sync related notification will be sent"),
            
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Name", "Name"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Seconds", "Seconds"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Enabled", "Enabled"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.StopOnError", "Stop On Error"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.IsRunning", "The schedule task is running"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.IsIncremental", "Is Incremental"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.LastStart", "Last Start"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.LastEnd", "Last End"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.LastSuccess", "Last Success"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.SlotsAndLogs", "Slots and Logs"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.RunNow", "Run Now"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.RunNow.Progress", "Running the schedule task"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.RunNow.Done", "Schedule task was run"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Edit.Details", "Edit the Scheduler Settings"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.BackToList", "Back to list"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Scheduler.Settings", "Scheduler Settings"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Error", "The \"{0}\" scheduled task failed with the \"{1}\" error. Task type: \"{2}\". Store name: \"{3}\". Task run address: \"{4}\"."),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.TimeoutError", "A scheduled task canceled. Timeout expired."),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Execute.Succeded", "\"{0}\" has started. Sync is running now"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Terminate", "The scheduled task cancellation is in progress, this might take few time."),

            new ("Plugin.Misc.NopStation.ErpDataScheduler.ErpActivityLogs.EditConfigurations", "Edit Erp Data Scheduler plugin configurations"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.ErpActivityLogs.EditSyncTask", "Edited a Sync task. (ID = \"{0}\", Name = \"{1}\")"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Configuration.Updated", "The settings have been updated successfully."),

            new ("Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.FileName", "File Name"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.FileSize", "File Size"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.FileDownloadLink", "Download"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.FileDelete", "Delete"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.SyncTaskLogs", "Sync Task Logs"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.FileNotFound", "File not found"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.FileCouldNotBeDeleted", "File could not be deleted"),

            new ("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.ErpAccountNumber", "Erp Account Number"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.ErpAccountNumber.Hint", "Erp Account Number"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.RouteCode", "Route Code"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.RouteCode.Hint", "Route Code"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.PriceCode", "Price Code"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.PriceCode.Hint", "Price Code"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.StockCode", "Stock Code"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.StockCode.Hint", "Stock Code"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.OrderNumber", "Order Number"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.OrderNumber.Hint", "Order Number"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.SyncNow", "Sync Now"),

            new ("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.Error.StockCodeEmptyInputError", "Stock Code cannot be empty"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.Error.RouteCodeEmptyInputError", "Route Code cannot be empty"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.Error.ErpAccountNumberEmptyInputError", "Erp Account Number cannot be empty"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.Error.PriceCodeAndStockCodeEmptyInputError", "Both Price Code and Stock Code cannot be empty"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.Error.ErpAccountNumberAndStockCodeEmptyInputError", "Both Erp Account Number and Stock Code cannot be empty"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.Error.ErpAccountNumberAndOrderNumberEmptyInputError", "Both Erp Account Number and Order Number cannot be empty"),

            new ("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync", "Partial Sync"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.ErpAccount", "Sync Erp Account"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.ErpAccountCredit", "Sync Erp Account Credit"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.ErpGroupPrice", "Sync Erp Group Price"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.ErpInvoice", "Sync Erp Invoice"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.ErpProduct", "Sync Erp Product"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.ErpShipToAddress", "Sync Erp Ship To Address"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.ErpSpecialPrice", "Sync Erp Special Price"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.ErpStock", "Sync Erp Stock"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.DeliveryRoute", "Sync Erp Delivery Route"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.ErpOrder", "Sync Erp Order"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.TaskScheduledToExecute", "The task has been scheduled to execute. This might take a few minutes."),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.RequestedTaskNotFound", "The requested task was not found"),
            new ("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.InternalServerError", "Internal server error")
        };
    }

    #region Message Templates

    private async Task InsertSyncFailedNotificationMessageTemplate()
    {
        var messageTemplate = await _messageTemplateService.GetMessageTemplatesByNameAsync(
            ErpDataSchedulerDefaults.SyncFailedNotificationMessageTemplate);

        if (messageTemplate.Count == 0)
        {
            var emailAccount = await _emailAccountService.GetAllEmailAccountsAsync();

            var notificationMessageTemplate = new MessageTemplate
            {
                Name = ErpDataSchedulerDefaults.SyncFailedNotificationMessageTemplate,
                Subject = "Critical Alert - The Scheduler Task %SyncNotification.SyncTaskName%, has stopped running due to an error",
                Body = $"<p>{Environment.NewLine}" +
                $"Dear Concern,{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}" +
                $"This is an automated notification to alert you that the scheduler task <strong>%SyncNotification.SyncTaskName%</strong> has unexpectedly stopped running " +
                $"on <strong>%SyncNotification.Datetime% UTC time</strong> due to the following error:{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}" +
                $"%SyncNotification.Message%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}" +
                $"If you need assistance, please contact the IT support team.{Environment.NewLine}</p>{Environment.NewLine}",
                IsActive = true,
                EmailAccountId = emailAccount.Count > 0 ? emailAccount.FirstOrDefault()?.Id ?? 0 : 0
            };

            await _messageTemplateService.InsertMessageTemplateAsync(notificationMessageTemplate);
        }
    }

    #endregion

    #region Register Jobs

    private async Task RegisterScheduleJobsAsync()
    {
        #region Erp Account Sync Task

        var erpAccountSyncTask = await _syncTaskService.GetTaskByTypeAsync(ErpDataSchedulerDefaults.ErpAccountSyncTask);

        if (erpAccountSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpAccountSyncTask>(ErpDataSchedulerDefaults.ErpAccountSyncTaskIdentity, true);

            erpAccountSyncTask.Enabled = true;
            erpAccountSyncTask.QuartzJobName = ErpDataSchedulerDefaults.ErpAccountSyncTaskIdentity;

            await _syncTaskService.UpdateTaskAsync(erpAccountSyncTask);
        }

        #endregion

        #region Erp Group Price Sync Task

        var erpGroupPriceSyncTask = await _syncTaskService.GetTaskByTypeAsync(ErpDataSchedulerDefaults.ErpGroupPriceSyncTask);

        if (erpGroupPriceSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpGroupPriceSyncTask>(ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskIdentity, true);

            erpGroupPriceSyncTask.Enabled = true;
            erpGroupPriceSyncTask.QuartzJobName = ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskIdentity;

            await _syncTaskService.UpdateTaskAsync(erpGroupPriceSyncTask);
        }

        #endregion

        #region Erp Invoice Sync Task

        var erpInvoiceSyncTask = await _syncTaskService.GetTaskByTypeAsync(ErpDataSchedulerDefaults.ErpInvoiceSyncTask);

        if (erpInvoiceSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpInvoiceSyncTask>(ErpDataSchedulerDefaults.ErpInvoiceSyncTaskIdentity, true);

            erpInvoiceSyncTask.Enabled = true;
            erpInvoiceSyncTask.QuartzJobName = ErpDataSchedulerDefaults.ErpInvoiceSyncTaskIdentity;

            await _syncTaskService.UpdateTaskAsync(erpInvoiceSyncTask);
        }

        #endregion

        #region Erp Order Sync Task

        var erpOrderSyncTask = await _syncTaskService.GetTaskByTypeAsync(ErpDataSchedulerDefaults.ErpOrderSyncTask);

        if (erpOrderSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpOrderSyncTask>(ErpDataSchedulerDefaults.ErpOrderSyncTaskIdentity, true);

            erpOrderSyncTask.Enabled = true;
            erpOrderSyncTask.QuartzJobName = ErpDataSchedulerDefaults.ErpOrderSyncTaskIdentity;

            await _syncTaskService.UpdateTaskAsync(erpOrderSyncTask);
        }

        #endregion

        #region Erp Product Sync Task

        var erpProductSyncTask = await _syncTaskService.GetTaskByTypeAsync(ErpDataSchedulerDefaults.ErpProductSyncTask);

        if (erpProductSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpProductSyncTask>(ErpDataSchedulerDefaults.ErpProductSyncTaskIdentity, true);

            erpProductSyncTask.Enabled = true;
            erpProductSyncTask.QuartzJobName = ErpDataSchedulerDefaults.ErpProductSyncTaskIdentity;

            await _syncTaskService.UpdateTaskAsync(erpProductSyncTask);
        }

        #endregion

        #region Erp Ship To Address Sync Task

        var erpShipToAddressSyncTask = await _syncTaskService.GetTaskByTypeAsync(ErpDataSchedulerDefaults.ErpShipToAddressSyncTask);

        if (erpShipToAddressSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpShipToAddressSyncTask>(ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskIdentity, true);

            erpShipToAddressSyncTask.Enabled = true;
            erpShipToAddressSyncTask.QuartzJobName = ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskIdentity;

            await _syncTaskService.UpdateTaskAsync(erpShipToAddressSyncTask);
        }

        #endregion

        #region Erp Special Price Sync Task

        var erpSpecialPriceSyncTask = await _syncTaskService.GetTaskByTypeAsync(ErpDataSchedulerDefaults.ErpSpecialPriceSyncTask);

        if (erpSpecialPriceSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpSpecialPriceSyncTask>(ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskIdentity, true);

            erpSpecialPriceSyncTask.Enabled = true;
            erpSpecialPriceSyncTask.QuartzJobName = ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskIdentity;

            await _syncTaskService.UpdateTaskAsync(erpSpecialPriceSyncTask);
        }

        #endregion

        #region Erp Stock Sync Task

        var erpStockSyncTask = await _syncTaskService.GetTaskByTypeAsync(ErpDataSchedulerDefaults.ErpStockSyncTask);

        if (erpStockSyncTask is not null)
        {
            await _nopStationScheduler.CreateScheduleJobAsync<ErpStockSyncTask>(ErpDataSchedulerDefaults.ErpStockSyncTaskIdentity, true);

            erpStockSyncTask.Enabled = true;
            erpStockSyncTask.QuartzJobName = ErpDataSchedulerDefaults.ErpStockSyncTaskIdentity;

            await _syncTaskService.UpdateTaskAsync(erpStockSyncTask);
        }

        #endregion
    }

    #endregion

    #endregion
}