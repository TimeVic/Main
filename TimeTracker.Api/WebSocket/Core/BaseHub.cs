using Autofac;
using Microsoft.AspNetCore.SignalR;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Dao;

namespace TimeTracker.Api.WebSocket.Core;

public class BaseHub: Hub
{
    private readonly ILifetimeScope _rootScope;
    protected readonly ILogger<BaseHub> _logger;

    public BaseHub(ILifetimeScope rootScope)
    {
        _rootScope = rootScope;
        _logger = _rootScope.Resolve<ILogger<BaseHub>>();
    }
    
    protected async Task ExecuteInScopeAsync(Func<IServiceProvider, Task> action) 
    {
        await using var scope = _rootScope.BeginLifetimeScope();

        var sp = scope.Resolve<IServiceProvider>();
        
        var db = sp.GetRequiredService<IDbSessionProvider>();
        var queue = sp.GetRequiredService<IQueueDao>();
        
        try
        {
            db.SetTransactional();
            await action(sp);
            await queue.Flush();
            await db.PerformCommitAsync();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            queue.Clear();
            await db.RollbackCommitAsync();
            throw;
        }
    }
}
