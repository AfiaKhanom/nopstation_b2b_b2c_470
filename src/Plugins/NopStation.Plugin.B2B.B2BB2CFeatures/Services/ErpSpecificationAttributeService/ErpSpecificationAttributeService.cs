using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpSpecificationAttributeService;

public class ErpSpecificationAttributeService : IErpSpecificationAttributeService
{
    #region Fields

    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IRepository<ProductSpecificationAttribute> _productSpecificationAttributeRepository;
    private readonly IRepository<SpecificationAttributeOption> _specificationAttributeOptionRepository;

    #endregion

    #region Ctor

    public ErpSpecificationAttributeService(
        IStaticCacheManager staticCacheManager,
        IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository,
        IRepository<SpecificationAttributeOption> specificationAttributeOptionRepository)
    {
        _staticCacheManager = staticCacheManager;
        _productSpecificationAttributeRepository = productSpecificationAttributeRepository;
        _specificationAttributeOptionRepository = specificationAttributeOptionRepository;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Get specification attribute options by identifiers
    /// </summary>
    /// <param name="specificationAttributeOptionIds">Identifiers</param>
    /// <returns>Specification attribute options</returns>
    public virtual async Task<IList<int>> GetSpecificationAttributeOptionIdsByNames(int specificationAttributeId, string preFilterFacets, int b2BAccountId)
    {
        if (b2BAccountId <= 0)
            return new List<int>();

        var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(B2BB2CFeaturesDefaults.ErpProductInfoSpecificationAttributeOptionIdsByNamesCacheKey, specificationAttributeId, preFilterFacets, b2BAccountId);

        return await _staticCacheManager.GetAsync(cacheKey, async () =>
        {
            if (specificationAttributeId == 0 || preFilterFacets == null)
                return new List<int>();

            var facetList = preFilterFacets.Split(',').ToList();

            var query = from sao in _specificationAttributeOptionRepository.Table
                        where facetList.Contains(sao.Name) && sao.SpecificationAttributeId == specificationAttributeId
                        select sao.Id;

            return query.Distinct().ToList();
        });
    }

    public virtual async Task<IList<int>> GetSpecificationAttributeOptionIdsForExcludeByNames(int specificationAttributeId, string specialExcludeOption)
    {
        var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(B2BB2CFeaturesDefaults.ErpSpecificationAttributeOptionIdsForSpecialExcludeOptionNamesCacheKey, specificationAttributeId, specialExcludeOption);

        return await _staticCacheManager.GetAsync(cacheKey, async () =>
        {
            if (specificationAttributeId == 0 || specialExcludeOption == null)
                return new List<int>();

            var facetList = specialExcludeOption.Split(',').ToList();

            var query = from sao in _specificationAttributeOptionRepository.Table
                        where facetList.Contains(sao.Name) && sao.SpecificationAttributeId == specificationAttributeId
                        select sao.Id;

            return query.Distinct().ToList();
        });
    }

    public virtual async Task<string> GetProductUOMByProductIdAndSpecificationAttributeId(int productId, int specificationAttributeId)
    {
        var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(B2BB2CFeaturesDefaults.ErpProductInfoUOMCacheKey, productId, specificationAttributeId);

        return await _staticCacheManager.GetAsync(cacheKey, async () =>
        {
            if (productId == 0 || specificationAttributeId == 0)
                return null;

            var query = from psa in _productSpecificationAttributeRepository.Table
                        join sao in _specificationAttributeOptionRepository.Table
                        on psa.SpecificationAttributeOptionId equals sao.Id
                        where psa.ProductId == productId && sao.SpecificationAttributeId == specificationAttributeId
                        select sao;

            return query.Select(x => x.Name).FirstOrDefault();
        });
    }

    public virtual async Task<IList<int>> GetProductIdBySpecificationAttributeOptionNames(int specificationAttributeId, string specialExcludes, int b2BAccountId)
    {
        var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(B2BB2CFeaturesDefaults.ErpProductIdsBySpecialExcludeOptionNamesCacheKey, specificationAttributeId, specialExcludes);

        return await _staticCacheManager.GetAsync(cacheKey, async () =>
        {
            if (b2BAccountId == 0 || specificationAttributeId == 0 || specialExcludes == null)
                return new List<int>();

            var specificationAttributeOptionIds = await GetSpecificationAttributeOptionIdsByNames(specificationAttributeId, specialExcludes, b2BAccountId);
            if (specificationAttributeOptionIds == null || specificationAttributeOptionIds.Count == 0)
                return new List<int>();

            var query = from psa in _productSpecificationAttributeRepository.Table
                        where specificationAttributeOptionIds.Contains(psa.SpecificationAttributeOptionId)
                        select psa.ProductId;

            return query.Distinct().ToList();
        });
    }

    #endregion
}