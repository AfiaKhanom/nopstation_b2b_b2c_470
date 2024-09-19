using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services.Caching;
internal class ErpNopUserAccountMapCacheEventConsumer : CacheEventConsumer<ErpNopUserAccountMap>
{
    protected override async Task ClearCacheAsync(ErpNopUserAccountMap entity, EntityEventType entityEventType)
    {
        await RemoveAsync(ERPIntegrationCoreDefaults.ErpNopUserAccountMapByErpUserIdCacheKey, entity.ErpUserId);
        await RemoveAsync(ERPIntegrationCoreDefaults.ErpNopUserAccountMapByErpAccountIdCacheKey, entity.ErpAccountId);
        await RemoveAsync(ERPIntegrationCoreDefaults.ErpNopUserAccountMapByErpAccountAndErpUserCacheKey, entity.ErpAccountId, entity.ErpUserId);
    }
}
