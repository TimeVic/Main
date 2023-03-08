using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Services.Storage;
using TimeTracker.WorkerServices.Core;

namespace TimeTracker.WorkerServices.Services
{
    internal class ImageUploadingHostedService : ABackgroundService
    {
        private readonly IFileStorage _fileStorage;

        public ImageUploadingHostedService(
            ILogger<ImageUploadingHostedService> logger,
            IServiceScopeFactory serviceScopeFactory
        ) : base(logger, serviceScopeFactory)
        {
            _fileStorage = ServiceProvider.GetService<IFileStorage>();
            ServiceName = "ImageUploadingHostedService";
        }

        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            LogDebug($"Image uploading worker started at: {DateTime.Now}");
            while (!cancellationToken.IsCancellationRequested)
            {
                await _fileStorage.UploadFirstPendingToCloud(cancellationToken);
                await DbSessionProvider.PerformCommitAsync(cancellationToken);
                DbSessionProvider.CurrentSession.Clear();
                await Task.Delay(500, cancellationToken);
            }
        }
    }
}
