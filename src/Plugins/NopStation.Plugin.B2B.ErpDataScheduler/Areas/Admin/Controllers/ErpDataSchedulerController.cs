using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Controllers
{
    public class ErpDataSchedulerController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IErpLogsService _erpLogsService;
        private readonly IErpActivityLogsService _erpActivityLogsService;
        private const string EDIT_SETTINGS_SYSTEM_KEYWORD = "Erp_EditSettings";

        #endregion

        #region Ctor

        public ErpDataSchedulerController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IErpLogsService erpLogsService,
            IErpActivityLogsService erpActivityLogsService)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _workContext = workContext;
            _erpLogsService = erpLogsService;
            _erpActivityLogsService = erpActivityLogsService;
        }

        #endregion

        #region Methods

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        [HttpGet]
        public virtual async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var settings = await _settingService.LoadSettingAsync<ErpDataSchedulerSettings>();

            return View(settings is not null ? settings.ToSettingsModel<ConfigurationModel>() : new ConfigurationModel());
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = model.ToSettings(await _settingService.LoadSettingAsync<ErpDataSchedulerSettings>(storeScope));

            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.SyncFromDate, model.SyncFromDate_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.NeedQuoteOrderCall, model.NeedQuoteOrderCall_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.StartProductSyncAfterLastSyncedProduct, model.StartProductSyncAfterLastSyncedProduct_OverrideForStore, storeScope, false);
            await _settingService.ClearCacheAsync();

            //activity log
            await _erpActivityLogsService.InsertErpActivityAsync(EDIT_SETTINGS_SYSTEM_KEYWORD, await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.ErpActivityLogs.EditConfigurations"));
            
            var successMsg = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Configuration.Updated");
            _notificationService.SuccessNotification(successMsg);

            await _erpLogsService.InformationAsync(successMsg, 0, customer: await _workContext.GetCurrentCustomerAsync());

            return View(model);
        }

        #endregion
    }
}
