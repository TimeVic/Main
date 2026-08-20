using Autofac;
using TimeTracker.Business.Services.Entity;
using TimeTracker.WorkerServices.Core;

namespace TimeTracker.WorkerServices.Services.TimeEntry;

internal class AntiForgetTimerHostedService : ABackgroundService
{
    private readonly IAntiForgetTimerService _antiForgetTimerService;

    public AntiForgetTimerHostedService(ILifetimeScope rootScope) : base(rootScope)
    {
        _antiForgetTimerService = DiScope.Resolve<IAntiForgetTimerService>();
    }

    protected override Task DoWorkAsync(CancellationToken cancellationToken)
    {
        return _antiForgetTimerService.CheckActiveTimersAsync(DateTime.UtcNow, cancellationToken);
    }

    protected override string GetCrontabExpression() => "*/15 * * * *";
}
