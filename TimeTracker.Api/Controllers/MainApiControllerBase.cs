using System.Net;
using System.Security.Authentication;
using Api.Requests.Abstractions;
using AspNetCore.ApiControllers.Abstractions;
using Domain.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Exceptions;
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
        var response = new BadResponseModel();
        var statusCode = (int) HttpStatusCode.BadRequest;
        if (exception is AuthenticationException)
        {
            response.Type = exception.GetType().Name;
            response.Message = "User not authorized exception";
            statusCode = (int)HttpStatusCode.Unauthorized;
        }
        else if (exception is IDomainException)
        {
            response.Type = exception.GetType().Name;
            response.Message = exception.Message;
        }
        else
        {
            Logger.LogError(exception, exception.Message);
            statusCode = (int)HttpStatusCode.InternalServerError;
            response.Message = "Server error";
        }
            
        var badResponse = new BadRequestObjectResult(response);
        badResponse.StatusCode = statusCode;
        return badResponse;
    }
}
