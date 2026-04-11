using Fluxor;
using TimeTracker.Web.Core.Helpers;
using TimeTracker.Web.Store.Auth;

namespace TimeTracker.Web.Services.DateTimes;

public class UserTimeZoneProviderService: IDisposable
{
    private readonly IState<AuthState> _authState;
    private TimeZoneInfo _userTimeZone;

    public UserTimeZoneProviderService(
        IState<AuthState> authState
    )
    {
        _authState = authState;
        _userTimeZone = TimeZoneInfo.Local;
        SetUserTimeZone();
        
        _authState.StateChanged += OnAuthStateChanged;
    }

    public TimeZoneInfo GetTimeZone()
    {
        return _userTimeZone;
    }
    
    private void OnAuthStateChanged(object? sender, EventArgs e)
    {
        SetUserTimeZone();
    }

    private void SetUserTimeZone()
    {
        _userTimeZone = _authState.Value.Workspace?.TimeZone != null
            ? TimeZoneInfo.FindSystemTimeZoneById(_authState.Value.Workspace.TimeZone)
            : TimeZoneInfo.Local;
    }

    public void Dispose()
    {
        _authState.StateChanged -= OnAuthStateChanged;
    }
}
