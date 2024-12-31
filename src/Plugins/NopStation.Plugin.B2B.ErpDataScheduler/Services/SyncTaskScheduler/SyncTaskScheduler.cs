using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Http;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Services.Logging;
using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models;
using NopStation.Plugin.B2B.ErpDataScheduler.Domain;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskScheduler
{
    /// <summary>
    /// Represents task manager
    /// </summary>
    public partial class SyncTaskScheduler : ISyncTaskScheduler
    {
        #region Fields

        protected static readonly List<SyncTaskThread> _taskThreads = new();
        protected readonly IErpLogsService _erpLogsService;
        protected readonly ISyncTaskService _syncTaskService;
        protected readonly IStoreContext _storeContext;
        protected readonly AppSettings _appSettings;
        protected static readonly Dictionary<SyncTaskThread, SyncTask> _taskThreadInfo = new();
        protected readonly ISyncLogService _erpSyncLogService;

        #endregion

        #region Ctor

        public SyncTaskScheduler(AppSettings appSettings,
            IHttpClientFactory httpClientFactory,
            ISyncTaskService syncTaskService,
            IServiceScopeFactory serviceScopeFactory,
            IErpLogsService erpLogsService,
            IStoreContext storeContext,
            ISyncLogService erpSyncLogService)
        {
            _appSettings = appSettings;
            SyncTaskThread.HttpClientFactory = httpClientFactory;
            _syncTaskService = syncTaskService;
            SyncTaskThread.ServiceScopeFactory = serviceScopeFactory;
            _erpLogsService = erpLogsService;
            _storeContext = storeContext;
            _erpSyncLogService = erpSyncLogService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes the task manager
        /// </summary>
        public async Task<bool> InitializeAsync()
        {
            if (!DataSettingsManager.IsDatabaseInstalled())
                return false;

            if (_taskThreads.Any())
            {
                await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Error, 0, "Task threads already initialized.");
                return false;
            }

            //initialize and start schedule tasks
            var syncTasks = (await _syncTaskService.GetAllTasksAsync()).OrderBy(x => x.Seconds).ToList();

            try
            {
                var store = await _storeContext.GetCurrentStoreAsync();

                var syncTaskUrl = $"{store.Url.TrimEnd('/')}/{SyncTaskDefaults.SyncTaskPath}";
                var timeout = _appSettings.Get<CommonConfig>().ScheduleTaskRunTimeout;

                foreach (var syncTask in syncTasks)
                {
                    var syncTaskThread = new SyncTaskThread(syncTask, syncTaskUrl, timeout, _erpLogsService, _erpSyncLogService)
                    {
                        Seconds = syncTask.Seconds,
                        SyncTaskInstance = this
                    };

                    // Calculate the next execution time based on selected weekdays and time slots
                    syncTaskThread.NextExecutionTime = CalculateNextExecutionTimeAsync(syncTask);

                    syncTaskThread.InitSeconds = (int)(GetNextTimeInterval(ErpDataSchedulerDefaults.DefaultSyncTaskTimeInterval) - DateTime.UtcNow).TotalSeconds + 1;

                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTask.Name,
                        GetSyncTaskType(syncTask.Type).syncTaskLevel,
                        $"Task {syncTask.Name}, Timer initialization after {syncTaskThread.InitSeconds} seconds, Next Execution: {syncTaskThread.NextExecutionTime}");

                    _taskThreadInfo[syncTaskThread] = syncTask;

                    _taskThreads.Add(syncTaskThread);
                }
            }
            catch (Exception ex)
            {
                await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Error, 0, ex.Message, ex.StackTrace);

                return false;
            }

            return true;
        }

        private (ErpSyncLevel syncTaskLevel, string syncTaskName) GetSyncTaskType(string taskType)
        {
            if (taskType == ErpDataSchedulerDefaults.ErpAccountSyncTask)
            {
                return (ErpSyncLevel.Account, ErpDataSchedulerDefaults.ErpAccountSyncTaskName);
            }
            else if (taskType == ErpDataSchedulerDefaults.ErpInvoiceSyncTask)
            {
                return (ErpSyncLevel.Invoice, ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName);
            }
            else if (taskType == ErpDataSchedulerDefaults.ErpProductSyncTask)
            {
                return (ErpSyncLevel.Product, ErpDataSchedulerDefaults.ErpProductSyncTaskName);
            }
            else if (taskType == ErpDataSchedulerDefaults.ErpGroupPriceSyncTask)
            {
                return (ErpSyncLevel.GroupPrice, ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskName);
            }
            else if (taskType == ErpDataSchedulerDefaults.ErpSpecialPriceSyncTask)
            {
                return (ErpSyncLevel.SpecialPrice, ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskName);
            }
            else if (taskType == ErpDataSchedulerDefaults.ErpStockSyncTask)
            {
                return ((ErpSyncLevel.Stock, ErpDataSchedulerDefaults.ErpStockSyncTaskName));
            }
            else if (taskType == ErpDataSchedulerDefaults.ErpShipToAddressSyncTask)
            {
                return (ErpSyncLevel.ShipToAddress, ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskName);
            }
            else if (taskType == ErpDataSchedulerDefaults.ErpOrderSyncTask)
            {
                return (ErpSyncLevel.Order, ErpDataSchedulerDefaults.ErpOrderSyncTaskName);
            }
            return (0, string.Empty);
        }

        private void UpdateNextExecutionTime(SyncTaskThread taskThread, DateTime nextExecutionTime)
        {
            taskThread.NextExecutionTime = nextExecutionTime;
        }

        private DateTime GetNextTimeInterval(int nthMinute)
        {
            if (nthMinute <= 0)
            {
                return DateTime.MinValue;
            }

            // Calculate the next nth-minute interval
            var currentUtcTime = DateTime.UtcNow;
            int minutesToAdd = nthMinute - (currentUtcTime.Minute % nthMinute);
            var nextNthMinuteInterval = currentUtcTime.AddMinutes(minutesToAdd);
            // Set the seconds and milliseconds to zero
            nextNthMinuteInterval = nextNthMinuteInterval.AddSeconds(-nextNthMinuteInterval.Second);
            nextNthMinuteInterval = nextNthMinuteInterval.AddMilliseconds(-nextNthMinuteInterval.Millisecond);

            return nextNthMinuteInterval;
        }

        private DateTime CalculateNextExecutionTimeAsync(SyncTask task)
        {
            if (string.IsNullOrEmpty(task.DayTimeSlots))
                return DateTime.MaxValue;

            var dayOfWeekSlots = JsonConvert.DeserializeObject<List<SyncTaskDaySlotModel>>(task.DayTimeSlots ?? string.Empty);

            // Calculate the number of days before next week's this day.
            var currentDateTime = DateTime.UtcNow.Date.AddHours(0).AddMinutes(0).AddSeconds(0);

            var tillNextWeekDay = currentDateTime.Date.AddDays(7).AddHours(23).AddMinutes(59).AddSeconds(59);

            // Loop to find the next valid execution time upto next week's this day.
            while (currentDateTime <= tillNextWeekDay && dayOfWeekSlots is not null)
            {
                // Check if the current day of the week is in the selected weekdays
                var checkTodaysSlot = dayOfWeekSlots.Exists(slot => slot.DayOfWeek == (int)currentDateTime.DayOfWeek);

                if (checkTodaysSlot)
                {
                    var selectedTimeSlots = dayOfWeekSlots
                                            .Where(slotDays => slotDays.DayOfWeek == (int)currentDateTime.DayOfWeek)
                                            .SelectMany(slotTime => slotTime.TimeSlots)
                                            .Select(slot => slot.TimeSlot)
                                            .ToList();

                    if (selectedTimeSlots.Any())
                    {
                        foreach (var slot in Enumerable.Range(0, 48))
                        {
                            var dateTimeOfSlot = currentDateTime.Add(new TimeSpan((slot * 30) / 60, (slot * 30) % 60, 0));

                            var timeSlot = dateTimeOfSlot.TimeOfDay.ToString(@"hh\:mm");

                            if (selectedTimeSlots.Exists(ts => ts == timeSlot) && DateTime.UtcNow <= dateTimeOfSlot)
                                return dateTimeOfSlot;
                        }
                    }
                }
                currentDateTime = currentDateTime.AddDays(1);
            }

            return DateTime.MaxValue;
        }


        /// <summary>
        /// Starts the task scheduler
        /// </summary>
        public void StartSyncTaskScheduler()
        {
            foreach (var taskThread in _taskThreads)
                taskThread.InitTimer();
        }

        /// <summary>
        /// Stops the task scheduler
        /// </summary>
        public void StopSyncTaskScheduler()
        {
            foreach (var taskThread in _taskThreads)
                taskThread.Dispose();
        }

        #endregion

        #region Nested class

        /// <summary>
        /// Represents task thread
        /// </summary>
        protected partial class SyncTaskThread : IDisposable
        {
            #region Fields

            public SyncTaskScheduler SyncTaskInstance { get; set; }

            protected readonly string _syncTaskUrl;
            protected readonly SyncTask _syncTask;
            protected readonly IErpLogsService _erpLogsService;
            protected readonly ISyncLogService _erpSyncLogService;
            protected readonly int? _timeout;

            protected Timer _timer;
            protected bool _disposed;

            internal static IHttpClientFactory HttpClientFactory { get; set; }
            internal static IServiceScopeFactory ServiceScopeFactory { get; set; }

            #endregion

            #region Ctor

            public SyncTaskThread(SyncTask task, string syncTaskUrl, int? timeout, IErpLogsService erpLogsService, ISyncLogService erpSyncLogService)
            {
                _syncTaskUrl = syncTaskUrl;
                _syncTask = task;
                _timeout = timeout;

                Seconds = 10 * 60;
                _erpLogsService = erpLogsService;
                _erpSyncLogService = erpSyncLogService;
            }

            #endregion

            #region Utilities

            private async Task RunAsync()
            {
                if (Seconds <= 0)
                    return;

                StartedUtc = DateTime.UtcNow;
                IsRunning = true;
                HttpClient client = new HttpClient();

                try
                {
                    //create and configure client
                    client = HttpClientFactory.CreateClient(NopHttpDefaults.DefaultHttpClient);

                    if (_timeout.HasValue)
                        client.Timeout = TimeSpan.FromSeconds(_timeout.Value);

                    //send post data
                    var data = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("taskType", _syncTask.Type) });

                    await client.PostAsync(_syncTaskUrl, data);
                }
                catch (Exception ex)
                {
                    using var scope = ServiceScopeFactory.CreateScope();
                    // Resolve
                    var logger = EngineContext.Current.Resolve<ILogger>(scope);

                    try
                    {
                        var localizationService = EngineContext.Current.Resolve<ILocalizationService>(scope);
                        var storeContext = EngineContext.Current.Resolve<IStoreContext>(scope);

                        var message = ex.InnerException?.GetType() == typeof(TaskCanceledException) ? await localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.TimeoutError") : ex.Message;
                        var store = await storeContext.GetCurrentStoreAsync();

                        message = string.Format(await localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Error"),
                            _syncTask?.Name ?? string.Empty, message ?? string.Empty, _syncTask?.Type ?? string.Empty, store?.Name ?? string.Empty, _syncTaskUrl ?? string.Empty);

                        await logger.ErrorAsync(message, ex);
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            _syncTask.Name,
                            0,
                            message, ex.StackTrace);
                    }
                    catch (Exception insideCatchEx)
                    {
                        //log error
                        await logger.ErrorAsync(insideCatchEx.Message, insideCatchEx);
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            _syncTask?.Name ?? string.Empty,
                            0,
                            insideCatchEx.Message,
                            insideCatchEx.StackTrace);
                    }
                }
                finally
                {
                    client?.Dispose();
                }

                IsRunning = false;
            }

            public async void TimerHandler(object state)
            {
                var task = _taskThreadInfo[this];
                var didAttempt = false;
                var syncType = SyncTaskInstance.GetSyncTaskType(task.Type);

                try
                {
                    _timer.Change(-1, -1);

                    if (DateTime.UtcNow.AddSeconds(1) >= NextExecutionTime)
                    {
                        RunAsync().Wait();
                        didAttempt = true;
                    }

                    // Update the NextExecutionTime for the task within the taskThread instance
                    NextExecutionTime = SyncTaskInstance.CalculateNextExecutionTimeAsync(task);

                    // Update the NextExecutionTime for the task within the SyncTask instance
                    SyncTaskInstance.UpdateNextExecutionTime(this, NextExecutionTime);

                    if (didAttempt)
                    {
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            syncType.syncTaskName,
                            syncType.syncTaskLevel,
                            $"Task {task.Name} attempted. Next execution at {NextExecutionTime}");

                    }
                }
                catch (Exception ex)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncType.syncTaskName,
                        syncType.syncTaskLevel,
                        ex.Message, ex.StackTrace);
                }
                finally
                {
                    if (!_disposed && _timer is not null)
                    {
                        if (RunOnlyOnce)
                        {
                            Dispose();
                        }
                        else
                        {
                            _timer.Change(Interval, Interval);
                        }
                    }
                }
            }

            #endregion

            #region Methods

            /// <summary>
            /// Disposes the instance
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            // Protected implementation of Dispose pattern.
            protected virtual void Dispose(bool disposing)
            {
                if (_disposed)
                    return;

                if (disposing)
                    lock (this)
                        _timer?.Dispose();

                _disposed = true;
            }

            /// <summary>
            /// Inits a timer
            /// </summary>
            public void InitTimer()
            {
                // Use a lambda expression to capture the syncTask object
                _timer ??= new Timer(TimerHandler, null, InitInterval, Interval);
            }


            #endregion

            #region Properties

            /// <summary>
            /// Gets or sets the interval in seconds at which to run the tasks
            /// </summary>
            public int Seconds { get; set; }

            /// <summary>
            /// Get or set the interval before timer first start 
            /// </summary>
            public int InitSeconds { get; set; }

            /// <summary>
            /// Get or sets a datetime when thread has been started
            /// </summary>
            public DateTime StartedUtc { get; private set; }

            /// <summary>
            /// Get or sets a datetime for the next execution time 
            /// </summary>
            public DateTime NextExecutionTime { get; set; }

            /// <summary>
            /// Get or sets a value indicating whether thread is running
            /// </summary>
            public bool IsRunning { get; private set; }

            /// <summary>
            /// Gets the interval (in milliseconds) at which to run the task
            /// </summary>
            public int Interval
            {
                get
                {
                    //if somebody entered more than "2147483" seconds, then an exception could be thrown (exceeds int.MaxValue)
                    var interval = Seconds * 1000;
                    if (interval <= 0)
                        interval = int.MaxValue;
                    return interval;
                }
            }

            /// <summary>
            /// Gets the due time interval (in milliseconds) at which to begin start the task
            /// </summary>
            public int InitInterval
            {
                get
                {
                    //if somebody entered less than "0" seconds, then an exception could be thrown
                    var interval = InitSeconds * 1000;
                    if (interval <= 0)
                        interval = 0;
                    return interval;
                }
            }

            /// <summary>
            /// Gets or sets a value indicating whether the thread would be run only once (on application start)
            /// </summary>
            public bool RunOnlyOnce { get; set; }

            /// <summary>
            /// Gets a value indicating whether the timer is started
            /// </summary>
            public bool IsStarted => _timer != null;

            /// <summary>
            /// Gets a value indicating whether the timer is disposed
            /// </summary>
            public bool IsDisposed => _disposed;

            #endregion
        }

        #endregion
    }
}