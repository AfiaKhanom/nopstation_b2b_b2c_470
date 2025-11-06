using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public interface IErpAccountService
{
    Task InsertErpAccountAsync(ErpAccount erpAccount);
    Task InsertErpAccountsAsync(List<ErpAccount> erpAccounts);
    Task InsertSalesRepErpAccountAsync(ErpSalesRepErpAccountMap erpSalesRepErpAccount);
    Task UpdateErpAccountAsync(ErpAccount erpAccount);
    Task UpdateErpAccountsAsync(List<ErpAccount> erpAccounts);
    Task DeleteErpAccountByIdAsync(int id);
    Task DeleteErpSalesRepErpAccountMapAsync(ErpSalesRepErpAccountMap salesRepErpAccountMap);
    Task<ErpSalesRepErpAccountMap> GetErpSalesRepErpAccountMapByIdAsync(int salesRepId, int? erpAccountId);
    Task<IList<ErpSalesRepErpAccountMap>> GetAllErpAccountsBySalesRepIdAsync(string erpSalesRepId = null);
    Task<ErpAccount> GetErpAccountByIdAsync(int id, bool filterOutDeleted = true);
    Task<ErpAccount> GetErpAccountByIdWithActiveAsync(int id);
    Task<IList<ErpAccount>> GetAllErpAccountsAsync();
    Task<IPagedList<ErpAccount>> GetAllErpAccountsAsync(int pageIndex = 0,
        int pageSize = int.MaxValue,
        bool? showHidden = null,
        bool getOnlyTotalCount = false,
        string erpAccountNo = null,
        int salesOrgId = 0,
        string email = null,
        string accountName = null,
        int erpAccountStatusTypeId = 0,
        bool filterDeleted = true);
    Task<IList<ErpAccount>> GetErpAccountListAsync(string accountNumber = null,
        string accountName = null,
        string email = null,
        int salesOrgId = 0,
        int erpAccountStatusTypeId = 0,
        bool filterDeleted = true,
        bool? showHidden = null);
    Task<IPagedList<ErpAccount>> GetAllErpAccountsByIdsAsync(int pageIndex = 0, 
        int pageSize = int.MaxValue, 
        bool showHidden = false,
        bool getOnlyTotalCount = false, 
        List<int> accountIds = null, 
        string email = "");
    Task<IList<ErpAccount>> GetAllErpAccountsByIdsAsync(bool showHidden = false,
        List<int> erpAccountIds = null,
        string email = "");
    Task<ErpAccount> GetErpAccountByErpAccountNumberAsync(string accountNumber);
    Task<ErpAccount> GetActiveErpAccountByCustomerIdAsync(int customerId);
    Task<ErpAccount> GetErpAccountByErpShipToAddressAsync(ErpShipToAddress erpShipToAddress);
    Task InActiveAllOldAccount(DateTime syncStartTime);
    Task<IList<ErpAccount>> GetErpAccountsOfOnlyActiveErpNopUsersAsync(int salesOrgId = 0, string accountNumber = "");
    Task<IList<ErpAccount>> GetAllErpAccountsBySaleOrgIdAsync(int salesOrgId);
    Task<ErpAccount> CheckErpAccountExist(string accountNumber, int salesOrgId);
}
