using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Services.Queue;
using TimeTracker.WorkerServices.Core;

namespace TimeTracker.WorkerServices.Services
{
    internal class QueueProcessingHostedService : ABackgroundService
    {
        private readonly IQueueService _queueService;

        public QueueProcessingHostedService(
            ILogger<ABackgroundService> logger,
            IQueueService queueService
        ) : base(logger)
        {
            _queueService = queueService;
            ServiceName = "NotificationProcessingHostedService";
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            LogDebug($"Notifications processing worker started at: {DateTime.Now}");
            while (!cancellationToken.IsCancellationRequested)
            {
                await _queueService.ProcessAsync(QueueChannel.Default, cancellationToken);
                await _queueService.ProcessAsync(QueueChannel.Notifications, cancellationToken);
                await _queueService.ProcessAsync(QueueChannel.ExternalClient, cancellationToken);
                await Task.Delay(1000, cancellationToken);
            }
        }
    }
}
