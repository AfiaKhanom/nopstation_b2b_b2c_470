using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Caching;
using Nop.Services;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.B2BB2CFeatures.Infrastructure;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories
{
    public class ErpActivityLogModelFactory : IErpActivityLogModelFactory
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IB2BB2CWorkContext _b2BB2CWorkContext;
        private readonly IErpLogsService _erpLogsService;
        private readonly IHtmlFormatter _htmlFormatter;

        #endregion

        #region ctor

        public ErpActivityLogModelFactory(
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper,
            ICustomerService customerService,
            IStaticCacheManager staticCacheManager,
            IB2BB2CWorkContext b2BB2CWorkContext,
            IErpLogsService erpLogsService,
            IHtmlFormatter htmlFormatter
            )
        {
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _customerService = customerService;
            _staticCacheManager = staticCacheManager;
            _b2BB2CWorkContext = b2BB2CWorkContext;
            _erpLogsService = erpLogsService;
            _htmlFormatter = htmlFormatter;
        }

        #endregion

        #region Utilities

        private async Task PrepareActivityTypesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            //prepare available b2b activity log
            var availableActivityTypes = await ErpLogLevel.Debug.ToSelectListAsync(false);
            foreach (var types in availableActivityTypes)
            {
                items.Add(types);
            }

            //insert special item for the default value
            await PrepareDefaultItemAsync(items, withSpecialDefaultItem, defaultItemText);
        }

        private async Task PrepareDefaultItemAsync(IList<SelectListItem> items, bool withSpecialDefaultItem, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            //whether to insert the first special item for the default value
            if (!withSpecialDefaultItem)
                return;

            //at now we use "0" as the default value
            const string value = "0";

            //prepare item text
            defaultItemText = defaultItemText ?? await _localizationService.GetResourceAsync("Admin.Common.All");

            //insert this default item at first
            items.Insert(0, new SelectListItem { Text = defaultItemText, Value = value });
        }

        private async Task<IList<SelectListItem>> PrepareSyncLabelItemAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            //prepare available Erp activity log sync label
            var availableActivityTypes = await ErpSyncLavel.Order.ToSelectListAsync(false);
            foreach (var types in availableActivityTypes)
            {
                items.Add(types);
            }

            //whether to insert the first special item for the default value
            if (!withSpecialDefaultItem)
                return items;

            //at now we use "0" as the default value
            const string value = "0";

            //prepare item text
            defaultItemText = defaultItemText ?? await _localizationService.GetResourceAsync("Admin.Common.All");

            //insert this default item at first
            items.Insert(0, new SelectListItem { Text = defaultItemText, Value = value });

            //insert special item for the default value
            return items;
        }

        #endregion

        #region Method

        public async Task<ErpActivityLogSearchModel> PrepareErpActivityLogSearchModelAsync(ErpActivityLogSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare activity log types name
            await PrepareActivityTypesAsync(searchModel.AvailableActivityType);

            var key = _staticCacheManager.PrepareKeyForDefaultCache(B2BB2CFeaturesDefaults.ErpCustomerAccountErpActivityLogSyncLabelSelectList);

            //prepare entity name
            searchModel.AvailableErpSyncLabel = await _staticCacheManager.GetAsync(key, async () =>
            {
                return await PrepareSyncLabelItemAsync(searchModel.AvailableErpSyncLabel);
            });

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare B2B Activity Log List Model
        /// </summary>
        /// <param name="b2BActivityLogListModel">list model</param>
        /// <returns></returns>
        public async Task<ErpActivityLogListModel> PrepareErpActivityLogListModelAsync(ErpActivityLogSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            var currCustomer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();
            var createdFrom = !searchModel.CreatedFrom.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedFrom.Value, await _dateTimeHelper.GetCustomerTimeZoneAsync(currCustomer));
            var createdTo = !searchModel.CreatedTo.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedTo.Value, await _dateTimeHelper.GetCustomerTimeZoneAsync(currCustomer)).AddDays(1);
            //get ERP_Activity_log
            var erpActivityLogs = await _erpLogsService.GetAllErpLogsAsync(ipAddress: searchModel.IpAddress, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize, logLevelId: searchModel.ActivityLogLevelId, syncLavelId: searchModel.ErpSyncLabelId, nopCustomerEmail: searchModel.NopCustomerEmail, createdFrom: createdFrom, createdTo: createdTo);

            //prepare list model
            var model = await new ErpActivityLogListModel().PrepareToGridAsync(searchModel, erpActivityLogs, () =>
            {
                return erpActivityLogs.SelectAwait(async activityLog =>
                {
                    var activityLogModel = activityLog.ToModel<ErpActivityLogModel>();

                    activityLogModel.ErpLogLevel = await _localizationService.GetLocalizedEnumAsync(activityLog.LogLevel);
                    activityLogModel.CreatedOnUtc = activityLog.CreatedOnUtc;
                    activityLogModel.ErpSyncLavel = await _localizationService.GetLocalizedEnumAsync(activityLog.ErpSyncLavel);
                    activityLogModel.ChangedByCustomerEmail = activityLog.CustomerId.HasValue ? (await _customerService.GetCustomerByIdAsync(activityLog.CustomerId.Value))?.Email : string.Empty;
                    activityLogModel.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(activityLog.CreatedOnUtc, DateTimeKind.Utc);

                    return activityLogModel;
                });
            });

            return model;
        }

        public async Task<ErpActivityLogModel> PrepareErpActivityLogModelAsync(ErpActivityLogModel model, ErpLogs log, bool excludeProperties = false)
        {
            if (log != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = log.ToModel<ErpActivityLogModel>();

                    model.ErpLogLevel = await _localizationService.GetLocalizedEnumAsync(log.LogLevel);
                    model.ErpSyncLavel = await _localizationService.GetLocalizedEnumAsync(log.ErpSyncLavel);
                    model.ShortMessage = _htmlFormatter.FormatText(log.ShortMessage, false, true, false, false, false, false);
                    model.FullMessage = _htmlFormatter.FormatText(log.FullMessage, false, true, false, false, false, false);
                    model.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(log.CreatedOnUtc, DateTimeKind.Utc);
                    model.ChangedByCustomerEmail = log.CustomerId.HasValue ? (await _customerService.GetCustomerByIdAsync(log.CustomerId.Value))?.Email : string.Empty;
                }
            }
            return model;
        }

        #endregion
    }
}
