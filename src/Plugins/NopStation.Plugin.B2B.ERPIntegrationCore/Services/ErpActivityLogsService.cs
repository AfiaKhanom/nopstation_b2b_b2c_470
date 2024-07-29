using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public partial class ErpActivityLogsService : IErpActivityLogsService
    {
        #region Fields

        private readonly IRepository<ErpActivityLogs> _erpActivityLogRepository;
        private readonly IRepository<ActivityLogType> _activityLogTypeRepository;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public ErpActivityLogsService(IRepository<ErpActivityLogs> erpActivityLogRepository,
            IRepository<ActivityLogType> activityLogTypeRepository,
            IWebHelper webHelper,
            IWorkContext workContext)
        {
            _erpActivityLogRepository = erpActivityLogRepository;
            _activityLogTypeRepository = activityLogTypeRepository;
            _webHelper = webHelper;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Inserts erp activity log types
        /// </summary>
        /// <param name="erpActivityLogTypes">List of Erp ActivityLogTypes</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        public virtual async Task InsertOrUpdateErpActivityTypesAsync(List<ActivityLogType> erpActivityLogTypes)
        {
            var updateLogTypes = new List<ActivityLogType>();
            var insertLogTypes = new List<ActivityLogType>();

            foreach(var logType in erpActivityLogTypes)
            {
                if (GetErpActivityTypeBySystemKeywordAsync(logType.SystemKeyword) != null)
                {
                    updateLogTypes.Add(logType);
                }
                else
                {
                    insertLogTypes.Add(logType);
                }
            }

            if (updateLogTypes.Count > 0)
            {
                await _activityLogTypeRepository.UpdateAsync(updateLogTypes);
            }
            if (insertLogTypes.Count > 0)
            {
                await _activityLogTypeRepository.InsertAsync(insertLogTypes);
            }
        }

        /// <summary>
        /// Gets activity log type item
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the activity log type item
        /// </returns>
        public virtual async Task<ActivityLogType> GetErpActivityTypeBySystemKeywordAsync(string systemKeyword = "")
        {
            var activityLogTypes = await _activityLogTypeRepository.GetAllAsync(query =>
            {
                return from alt in query
                       where alt.SystemKeyword.Equals(systemKeyword)
                       orderby alt.Name
                       select alt;
            }, cache => default);

            return activityLogTypes?.FirstOrDefault();
        }

        /// <summary>
        /// Gets all activity log type items
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the activity log type items
        /// </returns>
        public virtual async Task<IList<ActivityLogType>> GetAllErpActivityTypesAsync()
        {
            var activityLogTypes = await _activityLogTypeRepository.GetAllAsync(query =>
            {
                return from alt in query
                       where alt.SystemKeyword.Contains("Erp")
                       orderby alt.Name
                       select alt;
            });

            return activityLogTypes;
        }

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
        public virtual async Task<ErpActivityLogs> InsertErpActivityAsync(string systemKeyword, string comment, BaseEntity entity = null)
        {
            return await InsertErpActivityAsync(await _workContext.GetCurrentCustomerAsync(), systemKeyword, comment, entity);
        }

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
        public virtual async Task<ErpActivityLogs> InsertErpActivityAsync(Customer customer, string systemKeyword, string comment, BaseEntity entity = null)
        {
            if (customer == null)
                return null;

            //try to get activity log type by passed system keyword
            var activityLogType = (await GetAllErpActivityTypesAsync())
                .FirstOrDefault(type => type.SystemKeyword.Equals(systemKeyword));
            if (!activityLogType?.Enabled ?? true)
                return null;

            //insert log item
            var logItem = new ErpActivityLogs
            {
                ErpActivityLogTypeId = activityLogType.Id,
                EntityId = entity?.Id,
                EntityName = entity?.GetType().Name,
                CustomerId = customer.Id,
                Comment = CommonHelper.EnsureMaximumLength(comment ?? string.Empty, 4000),
                CreatedOnUtc = DateTime.UtcNow,
                IpAddress = _webHelper.GetCurrentIpAddress()
            };
            await _erpActivityLogRepository.InsertAsync(logItem);

            return logItem;
        }

        /// <summary>
        /// Deletes an erp activity log item
        /// </summary>
        /// <param name="activityLog">Erp Activity log</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteErpActivityAsync(ErpActivityLogs erpActivityLogs)
        {
            await _erpActivityLogRepository.DeleteAsync(erpActivityLogs);
        }

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
        public virtual async Task<IPagedList<ErpActivityLogs>> GetAllErpActivitiesAsync(DateTime? createdOnFrom = null, DateTime? createdOnTo = null,
            int? customerId = null, int? activityLogTypeId = null, string ipAddress = null, string entityName = null, int? entityId = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            return await _erpActivityLogRepository.GetAllPagedAsync(query =>
            {
                //filter by IP
                if (!string.IsNullOrEmpty(ipAddress))
                    query = query.Where(logItem => logItem.IpAddress.Contains(ipAddress));

                //filter by creation date
                if (createdOnFrom.HasValue)
                    query = query.Where(logItem => createdOnFrom.Value <= logItem.CreatedOnUtc);
                if (createdOnTo.HasValue)
                    query = query.Where(logItem => createdOnTo.Value >= logItem.CreatedOnUtc);

                //filter by log type
                if (activityLogTypeId.HasValue && activityLogTypeId.Value > 0)
                    query = query.Where(logItem => activityLogTypeId == logItem.ErpActivityLogTypeId);

                //filter by customer
                if (customerId.HasValue && customerId.Value > 0)
                    query = query.Where(logItem => customerId.Value == logItem.CustomerId);

                //filter by entity
                if (!string.IsNullOrEmpty(entityName))
                    query = query.Where(logItem => logItem.EntityName.Equals(entityName));
                if (entityId.HasValue && entityId.Value > 0)
                    query = query.Where(logItem => entityId.Value == logItem.EntityId);

                query = query.OrderByDescending(logItem => logItem.CreatedOnUtc).ThenBy(logItem => logItem.Id);

                return query;
            }, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets an erp activity log item
        /// </summary>
        /// <param name="erpActivityLogId">Erp Activity log identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the erp activity log item
        /// </returns>
        public virtual async Task<ErpActivityLogs> GetErpActivityByIdAsync(int erpActivityLogId)
        {
            return await _erpActivityLogRepository.GetByIdAsync(erpActivityLogId);
        }

        /// <summary>
        /// Clears erp activity log
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task ClearAllErpActivitiesAsync()
        {
            await _erpActivityLogRepository.TruncateAsync();
        }

        #endregion
    }
}