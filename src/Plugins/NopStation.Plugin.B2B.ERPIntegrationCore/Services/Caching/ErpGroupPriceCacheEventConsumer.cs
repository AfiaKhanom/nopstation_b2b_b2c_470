using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Services.Caching;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services.Caching;
public class ErpGroupPriceCacheEventConsumer : CacheEventConsumer<ErpGroupPrice>
{
    protected override async Task ClearCacheAsync(ErpGroupPrice entity, EntityEventType entityEventType)
    {
        await RemoveAsync(ERPIntegrationCoreDefaults.ErpProductPricingGroupPriceByProductIdCacheKey, entity.NopProductId);
        await RemoveAsync(ERPIntegrationCoreDefaults.ErpProductPricingGroupPriceByProductIdAndPriceGroupIdCacheKey, entity.NopProductId);

        await base.ClearCacheAsync(entity, entityEventType);
    }

}