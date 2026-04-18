using Api.Requests.Abstractions;
using Microsoft.Extensions.Configuration;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Public.User;
using TimeTracker.Business.Notifications.Senders.User;
using TimeTracker.Business.Services.Auth;
using TimeTracker.Business.Services.Queue;

namespace TimeTracker.Api.Controllers.Public.User.Actions;

public class LoginMagicRequestHandler : IAsyncRequestHandler<LoginMagicRequest>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IQueueService _queueService;
    private readonly string? _frontendUrl;

    public LoginMagicRequestHandler(
        IAuthorizationService authorizationService,
        IQueueService queueService,
        IConfiguration configuration
    )
    {
        _authorizationService = authorizationService;
        _queueService = queueService;
        _frontendUrl = configuration.GetValue<string>("App:FrontendUrl");
    }

    public async Task ExecuteAsync(LoginMagicRequest request)
    {
        var magicToken = await _authorizationService.GenerateMagicToken(request.Email);
        await _queueService.PushNotificationAsync(new MagicLoginNotificationItemContext(
            toAddress: magicToken.User.Email,
            frontendUrl: _frontendUrl!,
            token: magicToken.Token
        ));
    }
}
