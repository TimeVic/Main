using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Notifications.Senders.User;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Queue;

namespace TimeTracker.Api.Controllers.Public.User.Actions;

public class LoginMagicRequestHandler : IAsyncRequestHandler<LoginMagicRequest>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IQueueService _queueService;

    public LoginMagicRequestHandler(
        IAuthorizationService authorizationService,
        IQueueService queueService
    )
    {
        _authorizationService = authorizationService;
        _queueService = queueService;
    }

    public async Task ExecuteAsync(LoginMagicRequest request)
    {
        var magicToken = await _authorizationService.GenerateMagicToken(request.Email);
        await _queueService.PushNotificationAsync(new MagicLoginNotificationItemContext(magicToken.Id));
    }
}
