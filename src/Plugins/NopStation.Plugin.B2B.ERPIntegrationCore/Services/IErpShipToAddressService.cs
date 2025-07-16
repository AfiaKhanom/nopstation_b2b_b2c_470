using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public interface IErpShipToAddressService
{
    Task InsertErpShipToAddressAsync(ErpShipToAddress erpShipToAddress);

    Task InsertErpShipToAddressesAsync(IList<ErpShipToAddress> erpShipToAddresses);

    Task UpdateErpShipToAddressAsync(ErpShipToAddress erpShipToAddress);

    Task UpdateErpShipToAddressesAsync(IList<ErpShipToAddress> erpShipToAddresses);

    Task DeleteErpShipToAddressByIdAsync(int id);

    Task DeleteErpShipToAddressAsync(ErpShipToAddress erpShipToAddress);

    Task<ErpShipToAddress> GetErpShipToAddressByIdAsync(int id, bool filterOutDeleted = true);

    Task<ErpShipToAddress> GetErpShipToAddressByIdWithActiveAsync(int id);

    Task<IList<ErpShipToAddress>> GetErpShipToAddressesByErpAccountIdAsync(int erpAccountId, bool showHidden = false);

    Task<ErpShipToAddress> GetErpShipToAddressByShippingAddressIdAsync(int shippingAddressId);

    Task<IList<ErpShipToAddress>> GetAllErpShipToAddressesByErpAccountIdsAsync(int[] erpAccountIds, bool showHidden = false, bool isActiveOnly = false);

    Task<Dictionary<string, List<ErpShipToAddress>>> GetErpAccountShipToAddressMappingAsync(int[] erpAccountIds, bool showHidden = false, bool isActiveOnly = false);

    Task<IPagedList<ErpShipToAddress>> GetAllErpShipToAddressesAsync(string shipToCode = "",
        string shipToName = "", int erpAccountId = 0, string repNum = "", string repFullName = "", string repEmail = "",
        int pageIndex = 0, int pageSize = int.MaxValue, bool? showHidden = null, string emailAddresses = "", bool isForOrder = false);

    Task<ErpShiptoAddressErpAccountMap> GetErpShipToAddressErpAccountMapByErpShipToAddressIdAsync(int erpShipToAddressId);

    Task RemoveErpShipToAddressErpAccountMapAsync(ErpAccount erpAccount, ErpShipToAddress erpShipToAddress);

    Task InsertErpShipToAddressErpAccountMapAsync(ErpAccount erpAccount, ErpShipToAddress erpShipToAddress, ErpShipToAddressCreatedByType createdByType);

    Task InsertErpShipToAddressErpAccountMapsAsync(IList<ErpShiptoAddressErpAccountMap> erpShiptoAddressErpAccountMaps);

    Task<IList<ErpShipToAddress>> GetErpShipToAddressesByAccountIdAsync(bool showHidden = false, bool isActiveOnly = false, int accountId = 0);

    Task<ErpShipToAddress> GetErpShipToAddressAsync(int accountId, int erpShiptoAddressId);

    Task<ErpShipToAddress> GetCustomerBillingAddressAsync(ErpAccount erpAccount);

    Task<List<ErpShipToAddress>> GetErpShipToAddressesByCustomerAddressesAsync(int customerId, 
        int erpAccountId = 0, 
        bool isActiveOnly = true, 
        bool filterOutDeleted = true);

    Task<ErpShipToAddress> GetErpShipToAddressByShipToCodeAndErpAccountIdAsync(string shipToCode, int erpAccountId);

    Task<IList<ErpShiptoAddressErpAccountMap>> GetErpShipToAddressErpAccountMapsByErpAccountIdsAsync(int[] erpAccountIds);
}
