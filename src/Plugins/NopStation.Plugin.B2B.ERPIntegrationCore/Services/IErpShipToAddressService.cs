using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public interface IErpShipToAddressService
    {
        Task InsertErpShipToAddressAsync(ErpShipToAddress erpShipToAddress);

        Task UpdateErpShipToAddressAsync(ErpShipToAddress erpShipToAddress);

        Task DeleteErpShipToAddressByIdAsync(int id);

        Task DeleteErpShipToAddressAsync(ErpShipToAddress erpShipToAddress);

        Task<ErpShipToAddress> GetErpShipToAddressByIdAsync(int id);

        Task<ErpShipToAddress> GetErpShipToAddressByIdWithActiveAsync(int id);

        Task<IList<ErpShipToAddress>> GetErpShipToAddressesByErpAccountIdAsync(int erpAccountId, bool showHidden = false);

        Task<ErpShipToAddress> GetErpShipToAddressByShippingAddressIdAsync(int shippingAddressId);

        Task<IList<ErpShipToAddress>> GetAllErpShipToAddressesAsync(bool showHidden = false, bool isActiveOnly = false);

        Task<IPagedList<ErpShipToAddress>> GetAllErpShipToAddressesAsync(string shipToCode = "",
            string shipToName = "", int erpAccountId = 0, string repNum = "", string repFullName = "", string repEmail = "",
            int pageIndex = 0, int pageSize = int.MaxValue, bool? showHidden = null, string emailAddresses = "", bool isForOrder = false);

        Task<ErpShiptoAddressErpAccountMap> GetErpShipToAddressErpAccountMapByErpShipToAddressIdAsync(int erpShipToAddressId);

        Task RemoveErpShipToAddressErpAccountMapAsync(ErpAccount erpAccount, ErpShipToAddress erpShipToAddress);

        Task InsertErpShipToAddressErpAccountMapAsync(ErpAccount erpAccount, ErpShipToAddress erpShipToAddress);

        Task<IList<ErpShipToAddress>> GetErpShipToAddressesByAccountIdAsync(bool showHidden = false, bool isActiveOnly = false, int accountId = 0);

        Task<ErpShipToAddress> GetErpShipToAddressAsync(int accountId, int erpShiptoAddressId);

        Task<ErpShipToAddress> GetCustomerBillingAddressAsync(ErpAccount erpAccount);
    }
}
