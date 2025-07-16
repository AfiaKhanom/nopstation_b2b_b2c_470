using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpActivityLogs;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

/// <summary>
/// Represents the erp activity logs model factory implementation
/// </summary>
public partial class ErpActivityLogsModelFactory : IErpActivityLogsModelFactory
{
    #region Fields

    private readonly IErpActivityLogsService _erpActivityLogsService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly ICustomerService _customerService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public ErpActivityLogsModelFactory(IErpActivityLogsService erpActivityLogsService,
        ICustomerService customerService,
        IDateTimeHelper dateTimeHelper,
        ICustomerActivityService customerActivityService,
        ILocalizationService localizationService)
    {
        _erpActivityLogsService = erpActivityLogsService;
        _customerService = customerService;
        _dateTimeHelper = dateTimeHelper;
        _customerActivityService = customerActivityService;
        _localizationService = localizationService;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Prepare erp activity logs type models
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the list of erp activity logs type models
    /// </returns>
    protected virtual async Task<IList<ErpActivityLogsTypeModel>> PrepareErpActivityLogsTypeModelsAsync()
    {
        //prepare available activity log types
        var availableActivityTypes = await _erpActivityLogsService.GetAllErpActivityTypesAsync();
        var models = availableActivityTypes.Select(activityType => activityType.ToModel<ErpActivityLogsTypeModel>()).ToList();

        return models;
    }

    /// <summary>
    /// Prepare default item
    /// </summary>
    /// <param name="items">Available items</param>
    /// <param name="withSpecialDefaultItem">Whether to insert the first special item for the default value</param>
    /// <param name="defaultItemText">Default item text; pass null to use "All" text</param>
    /// <param name="defaultItemValue">Default item value; defaults 0</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected virtual async Task PrepareDefaultItemAsync(IList<SelectListItem> items, bool withSpecialDefaultItem, string defaultItemText = null, string defaultItemValue = "0")
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        //whether to insert the first special item for the default value
        if (!withSpecialDefaultItem)
            return;

        //prepare item text
        defaultItemText ??= await _localizationService.GetResourceAsync("Admin.Common.All");

        //insert this default item at first
        items.Insert(0, new SelectListItem { Text = defaultItemText, Value = defaultItemValue });
    }

    public virtual async Task PrepareErpActivityLogsTypesAsync(IList<SelectListItem> items, bool withSpecialDefaultItem = true, string defaultItemText = null)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        //prepare available erp activity log types
        var availableActivityTypes = await _erpActivityLogsService.GetAllErpActivityTypesAsync();
        foreach (var activityType in availableActivityTypes)
        {
            items.Add(new SelectListItem { Value = activityType.Id.ToString(), Text = activityType.Name });
        }

        //insert special item for the default value
        await PrepareDefaultItemAsync(items, withSpecialDefaultItem, defaultItemText);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Prepare erp activity logs types search model
    /// </summary>
    /// <param name="searchModel">Erp Activity logs types search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the erp activity logs types search model
    /// </returns>
    public virtual async Task<ErpActivityLogsTypeSearchModel> PrepareErpActivityLogsTypeSearchModelAsync(ErpActivityLogsTypeSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        searchModel.ErpActivityLogsTypeListModel = await PrepareErpActivityLogsTypeModelsAsync();

        //prepare grid
        searchModel.SetGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare erp activity logs search model
    /// </summary>
    /// <param name="searchModel">Erp Activity logs search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the erp activity logs search model
    /// </returns>
    public virtual async Task<ErpActivityLogsSearchModel> PrepareErpActivityLogsSearchModelAsync(ErpActivityLogsSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //prepare available activity log types
        await PrepareErpActivityLogsTypesAsync(searchModel.ErpActivityLogsType);

        //prepare grid
        searchModel.SetGridPageSize();

        return searchModel;
    }

    /// <summary>
    /// Prepare paged erp activity logs list model
    /// </summary>
    /// <param name="searchModel">Erp Activity logs search model</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the erp activity logs list model
    /// </returns>
    public virtual async Task<ErpActivityLogsListModel> PrepareErpActivityLogsListModelAsync(ErpActivityLogsSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        //get parameters to filter log
        var startDateValue = searchModel.CreatedOnFrom == null ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedOnFrom.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
        var endDateValue = searchModel.CreatedOnTo == null ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedOnTo.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

        //get log
        var activityLog = await _erpActivityLogsService.GetAllErpActivitiesAsync(createdOnFrom: startDateValue,
            createdOnTo: endDateValue,
            activityLogTypeId: searchModel.ErpActivityLogsTypeId,
            ipAddress: searchModel.IpAddress,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        if (activityLog is null)
            return new ErpActivityLogsListModel();

        //prepare list model
        var customerIds = activityLog.GroupBy(logItem => logItem.CustomerId).Select(logItem => logItem.Key);
        var activityLogCustomers = await _customerService.GetCustomersByIdsAsync(customerIds.ToArray());
        var model = await new ErpActivityLogsListModel().PrepareToGridAsync(searchModel, activityLog, () =>
        {
            return activityLog.SelectAwait(async logItem =>
            {
                //fill in model values from the entity
                var logItemModel = logItem.ToModel<ErpActivityLogsModel>();
                logItemModel.ErpActivityLogTypeName = (await _customerActivityService.GetActivityTypeByIdAsync(logItem.ErpActivityLogTypeId))?.Name;

                logItemModel.CustomerEmail = activityLogCustomers?.FirstOrDefault(x => x.Id == logItem.CustomerId)?.Email;

                //convert dates to the user time
                logItemModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(logItem.CreatedOnUtc, DateTimeKind.Utc);

                return logItemModel;
            });
        });

        return model;
    }

    #endregion
}