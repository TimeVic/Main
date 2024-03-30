using Api.Requests.Abstractions;
using Autofac;
using Microsoft.Extensions.Logging;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Mvc.Controllers;
using TimeTracker.Business.FileStorage.Services.Api;

namespace TimeTracker.Business.FileStorage.Mvc.Controllers;

public class BaseStorageController: MainApiControllerBase
{
    public BaseStorageController(ILifetimeScope scope) : base(scope)
    {
        var securityService = scope.Resolve<IFileStorageSecurityService>();    
        securityService.CheckIsAuthenticated().Wait();
    }
}
