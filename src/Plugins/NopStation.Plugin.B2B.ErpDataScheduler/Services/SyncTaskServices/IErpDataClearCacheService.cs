using Nop.Core;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public interface IErpDataClearCacheService
{
    Task ClearCacheOfEntity<T>(T entity) where T : BaseEntity;
    Task ClearCacheOfEntities<T>(IList<T> entities) where T : BaseEntity;
}