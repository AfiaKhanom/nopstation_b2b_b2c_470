using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using LinqToDB;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using Nop.Services.Customers;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public class ErpSalesRepService : IErpSalesRepService
{
    #region Fields

    private readonly IRepository<ErpSalesRep> _erpSalesRepRepository; 
    private readonly IRepository<ErpNopUser> _erpNopUserRepository;
    private readonly IRepository<ErpAccount> _erpErpAccountRepository;
    private readonly IRepository<ErpSalesRepSalesOrgMap> _erpErpSalesRepSalesOrgMapRepository;
    private readonly IRepository<ErpSalesRepErpAccountMap> _erpSalesRepErpAccountMapRepository;
    private readonly IRepository<ErpSalesOrg> _erpSalesOrgRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly ICustomerService _customerServive;
    private readonly IRepository<CustomerCustomerRoleMapping> _customerCustomerRoleMappingRepository;
    private readonly IRepository<ErpNopUserAccountMap> _erpNopUserAccountMapRepository;

    #endregion

    #region Ctor

    public ErpSalesRepService(IRepository<ErpSalesRep> erpSalesRepRepository,
        IRepository<ErpNopUser> erpNopUserRepository,
        IRepository<ErpAccount> erpErpAccountRepository,
        IRepository<ErpSalesRepSalesOrgMap> erpErpSalesRepSalesOrgMapRepository,
        IRepository<ErpSalesOrg> erpSalesOrgRepository,
        IRepository<Customer> customerRepository,
        ICustomerService customerServive,
        IRepository<CustomerCustomerRoleMapping> customerCustomerRoleMappingRepository,
        IRepository<ErpNopUserAccountMap> erpNopUserAccountMapRepository,
        IRepository<ErpSalesRepErpAccountMap> erpSalesRepErpAccountMapRepository)
    {
        _erpSalesRepRepository = erpSalesRepRepository;
        _erpNopUserRepository = erpNopUserRepository;
        _erpErpAccountRepository = erpErpAccountRepository;
        _erpErpSalesRepSalesOrgMapRepository = erpErpSalesRepSalesOrgMapRepository;
        _erpSalesOrgRepository = erpSalesOrgRepository;
        _customerRepository = customerRepository;
        _customerServive = customerServive;
        _customerCustomerRoleMappingRepository = customerCustomerRoleMappingRepository;
        _erpNopUserAccountMapRepository = erpNopUserAccountMapRepository;
        _erpSalesRepErpAccountMapRepository = erpSalesRepErpAccountMapRepository;
    }

    #endregion

    #region Methods

    #region Insert/Update

    public async Task InsertErpSalesRepAsync(ErpSalesRep erpSalesRep)
    {
        await _erpSalesRepRepository.InsertAsync(erpSalesRep);
    }

    public async Task UpdateErpSalesRepAsync(ErpSalesRep erpSalesRep)
    {
        await _erpSalesRepRepository.UpdateAsync(erpSalesRep);
    }

    #endregion

    #region Delete

    private async Task DeleteErpSalesRepAsync(ErpSalesRep erpSalesRep)
    {
        //as ErpBaseEntity dosen't inherit ISoftDelete but has that feature
        erpSalesRep.IsDeleted = true;
        await _erpSalesRepRepository.UpdateAsync(erpSalesRep);
    }

    public async Task DeleteErpSalesRepsAsync(IList<ErpSalesRep> erpSalesReps)
    {
        await _erpSalesRepRepository.DeleteAsync(erpSalesReps);
    }

    public async Task DeleteErpSalesRepByIdAsync(int id)
    {
        var erpSalesRep = await GetErpSalesRepByIdAsync(id);

        if (erpSalesRep != null)
        {
            await DeleteErpSalesRepAsync(erpSalesRep);
        }
    }

    #endregion

    #region Read

    public async Task<ErpSalesRep> GetErpSalesRepByIdAsync(int id)
    {
        if (id == 0)
            return null;

        var erpSalesRep = await _erpSalesRepRepository.GetByIdAsync(id, cache => default);

        if (erpSalesRep == null || erpSalesRep.IsDeleted)
            return null;

        return erpSalesRep;
    }

    public async Task<IList<ErpSalesRep>> GetErpSalesRepByIdsAsync(int[] erpSalesRepIds)
    {
        return await _erpSalesRepRepository.GetByIdsAsync(erpSalesRepIds, includeDeleted: false);
    }

    public async Task<ErpSalesRep> GetErpSalesRepByIdWithActiveAsync(int id)
    {
        if (id == 0)
            return null;

        var erpSalesRep = await _erpSalesRepRepository.GetByIdAsync(id, cache => default);

        if (erpSalesRep == null || !erpSalesRep.IsActive || erpSalesRep.IsDeleted)
            return null;

        return erpSalesRep;
    }

    public async Task<IPagedList<ErpSalesRep>> GetAllErpSalesRepAsync(int nopCustomerId = 0, 
        int salesRepTypeId = 0,
        int pageIndex = 0, 
        int pageSize = int.MaxValue, 
        bool showHidden = false, 
        bool getOnlyTotalCount = false,
        int[] erpSalesOrgIds = null)
    {
        var erpSalesReps = await _erpSalesRepRepository.GetAllPagedAsync(query =>
        {
            if (nopCustomerId > 0)
                query = query.Where(egp => egp.NopCustomerId == nopCustomerId);

            if (salesRepTypeId > 0)
                query = query.Where(egp => egp.SalesRepTypeId == salesRepTypeId);

            if (!showHidden)
                query = query.Where(egp => egp.IsActive);

            query = query.Where(egp => !egp.IsDeleted);

            if (erpSalesOrgIds != null && erpSalesOrgIds.Length > 0)
            {
                query = query.Join(_erpErpSalesRepSalesOrgMapRepository.Table, x => x.Id, y => y.ErpSalesRepId,
                        (x, y) => new { SalesRep = x, Mapping = y })
                    .Where(z => erpSalesOrgIds.Contains(z.Mapping.ErpSalesOrgId))
                    .Select(z => z.SalesRep)
                    .Distinct();
            }

            query = query.OrderBy(egp => egp.Id);
            return query;

        }, pageIndex, pageSize, getOnlyTotalCount);

        return erpSalesReps;
    }

    public async Task<IList<ErpSalesRep>> GetErpSalesRepsByNopCustomerIdAsync(int nopCustomerId, bool showHidden = false)
    {
        if (nopCustomerId == 0)
            return null;

        var erpSalesReps = await _erpSalesRepRepository.GetAllAsync(query =>
        {
            if (!showHidden)
                query = query.Where(egp => egp.IsActive);

            query = query.Where(egp => !egp.IsDeleted);
            query = query.Where(ei => ei.NopCustomerId == nopCustomerId);
            query = query.OrderBy(ei => ei.Id);
            return query;

        });

        return erpSalesReps;
    }

    #endregion

    #region Sales rep users

    public async Task<IPagedList<ErpNopUser>> GetAllSalesRepUsersAsync(int salesRepId = 0, 
        string erpAccontNo = null,
        string accountName = null, 
        string email = null, 
        string fullName = null,
        int pageIndex = 0, 
        int pageSize = int.MaxValue, 
        bool showHidden = false,
        bool getOnlyTotalCount = false)
    {
        var erpNopUsers = await _erpNopUserRepository.GetAllPagedAsync(async query =>
        {
            if (!showHidden)
                query = query.Where(v => v.IsActive);

            query = query.Where(v => !v.IsDeleted);

            //filter erpAccounts by account number, account name
            var account = _erpErpAccountRepository.Table;
            if (!string.IsNullOrWhiteSpace(erpAccontNo))
                account = account.Where(c => c.AccountNumber.Contains(erpAccontNo));
            if (!string.IsNullOrWhiteSpace(accountName))
                account = account.Where(c => c.AccountName.Contains(accountName));

            if (!string.IsNullOrWhiteSpace(email) || !string.IsNullOrWhiteSpace(fullName))
            {
                var customer = _customerRepository.Table;

                if (!string.IsNullOrWhiteSpace(email))
                    customer = customer.Where(c => c.Email.Contains(email));

                if (!string.IsNullOrWhiteSpace(fullName))
                    customer = customer.Where(c => (c.FirstName + " " + c.LastName).Contains(fullName));

                if (!string.IsNullOrWhiteSpace(erpAccontNo))
                    account = account.Where(c => c.AccountNumber.Contains(erpAccontNo));

                query = query.Join(
                    customer,
                    u => u.NopCustomerId,
                    c => c.Id,
                    (u, c) => new { ErPUser = u, Customer = c })
                .Select(t => t.ErPUser);
            }

            //for allUsers salesRep = 0, no need to filter by sales org

            //filter by sales org
            if (salesRepId > 0)
            {
                query = query.Join(account,
                    u => u.ErpAccountId,
                    a => a.Id,
                    (u, a) => new { user = u, account = a })
                    .Join(
                        _erpErpSalesRepSalesOrgMapRepository.Table,
                        t1 => t1.account.ErpSalesOrgId,
                        som => som.ErpSalesOrgId,
                        (t1, som) => new { user = t1.user, account = t1.account, som = som })
                    .Where(t2 => t2.som.ErpSalesRepId == salesRepId)
                    .Select(t2 => t2.user).Distinct();
            }
            else
            {
                query = from n in query
                        join a in account
                        on n.ErpAccountId equals a.Id
                        select n;
            }

            //customer role ids
            var administratorsRoleId = (await _customerServive.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName)).Id;
            var b2BCustomerRoleId = (await _customerServive.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BCustomerRoleSystemName)).Id;
            var b2CCustomerRoleId = (await _customerServive.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2CCustomerRoleSystemName)).Id;
            var b2bSalesRepId = (await _customerServive.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BSalesRepRoleSystemName)).Id;

            var customerRoleMappings = _customerCustomerRoleMappingRepository.Table;

            //all adminIds
            var adminCustomerIds = customerRoleMappings.Where(w => w.CustomerRoleId == administratorsRoleId).Select(s => s.CustomerId).Distinct();

            //filter by notAdmin role id
            query = query.Where(u => !adminCustomerIds.Contains(u.NopCustomerId));

            var selectedNopUsers = from n in query
                                   join m in _erpNopUserAccountMapRepository.Table on n.Id equals m.ErpUserId
                                   join crm in customerRoleMappings on n.NopCustomerId equals crm.CustomerId
                                   where !customerRoleMappings.Where(x => x.CustomerId == n.NopCustomerId &&                                    x.CustomerRoleId == b2bSalesRepId).Any() &&
                                         customerRoleMappings.Where(x => x.CustomerId == n.NopCustomerId &&
                                             (x.CustomerRoleId == b2CCustomerRoleId || x.CustomerRoleId == b2BCustomerRoleId)).Any()
                                   select n;

            selectedNopUsers = selectedNopUsers.DistinctBy(u => u.Id);
            query = selectedNopUsers.AsQueryable();
            query = query.OrderByDescending(ea => ea.Id);
            return query;
        }, pageIndex, pageSize, getOnlyTotalCount);

        return erpNopUsers;
    }


    public async Task<IPagedList<ErpNopUser>> GetAllSalesRepUsersBySalesRepIdAsync(int salesRepId = 0, 
        string erpAccontNo = null,
        string accountName = null, 
        string email = null, 
        string fullName = null,
        int pageIndex = 0, 
        int pageSize = int.MaxValue, 
        bool showHidden = false,
        bool getOnlyTotalCount = false)
    {
        var erpNopUsers = await _erpNopUserRepository.GetAllPagedAsync(async query =>
        {
            if (!showHidden)
                query = query.Where(v => v.IsActive);

            query = query.Where(v => !v.IsDeleted);

            //filter erpAccounts by account number, account name
            var account = _erpErpAccountRepository.Table;
            if (!string.IsNullOrWhiteSpace(erpAccontNo))
                account = account.Where(c => c.AccountNumber.Contains(erpAccontNo));
            if (!string.IsNullOrWhiteSpace(accountName))
                account = account.Where(c => c.AccountName.Contains(accountName));

            if (!string.IsNullOrWhiteSpace(email) || !string.IsNullOrWhiteSpace(fullName))
            {
                var customer = _customerRepository.Table;

                if (!string.IsNullOrWhiteSpace(email))
                    customer = customer.Where(c => c.Email.Contains(email));

                if (!string.IsNullOrWhiteSpace(fullName))
                    customer = customer.Where(c => (c.FirstName + " " + c.LastName).Contains(fullName));

                if (!string.IsNullOrWhiteSpace(erpAccontNo))
                    account = account.Where(c => c.AccountNumber.Contains(erpAccontNo));

                query = query.Join(
                    customer,
                    u => u.NopCustomerId,
                    c => c.Id,
                    (u, c) => new { ErPUser = u, Customer = c })
                .Select(t => t.ErPUser);
            }

            //for allUsers salesRep = 0, no need to filter by sales org

            //filter by sales org
            if (salesRepId > 0)
            {
                query = query.Join(account,
                    u => u.ErpAccountId,
                    a => a.Id,
                    (u, a) => new { user = u, account = a })
                .Join(
                    _erpSalesRepErpAccountMapRepository.Table,
                    t1 => t1.account.Id,
                    som => som.ErpAccountId,
                    (t1, srm) => new { user = t1.user, account = t1.account, srm = srm })
                .Where(t2 => t2.srm.ErpSalesRepId == salesRepId)
                .Select(t2 => t2.user).Distinct();
            }
            else
            {
                query = from n in query
                        join a in account
                        on n.ErpAccountId equals a.Id
                        select n;
            }

            //customer role ids
            var administratorsRoleId = (await _customerServive.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName)).Id;
            var b2BCustomerRoleId = (await _customerServive.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BCustomerRole)).Id;
            var b2CCustomerRoleId = (await _customerServive.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2CCustomerRole)).Id;
            var b2bSalesRepId = (await _customerServive.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BSalesRepRoleSystemName)).Id;

            var customerRoleMappings = _customerCustomerRoleMappingRepository.Table;
            //all adminIds
            var adminCustomerIds = _customerCustomerRoleMappingRepository.Table.Where(w => w.CustomerRoleId == administratorsRoleId).Select(s => s.CustomerId).Distinct();

            //filter by notAdmin role id
            query = query.Where(u => !adminCustomerIds.Contains(u.NopCustomerId));

            var selectedNopUsers = from n in query
                                   join m in _erpNopUserAccountMapRepository.Table on n.Id equals m.ErpUserId
                                   join crm in customerRoleMappings on n.NopCustomerId equals crm.CustomerId
                                   where !customerRoleMappings.Where(x => x.CustomerId == n.NopCustomerId && x.CustomerRoleId == b2bSalesRepId).Any() &&
                                         customerRoleMappings.Where(x => x.CustomerId == n.NopCustomerId &&
                                             (x.CustomerRoleId == b2CCustomerRoleId || x.CustomerRoleId == b2BCustomerRoleId)).Any()
                                   select n;

            selectedNopUsers = selectedNopUsers.DistinctBy(u => u.Id);
            query = selectedNopUsers.AsQueryable();
            query = query.OrderByDescending(ea => ea.Id);
            return query;
        }, pageIndex, pageSize, getOnlyTotalCount);

        return erpNopUsers;
    }

    public async Task<IPagedList<ErpSalesOrg>> GetSalesRepOrgsPagedAsync(int salesRepId,
        int pageIndex = 0, 
        int pageSize = int.MaxValue, 
        bool showHidden = false,
        bool getOnlyTotalCount = false)
    {
        var salesOrgs = await _erpSalesOrgRepository.GetAllPagedAsync(query =>
        {
            var qry = from o in query
                      join som in _erpErpSalesRepSalesOrgMapRepository.Table on o.Id equals som.ErpSalesOrgId
                      where som.ErpSalesRepId == salesRepId && !o.IsDeleted &&
                            (showHidden || o.IsActive)
                      select o;

            query = qry.Where(v => !v.IsDeleted);

            query = query.OrderByDescending(ea => ea.Id);

            return query;

        }, pageIndex, pageSize, getOnlyTotalCount);

        return salesOrgs;
    }

    public async Task<IList<ErpSalesOrg>> GetSalesRepOrgsAsync(int salesRepId, 
        bool showHidden = false)
    {
        return await _erpSalesOrgRepository.GetAllAsync(query =>
        {
            return from o in query
                   join som in _erpErpSalesRepSalesOrgMapRepository.Table on o.Id equals som.ErpSalesOrgId
                   where som.ErpSalesRepId == salesRepId && !o.IsDeleted &&
                         (showHidden || o.IsActive)
                   select o;

        }, cache => cache.PrepareKeyForDefaultCache(ERPIntegrationCoreDefaults.SalesRepOrgCacheKey, salesRepId, showHidden));
    }

    public async Task<IPagedList<Customer>> GetAllSalesRepCustomersAsync(int pageIndex = 0, 
        int pageSize = int.MaxValue, 
        bool getOnlyTotalCount = false)
    {
        var registeredCustomerRole = await _customerServive.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName);
        var customers = await _customerRepository.GetAllPagedAsync(query =>
        {
            if (registeredCustomerRole != null)
            {
                query = query.Join(_customerCustomerRoleMappingRepository.Table, x => x.Id, y => y.CustomerId,
                            (x, y) => new { Customer = x, Mapping = y })
                        .Where(z => z.Mapping.CustomerRoleId == registeredCustomerRole.Id)
                        .Select(z => z.Customer)
                        .Distinct();
            }

            query = query
                .GroupJoin(
                    _erpSalesRepRepository.Table,
                    customer => customer.Id,
                    salesRep => salesRep.NopCustomerId,
                    (customer, salesRepGroup) => new { Customer = customer, SalesReps = salesRepGroup }
                )
                .Where(joinedData => joinedData.SalesReps.Any())
                .Select(joinedData => joinedData.Customer);

            query = query.OrderByDescending(c => c.CreatedOnUtc);

            return query;
        }, pageIndex, pageSize, getOnlyTotalCount);

        return customers;
    }

    public async Task<IPagedList<Customer>> GetAllCustomersNotYetSalesRepAsync(int includeSalesRepCustomerId = 0, 
        int pageIndex = 0, 
        int pageSize = int.MaxValue, 
        bool getOnlyTotalCount = false)
    {
        var registeredCustomerRole = await _customerServive.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName);
        var customers = await _customerRepository.GetAllPagedAsync(query =>
        {
            if (registeredCustomerRole != null)
            {
                query = query.Join(_customerCustomerRoleMappingRepository.Table, x => x.Id, y => y.CustomerId,
                            (x, y) => new { Customer = x, Mapping = y })
                        .Where(z => z.Mapping.CustomerRoleId == registeredCustomerRole.Id)
                        .Select(z => z.Customer)
                        .Distinct();
            }

            query = query
                .GroupJoin(
                    _erpSalesRepRepository.Table.Where(s => !s.IsDeleted),
                    customer => customer.Id,
                    salesRep => salesRep.NopCustomerId,
                    (customer, salesRepGroup) => new { Customer = customer, SalesReps = salesRepGroup }
                )
                .Where(joinedData => !joinedData.SalesReps.Any()
                    || (includeSalesRepCustomerId > 0 && joinedData.Customer.Id == includeSalesRepCustomerId))
                .Select(joinedData => joinedData.Customer);

            query = query.OrderByDescending(c => c.CreatedOnUtc);

            return query;
        }, pageIndex, pageSize, getOnlyTotalCount);

        return customers;
    }

    #endregion

    #endregion
}
