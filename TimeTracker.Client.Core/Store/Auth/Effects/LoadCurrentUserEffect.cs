using Fluxor;
using TimeTracker.Client.Core.Services;
using TimeTracker.Client.Core.Services.Http;

namespace TimeTracker.Client.Core.Store.Auth.Effects;

public class LoadCurrentUserEffect: Effect<LoadCurrentUserAction>
{
    private readonly IApiService _apiService;
    private readonly ILogger<LoadCurrentUserEffect> _logger;
    private readonly IUserLocaleService _userLocaleService;

    public LoadCurrentUserEffect(
        IApiService apiService,
        ILogger<LoadCurrentUserEffect> logger,
        IUserLocaleService userLocaleService
    )
    {
        _apiService = apiService;
        _logger = logger;
        _userLocaleService = userLocaleService;
    }

    public override async Task HandleAsync(LoadCurrentUserAction action, IDispatcher dispatcher)
    {
        try
        {
            var user = await _apiService.UserGetCurrentAsync();
            if (user == null)
            {
                return;
            }

            dispatcher.Dispatch(new UpdateUserAction(user));
            await _userLocaleService.ApplyUserLocaleAsync(user);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }
}
