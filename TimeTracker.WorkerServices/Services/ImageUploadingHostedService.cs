using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Services.Storage;
using TimeTracker.WorkerServices.Core;

namespace TimeTracker.WorkerServices.Services
{
    internal class ImageUploadingHostedService : ABackgroundService
    {
        private readonly IFileStorage _fileStorage;
        private readonly IDbSessionProvider _dbSessionProvider;
        private readonly IServiceScope _scope;

        public ImageUploadingHostedService(
            ILogger<ABackgroundService> logger,
            IServiceScopeFactory serviceScopeFactory
        ) : base(logger)
        {
            _scope = serviceScopeFactory.CreateScope();
            _fileStorage = _scope.ServiceProvider.GetService<IFileStorage>();
            _dbSessionProvider = _scope.ServiceProvider.GetService<IDbSessionProvider>();
            ServiceName = "ImageUploadingHostedService";
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            LogDebug($"Image uploading worker started at: {DateTime.Now}");
            while (!cancellationToken.IsCancellationRequested)
            {
                await _fileStorage.UploadFirstPendingToCloud(cancellationToken);
                await _dbSessionProvider.PerformCommitAsync(cancellationToken);
                await Task.Delay(500, cancellationToken);
            }
        }
        
        public override void Dispose()
        {
            base.Dispose();
            _scope.Dispose();
        }
    }
}
