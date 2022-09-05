using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Services.Queue;
using TimeTracker.WorkerServices.Core;

namespace TimeTracker.WorkerServices.Services
{
    internal class NotificationProcessingHostedService : ABackgroundService
    {
        private readonly IQueueService _queueService;

        public NotificationProcessingHostedService(
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
                await _queueService.Process(QueueChannel.Default, cancellationToken);
                await _queueService.Process(QueueChannel.Notifications, cancellationToken);
                await Task.Delay(1000, cancellationToken);
            }
        }
    }
}
