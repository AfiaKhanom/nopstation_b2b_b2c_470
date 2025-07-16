using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services.Caching;

public class ErpGroupPriceCacheEventConsumer : CacheEventConsumer<ErpGroupPrice>
{
    protected override async Task ClearCacheAsync(ErpGroupPrice entity, EntityEventType entityEventType)
    {
        await RemoveAsync(ERPIntegrationCoreDefaults.ErpProductPricingGroupPriceByProductIdCacheKey, entity.NopProductId);
        await RemoveAsync(ERPIntegrationCoreDefaults.ErpProductPricingGroupPriceByProductIdAndPriceGroupIdCacheKey, entity.NopProductId, entity.ErpNopGroupPriceCodeId);

        await base.ClearCacheAsync(entity, entityEventType);
    }
}