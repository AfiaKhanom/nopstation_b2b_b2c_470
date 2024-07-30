using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskScheduler;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Controllers
{
    //do not inherit it from BasePublicController. otherwise a lot of extra action filters will be called
    //they can create guest account(s), etc
    [AutoValidateAntiforgeryToken]
    public partial class SyncTaskController : Controller
    {
        private readonly ISyncTaskService _syncTaskService;
        private readonly ISyncTaskRunner _taskRunner;

        public SyncTaskController(ISyncTaskService taskService,
            ISyncTaskRunner taskRunner)
        {
            _syncTaskService = taskService;
            _taskRunner = taskRunner;
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> RunTask(string taskType)
        {
            var task = await _syncTaskService.GetTaskByTypeAsync(taskType);
            if (task == null)
                //schedule task cannot be loaded
                return NoContent();

            await _taskRunner.ExecuteAsync(task);

            return NoContent();
        }
    }
}