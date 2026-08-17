using Autofac;
using TimeTracker.Business.Services.Entity;
using TimeTracker.WorkerServices.Core;

namespace TimeTracker.WorkerServices.Services.TimeEntry;

internal class AntiForgetTimerHostedService : ABackgroundService
{
    private readonly IAntiForgetTimerService _antiForgetTimerService;

    public AntiForgetTimerHostedService() : base()
    {
        _antiForgetTimerService = DiScope.Resolve<IAntiForgetTimerService>();
        ServiceName = nameof(AntiForgetTimerHostedService);
    }

    protected override Task DoWorkAsync(CancellationToken cancellationToken)
    {
        return _antiForgetTimerService.CheckActiveTimersAsync(DateTime.UtcNow, cancellationToken);
    }

    protected override string GetCrontabExpression() => "*/15 * * * *";
}
