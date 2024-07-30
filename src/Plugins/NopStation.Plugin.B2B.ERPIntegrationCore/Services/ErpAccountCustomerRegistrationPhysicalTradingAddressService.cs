using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public class ErpAccountCustomerRegistrationPhysicalTradingAddressService : IErpAccountCustomerRegistrationPhysicalTradingAddressService
    {
        #region Fields

        private readonly IRepository<ErpAccountCustomerRegistrationPhysicalTradingAddress> _erpAccountCustomerRegistrationPhysicalTradingAddressRepository;

        #endregion

        #region ctor

        public ErpAccountCustomerRegistrationPhysicalTradingAddressService(IRepository<ErpAccountCustomerRegistrationPhysicalTradingAddress> erpAccountCustomerRegistrationPhysicalTradingAddressRepository)
        {
            _erpAccountCustomerRegistrationPhysicalTradingAddressRepository = erpAccountCustomerRegistrationPhysicalTradingAddressRepository;
        }

        #endregion

        #region Methods

        #region Insert/Update

        public async Task InsertErpAccountCustomerRegistrationPhysicalTradingAddressAsync(ErpAccountCustomerRegistrationPhysicalTradingAddress erpAccountCustomerRegistrationPhysicalTradingAddress)
        {
            await _erpAccountCustomerRegistrationPhysicalTradingAddressRepository.InsertAsync(erpAccountCustomerRegistrationPhysicalTradingAddress);
        }

        public async Task UpdateErpAccountCustomerRegistrationPhysicalTradingAddressAsync(ErpAccountCustomerRegistrationPhysicalTradingAddress erpAccountCustomerRegistrationPhysicalTradingAddress)
        {
            await _erpAccountCustomerRegistrationPhysicalTradingAddressRepository.UpdateAsync(erpAccountCustomerRegistrationPhysicalTradingAddress);
        }

        #endregion

        #region Delete

        private async Task DeleteErpAccountCustomerRegistrationPhysicalTradingAddressAsync(ErpAccountCustomerRegistrationPhysicalTradingAddress erpAccountCustomerRegistrationPhysicalTradingAddress)
        {
            //as ErpBaseEntity dosen't inherit ISoftDelete but has that feature
            erpAccountCustomerRegistrationPhysicalTradingAddress.IsDeleted = true;
            await _erpAccountCustomerRegistrationPhysicalTradingAddressRepository.UpdateAsync(erpAccountCustomerRegistrationPhysicalTradingAddress);
        }

        public async Task DeleteErpAccountCustomerRegistrationPhysicalTradingAddressByIdAsync(int id)
        {
            var erpAccountCustomerRegistrationPhysicalTradingAddress = await GetErpAccountCustomerRegistrationPhysicalTradingAddressByIdAsync(id);
            if (erpAccountCustomerRegistrationPhysicalTradingAddress != null)
            {
                await DeleteErpAccountCustomerRegistrationPhysicalTradingAddressAsync(erpAccountCustomerRegistrationPhysicalTradingAddress);
            }
        }

        #endregion

        #region Read

        /// <summary>
        /// Gets an ErpAccountCustomerRegistrationPhysicalTradingAddress by Id
        /// </summary>
        /// <param name="id">ErpAccountCustomerRegistrationPhysicalTradingAddress identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ErpAccountCustomerRegistrationPhysicalTradingAddress
        /// </returns>
        public async Task<ErpAccountCustomerRegistrationPhysicalTradingAddress> GetErpAccountCustomerRegistrationPhysicalTradingAddressByIdAsync(int id)
        {
            if (id == 0)
                return null;

            var erpAccountCustomerRegistrationPhysicalTradingAddress = await _erpAccountCustomerRegistrationPhysicalTradingAddressRepository.GetByIdAsync(id, cache => default);

            if (erpAccountCustomerRegistrationPhysicalTradingAddress == null || erpAccountCustomerRegistrationPhysicalTradingAddress.IsDeleted)
                return null;

            return erpAccountCustomerRegistrationPhysicalTradingAddress;
        }

        /// <summary>
        /// Gets an ErpAccountCustomerRegistrationPhysicalTradingAddress by Id if it is active
        /// </summary>
        /// <param name="id">ErpAccountCustomerRegistrationPhysicalTradingAddress identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ErpAccountCustomerRegistrationPhysicalTradingAddress if it is active
        /// </returns>
        public async Task<ErpAccountCustomerRegistrationPhysicalTradingAddress> GetErpAccountCustomerRegistrationPhysicalTradingAddressByIdWithActiveAsync(int id)
        {
            if (id == 0)
                return null;

            var erpAccountCustomerRegistrationPhysicalTradingAddress = await _erpAccountCustomerRegistrationPhysicalTradingAddressRepository.GetByIdAsync(id, cache => default);

            if (erpAccountCustomerRegistrationPhysicalTradingAddress == null || !erpAccountCustomerRegistrationPhysicalTradingAddress.IsActive || erpAccountCustomerRegistrationPhysicalTradingAddress.IsDeleted)
                return null;

            return erpAccountCustomerRegistrationPhysicalTradingAddress;
        }

        /// <summary>
        /// Gets all ErpAccountCustomerRegistrationPhysicalTradingAddress
        /// </summary>
        /// <param name="pageIndex">Page number</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">If only total no of account needed or not</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains all the ErpAccountCustomerRegistrationPhysicalTradingAddress
        /// </returns>
        public async Task<IPagedList<ErpAccountCustomerRegistrationPhysicalTradingAddress>> GetAllErpAccountCustomerRegistrationPhysicalTradingAddressAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool getOnlyTotalCount = false, bool? overridePublished = false, int formId = 0, string fullName = null, string surname = null, string physicalTradingPostalCode = null)
        {
            var erpAccountCustomerRegistrationPhysicalTradingAddress = await _erpAccountCustomerRegistrationPhysicalTradingAddressRepository.GetAllPagedAsync(query =>
            {
                if (!showHidden)
                    query = query.Where(egp => egp.IsActive);

                if (formId > 0)
                    query = query.Where(egp => egp.FormId == formId);

                if (!string.IsNullOrEmpty(fullName))
                {
                    query = query.Where(egp => egp.FullName.Contains(fullName));
                }

                if (!string.IsNullOrEmpty(surname))
                {
                    query = query.Where(egp => egp.Surname.Contains(surname));
                }

                query = query.Where(egp => !egp.IsDeleted);

                query = query.OrderBy(egp => egp.Id);
                return query;

            }, pageIndex, pageSize, getOnlyTotalCount);

            return erpAccountCustomerRegistrationPhysicalTradingAddress;
        }

        public async Task<IList<ErpAccountCustomerRegistrationPhysicalTradingAddress>> GetErpAccountCustomerRegistrationPhysicalTradingAddressByFormIdAsync(int formId)
        {
            if (formId == 0)
                return null;

            return  (from egp in _erpAccountCustomerRegistrationPhysicalTradingAddressRepository.Table
                          where egp.FormId == formId && egp.IsDeleted != true && egp.IsActive == true
                          select egp).ToList();
        }

        #endregion

        #endregion
    }
}

