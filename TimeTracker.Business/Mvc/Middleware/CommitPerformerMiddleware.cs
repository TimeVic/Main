using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Dao;

namespace TimeTracker.Business.Mvc.Middleware;

public class CommitPerformerMiddleware : ActionFilterAttribute
{
    private readonly RequestDelegate _next;

    public CommitPerformerMiddleware(
        RequestDelegate next
    )
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILifetimeScope scope)
    {
        var dbSessionProvider = scope.Resolve<IDbSessionProvider>();
        var queueDao = scope.Resolve<IQueueDao>();
        try
        {
            dbSessionProvider.SetTransactional();

            await _next(context);

            queueDao.Flush();
            await dbSessionProvider.PerformCommitAsync();
        }
        catch (Exception)
        {
            queueDao.Clear();
            await dbSessionProvider.RollbackCommitAsync();
            throw;
        }
        finally
        {
            dbSessionProvider.Dispose();    
        }
    }
}
