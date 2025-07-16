using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Services.Caching;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services.Caching;

public class ProductCacheEventConsumer : CacheEventConsumer<Product>
{
    protected override async Task ClearCacheAsync(Product entity, EntityEventType entityEventType)
    {
        await base.ClearCacheAsync(entity, entityEventType);
    }
}
