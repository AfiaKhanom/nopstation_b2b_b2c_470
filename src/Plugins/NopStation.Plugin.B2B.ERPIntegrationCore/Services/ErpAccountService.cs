using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public class ErpAccountService : IErpAccountService
    {
        #region Fields

        private readonly IRepository<ErpAccount> _erpAccountRepository;
        private readonly IRepository<ErpSalesRepErpAccountMap> _erpSalesRepErpAccountMapRepository;
        private readonly IRepository<ErpShiptoAddressErpAccountMap> _erpShiptoAddressErpAccountMapRepository;
        private readonly IRepository<ErpOrderAdditionalData> _erpOrderAdditionalRepository;
        private readonly IRepository<Address> _addressRepository;
        private readonly IErpNopUserService _erpNopUserService;

        #endregion

        #region ctor

        public ErpAccountService(IRepository<ErpAccount> erpAccountRepository,
            IRepository<ErpOrderAdditionalData> erpOrderAdditionalRepository,
            IRepository<Address> addressRepository,
            IErpNopUserService erpNopUserService,
            IRepository<ErpSalesRepErpAccountMap> erpSalesRepErpAccountMapRepository,
            IRepository<ErpShiptoAddressErpAccountMap> erpShiptoAddressErpAccountMapRepository)
        {
            _erpAccountRepository = erpAccountRepository;
            _erpOrderAdditionalRepository = erpOrderAdditionalRepository;
            _addressRepository = addressRepository;
            _erpNopUserService = erpNopUserService;
            _erpSalesRepErpAccountMapRepository = erpSalesRepErpAccountMapRepository;
            _erpShiptoAddressErpAccountMapRepository = erpShiptoAddressErpAccountMapRepository;
        }

        #endregion

        #region Methods

        #region Insert/Update

        public async Task InsertErpAccountAsync(ErpAccount erpAccount)
        {
            await _erpAccountRepository.InsertAsync(erpAccount);
        }


        public async Task InsertSalesRepErpAccountAsync(ErpSalesRepErpAccountMap erpSalesRepErpAccount)
        {
            await _erpSalesRepErpAccountMapRepository.InsertAsync(erpSalesRepErpAccount);
        }

        public async Task UpdateErpAccountAsync(ErpAccount erpAccount)
        {
            await _erpAccountRepository.UpdateAsync(erpAccount);
        }

        #endregion

        #region Delete

        private async Task DeleteErpAccountAsync(ErpAccount erpAccount)
        {
            //as ErpBaseEntity dosen't inherit ISoftDelete but has that feature
            erpAccount.IsDeleted = true;
            await _erpAccountRepository.UpdateAsync(erpAccount);
        }

        public async Task DeleteErpAccountByIdAsync(int id)
        {
            var erpAccount = await GetErpAccountByIdAsync(id);
            if (erpAccount != null)
            {
                await DeleteErpAccountAsync(erpAccount);
            }
        }

        public async Task DeleteErpSalesRepErpAccountMapAsync(ErpSalesRepErpAccountMap salesRepErpAccountMap)
        {
            await _erpSalesRepErpAccountMapRepository.DeleteAsync(salesRepErpAccountMap);
        }

        #endregion

        #region Read

        /// <summary>
        /// Gets an ErpAccount by Id
        /// </summary>
        /// <param name="id">ErpAccount identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ErpAccount
        /// </returns>
        public async Task<ErpAccount> GetErpAccountByIdAsync(int id)
        {
            if (id == 0)
                return null;

            return await _erpAccountRepository.GetByIdAsync(id, cache => default);
        }

        public async Task<ErpAccount> GetErpAccountByErpShipToAddressAsync(ErpShipToAddress erpShipToAddress)
        {
            if (erpShipToAddress == null)
                return null;
            var query = from erpAccount in _erpAccountRepository.Table
                    join cam in _erpShiptoAddressErpAccountMapRepository.Table on erpAccount.Id equals cam.ErpAccountId
                    where cam.ErpShiptoAddressId == erpShipToAddress.Id
                    select erpAccount;

            return query.FirstOrDefault();
        }


        public async Task<ErpSalesRepErpAccountMap> GetErpSalesRepErpAccountMapByIdAsync(int salesRepId, int? erpAccountId)
        {
            // Check if both ids are provided and valid
            if (salesRepId <= 0 || (erpAccountId.HasValue && erpAccountId <= 0))
            {
                return null;
            }
            // Fetch the record based on salesRepId and erpAccountId
            var record = await _erpSalesRepErpAccountMapRepository.Table.FirstOrDefaultAsync(x => x.ErpSalesRepId == salesRepId && x.ErpAccountId == erpAccountId);
            return record;
        }




        /// <summary>
        /// Gets an ErpAccount by Id if it is active
        /// </summary>
        /// <param name="id">ErpAccount identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ErpAccount if it is activ
        /// </returns>
        public async Task<ErpAccount> GetErpAccountByIdWithActiveAsync(int id)
        {
            if (id == 0)
                return null;

            var erpAccount = await _erpAccountRepository.GetByIdAsync(id, cache => default);

            if (erpAccount == null || !erpAccount.IsActive)
                return null;

            return erpAccount;
        }

        /// <summary>
        /// Gets all ErpAccounts
        /// </summary>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">If only total no of account needed or not</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains all the ErpAccounts
        /// </returns>
        public async Task<IPagedList<ErpAccount>> GetAllErpAccountsAsync(int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool? showHidden = null,
            bool getOnlyTotalCount = false,
            string erpAccontNo = null,
            int salesOrgId = 0,
            string email = null,
            string accountName = null,
            int erpAccountStatusTypeId = 0)
        {
            var erpAccounts = await _erpAccountRepository.GetAllPagedAsync(query =>
            {
                // showHidden is null for getting all, true for only actives and false for only inactives
                if (showHidden.HasValue)
                {
                    if (!showHidden.Value)
                        query = query.Where(v => v.IsActive == true);
                    else
                        query = query.Where(v => v.IsActive == false);
                }

                if (erpAccountStatusTypeId > 0)
                    query = query.Where(c => c.ErpAccountStatusTypeId.Equals(erpAccountStatusTypeId));

                if (!string.IsNullOrWhiteSpace(erpAccontNo))
                    query = query.Where(c => c.AccountNumber.Contains(erpAccontNo));

                if (salesOrgId > 0)
                    query = query.Where(c => c.ErpSalesOrgId.Equals(salesOrgId));

                if (!string.IsNullOrWhiteSpace(accountName))
                    query = query.Where(c => c.AccountName.Contains(accountName));

                query = query.Where(v => !v.IsDeleted);

                if (!string.IsNullOrWhiteSpace(email))
                {
                    query = query.Join(_addressRepository.Table, x => x.BillingAddressId, y => y.Id,
                            (x, y) => new { ErpAccount = x, Address = y })
                        .Where(z => z.Address.Email.Contains(email))
                        .Select(z => z.ErpAccount)
                        .Distinct();
                }

                query = query.OrderBy(ea => ea.Id);
                return query;

            }, pageIndex, pageSize, getOnlyTotalCount);

            return erpAccounts;
        }




        //public async Task<IPagedList<ErpSalesRepErpAccountMap>> GetAllErpAccountsBySalesRepIdAsync(int pageIndex = 0,
        //    int pageSize = int.MaxValue,
        //    string erpSalesRepId = null,
        //    bool getOnlyTotalCount = false)
        //{
        //    var erpAccounts = await _erpSalesRepErpAccountMapRepository.GetAllPagedAsync(query =>
        //    {
        //        if (!string.IsNullOrWhiteSpace(erpSalesRepId))
        //            query = query.Where(c => c.ErpSalesRepId.Equals(Convert.ToInt32(erpSalesRepId)));

        //        query = query.OrderBy(ea => ea.Id);
        //        return query;

        //    }, pageIndex, pageSize, getOnlyTotalCount);

        //    return erpAccounts;
        //}

        public async Task<IList<ErpSalesRepErpAccountMap>> GetAllErpAccountsBySalesRepIdAsync(string erpSalesRepId = null)
        {
            var erpAccounts = await _erpSalesRepErpAccountMapRepository.GetAllAsync(query =>
            {
                if (!string.IsNullOrWhiteSpace(erpSalesRepId))
                    query = query.Where(c => c.ErpSalesRepId.Equals(Convert.ToInt32(erpSalesRepId)));

                query = query.OrderBy(ea => ea.Id);
                return query;

            });

            return erpAccounts;
        }




        /// <summary>
        /// Gets all ErpAccounts
        /// </summary>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">If only total no of account needed or not</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains all the ErpAccounts
        /// </returns>
        public async Task<IPagedList<ErpAccount>> GetAllErpAccountsByIdsAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false,
            bool getOnlyTotalCount = false, List<int> accountIds = null, string email = "")
        {
            var erpAccounts = await _erpAccountRepository.GetAllPagedAsync(query =>
            {
                if (!showHidden)
                    query = query.Where(v => v.IsActive);

                query = query.Where(c => accountIds.Contains(c.Id));

                if (!string.IsNullOrWhiteSpace(email))
                {
                    query = query.Join(_addressRepository.Table, x => x.BillingAddressId, y => y.Id,
                            (x, y) => new { ErpAccount = x, Address = y })
                        .Where(z => z.Address.Email.Contains(email))
                        .Select(z => z.ErpAccount)
                        .Distinct();
                }

                query = query.OrderBy(ea => ea.Id);
                return query;

            }, pageIndex, pageSize, getOnlyTotalCount);

            return erpAccounts;
        }

        public async Task<IList<ErpAccount>> GetAllErpAccountsAsync()
        {
            var erpAccounts = await _erpAccountRepository.GetAllAsync(query =>
            {
                query = query.Where(v => v.IsActive && !v.IsDeleted);
                query = query.OrderBy(ea => ea.Id);
                return query;
            });

            return erpAccounts;
        }

        public async Task<ErpAccount> GetErpAccountByErpAccountNumberAsync(string accountNumber)
        {
            if (string.IsNullOrEmpty(accountNumber))
                return null;


            var query = from c in _erpAccountRepository.Table
                        where c.AccountNumber == accountNumber
                        orderby c.Id
                        select c;
            return await query.FirstOrDefaultAsync();
        }

        public async Task<ErpAccount> GetActiveErpAccountByCustomerIdAsync(int customerId)
        {
            var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customerId);
            if (erpNopUser != null && !erpNopUser.IsDeleted && erpNopUser.IsActive)
            {
                var erpAccount = await GetErpAccountByIdWithActiveAsync(erpNopUser.ErpAccountId);
                if (erpAccount != null)
                    return erpAccount;
            }

            return null;
        }

        public async Task InActiveAllOldAccount(DateTime syncStartTime)
        {
            if (syncStartTime == DateTime.MinValue)
                return;
            var erpAccounts = await _erpAccountRepository.GetAllAsync(query =>
            {
                query = query.Where(a => a.UpdatedOnUtc < syncStartTime); 
                return query;
            });
            foreach ( var erpAccount in erpAccounts)
            {
                erpAccount.IsActive = false;
            }
            await _erpAccountRepository.UpdateAsync(erpAccounts);
        }


        #endregion

        #endregion
    }
}

