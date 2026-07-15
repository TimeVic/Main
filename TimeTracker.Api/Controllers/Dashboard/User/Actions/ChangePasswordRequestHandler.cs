using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Users;
using TimeTracker.Business.Common.Exceptions.Api.Auth;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Http;
using Persistence.Transactions.Behaviors;

namespace TimeTracker.Api.Controllers.Dashboard.User.Actions;

public class ChangePasswordRequestHandler : IAsyncRequestHandler<ChangePasswordRequest>
{
    private readonly IApiRequestService _apiRequestService;
    private readonly IPasswordService _passwordService;
    private readonly IDbSessionProvider _sessionProvider;

    public ChangePasswordRequestHandler(
        IApiRequestService apiRequestService,
        IPasswordService passwordService,
        IDbSessionProvider sessionProvider)
    {
        _apiRequestService = apiRequestService;
        _passwordService = passwordService;
        _sessionProvider = sessionProvider;
    }

    public async Task ExecuteAsync(ChangePasswordRequest request)
    {
        var user = await _apiRequestService.GetCurrentUser();
        if (!_passwordService.ValidatePassword(user, request.CurrentPassword))
        {
            throw new UserNotAuthorizedException();
        }

        user = _passwordService.SetUserPassword(user, request.NewPassword);
        await _sessionProvider.CurrentSession.SaveAsync(user);
    }
}
