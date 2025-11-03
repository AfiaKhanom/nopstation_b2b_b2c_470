using System.Threading.Tasks;
using Nop.Services.Caching;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services.Caching;
public class ErpSalesRepErpAccountMapEventConsumer : CacheEventConsumer<ErpSalesRepErpAccountMap>
{
    protected override async Task ClearCacheAsync(ErpSalesRepErpAccountMap entity, EntityEventType entityEventType)
    {
        await RemoveAsync(ERPIntegrationCoreDefaults.ErpSalesRepErpAccountMapByIdsCacheKey,entity.ErpSalesRepId,entity.ErpAccountId);
        await RemoveAsync(ERPIntegrationCoreDefaults.ErpSalesRepErpAccountMapBySalesRepIdCacheKey,entity.ErpSalesRepId);
        await base.ClearCacheAsync(entity, entityEventType);
    }
}
