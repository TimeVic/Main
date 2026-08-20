using Autofac;
using TimeTracker.Business.Services.Notification;
using TimeTracker.WorkerServices.Core;

namespace TimeTracker.WorkerServices.Services.Tasks
{
    internal class TaskNotificationHostedService : ABackgroundService
    {
        private readonly ITaskNotificationService _taskNotificationService;

        protected override bool IsContinuous => true;
        protected override bool IsEnableLogging => false;

        // Poll every 5 seconds — same cadence as the original inner loop delay
        protected override int GetPollingIntervalMs() => 5000;

        public TaskNotificationHostedService(ILifetimeScope rootScope) : base(rootScope)
        {
            _taskNotificationService = DiScope.Resolve<ITaskNotificationService>();
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            await _taskNotificationService.NotifyAboutTaskChanges();
            await _taskNotificationService.SendReminderNotification();
        }
    }
}
