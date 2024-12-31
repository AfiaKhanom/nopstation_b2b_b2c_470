using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public interface IErpNopUserService
    {
        Task InsertErpNopUserAsync(ErpNopUser erpNopUser);

        Task UpdateErpNopUserAsync(ErpNopUser erpNopUser);

        Task DeleteErpNopUserByIdAsync(int id);

        Task<ErpNopUser> GetErpNopUserByIdAsync(int id);

        Task<ErpNopUser> GetErpNopUserByIdWithActiveAsync(int id);

        Task<IPagedList<ErpNopUser>> GetAllErpNopUsersAsync(int pageIndex = 0, int pageSize = int.MaxValue, 
            bool? showHidden = null, 
            bool getOnlyTotalCount = false,  
            string email = null, 
            string name = null, 
            int accountId = 0, 
            int userType = 0, 
            int salesOrgId = 0, 
            int erpShipToAddressId = 0);

        Task<ErpNopUser> GetErpNopUserByCustomerIdAsync(int customerId);

        Task<ErpNopUser> GetErpNopUserByCustomerIdAsync(int customerId, int erpAccountId = 0);

        Task<IList<ErpNopUser>> GetAllErpNopUserByAccountIdAsync(int accountId, bool showHidden = false);

        Task<IList<int>> GetAllCustomersByOnlyTheseRoleIdsAsync(int id);

    }
}

