using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public interface IErpLogsService
    {
        Task<ErpLogs> InsertErpLogAsync(ErpLogLevel logLevel, ErpSyncLevel syncLevel, string shortMessage, string fullMessage = "", Customer customer = null);

        ErpLogs InsertErpLog(ErpLogLevel logLevel, ErpSyncLevel syncLevel, string shortMessage, string fullMessage = "", Customer customer = null);

        Task UpdateErpLogAsync(ErpLogs erpLog);

        Task DeleteErpLogByIdAsync(int id);

        Task<ErpLogs> GetErpLogByIdAsync(int id);

        Task<IPagedList<ErpLogs>> GetAllErpLogsAsync(string ipAddress, string message, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, int logLevelId = 0, int syncLevelId = 0, string nopCustomerEmail = null, DateTime? createdFrom = null, DateTime? createdTo = null);

        Task<IList<ErpLogs>> GetErpLogsByIdsAsync(int[] erpLogIds);

        Task InformationAsync(string message, ErpSyncLevel syncLevel, Exception exception = null, Customer customer = null);

        /// <summary>
        /// Information
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="exception">Exception</param>
        /// <param name="customer">Customer</param>
        void Information(string message, ErpSyncLevel syncLevel, Exception exception = null, Customer customer = null);

        /// <summary>
        /// Warning
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="exception">Exception</param>
        /// <param name="customer">Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task WarningAsync(string message, ErpSyncLevel syncLevel, Exception exception = null, Customer customer = null);

        /// <summary>
        /// Warning
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="exception">Exception</param>
        /// <param name="customer">Customer</param>
        void Warning(string message, ErpSyncLevel syncLevel, Exception exception = null, Customer customer = null);

        /// <summary>
        /// Error
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="exception">Exception</param>
        /// <param name="customer">Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task ErrorAsync(string message, ErpSyncLevel syncLevel, Exception exception = null, Customer customer = null);

        /// <summary>
        /// Error
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="exception">Exception</param>
        /// <param name="customer">Customer</param>
        void Error(string message, ErpSyncLevel syncLevel, Exception exception = null, Customer customer = null);
        /// <summary>
        /// Clears a log
        /// </summary>
        /// <param name="olderThan">The date that sets the restriction on deleting records. Leave null to remove all records</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task ClearLogAsync(DateTime? olderThan = null);

        Task DeleteErpLogsAsync(IList<ErpLogs> erpLogs);

    }
}

