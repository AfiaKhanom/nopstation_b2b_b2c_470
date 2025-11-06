using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using LinqToDB;
using Nop.Core.Caching;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public class ErpNopUserAccountMapService : IErpNopUserAccountMapService
{
    #region Fields

    private readonly IRepository<ErpNopUserAccountMap> _erpNopUserAccountMapRepository;
    private readonly IStaticCacheManager _staticCacheManager;

    #endregion

    #region Ctor

    public ErpNopUserAccountMapService(IRepository<ErpNopUserAccountMap> erpNopUserAccountMapRepository,
        IStaticCacheManager staticCacheManager)
    {
        _erpNopUserAccountMapRepository = erpNopUserAccountMapRepository;
        _staticCacheManager = staticCacheManager;
    }

    #endregion

    #region Methods

    #region Insert/Update

    public async Task InsertErpNopUserAccountMapAsync(ErpNopUserAccountMap erpNopUserAccountMap)
    {
        await _erpNopUserAccountMapRepository.InsertAsync(erpNopUserAccountMap);
    }

    public async Task UpdateErpNopUserAccountMapAsync(ErpNopUserAccountMap erpNopUserAccountMap)
    {
        await _erpNopUserAccountMapRepository.UpdateAsync(erpNopUserAccountMap);
    }

    #endregion

    #region Delete

    private async Task DeleteErpNopUserAccountMapAsync(ErpNopUserAccountMap erpNopUserAccountMap)
    {
        await _erpNopUserAccountMapRepository.DeleteAsync(erpNopUserAccountMap);
    }

    public async Task DeleteErpNopUserAccountMapByIdAsync(int id)
    {
        var erpNopUserAccountMap = await GetErpNopUserAccountMapByIdAsync(id);
        if (erpNopUserAccountMap != null)
        {
            await DeleteErpNopUserAccountMapAsync(erpNopUserAccountMap);
        }
    }

    #endregion

    #region Read

    public async Task<ErpNopUserAccountMap> GetErpNopUserAccountMapByIdAsync(int id)
    {
        if (id == 0)
            return null;

        return await _erpNopUserAccountMapRepository.GetByIdAsync(id, cache => default);
    }

    public async Task<IList<ErpNopUserAccountMap>> GetAllErpNopUserAccountMapsAsync(List<int> erpAccountIds = null,
        List<int> customerRoleIds = null,
        List<int> erpNopUserIds = null,
        int erpNopUserTypeId = 0)
    {
        return await _erpNopUserAccountMapRepository.GetAllAsync(query =>
        {
            if (erpAccountIds != null && erpAccountIds.Count != 0)
                query = query.Where(x => erpAccountIds.Contains(x.ErpAccountId));

            if (customerRoleIds != null && customerRoleIds.Count != 0)
                query = query.Where(x => !string.IsNullOrWhiteSpace(x.CustomerRolesIds) &&
                    customerRoleIds.Any(roleId => $",{x.CustomerRolesIds},".Contains($",{roleId},")));

            if (erpNopUserIds != null && erpNopUserIds.Count != 0)
                query = query.Where(x => erpNopUserIds.Contains(x.ErpUserId));

            if (erpNopUserTypeId != 0)
                query = query.Where(x => x.ErpUserTypeId == erpNopUserTypeId);

            query = query.OrderBy(ei => ei.Id);
            return query;
        });
    }

    public async Task<ErpNopUserAccountMap> GetErpNopUserAccountMapByAccountAndUserIdAsync(int accountId, int userId)
    {
        if (accountId == 0 || userId == 0)
            return null;

        var key = _staticCacheManager.PrepareKeyForDefaultCache(ERPIntegrationCoreDefaults.ErpNopUserAccountMapByErpAccountAndErpUserCacheKey, 
            accountId, userId);

        var query = _erpNopUserAccountMapRepository.Table;

        return await _staticCacheManager.GetAsync(key, 
            async () => await query.FirstOrDefaultAsync(e => e.ErpAccountId == accountId && e.ErpUserId == userId));
    }

    public async Task<IList<ErpNopUserAccountMap>> GetAllErpNopUserAccountMapsByUserIdAsync(int userId)
    {
        if (userId == 0)
            return null;

        var erpNopUserAccountMaps = await _erpNopUserAccountMapRepository.GetAllAsync(query =>
        {
            return from eam in query
                   where eam.ErpUserId == userId
                   orderby eam.ErpAccountId
                   select eam;
        }, cache => cache.PrepareKeyForDefaultCache(ERPIntegrationCoreDefaults.ErpNopUserAccountMapByErpUserCacheKey, userId));

        return erpNopUserAccountMaps;
    }

    public async Task<IList<ErpNopUserAccountMap>> GetAllErpNopUserAccountMapsByAccountIdAsync(int accountId)
    {
        if (accountId == 0)
            return null;

        var erpNopUserAccountMaps = await _erpNopUserAccountMapRepository.GetAllAsync(query =>
        {
            return from eam in query
                   where eam.ErpAccountId == accountId
                   orderby eam.ErpAccountId
                   select eam;
        }, cache => cache.PrepareKeyForDefaultCache(ERPIntegrationCoreDefaults.ErpNopUserAccountMapByErpAccountCacheKey, accountId));

        return erpNopUserAccountMaps;
    }

    public async Task<bool> CheckAnyErpNopUserAccountMapExistWithAccountIdAndUserIdAsync(int erpAccountId, int erpUserId)
    {
        if (erpAccountId == 0 || erpUserId == 0)
            return false;

        return await GetErpNopUserAccountMapByAccountAndUserIdAsync(erpAccountId, erpUserId) != null;
    }

    public async Task<IList<int>> GetErpNopUserRolesByErpNopUserAsync(ErpNopUser user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var listOferpNopUserRoleIds = new List<int>();

        var erpNopUserAccountMap = await GetErpNopUserAccountMapByAccountAndUserIdAsync(user.ErpAccountId, user.Id);
        if (erpNopUserAccountMap == null)
            return listOferpNopUserRoleIds;

        var erpNopUserRoleIds = (erpNopUserAccountMap.CustomerRolesIds ?? string.Empty).Split(",");

        foreach (var roleId in erpNopUserRoleIds)
        {
            if (!string.IsNullOrEmpty(roleId))
                listOferpNopUserRoleIds.Add(Convert.ToInt32(roleId));
        }

        return listOferpNopUserRoleIds;
    }

    #endregion

    #endregion
}

