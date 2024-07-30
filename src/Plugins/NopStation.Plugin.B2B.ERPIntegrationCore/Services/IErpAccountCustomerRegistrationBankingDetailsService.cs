using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public interface IErpAccountCustomerRegistrationPhysicalTradingAddressService
    {
        Task InsertErpAccountCustomerRegistrationPhysicalTradingAddressAsync(ErpAccountCustomerRegistrationPhysicalTradingAddress erpAccountCustomerRegistrationPhysicalTradingAddress);

        Task UpdateErpAccountCustomerRegistrationPhysicalTradingAddressAsync(ErpAccountCustomerRegistrationPhysicalTradingAddress erpAccountCustomerRegistrationPhysicalTradingAddress);

        Task DeleteErpAccountCustomerRegistrationPhysicalTradingAddressByIdAsync(int id);

        Task<ErpAccountCustomerRegistrationPhysicalTradingAddress> GetErpAccountCustomerRegistrationPhysicalTradingAddressByIdAsync(int id);

        Task<ErpAccountCustomerRegistrationPhysicalTradingAddress> GetErpAccountCustomerRegistrationPhysicalTradingAddressByIdWithActiveAsync(int id);

        Task<IPagedList<ErpAccountCustomerRegistrationPhysicalTradingAddress>> GetAllErpAccountCustomerRegistrationPhysicalTradingAddressAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool getOnlyTotalCount = false, bool? overridePublished = false, int formId = 0, string fullName = null, string surname = null, string physicalTradingPostalCode = null);

        Task<IList<ErpAccountCustomerRegistrationPhysicalTradingAddress>> GetErpAccountCustomerRegistrationPhysicalTradingAddressByFormIdAsync(int formId);
    }
}

