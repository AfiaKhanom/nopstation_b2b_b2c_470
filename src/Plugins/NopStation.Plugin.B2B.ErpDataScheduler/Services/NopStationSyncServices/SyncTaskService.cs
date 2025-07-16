using Nop.Data;
using NopStation.Plugin.B2B.ErpDataScheduler.Domain;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.NopStationSyncServices;

public partial class SyncTaskService : ISyncTaskService
{
    #region Fields

    private readonly IRepository<SyncTask> _taskRepository;

    #endregion

    #region Ctor

    public SyncTaskService(IRepository<SyncTask> taskRepository)
    {
        _taskRepository = taskRepository;
    }

    #endregion

    #region Methods

    public virtual async Task DeleteTaskAsync(SyncTask task)
    {
        await _taskRepository.DeleteAsync(task, false);
    }

    public virtual async Task<SyncTask> GetTaskByIdAsync(int taskId)
    {
        return await _taskRepository.GetByIdAsync(taskId, _ => default);
    }

    public virtual async Task<SyncTask> GetTaskByTypeAsync(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return null;

        return await _taskRepository.Table.Where(st => st.Type == type).OrderByDescending(t => t.Id).FirstOrDefaultAsync();
    }

    public virtual async Task<IList<SyncTask>> GetAllTasksAsync()
    {
        return await _taskRepository.GetAllAsync(query =>
        {
            query = query.OrderByDescending(t => t.LastEnabledUtc);
            return query;
        });
    }

    public virtual async Task InsertTaskAsync(SyncTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        if (task is { Enabled: true, LastEnabledUtc: null })
            task.LastEnabledUtc = DateTime.UtcNow;

        await _taskRepository.InsertAsync(task, false);
    }

    public virtual async Task UpdateTaskAsync(SyncTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        await _taskRepository.UpdateAsync(task, false);
    }

    public async Task<SyncTask> GetTaskByQuartzJobNameAsync(string quartzJobName)
    {
        return await _taskRepository.Table.Where(x => x.QuartzJobName == quartzJobName)
            .FirstOrDefaultAsync();
    }

    #endregion
}