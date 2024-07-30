using Nop.Core;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.B2B.ErpDataScheduler.Domain;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskScheduler;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.B2B.ErpDataScheduler
{
    public class ErpDataSchedulerPlugin : BasePlugin, IAdminMenuPlugin, IMiscPlugin, INopStationPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ISyncTaskService _syncTaskService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IWebHelper _webHelper;
        private const string CONFIG_PAGE_URL_EXTENSION = "Admin/ErpDataScheduler/Configure";
        private const string THIRD_PARTY_PLUGINS = "Third party plugins";
        private const string PLUGIN_SYSTEM_NAME = "NopStation.ErpDataScheduler";
        private const string PLUGIN_TITLE = "Erp Data Scheduler";
        private const string PLUGIN_ICON_CLASS = "nav-icon fas fa-cube";
        private const bool PLUGIN_VISIBLE = true;
        private const string PLUGIN_VERSION = "1.70";
        private const string CHILD_NODE_CONFIG_SYSTEM_NAME = "NopStation.ErpDataScheduler.Configuration";
        private const string CHILD_NODE_CONFIG_TITLE = "Configuration";
        private const string CHILD_NODE_CONFIG_CONTROLLER_NAME = "ErpDataScheduler";
        private const string CHILD_NODE_CONFIG_ACTION_NAME = "Configure";
        private const string CHILD_NODE_CONFIG_ICON_CLASS = "nav-icon fas fa-cogs";
        private const bool CHILD_NODE_CONFIG_VISIBLE = true;
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
            IScheduleTaskService scheduleTaskService)
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
            _syncTaskService = taskService;
            _scheduleTaskService = scheduleTaskService;
        }

        #endregion

        #region Method

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync();

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

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
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
            /*if (targetVersion == currentVersion || targetVersion != PLUGIN_VERSION)
            {
                return;
            }*/

            var keyValuePairs = PluginResouces().ToDictionary(kv => kv.Key, kv => kv.Value);
            foreach (var keyValuePair in keyValuePairs)
            {
                await _localizationService.AddOrUpdateLocaleResourceAsync(keyValuePair.Key, keyValuePair.Value);
            }

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

            await base.UpdateAsync(currentVersion, targetVersion);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {

            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure", "Erp Data Scheduler Configuration"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure.Block.SyncFromDate", "Date Selection to Sync from"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure.Fields.SyncFromDate", "Sync from"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure.Fields.NeedQuoteOrderCall", "Need Quote Order Call"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure.Fields.StartProductSyncAfterLastSyncedProduct", "Start Product Sync after Last Synced Product"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.Admin.SyncTaskList", "Erp Data Scheduler Sync Tasks"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Name", "Name"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Seconds", "Seconds"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Enabled", "Enabled"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.StopOnError", "Stop On Error"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.IsRunning", "The schedule task is already running"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.LastStart", "Last Start"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.LastEnd", "Last End"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.LastSuccess", "Last Success"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.SlotsAndLogs", "Slots and Logs"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.RunNow", "Run Now"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.RunNow.Progress", "Running the schedule task"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.RunNow.Done", "Schedule task was run"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.24days", "Erp Data Scheduler Task period should not exceed 24 days."),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Edit.Details", "Edit the Scheduler Settings"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.BackToList", "Back to list"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Scheduler.Settings", "Scheduler Settings"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Error", "The \"{0}\" scheduled task failed with the \"{1}\" error. Task type: \"{2}\". Store name: \"{3}\". Task run address: \"{4}\"."),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.TimeoutError", "A scheduled task canceled. Timeout expired."),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.ErpActivityLogs.EditConfigurations", "Edit Erp Data Scheduler plugin configurations"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.ErpActivityLogs.EditSyncTask", "Edited a Sync task. (ID = \"{0}\", Name = \"{1}\")"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.Configuration.Updated", "The settings have been updated successfully."),

                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.FileName", "File Name"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.FileSize", "File Size"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.FileDownloadLink", "Download"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.FileDelete", "Delete"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.SyncTaskLogs", "Sync Task Logs"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.FileNotFound", "File not found"),
                new KeyValuePair<string, string>("Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.FileCouldNotBeDeleted", "File could not be deleted")
            };

            return list;
        }

        #endregion
    }
}