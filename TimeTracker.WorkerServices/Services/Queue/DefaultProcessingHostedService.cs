using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Services.Queue;
using TimeTracker.WorkerServices.Core;

namespace TimeTracker.WorkerServices.Services.Queue
{
    internal class DefaultProcessingHostedService : ABackgroundService
    {
        private readonly IQueueService _queueService;
        private readonly IDbSessionProvider _dbSessionProvider;
        private readonly IServiceScope _scope;

        public DefaultProcessingHostedService(
            ILogger<ABackgroundService> logger,
            IServiceScopeFactory serviceScopeFactory
        ) : base(logger)
        {
            _scope = serviceScopeFactory.CreateScope();
            _queueService = _scope.ServiceProvider.GetService<IQueueService>();
            _dbSessionProvider = _scope.ServiceProvider.GetService<IDbSessionProvider>();
            ServiceName = "DefaultProcessingHostedService";
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            LogDebug($"Worker started at: {DateTime.Now}");
            while (!cancellationToken.IsCancellationRequested)
            {
                await _queueService.ProcessAsync(QueueChannel.Default, cancellationToken);
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
