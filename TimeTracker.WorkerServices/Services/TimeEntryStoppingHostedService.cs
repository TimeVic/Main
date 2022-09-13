using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Services.Queue;
using TimeTracker.Business.Services.TimeEntry;
using TimeTracker.WorkerServices.Core;

namespace TimeTracker.WorkerServices.Services
{
    internal class TimeEntryStoppingHostedService : ABackgroundService
    {
        private readonly ITimeEntryService _timeEntryService;
        
        public TimeEntryStoppingHostedService(
            ILogger<ABackgroundService> logger,
            ITimeEntryService timeEntryService
        ) : base(logger)
        {
            _timeEntryService = timeEntryService;
            ServiceName = "NotificationProcessingHostedService";
        }

        protected virtual string GetCrontabExpression() => "1 0 * * *";
        
        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            LogDebug($"Time entry processing worker started at: {DateTime.Now}");
            while (!cancellationToken.IsCancellationRequested)
            {
                await _timeEntryService.StopActiveEntriesFromPastDayAsync();
                await Task.Delay(1000, cancellationToken);
            }
        }
    }
}
