using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
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

    Task<IPagedList<ErpNopUserAccountMap>> GetAllErpNopUserAccountMapsAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);

    Task<ErpNopUserAccountMap> GetErpNopUserAccountMapByAccountAndUserIdAsync(int accountId, int userId);

    Task<bool> CheckAnyErpNopUserAccountMapExistWithAccountIdAndUserIdAsync(int erpAccountId, int erpUserId);
}

