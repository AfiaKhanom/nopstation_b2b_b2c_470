using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public class ErpNopUserService : IErpNopUserService
    {
        #region Fields

        private readonly IRepository<ErpNopUser> _erpNopUserRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<CustomerCustomerRoleMapping> _customerCustomerRoleMappingRepository;

        #endregion

        #region ctor

        public ErpNopUserService(IRepository<ErpNopUser> erpNopUserRepository,
            IRepository<Customer> customerRepository,
            IRepository<CustomerCustomerRoleMapping> customerCustomerRoleMappingRepository)
        {
            _erpNopUserRepository = erpNopUserRepository;
            _customerRepository = customerRepository;
            _customerCustomerRoleMappingRepository = customerCustomerRoleMappingRepository;
        }

        #endregion

        #region Methods

        #region Insert/Update

        public async Task InsertErpNopUserAsync(ErpNopUser erpNopUser)
        {
            await _erpNopUserRepository.InsertAsync(erpNopUser);
        }

        public async Task UpdateErpNopUserAsync(ErpNopUser erpNopUser)
        {
            await _erpNopUserRepository.UpdateAsync(erpNopUser);
        }

        #endregion

        #region Delete

        private async Task DeleteErpNopUserAsync(ErpNopUser erpNopUser)
        {
            //as ErpBaseEntity dosen't inherit ISoftDelete but has that feature
            erpNopUser.IsDeleted = true;
            await _erpNopUserRepository.UpdateAsync(erpNopUser);
        }

        public async Task DeleteErpNopUserByIdAsync(int id)
        {
            var erpNopUser = await GetErpNopUserByIdAsync(id);
            if (erpNopUser != null)
            {
                await DeleteErpNopUserAsync(erpNopUser);
            }
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
        public async Task<ErpNopUser> GetErpNopUserByIdAsync(int id)
        {
            if (id == 0)
                return null;

            var erpNopUser = await _erpNopUserRepository.GetByIdAsync(id, cache => default);

            if (erpNopUser == null || erpNopUser.IsDeleted)
                return null;

            return erpNopUser;
        }

        /// <summary>
        /// Gets an ErpAccount by Id if it is active
        /// </summary>
        /// <param name="id">ErpAccount identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ErpAccount if it is activ
        /// </returns>
        public async Task<ErpNopUser> GetErpNopUserByIdWithActiveAsync(int id)
        {
            if (id == 0)
                return null;

            var erpNopUser = await _erpNopUserRepository.GetByIdAsync(id, cache => default);

            if (erpNopUser == null || !erpNopUser.IsActive || erpNopUser.IsDeleted)
                return null;

            return erpNopUser;
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
        public async Task<IPagedList<ErpNopUser>> GetAllErpNopUsersAsync(int pageIndex = 0, int pageSize = int.MaxValue,
            bool? showHidden = null,
            bool getOnlyTotalCount = false,
            string email = null,
            string name = null,
            int accountId = 0,
            int userType = 0,
            int salesOrgId = 0,
            int erpShipToAddressId = 0)
        {
            var erpNopUsers = await _erpNopUserRepository.GetAllPagedAsync(query =>
            {
                // showHidden is null for getting all, true for only actives and false for only inactives
                if (showHidden.HasValue)
                {
                    if (!showHidden.Value)
                        query = query.Where(v => v.IsActive == true);
                    else
                        query = query.Where(v => v.IsActive == false);
                }

                query = query.Where(enu => !enu.IsDeleted);

                query = query.Join(_customerRepository.Table, x => x.NopCustomerId, y => y.Id,
                            (x, y) => new { ErpNopUser = x, Customer = y })
                        .Where(z => z.Customer.Active && !z.Customer.Deleted)
                        .Select(z => z.ErpNopUser)
                        .Distinct();

                if (!string.IsNullOrEmpty(name))
                {
                    query = query.Join(_customerRepository.Table, x => x.NopCustomerId, y => y.Id,
                            (x, y) => new { ErpNopUser = x, Customer = y })
                        .Where(z => z.Customer.FirstName.Contains(name) || z.Customer.LastName.Contains(name))
                        .Select(z => z.ErpNopUser)
                        .Distinct();
                }

                if (!string.IsNullOrEmpty(email))
                {
                    query = query.Join(_customerRepository.Table, x => x.NopCustomerId, y => y.Id,
                            (x, y) => new { ErpNopUser = x, Customer = y })
                        .Where(z => z.Customer.Email.Contains(email))
                        .Select(z => z.ErpNopUser)
                        .Distinct();
                }

                if (accountId > 0)
                    query = query.Where(enu => enu.ErpAccountId.Equals(accountId));

                if (erpShipToAddressId > 0)
                    query = query.Where(enu => enu.ErpShipToAddressId.Equals(erpShipToAddressId));

                if (userType > 0)
                    query = query.Where(enu => enu.ErpUserTypeId.Equals(userType));

                query = query.OrderByDescending(enu => enu.CreatedOnUtc);

                return query;

            }, pageIndex, pageSize, getOnlyTotalCount);

            return erpNopUsers;
        }

        /// <summary>
        /// Gets an ErpAccount by OrderId
        /// </summary>
        /// <param name="orderId">Order identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ErpAccount
        /// </returns>
        public async Task<ErpNopUser> GetErpNopUserByCustomerIdAsync(int customerId, int erpAccountId = 0)
        {
            if (customerId == 0)
                return null;

            var query = from enu in _erpNopUserRepository.Table
                        where enu.NopCustomerId == customerId && enu.IsDeleted != true && enu.IsActive == true
                        select enu;
            if (erpAccountId > 0)
            {
                query = query.Where(x => x.ErpAccountId == erpAccountId);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IList<ErpNopUser>> GetAllErpNopUserByAccountIdAsync(int accountId, bool showHidden = false)
        {
            if (accountId == 0)
                return null;

            var erpNopUsers = await _erpNopUserRepository.GetAllAsync(query =>
            {
                if (!showHidden)
                    query = query.Where(enu => enu.IsActive);

                query = query.Where(enu => !enu.IsDeleted);
                query = query.Where(enu => enu.ErpAccountId == accountId);
                query = query.OrderBy(enu => enu.Id);
                return query;

            });

            return erpNopUsers;
        }

        public async Task<IList<int>> GetAllCustomersByOnlyTheseRoleIdsAsync(int id)
        {
            if (id <= 0)
                return null;

            var customerIds = _customerCustomerRoleMappingRepository.Table
                .GroupBy(mapping => mapping.CustomerId)
                .Where(group => group.Count() == 1 && group.Max(mapping => mapping.CustomerRoleId) == id)
                .Select(group => group.Key);

            return await customerIds.ToListAsync();
        }

        #endregion

        #endregion
    }
}

