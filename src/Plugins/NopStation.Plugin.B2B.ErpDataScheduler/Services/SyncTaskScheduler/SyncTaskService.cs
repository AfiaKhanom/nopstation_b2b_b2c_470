using Nop.Data;
using NopStation.Plugin.B2B.ErpDataScheduler.Domain;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskScheduler
{
    /// <summary>
    /// Task service
    /// </summary>
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

        /// <summary>
        /// Deletes a task
        /// </summary>
        /// <param name="task">Task</param>
        public virtual async Task DeleteTaskAsync(SyncTask task)
        {
            await _taskRepository.DeleteAsync(task, false);
        }

        /// <summary>
        /// Gets a task
        /// </summary>
        /// <param name="taskId">Task identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the schedule task
        /// </returns>
        public virtual async Task<SyncTask> GetTaskByIdAsync(int taskId)
        {
            return await _taskRepository.GetByIdAsync(taskId, _ => default);
        }

        /// <summary>
        /// Gets a task by its type
        /// </summary>
        /// <param name="type">Task type</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the schedule task
        /// </returns>
        public virtual async Task<SyncTask> GetTaskByTypeAsync(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return null;

            return await _taskRepository.Table.Where(st => st.Type == type).OrderByDescending(t => t.Id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Gets all tasks
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of schedule task
        /// </returns>
        public virtual async Task<IList<SyncTask>> GetAllTasksAsync()
        {
            return await _taskRepository.GetAllAsync(query =>
            {
                query = query.OrderByDescending(t => t.LastEnabledUtc);
                return query;
            });
        }

        /// <summary>
        /// Inserts a task
        /// </summary>
        /// <param name="task">Task</param>
        public virtual async Task InsertTaskAsync(SyncTask task)
        {
            if (task is null)
                throw new ArgumentNullException(nameof(task));

            if (task is { Enabled: true, LastEnabledUtc: null })
                task.LastEnabledUtc = DateTime.UtcNow;

            await _taskRepository.InsertAsync(task, false);
        }

        /// <summary>
        /// Updates the task
        /// </summary>
        /// <param name="task">Task</param>
        public virtual async Task UpdateTaskAsync(SyncTask task)
        {
            if (task is null)
                throw new ArgumentNullException(nameof(task));

            await _taskRepository.UpdateAsync(task, false);
        }

        #endregion
    }
}