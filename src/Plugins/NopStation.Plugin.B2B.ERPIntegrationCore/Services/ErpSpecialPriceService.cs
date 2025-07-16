using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Services.Catalog;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public class ErpSpecialPriceService : IErpSpecialPriceService
{
    #region Fields

    private readonly IRepository<ErpSpecialPrice> _erpSpecialPriceRepository;
    private readonly IRepository<ErpAccount> _erpAccountRepository;
    protected readonly IStaticCacheManager _staticCacheManager;

    #endregion

    #region Ctor

    public ErpSpecialPriceService(IRepository<ErpSpecialPrice> erpSpecialPriceRepository,
        IRepository<ErpAccount> erpAccountRepository, 
        IStaticCacheManager staticCacheManager)
    {
        _erpSpecialPriceRepository = erpSpecialPriceRepository;
        _erpAccountRepository = erpAccountRepository;
        _staticCacheManager = staticCacheManager;
    }

    #endregion

    #region Methods

    #region Insert/Update

    public async Task InsertErpSpecialPriceAsync(ErpSpecialPrice erpSpecialPrice)
    {
        await _erpSpecialPriceRepository.InsertAsync(erpSpecialPrice);
    }

    public async Task InsertErpSpecialPricesAsync(List<ErpSpecialPrice> erpSpecialPrices)
    {
        await _erpSpecialPriceRepository.InsertAsync(erpSpecialPrices);
    }

    public async Task UpdateErpSpecialPriceAsync(ErpSpecialPrice erpSpecialPrice)
    {
        await _erpSpecialPriceRepository.UpdateAsync(erpSpecialPrice);
    }

    public async Task UpdateErpSpecialPricesAsync(List<ErpSpecialPrice> erpSpecialPrices)
    {
        await _erpSpecialPriceRepository.UpdateAsync(erpSpecialPrices);
    }

    #endregion

    #region Delete

    private async Task DeleteErpSpecialPriceAsync(ErpSpecialPrice erpSpecialPrice)
    {
        await _erpSpecialPriceRepository.DeleteAsync(erpSpecialPrice);
    }

    public async Task DeleteErpSpecialPriceByIdAsync(int id)
    {
        var erpSpecialPrice = await GetErpSpecialPriceByIdAsync(id);
        if (erpSpecialPrice != null)
        {
            await DeleteErpSpecialPriceAsync(erpSpecialPrice);
        }
    }

    #endregion

    #region Read

    public async Task<ErpSpecialPrice> GetErpSpecialPriceByIdAsync(int id)
    {
        if (id == 0)
            return null;

        return await _erpSpecialPriceRepository.GetByIdAsync(id, cache => default);
    }

    public async Task<IPagedList<ErpSpecialPrice>> GetAllErpSpecialPricesAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, bool? overridePublished = null, int productId = 0, int accountId = 0, bool onlyIncludeActiveErpAccountsMappedPrices = false)
    {
        var erpSpecialPrice = await _erpSpecialPriceRepository.GetAllPagedAsync(query =>
        {
            if (productId > 0)
                query = query.Where(ei => ei.NopProductId == productId);

            if (accountId > 0)
                query = query.Where(ei => ei.ErpAccountId == accountId);

            if (onlyIncludeActiveErpAccountsMappedPrices)
            {
                query = query.Join(_erpAccountRepository.Table,
                    specialPrice => specialPrice.ErpAccountId,
                    account => account.Id,
                    (specialPrice, account) => new { SpecialPrice = specialPrice, Account = account })
                .Where(joined => joined.Account.IsActive)
                .Select(joined => joined.SpecialPrice);
            }

            query = query.OrderBy(ei => ei.Id);

            return query;
        }, pageIndex, pageSize, getOnlyTotalCount);

        return erpSpecialPrice;
    }

    public async Task<IList<ErpSpecialPrice>> GetErpSpecialPricesByErpAccountIdAsync(int erpAcoountId)
    {
        if (erpAcoountId == 0)
            return null;

        var erpSpecialPrices = await _erpSpecialPriceRepository.GetAllAsync(query =>
        {
            query = query.Where(ei => ei.ErpAccountId == erpAcoountId);
            query = query.OrderBy(ei => ei.Id);
            return query;

        });

        return erpSpecialPrices;
    }

    public async Task<IList<ErpSpecialPrice>> GetErpSpecialPricesByNopProductIdAsync(int nopProductId)
    {
        if (nopProductId == 0)
            return null;

        var erpSpecialPrices = await _erpSpecialPriceRepository.GetAllAsync(query =>
        {
            return from sp in query
                   where sp.NopProductId == nopProductId
                   orderby sp.Id descending
                   select sp;
        }, cache => cache.PrepareKeyForDefaultCache(ERPIntegrationCoreDefaults.ErpProductPricingSpecialPriceByProductCacheKey, nopProductId));

        return erpSpecialPrices;
    }

    public async Task<ErpSpecialPrice> GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(int accountId, int nopProductId)
    {
        if (accountId == 0 || nopProductId == 0)
            return null;

        var key = _staticCacheManager.PrepareKeyForDefaultCache(ERPIntegrationCoreDefaults.ErpProductPricingSpecialPriceByProductIdAndAccountCacheKey, nopProductId, accountId);

        var query = _erpSpecialPriceRepository.Table.Where(b => b.ErpAccountId == accountId && b.NopProductId == nopProductId);

        return await _staticCacheManager.GetAsync(key, async () => await query.FirstOrDefaultAsync());
    }

    public async Task<bool> CheckAnySpecialPriceExistWithAccountIdAndProductId(int accountId, int productId)
    {
        if (accountId == 0 || productId == 0)
            return false;

        return await GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(accountId, productId) != null;
    }

    #endregion

    #endregion
}

