using Autofac;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Services.Queue;
using TimeTracker.WorkerServices.Core;

namespace TimeTracker.WorkerServices.Services.Queue
{
    internal class NotificationProcessingHostedService : ABackgroundService
    {
        private readonly IQueueService _queueService;

        public NotificationProcessingHostedService() : base()
        {
            _queueService = DiScope.Resolve<IQueueService>();
            ServiceName = "NotificationProcessingHostedService";
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            Log($"Worker started at: {DateTime.Now}");
            while (!cancellationToken.IsCancellationRequested)
            {
                await _queueService.ProcessAsync(QueueChannel.Notifications, cancellationToken);
                await DbSessionProvider.PerformCommitAsync(true, cancellationToken);
                DbSessionProvider.CurrentSession.Clear();
                await Task.Delay(1000, cancellationToken);
            }
        }
    }
}
