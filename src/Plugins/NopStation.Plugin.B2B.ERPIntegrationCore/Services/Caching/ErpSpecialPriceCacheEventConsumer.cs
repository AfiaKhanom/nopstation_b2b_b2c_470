using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services.Caching;
public class ErpSpecialPriceCacheEventConsumer : CacheEventConsumer<ErpSpecialPrice>
{
    protected override async Task ClearCacheAsync(ErpSpecialPrice entity, EntityEventType entityEventType)
    {
        await RemoveAsync(ERPIntegrationCoreDefaults.ErpProductPricingSpecialPriceByProductCacheKey, entity.NopProductId);
        await RemoveAsync(ERPIntegrationCoreDefaults.ErpProductPricingSpecialPriceByProductIdAndAccountCacheKey, entity.NopProductId, entity.ErpAccountId);

        await base.ClearCacheAsync(entity, entityEventType);
    }

}
