using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.Storage;
using TimeTracker.WorkerServices.Core;

namespace TimeTracker.WorkerServices.Services
{
    internal class ImageUploadingHostedService : ABackgroundService
    {
        private readonly IFileStorage _fileStorage;
        private readonly IDbSessionProvider _dbSessionProvider;

        public ImageUploadingHostedService(
            ILogger<ABackgroundService> logger,
            IFileStorage fileStorage,
            IDbSessionProvider dbSessionProvider
        ) : base(logger)
        {
            _fileStorage = fileStorage;
            _dbSessionProvider = dbSessionProvider;
            ServiceName = "NotificationProcessingHostedService";
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            LogDebug($"Image uploading worker started at: {DateTime.Now}");
            while (!cancellationToken.IsCancellationRequested)
            {
                await _fileStorage.UploadFirstPendingToCloud(cancellationToken);
                await _dbSessionProvider.PerformCommitAsync(cancellationToken);
                _dbSessionProvider.CurrentSession.Clear();
                await Task.Delay(500, cancellationToken);
            }
        }
    }
}
