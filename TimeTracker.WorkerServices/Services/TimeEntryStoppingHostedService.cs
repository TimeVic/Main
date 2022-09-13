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
            ILogger<TimeEntryStoppingHostedService> logger,
            ITimeEntryService timeEntryService
        ) : base(logger)
        {
            _timeEntryService = timeEntryService;
            ServiceName = "NotificationProcessingHostedService";
        }

        protected virtual string GetCrontabExpression() => "* * * * *";
        
        protected override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            await _timeEntryService.StopActiveEntriesFromPastDayAsync(cancellationToken);
        }
    }
}
