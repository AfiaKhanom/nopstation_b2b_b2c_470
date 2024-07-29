using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public class ErpLogsService : IErpLogsService
    {
        #region Fields

        private readonly IRepository<ErpLogs> _erpLogsRepository;
        private readonly CommonSettings _commonSettings;
        private readonly IWebHelper _webHelper;
        private readonly IRepository<Customer> _customerRepository;

        #endregion

        #region ctor

        public ErpLogsService(IRepository<ErpLogs> erpLogsRepository,
            CommonSettings commonSettings,
            IWebHelper webHelper,
            IRepository<Customer> customerRepository
            )
        {
            _erpLogsRepository = erpLogsRepository;
            _commonSettings = commonSettings;
            _webHelper = webHelper;
            _customerRepository = customerRepository;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets a value indicating whether this message should not be logged
        /// </summary>
        /// <param name="message">Message</param>
        /// <returns>Result</returns>
        protected virtual bool IgnoreLog(string message)
        {
            if (!_commonSettings.IgnoreLogWordlist.Any())
                return false;

            if (string.IsNullOrWhiteSpace(message))
                return false;

            return _commonSettings
                .IgnoreLogWordlist
                .Any(x => message.Contains(x, StringComparison.InvariantCultureIgnoreCase));
        }

        public virtual bool IsEnabled(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => false,
                _ => true,
            };
        }

        public async Task ClearLogAsync(DateTime? olderThan = null)
        {
            if (olderThan == null)
                await _erpLogsRepository.TruncateAsync();
            else
                await _erpLogsRepository.DeleteAsync(p => p.CreatedOnUtc < olderThan.Value);
        }

        #endregion

        #region Methods

        #region Insert/Update

        public async Task<ErpLogs> InsertErpLogAsync(ErpLogLevel logLevel, ErpSyncLavel syncLavel, string shortMessage, string fullMessage = "", Customer customer = null)
        {
            if (IgnoreLog(shortMessage) || IgnoreLog(fullMessage))
                return null;

            var log = new ErpLogs
            {
                ErpLogLevelId = (int)logLevel,
                ShortMessage = shortMessage,
                FullMessage = fullMessage,
                ErpSyncLavelId = (int)syncLavel,
                IpAddress = _webHelper.GetCurrentIpAddress(),
                CustomerId = customer?.Id,
                PageUrl = _webHelper.GetThisPageUrl(true),
                ReferrerUrl = _webHelper.GetUrlReferrer() ?? string.Empty,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _erpLogsRepository.InsertAsync(log, false);

            return log;
        }

        public ErpLogs InsertErpLog(ErpLogLevel logLevel, ErpSyncLavel syncLavel, string shortMessage, string fullMessage = "", Customer customer = null)
        {
            if (IgnoreLog(shortMessage) || IgnoreLog(fullMessage))
                return null;

            var log = new ErpLogs
            {
                ErpLogLevelId = (int)logLevel,
                ShortMessage = shortMessage,
                FullMessage = fullMessage,
                ErpSyncLavelId = (int)syncLavel,
                IpAddress = _webHelper.GetCurrentIpAddress(),
                CustomerId = customer?.Id,
                PageUrl = _webHelper.GetThisPageUrl(true),
                ReferrerUrl = _webHelper.GetUrlReferrer(),
                CreatedOnUtc = DateTime.UtcNow
            };

            _erpLogsRepository.InsertAsync(log, false);
            return log;
        }


        public async Task UpdateErpLogAsync(ErpLogs erpLog)
        {
            await _erpLogsRepository.UpdateAsync(erpLog);
        }

        #endregion

        #region Delete

        private async Task DeleteErpLogAsync(ErpLogs erpLog)
        {
            await _erpLogsRepository.DeleteAsync(erpLog);
        }

        public async Task DeleteErpLogByIdAsync(int id)
        {
            var erpLog = await GetErpLogByIdAsync(id);
            if (erpLog != null)
            {
                await DeleteErpLogAsync(erpLog);
            }
        }

        public async Task DeleteErpLogsAsync(IList<ErpLogs> erpLogs)
        {
            await _erpLogsRepository.DeleteAsync(erpLogs, false);
        }

        #endregion

        #region Read

        public async Task<ErpLogs> GetErpLogByIdAsync(int id)
        {
            if (id == 0)
                return null;

            return await _erpLogsRepository.GetByIdAsync(id, cache => default);
        }

        public async Task<IPagedList<ErpLogs>> GetAllErpLogsAsync(string ipAddress, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, int logLevelId = 0, int syncLavelId = 0, string nopCustomerEmail = null, DateTime? createdFrom = null, DateTime? createdTo = null)
        {
            var erpLogs = await _erpLogsRepository.GetAllPagedAsync(query =>
            {
                if (createdFrom != null && createdFrom.HasValue)
                    query = query.Where(w => w.CreatedOnUtc >= createdFrom.Value);

                if (createdTo != null && createdTo.HasValue)
                    query = query.Where(w => w.CreatedOnUtc <= createdTo.Value);

                if (!string.IsNullOrEmpty(ipAddress))
                    query = query.Where(x => x.IpAddress.Contains(ipAddress));

                if (logLevelId > 0)
                    query = query.Where(x => x.ErpLogLevelId.Equals(logLevelId));

                if (syncLavelId > 0)
                    query = query.Where(x => x.ErpSyncLavelId.Equals(syncLavelId));

                if (!string.IsNullOrEmpty(nopCustomerEmail))
                {
                    query = query.Join(_customerRepository.Table, x => x.CustomerId, y => y.Id,
                            (x, y) => new { ErpActivityLogs = x, Customer = y })
                        .Where(z => z.Customer.Email.Contains(nopCustomerEmail))
                        .Select(z => z.ErpActivityLogs)
                        .Distinct();
                }

                query = query.OrderByDescending(ei => ei.Id);
                return query;

            }, pageIndex, pageSize, getOnlyTotalCount);

            return erpLogs;
        }

        public async Task<IList<ErpLogs>> GetErpLogsByIdsAsync(int[] erpLogIds)
        {
            return await _erpLogsRepository.GetByIdsAsync(erpLogIds);
        }

        public async Task InformationAsync(string message, ErpSyncLavel syncLavel, Exception exception = null, Customer customer = null)
        {

            //don't log thread abort exception
            if (exception is System.Threading.ThreadAbortException)
                return;

            if (IsEnabled(LogLevel.Information))
                await InsertErpLogAsync(ErpLogLevel.Information, syncLavel, message, exception?.ToString() ?? string.Empty, customer);
        }

        public void Information(string message, ErpSyncLavel syncLavel, Exception exception = null, Customer customer = null)
        {

            //don't log thread abort exception
            if (exception is System.Threading.ThreadAbortException)
                return;

            if (IsEnabled(LogLevel.Information))
                InsertErpLog(ErpLogLevel.Information, syncLavel, message, exception?.ToString() ?? string.Empty, customer);
        }

        public async Task WarningAsync(string message, ErpSyncLavel syncLavel, Exception exception = null, Customer customer = null)
        {

            //don't log thread abort exception
            if (exception is System.Threading.ThreadAbortException)
                return;

            if (IsEnabled(LogLevel.Warning))
                await InsertErpLogAsync(ErpLogLevel.Warning, syncLavel, message, exception?.ToString() ?? string.Empty, customer);
        }

        public void Warning(string message, ErpSyncLavel syncLavel, Exception exception = null, Customer customer = null)
        {

            //don't log thread abort exception
            if (exception is System.Threading.ThreadAbortException)
                return;

            if (IsEnabled(LogLevel.Warning))
                InsertErpLog(ErpLogLevel.Warning, syncLavel, message, exception?.ToString() ?? string.Empty, customer);
        }

        public async Task ErrorAsync(string message, ErpSyncLavel syncLavel, Exception exception = null, Customer customer = null)
        {

            //don't log thread abort exception
            if (exception is System.Threading.ThreadAbortException)
                return;

            if (IsEnabled(LogLevel.Error))
                await InsertErpLogAsync(ErpLogLevel.Error, syncLavel, message, exception?.ToString() ?? string.Empty, customer);
        }

        public void Error(string message, ErpSyncLavel syncLavel, Exception exception = null, Customer customer = null)
        {
            //don't log thread abort exception
            if (exception is System.Threading.ThreadAbortException)
                return;

            if (IsEnabled(LogLevel.Error))
                InsertErpLog(ErpLogLevel.Error, syncLavel, message, exception?.ToString() ?? string.Empty, customer);
        }


        #endregion

        #endregion
    }
}

