using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Abstractions;
using Domain.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Exceptions.Api;

namespace TimeTracker.Api.Controllers;

public class MainApiControllerBase: ApiControllerBase
{
    protected readonly ILogger<MainApiControllerBase> Logger;

    public MainApiControllerBase(
        IAsyncRequestBuilder asyncRequestBuilder, 
        IDbSessionProvider commitPerformer,
        ILogger<MainApiControllerBase> logger
    ) : base(asyncRequestBuilder, commitPerformer)
    {
        Logger = logger;
    }
        
    public override Func<Exception, IActionResult> Fail => ProcessFail;

    private IActionResult ProcessFail(Exception exception)
    {
        var message = exception?.Message;
        if (exception is not IDomainException)
        {
            Logger.LogError(exception, exception?.Message);
            exception = new ServerException();
        }

        return new BadRequestObjectResult(
            new BadResponseModel(exception)
        );
    }
}
