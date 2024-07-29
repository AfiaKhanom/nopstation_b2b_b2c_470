using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public partial interface IErpActivityLogsService
    {
        /// <summary>
        /// Inserts erp activity log types
        /// </summary>
        /// <param name="erpActivityLogTypes">List of Erp ActivityLogTypes</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        Task InsertOrUpdateErpActivityTypesAsync(List<ActivityLogType> erpActivityLogTypes);

        /// <summary>
        /// Gets activity log type item
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the activity log type item
        /// </returns>
        Task<ActivityLogType> GetErpActivityTypeBySystemKeywordAsync(string systemKeyword = "");

        /// <summary>
        /// Gets all erp activity logs type items
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the erp activity logs type items
        /// </returns>
        Task<IList<ActivityLogType>> GetAllErpActivityTypesAsync();

        /// <summary>
        /// Inserts an erp activity log item
        /// </summary>
        /// <param name="systemKeyword">System keyword</param>
        /// <param name="comment">Comment</param>
        /// <param name="entity">Entity</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the erp activity log item
        /// </returns>
        Task<ErpActivityLogs> InsertErpActivityAsync(string systemKeyword, string comment, BaseEntity entity = null);

        /// <summary>
        /// Inserts an erp activity log item
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="systemKeyword">System keyword</param>
        /// <param name="comment">Comment</param>
        /// <param name="entity">Entity</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the erp activity log item
        /// </returns>
        Task<ErpActivityLogs> InsertErpActivityAsync(Customer customer, string systemKeyword, string comment, BaseEntity entity = null);

        /// <summary>
        /// Deletes an erp activity log item
        /// </summary>
        /// <param name="activityLog">Erp Activity log</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteErpActivityAsync(ErpActivityLogs erpActivityLogs);

        /// <summary>
        /// Gets all erp activity log items
        /// </summary>
        /// <param name="createdOnFrom">Log item creation from; pass null to load all records</param>
        /// <param name="createdOnTo">Log item creation to; pass null to load all records</param>
        /// <param name="customerId">Customer identifier; pass null to load all records</param>
        /// <param name="activityLogTypeId">Activity log type identifier; pass null to load all records</param>
        /// <param name="ipAddress">IP address; pass null or empty to load all records</param>
        /// <param name="entityName">Entity name; pass null to load all records</param>
        /// <param name="entityId">Entity identifier; pass null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the erp activity log items
        /// </returns>
        Task<IPagedList<ErpActivityLogs>> GetAllErpActivitiesAsync(DateTime? createdOnFrom = null, DateTime? createdOnTo = null,
            int? customerId = null, int? activityLogTypeId = null, string ipAddress = null, string entityName = null, int? entityId = null,
            int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets an erp activity log item
        /// </summary>
        /// <param name="erpActivityLogId">Erp Activity log identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the erp activity log item
        /// </returns>
        Task<ErpActivityLogs> GetErpActivityByIdAsync(int erpActivityLogId);

        /// <summary>
        /// Clears erp activity log
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task ClearAllErpActivitiesAsync();
    }
}
