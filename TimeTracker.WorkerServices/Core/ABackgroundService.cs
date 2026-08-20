using Autofac;
using NCrontab;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Dao;
using Microsoft.Extensions.Logging;

namespace TimeTracker.WorkerServices.Core;

public abstract class ABackgroundService : BackgroundService
{
    protected readonly ILogger<ABackgroundService> _logger;
    private readonly CrontabSchedule _crontabScheduler;

    protected string ServiceName = "BackgroundService";
    protected readonly IQueueDao QueueDao;

    private DateTime _nextTickTime;
    protected ILifetimeScope DiScope { get; set; }
    protected readonly IDbSessionProvider DbSessionProvider;

    // When true, DoWorkAsync is called every GetPollingIntervalMs() regardless of cron schedule.
    // When false, the cron expression controls execution timing.
    protected virtual bool IsContinuous => false;

    private bool _isShouldRunWork => IsContinuous || DateTime.UtcNow > _nextTickTime;

    protected virtual bool IsEnableLogging { get; set; } = true;

    protected ABackgroundService(ILifetimeScope rootScope)
    {
        // Create a child scope per service so each service gets its own IDbSessionProvider
        // while sharing a single NHibernate SessionFactory (registered as SingleInstance).
        DiScope = rootScope.BeginLifetimeScope();

        _logger = DiScope.Resolve<ILogger<ABackgroundService>>();
        QueueDao = DiScope.Resolve<IQueueDao>();
        DbSessionProvider = DiScope.Resolve<IDbSessionProvider>();

        _crontabScheduler = CrontabSchedule.Parse(GetCrontabExpression());
        UpdateNextTickTime();
        ServiceName = GetType().Name;
    }

    protected virtual int GetPollingIntervalMs() => 1000;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log("Processing Hosted Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            if (_isShouldRunWork)
            {
                var startTime = DateTime.UtcNow;
                try
                {
                    await DoWorkAsync(stoppingToken);
                    await QueueDao.Flush();
                    await DbSessionProvider.PerformCommitAsync(true, stoppingToken);
                }
                catch (Exception e)
                {
                    QueueDao.Clear();
                    _logger.LogError(e, e.Message);
                }
                finally
                {
                    DbSessionProvider.CloseCurrentSession();
                }

                if (IsEnableLogging)
                    Log("Duration of work: " + (DateTime.UtcNow - startTime).ToString("g"));

                if (!IsContinuous)
                    UpdateNextTickTime();
            }

            try
            {
                await Task.Delay(GetPollingIntervalMs(), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        Log("Processing Hosted Service is stopping.");
        await base.StopAsync(stoppingToken);
    }

    private void UpdateNextTickTime()
    {
        _nextTickTime = _crontabScheduler.GetNextOccurrence(DateTime.UtcNow, DateTime.MaxValue);
        Log($"Next work scheduled at: {_nextTickTime}");
    }

    protected void Log(string message)
    {
        _logger.LogInformation("{ServiceName}: {Message}", ServiceName, message);
    }

    public override void Dispose()
    {
        DbSessionProvider.Dispose();
        DiScope.Dispose();
        base.Dispose();
    }

    protected virtual string GetCrontabExpression() => "* * * * *";

    protected abstract Task DoWorkAsync(CancellationToken cancellationToken);
}
