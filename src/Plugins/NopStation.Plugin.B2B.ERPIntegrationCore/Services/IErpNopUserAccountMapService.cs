using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public interface IErpNopUserAccountMapService
{
    Task InsertErpNopUserAccountMapAsync(ErpNopUserAccountMap erpNopUserAccountMap);

    Task UpdateErpNopUserAccountMapAsync(ErpNopUserAccountMap erpNopUserAccountMap);

    Task DeleteErpNopUserAccountMapByIdAsync(int id);

    Task<ErpNopUserAccountMap> GetErpNopUserAccountMapByIdAsync(int id);

    Task<IList<ErpNopUserAccountMap>> GetAllErpNopUserAccountMapsByUserIdAsync(int userId);

    Task<IList<ErpNopUserAccountMap>> GetAllErpNopUserAccountMapsByAccountIdAsync(int accountId);

    Task<IList<ErpNopUserAccountMap>> GetAllErpNopUserAccountMapsAsync(List<int> erpAccountIds = null, 
        List<int> customerRoleIds = null, 
        List<int> erpNopUserIds = null, 
        int erpNopUserTypeId = 0);

    Task<ErpNopUserAccountMap> GetErpNopUserAccountMapByAccountAndUserIdAsync(int accountId, int userId);

    Task<bool> CheckAnyErpNopUserAccountMapExistWithAccountIdAndUserIdAsync(int erpAccountId, int erpUserId);

    Task<IList<int>> GetErpNopUserRolesByErpNopUserAsync(ErpNopUser user);
}

