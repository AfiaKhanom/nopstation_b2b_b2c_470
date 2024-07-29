using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public interface IErpAccountService
    {
        Task InsertErpAccountAsync(ErpAccount erpAccount);
        Task InsertSalesRepErpAccountAsync(ErpSalesRepErpAccountMap erpSalesRepErpAccount);

        Task UpdateErpAccountAsync(ErpAccount erpAccount);

        Task DeleteErpAccountByIdAsync(int id);
        Task DeleteErpSalesRepErpAccountMapAsync(ErpSalesRepErpAccountMap salesRepErpAccountMap);

        Task<ErpAccount> GetErpAccountByIdAsync(int id);
        Task<ErpSalesRepErpAccountMap> GetErpSalesRepErpAccountMapByIdAsync(int salesRepId, int? erpAccountId);

        Task<ErpAccount> GetErpAccountByIdWithActiveAsync(int id);

        Task<IPagedList<ErpAccount>> GetAllErpAccountsAsync(int pageIndex = 0, 
            int pageSize = int.MaxValue, 
            bool? showHidden = null,
            bool getOnlyTotalCount = false, 
            string erpAccontNo =null, 
            int salesOrgId =0, 
            string email=null, 
            string accountName=null,
            int erpAccountStatusTypeId = 0);

        //Task<IPagedList<ErpSalesRepErpAccountMap>> GetAllErpAccountsBySalesRepIdAsync(int pageIndex = 0,
        //    int pageSize = int.MaxValue,
        //    string erpSalesRepId = null,
        //    bool getOnlyTotalCount = false);

        Task<IList<ErpSalesRepErpAccountMap>> GetAllErpAccountsBySalesRepIdAsync(string erpSalesRepId = null);

        Task<IPagedList<ErpAccount>> GetAllErpAccountsByIdsAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false,
            bool getOnlyTotalCount = false, List<int> accountIds = null, string email = "");

        Task<ErpAccount> GetErpAccountByErpAccountNumberAsync(string accountNumber);

        Task<IList<ErpAccount>> GetAllErpAccountsAsync();

        Task<ErpAccount> GetActiveErpAccountByCustomerIdAsync(int customerId);
        Task InActiveAllOldAccount(DateTime syncStartTime);

        Task<ErpAccount> GetErpAccountByErpShipToAddressAsync(ErpShipToAddress erpShipToAddress);
    }
}

