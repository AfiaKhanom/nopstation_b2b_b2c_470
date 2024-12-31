using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Services.Customers;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public class ErpShipToAddressService : IErpShipToAddressService
    {
        #region Fields

        private readonly IRepository<ErpShipToAddress> _erpShipToAddressRepository;
        private readonly IRepository<ErpShiptoAddressErpAccountMap> _erpShiptoAddressErpAccountMapRepository;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public ErpShipToAddressService(IRepository<ErpShipToAddress> erpShipToAddressRepository,
            IRepository<ErpShiptoAddressErpAccountMap> erpShiptoAddressErpAccountMap,
            IStaticCacheManager staticCacheManager)
        {
            _erpShipToAddressRepository = erpShipToAddressRepository;
            _erpShiptoAddressErpAccountMapRepository = erpShiptoAddressErpAccountMap;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Methods

        #region Insert/Update/Delete

        public async Task InsertErpShipToAddressAsync(ErpShipToAddress erpShipToAddress)
        {
            await _erpShipToAddressRepository.InsertAsync(erpShipToAddress);
        }

        public async Task UpdateErpShipToAddressAsync(ErpShipToAddress erpShipToAddress)
        {
            await _erpShipToAddressRepository.UpdateAsync(erpShipToAddress);
        }

        public async Task DeleteErpShipToAddressAsync(ErpShipToAddress erpShipToAddress)
        {
            //as ErpBaseEntity dosen't inherit ISoftDelete but has that feature
            erpShipToAddress.IsDeleted = true;
            await _erpShipToAddressRepository.UpdateAsync(erpShipToAddress);
        }

        public async Task DeleteErpShipToAddressByIdAsync(int id)
        {
            var erpShipToAddress = await GetErpShipToAddressByIdAsync(id);

            if (erpShipToAddress != null)
            {
                await DeleteErpShipToAddressAsync(erpShipToAddress);
            }
        }

        #endregion

        #region Read

        /// <summary>
        /// Gets an ErpShipToAddress by Id
        /// </summary>
        /// <param name="id">ErpShipToAddress identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ErpShipToAddress
        /// </returns>
        public async Task<ErpShipToAddress> GetErpShipToAddressByIdAsync(int id)
        {
            if (id == 0)
                return null;

            var erpShipToAddress = await _erpShipToAddressRepository.GetByIdAsync(id, cache => default);

            if (erpShipToAddress == null || erpShipToAddress.IsDeleted)
                return null;

            return erpShipToAddress;
        }

        /// <summary>
        /// Gets an ErpShipToAddress by Id if it is active
        /// </summary>
        /// <param name="id">ErpShipToAddress identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ErpShipToAddress if it is activ
        /// </returns>
        public async Task<ErpShipToAddress> GetErpShipToAddressByIdWithActiveAsync(int id)
        {
            if (id == 0)
                return null;

            var erpShipToAddress = await _erpShipToAddressRepository.GetByIdAsync(id);

            if (erpShipToAddress == null || !erpShipToAddress.IsActive || erpShipToAddress.IsDeleted)
                return null;

            return erpShipToAddress;
        }

        /// <summary>
        /// Gets all ErpShipToAddress
        /// </summary>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">If only total no of account needed or not</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains all the ErpShipToAddress
        /// </returns>
        public virtual async Task<IPagedList<ErpShipToAddress>> GetAllErpShipToAddressesAsync(string shipToCode = "",
            string shipToName = "", int erpAccountId = 0, string repNum = "", string repFullName = "", string repEmail = "",
            int pageIndex = 0, int pageSize = int.MaxValue, bool? showHidden = null, string emailAddresses = "", bool isForOrder = false)
        {
            var erpShipToAddresses = await _erpShipToAddressRepository.GetAllPagedAsync(query =>
            {
                if (erpAccountId > 0)
                {
                    query = from address in _erpShipToAddressRepository.Table
                            join cam in _erpShiptoAddressErpAccountMapRepository.Table on address.Id equals cam.ErpShiptoAddressId
                            where cam.ErpAccountId == erpAccountId
                            select address;
                }
                else
                {
                    query = from address in _erpShipToAddressRepository.Table
                            join cam in _erpShiptoAddressErpAccountMapRepository.Table on address.Id equals cam.ErpShiptoAddressId
                            where cam.ErpAccountId > 0
                            select address;
                }

                if (showHidden.HasValue)
                {
                    if (!showHidden.Value)
                        query = query.Where(v => v.IsActive);
                    else
                        query = query.Where(v => !v.IsActive);
                }
                if (!string.IsNullOrEmpty(emailAddresses))
                    query = query.Where(v => v.EmailAddresses.Contains(emailAddresses));

                if (!string.IsNullOrEmpty(shipToCode))
                    query = query.Where(v => v.ShipToCode.Contains(shipToCode));

                if (!string.IsNullOrEmpty(shipToName))
                    query = query.Where(v => v.ShipToName.Contains(shipToName));

                if (!string.IsNullOrEmpty(repNum))
                    query = query.Where(v => v.RepNumber.Contains(repNum));

                if (!string.IsNullOrEmpty(repFullName))
                    query = query.Where(v => v.RepFullName.Contains(repFullName));

                if (!string.IsNullOrEmpty(repEmail))
                    query = query.Where(v => v.RepEmail.Contains(repEmail));

                query = query.Where(egp => !egp.IsDeleted);
                query = query.OrderByDescending(c => c.CreatedOnUtc);

                return query;
            }, pageIndex, pageSize);

            return erpShipToAddresses;
        }

        public async Task<IList<ErpShipToAddress>> GetErpShipToAddressesByErpAccountIdAsync(int erpAccountId, bool showHidden = false)
        {
            if (erpAccountId == 0)
                return null;

            var erpShipToAddresses = await _erpShipToAddressRepository.GetAllAsync(query =>
            {
                if (erpAccountId > 0)
                {
                    query = from address in _erpShipToAddressRepository.Table
                            join cam in _erpShiptoAddressErpAccountMapRepository.Table on address.Id equals cam.ErpShiptoAddressId
                            where cam.ErpAccountId == erpAccountId
                            select address;
                }
                else
                {
                    query = from address in _erpShipToAddressRepository.Table
                            join cam in _erpShiptoAddressErpAccountMapRepository.Table on address.Id equals cam.ErpShiptoAddressId
                            where cam.ErpAccountId > 0
                            select address;
                }
                if (!showHidden)
                    query = query.Where(egp => egp.IsActive);

                query = query.Where(egp => !egp.IsDeleted);
                query = query.OrderBy(ei => ei.Id);
                return query;

            });

            return erpShipToAddresses;
        }

        public async Task<IList<ErpShipToAddress>> GetAllErpShipToAddressesAsync(bool showHidden = false, bool isActiveOnly = false)
        {
            var query = _erpShipToAddressRepository.Table;

            if (!showHidden)
                query = query.Where(b => !b.IsDeleted);

            if (isActiveOnly)
                query = query.Where(b => b.IsActive);


            query = query.OrderBy(b => b.ShipToCode);

            return query.ToList();
        }

        public async Task<ErpShipToAddress> GetErpShipToAddressByShippingAddressIdAsync(int shippingAddressId)
        {
            if (shippingAddressId == 0)
                return null;

            return await _erpShipToAddressRepository.Table.FirstOrDefaultAsync(e => e.AddressId == shippingAddressId);
        }

        #endregion

        #endregion

        #region ErpShipToAddressErpAccountMap

        public virtual async Task<ErpShiptoAddressErpAccountMap> GetErpShipToAddressErpAccountMapByErpShipToAddressIdAsync(int erpShipToAddressId)
        {
            if (erpShipToAddressId == 0)
                return null;

            return await _erpShiptoAddressErpAccountMapRepository.Table
                .FirstOrDefaultAsync(m => m.ErpShiptoAddressId == erpShipToAddressId);
        }

        public virtual async Task RemoveErpShipToAddressErpAccountMapAsync(ErpAccount erpAccount, ErpShipToAddress erpShipToAddress)
        {
            if (erpAccount == null)
                throw new ArgumentNullException(nameof(erpAccount));

            if (await _erpShiptoAddressErpAccountMapRepository.Table
                .FirstOrDefaultAsync(m => m.ErpShiptoAddressId == erpShipToAddress.Id && m.ErpAccountId == erpAccount.Id)
                is ErpShiptoAddressErpAccountMap mapping)
            {
                if (erpAccount.BillingAddressId == erpShipToAddress.Id)
                    erpAccount.BillingAddressId = null;

                await _erpShiptoAddressErpAccountMapRepository.DeleteAsync(mapping);
            }
        }

        public virtual async Task InsertErpShipToAddressErpAccountMapAsync(ErpAccount erpAccount, ErpShipToAddress erpShipToAddress)
        {
            if (erpAccount is null)
                throw new ArgumentNullException(nameof(erpAccount));

            if (erpAccount is null)
                throw new ArgumentNullException(nameof(erpAccount));

            if (await _erpShiptoAddressErpAccountMapRepository.Table
                .FirstOrDefaultAsync(m => m.ErpShiptoAddressId == erpShipToAddress.Id && m.ErpAccountId == erpAccount.Id)
                is null)
            {
                var mapping = new ErpShiptoAddressErpAccountMap
                {
                    ErpShiptoAddressId = erpShipToAddress.Id,
                    ErpAccountId = erpAccount.Id
                };

                await _erpShiptoAddressErpAccountMapRepository.InsertAsync(mapping);
            }
        }

        public virtual async Task<IList<ErpShipToAddress>> GetErpShipToAddressesByAccountIdAsync(bool showHidden = false, bool isActiveOnly = false, int accountId = 0)
        {
            var query = from address in _erpShipToAddressRepository.Table
                        join cam in _erpShiptoAddressErpAccountMapRepository.Table on address.Id equals cam.ErpShiptoAddressId
                        where cam.ErpAccountId == accountId 
                        select address;

            if (!showHidden)
                query = query.Where(b => !b.IsDeleted);

            if (isActiveOnly)
                query = query.Where(b => b.IsActive);

            var key = _staticCacheManager.PrepareKeyForDefaultCache(NopCustomerServicesDefaults.CustomerAddressesCacheKey, accountId);

            return await _staticCacheManager.GetAsync(key, async () => await query.ToListAsync());
        }

        public virtual async Task<ErpShipToAddress> GetErpShipToAddressAsync(int accountId, int erpShiptoAddressId)
        {
            if (accountId == 0 || erpShiptoAddressId == 0)
                return null;

            var query = from address in _erpShipToAddressRepository.Table
                        join cam in _erpShiptoAddressErpAccountMapRepository.Table on address.Id equals cam.ErpShiptoAddressId
                        where cam.ErpAccountId == accountId && address.Id == erpShiptoAddressId
                        select address;

            var key = _staticCacheManager.PrepareKeyForDefaultCache(NopCustomerServicesDefaults.CustomerAddressCacheKey, accountId, erpShiptoAddressId);

            return await _staticCacheManager.GetAsync(key, async () => await query.FirstOrDefaultAsync());
        }

        public virtual async Task<ErpShipToAddress> GetCustomerBillingAddressAsync(ErpAccount erpAccount)
        {
            if (erpAccount is null)
                throw new ArgumentNullException(nameof(erpAccount));

            return await GetErpShipToAddressAsync(erpAccount.Id, erpAccount.BillingAddressId ?? 0);
        }

        #endregion
    }
}
