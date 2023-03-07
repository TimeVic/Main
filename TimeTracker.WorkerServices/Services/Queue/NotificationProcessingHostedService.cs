using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Services.Queue;
using TimeTracker.WorkerServices.Core;

namespace TimeTracker.WorkerServices.Services.Queue
{
    internal class NotificationProcessingHostedService : ABackgroundService
    {
        private readonly IQueueService _queueService;
        private readonly IDbSessionProvider _dbSessionProvider;
        private readonly IServiceScope _scope;

        public NotificationProcessingHostedService(
            ILogger<ABackgroundService> logger,
            IServiceScopeFactory serviceScopeFactory
        ) : base(logger)
        {
            _scope = serviceScopeFactory.CreateScope();
            _queueService = _scope.ServiceProvider.GetService<IQueueService>();
            _dbSessionProvider = _scope.ServiceProvider.GetService<IDbSessionProvider>();
            ServiceName = "NotificationProcessingHostedService";
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            LogDebug($"Worker started at: {DateTime.Now}");
            while (!cancellationToken.IsCancellationRequested)
            {
                await _queueService.ProcessAsync(QueueChannel.Notifications, cancellationToken);
                await _dbSessionProvider.PerformCommitAsync(cancellationToken);
                _dbSessionProvider.CurrentSession.Clear();
                await Task.Delay(1000, cancellationToken);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _scope.Dispose();
        }
    }
}
