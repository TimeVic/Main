using NCrontab;
using Persistence.Transactions.Behaviors;

namespace TimeTracker.WorkerServices.Core;

public abstract class ABackgroundService: BackgroundService
{
    protected readonly ILogger<ABackgroundService> _logger;
    private readonly CrontabSchedule _crontabScheduler;

    protected string ServiceName = "BackgroundService";
    
    private DateTime _nextTickTime;
    protected readonly IServiceScope JobScope;
    protected IServiceProvider ServiceProvider { get; set; }
    protected readonly IDbSessionProvider DbSessionProvider;
    
    private bool _isShouldRunWork
    {
        get => DateTime.UtcNow > _nextTickTime;
    }

    public ABackgroundService(
        ILogger<ABackgroundService> logger,
        IServiceScopeFactory serviceScopeFactory
    )
    {
        _logger = logger;
        JobScope = serviceScopeFactory.CreateScope();
        ServiceProvider = JobScope.ServiceProvider;
        DbSessionProvider = ServiceProvider.GetService<IDbSessionProvider>();
        _crontabScheduler = CrontabSchedule.Parse(
            GetCrontabExpression()
        );
        UpdateNextTickTime();
    }
    
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        LogDebug("Processing Hosted Service is starting.");

        cancellationToken.Register(() =>
        {
            LogDebug($"Processing Hosted Service is stopping because cancelled.");
        });

        Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_isShouldRunWork)
                {
                    var startTime = DateTime.UtcNow;
                    try
                    {
                        await DoWorkAsync(cancellationToken);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, e.Message);
                    }

                    var difference = DateTime.UtcNow - startTime;
                    LogDebug("Duration of work: " + difference.ToString("g"));
                    UpdateNextTickTime();
                }

                Thread.Sleep(1000);
            }
        }, cancellationToken);
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        LogDebug($"Processing Hosted Service is stopping.");
        await base.StopAsync(stoppingToken);
    }

    private void UpdateNextTickTime()
    {
        _nextTickTime = _crontabScheduler.GetNextOccurrence(DateTime.UtcNow, DateTime.MaxValue);
        LogDebug($"Next work scheduled at: {_nextTickTime}");
    }

    protected void LogDebug(string message)
    {
        _logger.LogDebug($"{ServiceName}: {message}");
    }

    public override void Dispose()
    {
        base.Dispose();
        JobScope.Dispose();
    }
    
    protected virtual string GetCrontabExpression() => "* * * * *";

    protected abstract Task DoWorkAsync(CancellationToken cancellationToken);
}
