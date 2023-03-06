using Autofac;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Services.Queue;
using TimeTracker.WorkerServices.Core;

namespace TimeTracker.WorkerServices.Services
{
    internal class QueueProcessingHostedService : ABackgroundService
    {
        private readonly IQueueService _queueService;
        private readonly IDbSessionProvider _dbSessionProvider;
        private readonly IServiceScope _scope;

        public QueueProcessingHostedService(
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
            LogDebug($"Notifications processing worker started at: {DateTime.Now}");
            while (!cancellationToken.IsCancellationRequested)
            {
                await _queueService.ProcessAsync(QueueChannel.Default, cancellationToken);
                await _queueService.ProcessAsync(QueueChannel.Notifications, cancellationToken);
                await _queueService.ProcessAsync(QueueChannel.ExternalClient, cancellationToken);
                await _dbSessionProvider.PerformCommitAsync(cancellationToken);
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
