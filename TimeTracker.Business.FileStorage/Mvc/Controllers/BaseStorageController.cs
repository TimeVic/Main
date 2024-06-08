using Api.Requests.Abstractions;
using Autofac;
using Microsoft.Extensions.Logging;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.FileStorage.Services.Api;
using TimeTracker.Business.Mvc.Controllers;

namespace TimeTracker.Business.FileStorage.Mvc.Controllers;

public class BaseStorageController: MainApiControllerBase
{
    public BaseStorageController(ILifetimeScope scope) : base(scope)
    {
        // var securityService = scope.Resolve<IFileStorageSecurityService>();    
        // securityService.CheckIsAuthenticated().Wait();
    }
}
