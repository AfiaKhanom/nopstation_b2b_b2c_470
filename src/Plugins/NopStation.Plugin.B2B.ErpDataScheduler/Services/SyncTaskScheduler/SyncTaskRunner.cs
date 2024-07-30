using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Logging;
using NopStation.Plugin.B2B.ErpDataScheduler.Domain;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskScheduler
{
    /// <summary>
    /// Schedule task runner
    /// </summary>
    public partial class SyncTaskRunner : ISyncTaskRunner
    {
        #region Fields

        protected readonly ILocalizationService _localizationService;
        protected readonly ILocker _locker;
        protected readonly ILogger _logger;
        protected readonly IErpLogsService _erpLogsService;
        protected readonly ISyncTaskService _syncTaskService;
        protected readonly IStoreContext _storeContext;
        protected readonly ISyncLogService _erpSyncLogService;

        #endregion

        #region Ctor

        public SyncTaskRunner(ILocalizationService localizationService,
            ILocker locker,
            ILogger logger,
            IErpLogsService erpLogsService,
            ISyncTaskService syncTaskService,
            IStoreContext storeContext,
            ISyncLogService erpSyncLogService)
        {
            _localizationService = localizationService;
            _locker = locker;
            _logger = logger;
            _erpLogsService = erpLogsService;
            _syncTaskService = syncTaskService;
            _storeContext = storeContext;
            _erpSyncLogService = erpSyncLogService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Initialize and execute task
        /// </summary>
        protected async Task PerformTaskAsync(SyncTask syncTask)
        {
            var type = Type.GetType(syncTask.Type) ?? AppDomain.CurrentDomain.GetAssemblies()
                                                            .Select(assembly => assembly.GetType(syncTask.Type))
                                                            .FirstOrDefault(t => t is not null);
            //ensure that it works fine when only the type name is specified (do not require fully qualified names)

            if (type is null)
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    syncTask.Name,
                    0,
                    $"Schedule task ({syncTask.Type}) cannot be instantiated");

                throw new SyncTaskCustomException($"Schedule task ({syncTask.Type}) cannot be instantiated.");
            }

            object instance = null;

            try
            {
                instance = EngineContext.Current.Resolve(type);
            }
            catch
            {
                // ignored
            }

            instance ??= EngineContext.Current.ResolveUnregistered(type);

            if (instance is not ISyncTask task)
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    syncTask.Name,
                    0,
                    $"Schedule task ({syncTask.Type}) cannot be resolved as an instance of ISyncTask.");

                return;
            }

            syncTask.LastStartUtc = DateTime.UtcNow;
            //update appropriate datetime properties
            await _syncTaskService.UpdateTaskAsync(syncTask);
            await task.ExecuteAsync();
            syncTask.LastEndUtc = syncTask.LastSuccessUtc = DateTime.UtcNow.AddMilliseconds(500);
            //update appropriate datetime properties
            await _syncTaskService.UpdateTaskAsync(syncTask);
        }

        /// <summary>
        /// Is task already running?
        /// </summary>
        /// <param name="syncTask">Schedule task</param>
        /// <returns>Result</returns>
        protected virtual bool IsTaskAlreadyRunning(SyncTask syncTask)
        {
            //task run for the first time
            if (!syncTask.LastStartUtc.HasValue && !syncTask.LastEndUtc.HasValue)
                return false;

            var lastStartUtc = syncTask.LastStartUtc ?? DateTime.UtcNow;

            //task already finished
            if (syncTask.LastEndUtc.HasValue && lastStartUtc < syncTask.LastEndUtc)
                return false;

            return true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes the task
        /// </summary>
        /// <param name="task">Schedule task</param>
        /// <param name="forceRun">Force run</param>
        /// <param name="throwException">A value indicating whether exception should be thrown if some error happens</param>
        /// <param name="ensureRunOncePerPeriod">A value indicating whether we should ensure this task is run once per run period</param>
        public async Task ExecuteAsync(SyncTask task, bool forceRun = false, bool throwException = false, bool ensureRunOncePerPeriod = true)
        {
            if (task is null || !(forceRun || task is not null && task.Enabled))
            {
                await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Error, 0, $"Schedule task ({task?.Type}) isn't enabled or cannot be found.");

                if (task is not null)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    task.Name,
                    0,
                    $"Schedule task ({task.Type}) isn't enabled or cannot be found.");
                }

                return;
            }
            else if (ensureRunOncePerPeriod && IsTaskAlreadyRunning(task))
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    task.Name,
                    0,
                    $"Schedule task ({task.Type}) is already running from {task.LastStartUtc}.");

                return;
            }

            try
            {
                //get expiration time
                var expirationInSeconds = Math.Min((double)task.Seconds, 300) - 1;
                var expiration = TimeSpan.FromSeconds(expirationInSeconds);

                //execute task with lock
                await _locker.PerformActionWithLockAsync(task.Type, expiration, () => PerformTaskAsync(task));
            }
            catch (Exception exc)
            {
                try
                {
                    var store = await _storeContext.GetCurrentStoreAsync();

                    var syncTaskUrl = $"{store.Url}{SyncTaskDefaults.SyncTaskPath}";

                    task.Enabled = !task.StopOnError;
                    task.LastEndUtc = DateTime.UtcNow;
                    await _syncTaskService.UpdateTaskAsync(task);

                    var message = string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Error"),
                        task?.Name ?? string.Empty, exc?.Message ?? string.Empty, task?.Type ?? string.Empty, store?.Name ?? string.Empty, syncTaskUrl ?? string.Empty);

                    //log error
                    await _logger.ErrorAsync(message, exc);
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        task.Name,
                        0,
                        message,
                        exc.StackTrace);
                }
                catch (Exception insideCatchEx)
                {
                    //log error
                    await _logger.ErrorAsync(insideCatchEx.Message, insideCatchEx);
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        task?.Name ?? string.Empty,
                        0,
                        insideCatchEx.Message,
                        insideCatchEx.StackTrace);

                    if (throwException)
                        throw;
                }

                if (throwException)
                    throw;
            }
        }

        #endregion
    }
}