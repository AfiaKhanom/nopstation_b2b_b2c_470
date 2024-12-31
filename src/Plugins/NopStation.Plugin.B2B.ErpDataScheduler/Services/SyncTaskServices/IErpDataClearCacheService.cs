using Nop.Core;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public interface IErpDataClearCacheService
{
    Task ClearCacheOfEntity<T>(T entity, int id) where T : BaseEntity;
}