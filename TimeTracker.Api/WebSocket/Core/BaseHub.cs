using Autofac;
using Microsoft.AspNetCore.SignalR;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Dao.Messaging;
using TimeTracker.Business.Orm.Dao.User;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Services.Auth;

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
    
    private async Task<UserEntity> GetCurrentUser(IServiceProvider serviceProvider)
    {
        var authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();
        var systemUserDao = serviceProvider.GetRequiredService<IUserDao>();
        var loggedUserUid = authorizationService.GetCurrentLoggedInUserUid();
        if (loggedUserUid != null)
        {
            return (await systemUserDao.GetById(loggedUserUid.Value))!;
        }
        throw new UnauthorizedAccessException();
    }

    public override async Task OnConnectedAsync()
    {
        await ExecuteInScopeAsync(async sp =>
        {
            var messagingDao = sp.GetRequiredService<IMessagingDao>();
            var currentUser = await GetCurrentUser(sp);
            await messagingDao.SetConnection(currentUser, Context.ConnectionId);
        });
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await ExecuteInScopeAsync(async sp =>
        {
            var messagingDao = sp.GetRequiredService<IMessagingDao>();
            var currentUser = await GetCurrentUser(sp);
            await messagingDao.DeleteConnection(currentUser, Context.ConnectionId);
        });
    }
}
