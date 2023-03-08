using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Services.Queue;
using TimeTracker.WorkerServices.Core;

namespace TimeTracker.WorkerServices.Services.Queue
{
    internal class NotificationProcessingHostedService : ABackgroundService
    {
        private readonly IQueueService _queueService;

        public NotificationProcessingHostedService(
            ILogger<NotificationProcessingHostedService> logger,
            IServiceScopeFactory serviceScopeFactory
        ) : base(logger, serviceScopeFactory)
        {
            _queueService = ServiceProvider.GetService<IQueueService>();
            ServiceName = "NotificationProcessingHostedService";
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            LogDebug($"Worker started at: {DateTime.Now}");
            while (!cancellationToken.IsCancellationRequested)
            {
                await _queueService.ProcessAsync(QueueChannel.Notifications, cancellationToken);
                await DbSessionProvider.PerformCommitAsync(cancellationToken);
                DbSessionProvider.CurrentSession.Clear();
                await Task.Delay(1000, cancellationToken);
            }
        }
    }
}
