using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public interface IErpSpecialPriceService
{
    Task InsertErpSpecialPriceAsync(ErpSpecialPrice erpSpecialPrice);
    Task InsertErpSpecialPricesAsync(List<ErpSpecialPrice> erpSpecialPrices);

    Task UpdateErpSpecialPriceAsync(ErpSpecialPrice erpSpecialPrice);
    Task UpdateErpSpecialPricesAsync(List<ErpSpecialPrice> erpSpecialPrices);

    Task DeleteErpSpecialPriceByIdAsync(int id);

    Task<ErpSpecialPrice> GetErpSpecialPriceByIdAsync(int id);

    Task<IPagedList<ErpSpecialPrice>> GetAllErpSpecialPricesAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, bool? overridePublished = null, int productId = 0, int accountId = 0, bool onlyIncludeActiveErpAccountsMappedPrices = false);

    Task<IList<ErpSpecialPrice>> GetErpSpecialPricesByErpAccountIdAsync(int erpAcoountId);

    Task<IList<ErpSpecialPrice>> GetErpSpecialPricesByNopProductIdAsync(int nopProductId);

    Task<ErpSpecialPrice> GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(int accountId, int nopProductId);

    Task<bool> CheckAnySpecialPriceExistWithAccountIdAndProductId(int accountId, int productId);
}
