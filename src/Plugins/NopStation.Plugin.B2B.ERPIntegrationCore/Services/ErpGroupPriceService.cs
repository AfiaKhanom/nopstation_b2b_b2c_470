using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public class ErpGroupPriceService : IErpGroupPriceService
{
    #region Fields

    private readonly IRepository<ErpGroupPrice> _erpGroupPriceRepository;
    private readonly IRepository<ErpGroupPriceCode> _erpGroupPriceCodeRepository;
    protected readonly IStaticCacheManager _staticCacheManager;
    private readonly INopDataProvider _nopDataProvider;

    #endregion

    #region Ctor

    public ErpGroupPriceService(IRepository<ErpGroupPrice> erpGroupPriceRepository,
        IRepository<ErpGroupPriceCode> erpGroupPriceCodeRepository,
        IStaticCacheManager staticCacheManager,
        INopDataProvider nopDataProvider)
    {
        _erpGroupPriceRepository = erpGroupPriceRepository;
        _erpGroupPriceCodeRepository = erpGroupPriceCodeRepository;
        _staticCacheManager = staticCacheManager;
        _nopDataProvider = nopDataProvider;
    }

    #endregion

    #region Methods

    #region Insert/Update

    public async Task InsertErpGroupPriceAsync(ErpGroupPrice erpGroupPrice)
    {
        await _erpGroupPriceRepository.InsertAsync(erpGroupPrice);
    }

    public async Task InsertErpGroupPricesAsync(IList<ErpGroupPrice> erpGroupPrices)
    {
        await _erpGroupPriceRepository.InsertAsync(erpGroupPrices);
    }

    public async Task UpdateErpGroupPriceAsync(ErpGroupPrice erpGroupPrice)
    {
        await _erpGroupPriceRepository.UpdateAsync(erpGroupPrice);
    }

    public async Task UpdateErpGroupPricesAsync(IList<ErpGroupPrice> erpGroupPrices)
    {
        await _erpGroupPriceRepository.UpdateAsync(erpGroupPrices);
    }

    #endregion

    #region Delete

    private async Task DeleteErpGroupPriceAsync(ErpGroupPrice erpGroupPrice)
    {
        //as ErpBaseEntity dosen't inherit ISoftDelete but has that feature
        erpGroupPrice.IsDeleted = true;
        await _erpGroupPriceRepository.UpdateAsync(erpGroupPrice);
    }

    public async Task DeleteErpGroupPriceByIdAsync(int id)
    {
        var erpGroupPrice = await GetErpGroupPriceByIdAsync(id);
        if (erpGroupPrice != null)
        {
            await DeleteErpGroupPriceAsync(erpGroupPrice);
        }
    }

    #endregion

    #region Read

    /// <summary>
    /// Gets an ErpGroupPrice by Id
    /// </summary>
    /// <param name="id">ErpGroupPrice identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the ErpGroupPrice
    /// </returns>
    public async Task<ErpGroupPrice> GetErpGroupPriceByIdAsync(int id)
    {
        if (id == 0)
            return null;

        var erpGroupPrice = await _erpGroupPriceRepository.GetByIdAsync(id, cache => default);

        if (erpGroupPrice == null || erpGroupPrice.IsDeleted)
            return null;

        return erpGroupPrice;
    }

    /// <summary>
    /// Gets an ErpGroupPrice by Id if it is active
    /// </summary>
    /// <param name="id">ErpGroupPrice identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the ErpGroupPrice if it is activ
    /// </returns>
    public async Task<ErpGroupPrice> GetErpGroupPriceByIdWithActiveAsync(int id)
    {
        if (id == 0)
            return null;

        var erpGroupPrice = await _erpGroupPriceRepository.GetByIdAsync(id, cache => default);

        if (erpGroupPrice == null || !erpGroupPrice.IsActive || erpGroupPrice.IsDeleted)
            return null;

        return erpGroupPrice;
    }

    /// <summary>
    /// Gets all ErpGroupPrices
    /// </summary>
    /// <param name="pageIndex">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="getOnlyTotalCount">If only total no of account needed or not</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains all the ErpGroupPrices
    /// </returns>
    public async Task<IPagedList<ErpGroupPrice>> GetAllErpGroupPricesAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool getOnlyTotalCount = false, bool? overridePublished = false,
        int productId = 0, string groupCode = null)
    {
        var erpGroupPrices = await _erpGroupPriceRepository.GetAllPagedAsync(query =>
        {
            if (!showHidden)
                query = query.Where(egp => egp.IsActive);

            if (productId > 0)
                query = query.Where(egp => egp.NopProductId == productId);

            if (!string.IsNullOrEmpty(groupCode))
            {
                query = query.Join(_erpGroupPriceCodeRepository.Table, x => x.ErpNopGroupPriceCodeId, y => y.Id,
                        (x, y) => new { ErpGroupPrice = x, ErpGroupPriceCode = y })
                    .Where(z => z.ErpGroupPriceCode.Code.Contains(groupCode))
                    .Select(z => z.ErpGroupPrice)
                    .Distinct();
            }

            query = query.Where(egp => !egp.IsDeleted);

            query = query.OrderBy(egp => egp.Id);

            return query;
        }, pageIndex, pageSize, getOnlyTotalCount);

        return erpGroupPrices;
    }

    public async Task<IList<ErpGroupPrice>> GetErpGroupPriceByProductIdAsync(int productId)
    {
        if (productId == 0)
            return null;

        var erpGroupPrices = await _erpGroupPriceRepository.GetAllAsync(query =>
        {
            return from egp in query
                   where egp.NopProductId == productId && !egp.IsDeleted && egp.IsActive
                   orderby egp.Id descending
                   select egp;
        }, cache => cache.PrepareKeyForDefaultCache(ERPIntegrationCoreDefaults.ErpProductPricingGroupPriceByProductIdCacheKey, productId));

        return erpGroupPrices;
    }


    public async Task<ErpGroupPrice> GetErpGroupPriceByErpPriceGroupCodeAndProductId(int priceGroupCodeId, int productId)
    {
        if (productId == 0 || priceGroupCodeId == 0)
            return null;

        var key = _staticCacheManager.PrepareKeyForDefaultCache(ERPIntegrationCoreDefaults.ErpProductPricingGroupPriceByProductIdAndPriceGroupIdCacheKey, productId, priceGroupCodeId);

        var query = _erpGroupPriceRepository.Table
            .Where(egp => egp.NopProductId == productId && egp.ErpNopGroupPriceCodeId == priceGroupCodeId && !egp.IsDeleted && egp.IsActive);

        return await _staticCacheManager.GetAsync(key, async () => await query.FirstOrDefaultAsync());
    }

    public async Task<bool> CheckAnyErpGroupPriceExistWithProductIdAndErpGroupPriceCodeId(int prouctdId, int priceGroupCodeId)
    {
        if (prouctdId == 0 || priceGroupCodeId == 0)
            return false;

        return await GetErpGroupPriceByErpPriceGroupCodeAndProductId(priceGroupCodeId, prouctdId) != null;
    }

    public async Task InActiveAllOldGroupPrice(DateTime syncStartTime)
    {
        if (syncStartTime == DateTime.MinValue)
            return;

        var connectionString = new SqlConnectionStringBuilder(DataSettingsManager.LoadSettings().ConnectionString);

        var sqlCommand = $"Update [{connectionString.InitialCatalog}].[dbo].[Erp_Group_Price] Set [IsActive] = 0 Where [UpdatedOnUtc] < '{syncStartTime:yyyy-MM-dd HH:mm:ss}'";

        await _nopDataProvider.ExecuteNonQueryAsync(sqlCommand);
    }

    #endregion

    #endregion
}
