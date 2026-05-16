using Fluxor;
using TimeTracker.Web.Services;
using TimeTracker.Web.Services.Http;

namespace TimeTracker.Web.Store.Auth.Effects;

public class LoadCurrentUserEffect: Effect<LoadCurrentUserAction>
{
    private readonly ApiService _apiService;
    private readonly ILogger<LoadCurrentUserEffect> _logger;
    private readonly UserLocaleService _userLocaleService;

    public LoadCurrentUserEffect(
        ApiService apiService,
        ILogger<LoadCurrentUserEffect> logger,
        UserLocaleService userLocaleService
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
