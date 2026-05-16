using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Services.Http;

namespace TimeTracker.Api.Controllers.Public.User.Actions;

public class LogoutRequestHandler : IAsyncRequestHandler<LogoutRequest>
{
    private readonly IHttpCookiesService _cookiesService;

    public LogoutRequestHandler(IHttpCookiesService cookiesService)
    {
        _cookiesService = cookiesService;
    }

    public Task ExecuteAsync(LogoutRequest request)
    {
        _cookiesService.CleanUpAuthCookies();
        return Task.CompletedTask;
    }
}
