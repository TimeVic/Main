using System.Net;
using System.Security.Authentication;
using Domain.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Dto;
using TimeTracker.Business.Common.Exceptions;
using TimeTracker.Business.Common.Exceptions.Api;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Business.Mvc.Filters;

public class ExceptionHandlerActionFilter : ActionFilterAttribute
{
    private readonly ILogger<ExceptionHandlerActionFilter> _logger;
    private readonly IApiRequestService _apiRequestService;

    public ExceptionHandlerActionFilter(
        ILogger<ExceptionHandlerActionFilter> logger,
        IApiRequestService apiRequestService
    )
    {
        _logger = logger;
        _apiRequestService = apiRequestService;
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        var exception = context.Exception;
        if (exception != null)
        {
            var response = new JsonCommonResponse
            {
                Status = HttpResponseStatus.Fail
            };
            var statusCode = (int) HttpStatusCode.BadRequest;
            if (exception is AuthenticationException)
            {
                response.ErrorCode = exception.GetType().Name;
                response.Message = "User not authorized(action executing)";
                statusCode = (int)HttpStatusCode.Unauthorized;
            }
            else if (exception is ForbiddenException)
            {
                response.ErrorCode = exception.GetType().Name;
                response.Message = exception.Message;
                statusCode = (int)HttpStatusCode.Forbidden;
            }
            else if (exception is IDomainException)
            {
                response.ErrorCode = exception.GetType().Name;
                response.Message = exception.Message;
            }
            else
            {
                _logger.LogError(
                    exception,
                    exception.Message,
                    new Dictionary<string, object>
                    {
                        { "LoggedInUserId", _apiRequestService.GetCurrentUserId().ToString() },
                        { "RequestUrl", _apiRequestService.GetRequestUrl() ?? string.Empty },
                    }
                );
                statusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "Server error";
            }
            
            var badResponse = new BadRequestObjectResult(response)
            {
                StatusCode = statusCode
            };
            context.Result = badResponse;
            context.ExceptionHandled = true;
        }
        
        // Code to execute after the action
        base.OnActionExecuted(context);
    }
}
