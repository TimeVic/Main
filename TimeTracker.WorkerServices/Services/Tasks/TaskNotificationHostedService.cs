using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Services.Notification;
using TimeTracker.Business.Services.Queue;
using TimeTracker.WorkerServices.Core;

namespace TimeTracker.WorkerServices.Services.Tasks
{
    internal class TaskNotificationHostedService : ABackgroundService
    {
        private readonly ITaskNotificationService _taskNotificationService;

        public TaskNotificationHostedService(
            ILogger<ABackgroundService> logger,
            IServiceScopeFactory serviceScopeFactory
        ) : base(logger, serviceScopeFactory)
        {
            _taskNotificationService = ServiceProvider.GetService<ITaskNotificationService>();
            ServiceName = "TaskNotificationHostedService";
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            LogDebug($"Worker started at: {DateTime.Now}");
            while (!cancellationToken.IsCancellationRequested)
            {
                await _taskNotificationService.NotifyAboutTaskChanges();
                await DbSessionProvider.PerformCommitAsync(cancellationToken);
                DbSessionProvider.CurrentSession.Clear();
                await Task.Delay(5000, cancellationToken);
            }
        }
    }
}
