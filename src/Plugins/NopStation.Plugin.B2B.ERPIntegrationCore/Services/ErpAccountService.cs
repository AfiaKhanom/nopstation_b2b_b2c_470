using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Common;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public class ErpAccountService : IErpAccountService
{
    #region Fields

    private readonly IRepository<ErpAccount> _erpAccountRepository;
    private readonly IRepository<ErpSalesRepErpAccountMap> _erpSalesRepErpAccountMapRepository;
    private readonly IRepository<ErpShiptoAddressErpAccountMap> _erpShiptoAddressErpAccountMapRepository;
    private readonly INopDataProvider _nopDataProvider;
    private readonly IRepository<ErpNopUser> _erpNopUserRepository;
    private readonly IRepository<ErpNopUserAccountMap> _erpNopUserAccountMapRepository;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IRepository<Address> _addressRepository;
    private readonly IErpNopUserService _erpNopUserService;

    #endregion Fields

    #region Ctor

    public ErpAccountService(IRepository<ErpAccount> erpAccountRepository,
        IRepository<Address> addressRepository,
        IErpNopUserService erpNopUserService,
        IRepository<ErpSalesRepErpAccountMap> erpSalesRepErpAccountMapRepository,
        IRepository<ErpShiptoAddressErpAccountMap> erpShiptoAddressErpAccountMapRepository,
        INopDataProvider nopDataProvider,
        IRepository<ErpNopUser> erpNopUserRepository,
        IRepository<ErpNopUserAccountMap> erpNopUserAccountMapRepository,
        IStaticCacheManager staticCacheManager)
    {
        _erpAccountRepository = erpAccountRepository;
        _addressRepository = addressRepository;
        _erpNopUserService = erpNopUserService;
        _erpSalesRepErpAccountMapRepository = erpSalesRepErpAccountMapRepository;
        _erpShiptoAddressErpAccountMapRepository = erpShiptoAddressErpAccountMapRepository;
        _nopDataProvider = nopDataProvider;
        _erpNopUserRepository = erpNopUserRepository;
        _erpNopUserAccountMapRepository = erpNopUserAccountMapRepository;
        _staticCacheManager = staticCacheManager;
    }

    #endregion Ctor

    #region Methods

    #region Insert/Update

    public async Task InsertErpAccountAsync(ErpAccount erpAccount)
    {
        await _erpAccountRepository.InsertAsync(erpAccount);
    }

    public async Task InsertErpAccountsAsync(List<ErpAccount> erpAccounts)
    {
        await _erpAccountRepository.InsertAsync(erpAccounts);
    }

    public async Task InsertSalesRepErpAccountAsync(ErpSalesRepErpAccountMap erpSalesRepErpAccount)
    {
        await _erpSalesRepErpAccountMapRepository.InsertAsync(erpSalesRepErpAccount);
    }

    public async Task UpdateErpAccountAsync(ErpAccount erpAccount)
    {
        await _erpAccountRepository.UpdateAsync(erpAccount);
    }

    public async Task UpdateErpAccountsAsync(List<ErpAccount> erpAccounts)
    {
        await _erpAccountRepository.UpdateAsync(erpAccounts);
    }

    #endregion Insert/Update

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

    #endregion Delete

    #region Read

    /// <summary>
    /// Gets an ErpAccount by Id
    /// </summary>
    /// <param name="id">ErpAccount identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the ErpAccount
    /// </returns>
    public async Task<ErpAccount> GetErpAccountByIdAsync(int id, bool filterOutDeleted = true)
    {
        if (id == 0)
            return null;

        var key = _staticCacheManager.PrepareKeyForDefaultCache(ERPIntegrationCoreDefaults.ErpAccountByIdCacheKey, id, filterOutDeleted);

        var query = _erpAccountRepository.Table.Where(x => x.Id == id);

        if (filterOutDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        return await _staticCacheManager.GetAsync(key, async () => await query.FirstOrDefaultAsync());
    }

    public async Task<ErpAccount> GetErpAccountByErpShipToAddressAsync(ErpShipToAddress erpShipToAddress)
    {
        if (erpShipToAddress == null)
            return null;

        var key = _staticCacheManager.PrepareKeyForDefaultCache(ERPIntegrationCoreDefaults.ErpAccountByErpShipToAddressCacheKey, erpShipToAddress.Id);

        var query = from erpAccount in _erpAccountRepository.Table
                    join cam in _erpShiptoAddressErpAccountMapRepository.Table on erpAccount.Id equals cam.ErpAccountId
                    where cam.ErpShiptoAddressId == erpShipToAddress.Id && !erpAccount.IsDeleted
                    select erpAccount;

        return await _staticCacheManager.GetAsync(key, async () => await query.FirstOrDefaultAsync());
    }

    public async Task<ErpSalesRepErpAccountMap> GetErpSalesRepErpAccountMapByIdAsync(int salesRepId, int? erpAccountId)
    {
        if (salesRepId <= 0 || (erpAccountId.HasValue && erpAccountId <= 0))
        {
            return null;
        }

        var key = _staticCacheManager.PrepareKeyForDefaultCache(
            ERPIntegrationCoreDefaults.ErpSalesRepErpAccountMapByIdsCacheKey,
            salesRepId,
            erpAccountId ?? 0
        );

        return await _staticCacheManager.GetAsync(key, async () =>
            await _erpSalesRepErpAccountMapRepository.Table
                .FirstOrDefaultAsync(x => x.ErpSalesRepId == salesRepId && x.ErpAccountId == erpAccountId)
        );
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

        var key = _staticCacheManager.PrepareKeyForDefaultCache(ERPIntegrationCoreDefaults.ErpAccountByIdWithActiveCacheKey, id);

        var query = _erpAccountRepository.Table.FirstOrDefaultAsync(x => x.Id == id && x.IsActive && !x.IsDeleted);

        return await _staticCacheManager.GetAsync(key, async () => await query);
    }

    public async Task<IList<ErpAccount>> GetAllErpAccountsAsync()
    {
        var key = ERPIntegrationCoreDefaults.ErpAccountAllActiveCacheKey;

        var query = _erpAccountRepository.Table.Where(v => v.IsActive && !v.IsDeleted).OrderBy(ea => ea.Id);

        return await _staticCacheManager.GetAsync(key, async () => await query.ToListAsync());
    }

    public async Task<IPagedList<ErpAccount>> GetAllErpAccountsAsync(int pageIndex = 0,
        int pageSize = int.MaxValue,
        bool? showHidden = null,
        bool getOnlyTotalCount = false,
        string erpAccountNo = null,
        int salesOrgId = 0,
        string email = null,
        string accountName = null,
        int erpAccountStatusTypeId = 0,
        bool filterDeleted = true)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(
            ERPIntegrationCoreDefaults.ErpAccountPagedCacheKey,
            pageIndex, pageSize, showHidden, getOnlyTotalCount, erpAccountNo ?? "", salesOrgId, email ?? "", accountName ?? "", erpAccountStatusTypeId, filterDeleted
        );

        return await _staticCacheManager.GetAsync(key, async () =>
            await _erpAccountRepository.GetAllPagedAsync(query =>
            {
                if (filterDeleted)
                    query = query.Where(v => !v.IsDeleted);

                if (showHidden.HasValue)
                {
                    if (!showHidden.Value)
                        query = query.Where(v => v.IsActive);
                    else
                        query = query.Where(v => !v.IsActive);
                }

                if (erpAccountStatusTypeId > 0)
                    query = query.Where(c => c.ErpAccountStatusTypeId.Equals(erpAccountStatusTypeId));

                if (!string.IsNullOrWhiteSpace(erpAccountNo))
                    query = query.Where(c => c.AccountNumber.Contains(erpAccountNo));

                if (salesOrgId > 0)
                    query = query.Where(c => c.ErpSalesOrgId.Equals(salesOrgId));

                if (!string.IsNullOrWhiteSpace(accountName))
                    query = query.Where(c => c.AccountName.Contains(accountName));

                if (!string.IsNullOrWhiteSpace(email))
                {
                    query = query.Join(_addressRepository.Table, x => x.BillingAddressId, y => y.Id,
                            (x, y) => new { ErpAccount = x, Address = y })
                        .Where(z => z.Address.Email.Contains(email))
                        .Select(z => z.ErpAccount)
                        .Distinct();
                }

                query = query.OrderByDescending(ea => ea.CreatedOnUtc);
                return query;
            }, pageIndex, pageSize, getOnlyTotalCount)
        );
    }

    public async Task<IList<ErpAccount>> GetErpAccountListAsync(string accountNumber = null,
        string accountName = null,
        string email = null,
        int salesOrgId = 0,
        int erpAccountStatusTypeId = 0,
        bool filterDeleted = true,
        bool? showHidden = null)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(
            ERPIntegrationCoreDefaults.ErpAccountListCacheKey,
            accountNumber ?? "", accountName ?? "", email ?? "", salesOrgId, erpAccountStatusTypeId, filterDeleted, showHidden
        );

        return await _staticCacheManager.GetAsync(key, async () =>
            await _erpAccountRepository.GetAllAsync(query =>
            {
                if (filterDeleted)
                    query = query.Where(v => !v.IsDeleted);

                if (showHidden.HasValue)
                {
                    if (!showHidden.Value)
                        query = query.Where(v => v.IsActive);
                    else
                        query = query.Where(v => !v.IsActive);
                }

                if (erpAccountStatusTypeId > 0)
                    query = query.Where(c => c.ErpAccountStatusTypeId.Equals(erpAccountStatusTypeId));

                if (!string.IsNullOrWhiteSpace(accountNumber))
                    query = query.Where(c => c.AccountNumber.Contains(accountNumber));

                if (salesOrgId > 0)
                    query = query.Where(c => c.ErpSalesOrgId.Equals(salesOrgId));

                if (!string.IsNullOrWhiteSpace(accountName))
                    query = query.Where(c => c.AccountName.Contains(accountName));

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
            })
        );
    }

    public async Task<IList<ErpSalesRepErpAccountMap>> GetAllErpAccountsBySalesRepIdAsync(string erpSalesRepId = null)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(
            ERPIntegrationCoreDefaults.ErpSalesRepErpAccountMapBySalesRepIdCacheKey,
            erpSalesRepId ?? ""
        );

        return await _staticCacheManager.GetAsync(key, async () =>
            await _erpSalesRepErpAccountMapRepository.GetAllAsync(query =>
            {
                if (!string.IsNullOrWhiteSpace(erpSalesRepId))
                    query = query.Where(c => c.ErpSalesRepId.Equals(Convert.ToInt32(erpSalesRepId)));

                query = query.OrderBy(ea => ea.Id);
                return query;
            })
        );
    }

    public async Task<IPagedList<ErpAccount>> GetAllErpAccountsByIdsAsync(int pageIndex = 0,
        int pageSize = int.MaxValue,
        bool showHidden = false,
        bool getOnlyTotalCount = false,
        List<int> accountIds = null,
        string email = "")
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(
            ERPIntegrationCoreDefaults.ErpAccountPagedByIdsCacheKey,
            pageIndex, pageSize, showHidden, getOnlyTotalCount, accountIds != null ? string.Join(",", accountIds) : "", email ?? ""
        );

        return await _staticCacheManager.GetAsync(key, async () =>
            await _erpAccountRepository.GetAllPagedAsync(query =>
            {
                query = query.Where(c => !c.IsDeleted);

                if (!showHidden)
                    query = query.Where(v => v.IsActive);

                if (accountIds != null && accountIds.Any())
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
            }, pageIndex, pageSize, getOnlyTotalCount)
        );
    }

    public async Task<IList<ErpAccount>> GetAllErpAccountsByIdsAsync(bool showHidden = false,
        List<int> erpAccountIds = null,
        string email = "")
    {
        return await _erpAccountRepository.GetAllAsync(query =>
        {
            query = query.Where(c => !c.IsDeleted);

            if (!showHidden)
                query = query.Where(v => v.IsActive);

            if (erpAccountIds != null && erpAccountIds.Any())
                query = query.Where(c => erpAccountIds.Contains(c.Id));

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
        });
    }

    public async Task<IList<ErpAccount>> GetErpAccountsOfOnlyActiveErpNopUsersAsync(int salesOrgId = 0, string accountNumber = "")
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(
            ERPIntegrationCoreDefaults.ErpAccountOfActiveNopUsersCacheKey,
            salesOrgId, accountNumber ?? ""
        );

        return await _staticCacheManager.GetAsync(key, async () =>
        {
            var erpAccountQuery = _erpAccountRepository.Table.Where(ea => ea.IsActive && !ea.IsDeleted);

            if (salesOrgId > 0)
            {
                erpAccountQuery = erpAccountQuery.Where(ea => ea.ErpSalesOrgId == salesOrgId);
            }

            if (!string.IsNullOrWhiteSpace(accountNumber))
            {
                erpAccountQuery = erpAccountQuery.Where(ea => ea.AccountNumber.Contains(accountNumber));
            }

            var query = from ea in erpAccountQuery
                        join eaMap in _erpNopUserAccountMapRepository.Table on ea.Id equals eaMap.ErpAccountId
                        join enu in _erpNopUserRepository.Table on eaMap.ErpUserId equals enu.Id
                        where !enu.IsDeleted && enu.IsActive
                        select ea;

            return await query.Distinct().ToListAsync();
        });
    }

    public async Task<ErpAccount> GetErpAccountByErpAccountNumberAsync(string accountNumber)
    {
        if (string.IsNullOrEmpty(accountNumber))
            return null;

        var key = _staticCacheManager.PrepareKeyForDefaultCache(
            ERPIntegrationCoreDefaults.ErpAccountByAccountNumberCacheKey,
            accountNumber
        );

        var query = from c in _erpAccountRepository.Table
                    where c.AccountNumber == accountNumber && !c.IsDeleted
                    orderby c.Id
                    select c;

        return await _staticCacheManager.GetAsync(key, async () => await query.FirstOrDefaultAsync());
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

        var connectionString = new SqlConnectionStringBuilder(DataSettingsManager.LoadSettings().ConnectionString);

        var sqlCommand = $"Update [{connectionString.InitialCatalog}].[dbo].[Erp_Account] Set [IsActive] = 0 Where [UpdatedOnUtc] < '{syncStartTime:yyyy-MM-dd HH:mm:ss}'";

        await _nopDataProvider.ExecuteNonQueryAsync(sqlCommand);
    }

    public async Task<IList<ErpAccount>> GetAllErpAccountsBySaleOrgIdAsync(int salesOrgId)
    {
        var key = _staticCacheManager.PrepareKeyForDefaultCache(
            ERPIntegrationCoreDefaults.ErpAccountBySalesOrgIdCacheKey,
            salesOrgId
        );

        if (salesOrgId < 1)
            return new List<ErpAccount>();

        return await _staticCacheManager.GetAsync(key, async () =>
            await _erpAccountRepository.Table.Where(x => x.ErpSalesOrgId == salesOrgId && x.IsActive && !x.IsDeleted).ToListAsync()
        );
    }

    public async Task<ErpAccount> CheckErpAccountExist(string accountNumber, int salesOrgId)
    {
        return await _erpAccountRepository.Table
            .Where(a => a.AccountNumber == accountNumber
                        && a.ErpSalesOrgId == salesOrgId)
            .FirstOrDefaultAsync();
    }

    #endregion Read

    #endregion Methods
}