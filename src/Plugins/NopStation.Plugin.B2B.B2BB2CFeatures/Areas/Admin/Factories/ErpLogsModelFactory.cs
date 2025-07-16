using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpLogs;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public class ErpLogsModelFactory : IErpLogsModelFactory
{
    #region Fields

    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly ICustomerService _customerService;
    private readonly ILocalizationService _localizationService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IWorkContext _workContext;
    private readonly IErpLogsService _erpLogsService;
    private readonly IHtmlFormatter _htmlFormatter;

    #endregion

    #region Ctor

    public ErpLogsModelFactory(ILocalizationService localizationService,
        IDateTimeHelper dateTimeHelper,
        ICustomerService customerService,
        IStaticCacheManager staticCacheManager,
        IWorkContext workContext,
        IErpLogsService erpLogsService,
        IHtmlFormatter htmlFormatter)
    {
        _localizationService = localizationService;
        _dateTimeHelper = dateTimeHelper;
        _customerService = customerService;
        _staticCacheManager = staticCacheManager;
        _workContext = workContext;
        _erpLogsService = erpLogsService;
        _htmlFormatter = htmlFormatter;
    }

    #endregion

    #region Utilities

    private async Task PrepareLogTypesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
    {
        ArgumentNullException.ThrowIfNull(items);

        //prepare available erp log
        var availableLogTypes = await ErpLogLevel.Debug.ToSelectListAsync(false);
        foreach (var types in availableLogTypes)
        {
            items.Add(types);
        }

        //insert special item for the default value
        await PrepareDefaultItemAsync(items, withSpecialDefaultItem, defaultItemText);
    }

    private async Task PrepareDefaultItemAsync(IList<SelectListItem> items, bool withSpecialDefaultItem, string defaultItemText = null)
    {
        ArgumentNullException.ThrowIfNull(items);

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
        ArgumentNullException.ThrowIfNull(items);

        //prepare available Erp log sync label
        var availableLogTypes = await ErpSyncLevel.Order.ToSelectListAsync(false);
        foreach (var types in availableLogTypes)
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

    public async Task<ErpLogsSearchModel> PrepareErpLogsSearchModelAsync(ErpLogsSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        await PrepareLogTypesAsync(searchModel.AvailableLogType);

        var key = _staticCacheManager.PrepareKeyForDefaultCache(B2BB2CFeaturesDefaults.ErpCustomerAccountErpLogsSyncLabelSelectList);

        searchModel.AvailableErpSyncLabel = await _staticCacheManager.GetAsync(key, async () =>
        {
            return await PrepareSyncLabelItemAsync(searchModel.AvailableErpSyncLabel);
        });

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    public async Task<ErpLogsListModel> PrepareErpLogsListModelAsync(ErpLogsSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var createdFrom = !searchModel.CreatedFrom.HasValue ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedFrom.Value, await _dateTimeHelper.GetCustomerTimeZoneAsync(currCustomer));
        var createdTo = !searchModel.CreatedTo.HasValue ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedTo.Value, await _dateTimeHelper.GetCustomerTimeZoneAsync(currCustomer)).AddDays(1);

        var erpLogs = await _erpLogsService.GetAllErpLogsAsync(ipAddress: searchModel.IpAddress,
            message: searchModel.ShortMessage,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize,
            logLevelId: searchModel.ErpLogLevelId,
            syncLevelId: searchModel.ErpSyncLabelId,
            nopCustomerEmail: searchModel.NopCustomerEmail,
            createdFrom: createdFrom,
            createdTo: createdTo);

        var model = await new ErpLogsListModel().PrepareToGridAsync(searchModel, erpLogs, () =>
        {
            return erpLogs.SelectAwait(async erpLogs =>
            {
                var erpLogsModel = erpLogs.ToModel<ErpLogsModel>();

                erpLogsModel.ErpLogLevel = await _localizationService.GetLocalizedEnumAsync(erpLogs.ErpLogLevel);
                erpLogsModel.CreatedOnUtc = erpLogs.CreatedOnUtc;
                erpLogsModel.ErpSyncLevel = await _localizationService.GetLocalizedEnumAsync(erpLogs.ErpSyncLevel);
                erpLogsModel.ChangedByCustomerEmail = erpLogs.CustomerId.HasValue ? (await _customerService.GetCustomerByIdAsync(erpLogs.CustomerId.Value))?.Email : string.Empty;
                erpLogsModel.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(erpLogs.CreatedOnUtc, DateTimeKind.Utc);

                return erpLogsModel;
            });
        });

        return model;
    }

    public async Task<ErpLogsModel> PrepareErpLogsModelAsync(ErpLogsModel model, ErpLogs log, bool excludeProperties = false)
    {
        if (log != null && model == null)
        {
            model = log.ToModel<ErpLogsModel>();

            model.ErpLogLevel = await _localizationService.GetLocalizedEnumAsync(log.ErpLogLevel);
            model.ErpSyncLevel = await _localizationService.GetLocalizedEnumAsync(log.ErpSyncLevel);
            model.ShortMessage = _htmlFormatter.FormatText(log.ShortMessage, false, true, false, false, false, false);
            model.FullMessage = _htmlFormatter.FormatText(log.FullMessage, false, true, false, false, false, false);
            model.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(log.CreatedOnUtc, DateTimeKind.Utc);
            model.ChangedByCustomerEmail = log.CustomerId.HasValue ? (await _customerService.GetCustomerByIdAsync(log.CustomerId.Value))?.Email : string.Empty;
        }

        return model;
    }

    #endregion
}
