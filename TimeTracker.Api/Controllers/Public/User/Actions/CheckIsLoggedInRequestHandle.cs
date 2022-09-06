using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;

namespace TimeTracker.Api.Controllers.Public.User.Actions;

public class CheckIsLoggedInRequestHandle : IAsyncRequestHandler<CheckIsLoggedInRequest>
{
    public Task ExecuteAsync(CheckIsLoggedInRequest request)
    {
        return Task.CompletedTask;
    }
}
