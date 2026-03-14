using Autofac;
using Domain.Abstractions.Api;
using NHibernate;
using Persistence.Transactions.Behaviors;

namespace TimeTracker.Business.Orm.Dao.Common;

public abstract class BaseDao
{
    private readonly IDbSessionProvider _dbSessionProvider;
    protected readonly IBaseApiRequestService? _apiRequestService;

    protected ISession Session => _dbSessionProvider.CurrentSession;
    
    protected BaseDao(ILifetimeScope scope)
    {
        _dbSessionProvider = scope.Resolve<IDbSessionProvider>();
        scope.TryResolve(out IBaseApiRequestService? apiRequestService);
        _apiRequestService = apiRequestService;
    }
}
