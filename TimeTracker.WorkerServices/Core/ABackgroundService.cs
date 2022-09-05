using NCrontab;

namespace TimeTracker.WorkerServices.Core;

public abstract class ABackgroundService: BackgroundService
{
    protected readonly ILogger<ABackgroundService> _logger;
    private CancellationToken _cancelationToken;
    private readonly CrontabSchedule _crontabScheduler;

    protected string ServiceName = "BackgroundService";
    
    private DateTime _nextTickTime;
    private bool _isShouldRunWork
    {
        get => DateTime.Now > _nextTickTime;
    }

    public ABackgroundService(ILogger<ABackgroundService> logger)
    {
        _logger = logger;
        _crontabScheduler = CrontabSchedule.Parse(
            GetCrontabExpression()
        );
        UpdateNextTickTime();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _cancelationToken = stoppingToken;
        LogDebug("Processing Hosted Service is starting.");

        stoppingToken.Register(() => LogDebug($"Processing Hosted Service is stopping because cancelled."));

        Task.Run(async () =>
        {
            while (!_cancelationToken.IsCancellationRequested)
            {
                if (_isShouldRunWork)
                {
                    var startTime = DateTime.UtcNow;
                    try
                    {
                        await DoWorkAsync(_cancelationToken);
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
        }, _cancelationToken);
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        LogDebug($"Processing Hosted Service is stopping.");
        await base.StopAsync(stoppingToken);
    }

    private void UpdateNextTickTime()
    {
        _nextTickTime = _crontabScheduler.GetNextOccurrence(DateTime.Now, DateTime.MaxValue);
        LogDebug($"Next work scheduled at: {_nextTickTime}");
    }

    protected void LogDebug(string message)
    {
        _logger.LogDebug($"{ServiceName}: {message}");
    }

    protected virtual string GetCrontabExpression() => "* * * * *";

    protected abstract Task DoWorkAsync(CancellationToken cancellationToken);
}
