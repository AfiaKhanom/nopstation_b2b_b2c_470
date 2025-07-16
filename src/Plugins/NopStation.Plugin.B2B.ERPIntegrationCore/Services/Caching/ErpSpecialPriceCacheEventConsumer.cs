using System.Threading.Tasks;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services.Caching;
public class ErpSpecialPriceCacheEventConsumer : CacheEventConsumer<ErpSpecialPrice>
{
    protected override async Task ClearCacheAsync(ErpSpecialPrice entity, EntityEventType entityEventType)
    {
        await RemoveByPrefixAsync(ERPIntegrationCoreDefaults.ErpProductPricingPrefix);
        await RemoveByPrefixAsync(NopCatalogDefaults.ProductPricePrefix, entity.NopProductId);
        await RemoveByPrefixAsync(NopCatalogDefaults.ProductMultiplePricePrefix, entity.NopProductId);
        await base.ClearCacheAsync(entity, entityEventType);
    }
}
