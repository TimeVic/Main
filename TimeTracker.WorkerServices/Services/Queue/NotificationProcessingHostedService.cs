using Autofac;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Services.Queue;
using TimeTracker.WorkerServices.Core;

namespace TimeTracker.WorkerServices.Services.Queue
{
    internal class NotificationProcessingHostedService : ABackgroundService
    {
        private readonly IQueueService _queueService;

        protected override bool IsContinuous => true;
        protected override bool IsEnableLogging => false;

        public NotificationProcessingHostedService(ILifetimeScope rootScope) : base(rootScope)
        {
            _queueService = DiScope.Resolve<IQueueService>();
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            await _queueService.ProcessAsync(QueueChannel.Notifications, cancellationToken);
        }
    }
}
