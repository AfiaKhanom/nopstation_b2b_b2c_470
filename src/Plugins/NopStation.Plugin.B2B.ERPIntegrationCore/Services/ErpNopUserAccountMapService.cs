using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using LinqToDB;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public class ErpNopUserAccountMapService : IErpNopUserAccountMapService
    {
        #region Fields

        private readonly IRepository<ErpNopUserAccountMap> _erpNopUserAccountMapRepository;

        #endregion

        #region ctor

        public ErpNopUserAccountMapService(IRepository<ErpNopUserAccountMap> erpNopUserAccountMapRepository)
        {
            _erpNopUserAccountMapRepository = erpNopUserAccountMapRepository;
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

        public async Task<IPagedList<ErpNopUserAccountMap>> GetAllErpNopUserAccountMapsAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            var erpNopUserAccountMaps = await _erpNopUserAccountMapRepository.GetAllPagedAsync(query =>
            {
                query = query.OrderBy(ei => ei.Id);
                return query;

            }, pageIndex, pageSize, getOnlyTotalCount);

            return erpNopUserAccountMaps;
        }

        public async Task<ErpNopUserAccountMap> GetErpNopUserAccountMapByAccountAndUserIdAsync(int accountId, int userId)
        {
            var erpNopUserAccountMaps = _erpNopUserAccountMapRepository.Table.Where(e => e.ErpAccountId == accountId && e.ErpUserId == userId).FirstOrDefault();

            return erpNopUserAccountMaps;
        }

        public async Task<IList<ErpNopUserAccountMap>> GetAllErpNopUserAccountMapsByUserIdAsync(int userId)
        {
            var erpNopUserAccountMaps = await _erpNopUserAccountMapRepository.GetAllAsync(query =>
            {
                query = query.Where(eam => eam.ErpUserId == userId);
                query = query.OrderBy(eam => eam.ErpAccountId);
                return query;
            });

            return erpNopUserAccountMaps;
        }

        public async Task<IList<ErpNopUserAccountMap>> GetAllErpNopUserAccountMapsByAccountIdAsync(int accountId)
        {
            var erpNopUserAccountMaps = await _erpNopUserAccountMapRepository.GetAllAsync(query =>
            {
                query = query.Where(eam => eam.ErpAccountId == accountId);
                query = query.OrderBy(eam => eam.ErpAccountId);
                return query;
            });

            return erpNopUserAccountMaps;
        }

        public async Task<IList<int>> GetErpNopUserRolesByAsync(ErpNopUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            var listOferpNopUserRoleIds = new List<int>();
            var erpNopUserRoleIdss = _erpNopUserAccountMapRepository.Table.Where(w => w.ErpUserId == user.Id && w.ErpAccountId == user.ErpAccountId).ToList();
            var erpNopUserRoleIdsString = "";

            if (erpNopUserRoleIdss.Any())
                erpNopUserRoleIdsString = erpNopUserRoleIdss.FirstOrDefault().CustomerRolesIds;

            var erpNopUserRoleIds = erpNopUserRoleIdsString.Split(",");

            foreach (var roleId in erpNopUserRoleIds)
            {
                if(!string.IsNullOrEmpty(roleId))
                    listOferpNopUserRoleIds.Add(Convert.ToInt32(roleId));
            }

            return listOferpNopUserRoleIds;
        }
        public async Task<bool> CheckAnyErpNopUserAccountMapExistWithAccountIdAndUserIdAsync(int erpAccountId, int erpUserId)
        {
            if (erpAccountId == 0 || erpUserId == 0)
                return false;

            var query = _erpNopUserAccountMapRepository.Table;

            return query.Any(b => b.ErpAccountId == erpAccountId && b.ErpUserId == erpUserId);
        }

        #endregion

        #endregion
    }
}

