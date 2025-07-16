using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.NopStationSyncServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Controllers;

public class PartialSyncController : NopStationAdminController
{
    #region Fields

    private readonly ISyncTaskService _syncTaskService;
    private readonly ILocalizationService _localizationService;
    private readonly INopStationScheduler _nopStationScheduler;
    private readonly ISyncLogService _syncLogService;

    #endregion

    #region Ctor

    public PartialSyncController(INopStationScheduler nopStationScheduler,
        ISyncLogService syncLogService,
        ISyncTaskService syncTaskService,
        ILocalizationService localizationService)
    {
        _nopStationScheduler = nopStationScheduler;
        _syncLogService = syncLogService;
        _syncTaskService = syncTaskService;
        _localizationService = localizationService;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> Index()
    {
        var model = new PartialSyncModel();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SyncErpAccount(ErpAccountPartialSyncModel model)
    {
        //try to get a schedule task with the specified id
        var task = await _syncTaskService.GetTaskByQuartzJobNameAsync(ErpDataSchedulerDefaults.ErpAccountSyncTaskIdentity);

        if (task is null)
        {
            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.RequestedTaskNotFound")
            });
        }

        try
        {
            var dataMapList = new List<KeyValuePair<string, object>>
            {
                new(nameof(ErpAccountPartialSyncModel.ErpAccountNumber), model.ErpAccountNumber)
            };

            var jobDataMap = _nopStationScheduler.PrepareJobDataMap(dataMapList);

            await _nopStationScheduler.ExecuteSchedulerAsync(task.QuartzJobName, jobDataMap);

            return Json(new
            {
                Success = true,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.TaskScheduledToExecute")
            });
        }
        catch (Exception ex)
        {
            await _syncLogService.SyncLogSaveOnFileAsync(task.Name, 0, ex.Message, ex.StackTrace ?? string.Empty);

            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.InternalServerError")
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SyncErpGroupPrice(ErpGroupPricePartialSyncModel model)
    {
        //try to get a schedule task with the specified id
        var task = await _syncTaskService.GetTaskByQuartzJobNameAsync(ErpDataSchedulerDefaults.ErpGroupPriceSyncTaskIdentity);

        if (task is null)
        {
            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.RequestedTaskNotFound")
            });
        }

        try
        {
            var dataMapList = new List<KeyValuePair<string, object>>
            {
                new(nameof(ErpGroupPricePartialSyncModel.PriceCode), model.PriceCode),
                new(nameof(ErpGroupPricePartialSyncModel.StockCode), model.StockCode),
            };

            var jobDataMap = _nopStationScheduler.PrepareJobDataMap(dataMapList);

            await _nopStationScheduler.ExecuteSchedulerAsync(task.QuartzJobName, jobDataMap);

            return Json(new
            {
                Success = true,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.TaskScheduledToExecute")
            });
        }
        catch (Exception ex)
        {
            await _syncLogService.SyncLogSaveOnFileAsync(task.Name, 0, ex.Message, ex.StackTrace ?? string.Empty);

            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.InternalServerError")
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SyncErpInvoice(ErpInvoicePartialSyncModel model)
    {
        //try to get a schedule task with the specified id
        var task = await _syncTaskService.GetTaskByQuartzJobNameAsync(ErpDataSchedulerDefaults.ErpInvoiceSyncTaskIdentity);

        if (task is null)
        {
            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.RequestedTaskNotFound")
            });
        }

        try
        {
            var dataMapList = new List<KeyValuePair<string, object>>
            {
                new(nameof(ErpInvoicePartialSyncModel.ErpAccountNumber), model.ErpAccountNumber),
            };

            var jobDataMap = _nopStationScheduler.PrepareJobDataMap(dataMapList);

            await _nopStationScheduler.ExecuteSchedulerAsync(task.QuartzJobName, jobDataMap);

            return Json(new
            {
                Success = true,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.TaskScheduledToExecute")
            });
        }
        catch (Exception ex)
        {
            await _syncLogService.SyncLogSaveOnFileAsync(task.Name, 0, ex.Message, ex.StackTrace ?? string.Empty);

            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.InternalServerError")
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SyncErpProduct(ErpProductPartialSyncModel model)
    {
        //try to get a schedule task with the specified id
        var task = await _syncTaskService.GetTaskByQuartzJobNameAsync(ErpDataSchedulerDefaults.ErpProductSyncTaskIdentity);

        if (task is null)
        {
            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.RequestedTaskNotFound")
            });
        }

        try
        {
            var dataMapList = new List<KeyValuePair<string, object>>
            {
                new(nameof(ErpProductPartialSyncModel.StockCode), model.StockCode),
            };

            var jobDataMap = _nopStationScheduler.PrepareJobDataMap(dataMapList);

            await _nopStationScheduler.ExecuteSchedulerAsync(task.QuartzJobName, jobDataMap);

            return Json(new
            {
                Success = true,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.TaskScheduledToExecute")
            });
        }
        catch (Exception ex)
        {
            await _syncLogService.SyncLogSaveOnFileAsync(task.Name, 0, ex.Message, ex.StackTrace ?? string.Empty);

            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.InternalServerError")
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SyncErpShipToAddress(ErpShipToAddressPartialSyncModel model)
    {
        //try to get a schedule task with the specified id
        var task = await _syncTaskService.GetTaskByQuartzJobNameAsync(ErpDataSchedulerDefaults.ErpShipToAddressSyncTaskIdentity);

        if (task is null)
        {
            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.RequestedTaskNotFound")
            });
        }

        try
        {
            var dataMapList = new List<KeyValuePair<string, object>>
            {
                new(nameof(ErpShipToAddressPartialSyncModel.ErpAccountNumber), model.ErpAccountNumber),
            };

            var jobDataMap = _nopStationScheduler.PrepareJobDataMap(dataMapList);

            await _nopStationScheduler.ExecuteSchedulerAsync(task.QuartzJobName, jobDataMap);

            return Json(new
            {
                Success = true,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.TaskScheduledToExecute")
            });
        }
        catch (Exception ex)
        {
            await _syncLogService.SyncLogSaveOnFileAsync(task.Name, 0, ex.Message, ex.StackTrace ?? string.Empty);

            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.InternalServerError")
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SyncErpSpecialPrice(ErpSpecialPricePartialSyncModel model)
    {
        //try to get a schedule task with the specified id
        var task = await _syncTaskService.GetTaskByQuartzJobNameAsync(ErpDataSchedulerDefaults.ErpSpecialPriceSyncTaskIdentity);

        if (task is null)
        {
            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.RequestedTaskNotFound")
            });
        }

        try
        {
            var dataMapList = new List<KeyValuePair<string, object>>
            {
                new(nameof(ErpSpecialPricePartialSyncModel.ErpAccountNumber), model.ErpAccountNumber),
                new(nameof(ErpSpecialPricePartialSyncModel.StockCode), model.StockCode),
            };

            var jobDataMap = _nopStationScheduler.PrepareJobDataMap(dataMapList);

            await _nopStationScheduler.ExecuteSchedulerAsync(task.QuartzJobName, jobDataMap);

            return Json(new
            {
                Success = true,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.TaskScheduledToExecute")
            });
        }
        catch (Exception ex)
        {
            await _syncLogService.SyncLogSaveOnFileAsync(task.Name, 0, ex.Message, ex.StackTrace ?? string.Empty);

            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.InternalServerError")
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SyncErpOrder(ErpOrderPartialSyncModel model)
    {
        //try to get a schedule task with the specified id
        var task = await _syncTaskService.GetTaskByQuartzJobNameAsync(ErpDataSchedulerDefaults.ErpOrderSyncTaskIdentity);

        if (task is null)
        {
            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.RequestedTaskNotFound")
            });
        }

        try
        {
            var dataMapList = new List<KeyValuePair<string, object>>
            {
                new(nameof(ErpOrderPartialSyncModel.ErpAccountNumber), model.ErpAccountNumber),
                new(nameof(ErpOrderPartialSyncModel.OrderNumber), model.OrderNumber),
            };

            var jobDataMap = _nopStationScheduler.PrepareJobDataMap(dataMapList);

            await _nopStationScheduler.ExecuteSchedulerAsync(task.QuartzJobName, jobDataMap);

            return Json(new
            {
                Success = true,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.TaskScheduledToExecute")
            });
        }
        catch (Exception ex)
        {
            await _syncLogService.SyncLogSaveOnFileAsync(task.Name, 0, ex.Message, ex.StackTrace ?? string.Empty);

            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.InternalServerError")
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SyncErpStock(ErpStockPartialSyncModel model)
    {
        //try to get a schedule task with the specified id
        var task = await _syncTaskService.GetTaskByQuartzJobNameAsync(ErpDataSchedulerDefaults.ErpStockSyncTaskIdentity);

        if (task is null)
        {
            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.RequestedTaskNotFound")
            });
        }

        try
        {
            var dataMapList = new List<KeyValuePair<string, object>>
            {
                new(nameof(ErpStockPartialSyncModel.StockCode), model.StockCode),
            };

            var jobDataMap = _nopStationScheduler.PrepareJobDataMap(dataMapList);

            await _nopStationScheduler.ExecuteSchedulerAsync(task.QuartzJobName, jobDataMap);

            return Json(new
            {
                Success = true,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.TaskScheduledToExecute")
            });
        }
        catch (Exception ex)
        {
            await _syncLogService.SyncLogSaveOnFileAsync(task.Name, 0, ex.Message, ex.StackTrace ?? string.Empty);

            return Json(new
            {
                Success = false,
                Message = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Admin.PartialSync.InternalServerError")
            });
        }
    }

    #endregion
}
