using Nop.Core;
using Nop.Core.Caching;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices
{
    public class ErpDataClearCacheService : IErpDataClearCacheService
    {
        private readonly IStaticCacheManager _staticCacheManager;

        public ErpDataClearCacheService(IStaticCacheManager staticCacheManager)
        {
            _staticCacheManager = staticCacheManager;
        }
        public async Task ClearCacheOfEntity<T>(T entity, int id) where T : BaseEntity
        {
            await _staticCacheManager.RemoveAsync(_staticCacheManager.PrepareKeyForDefaultCache(NopEntityCacheDefaults<T>.ByIdCacheKey, id));
        }
    }
}