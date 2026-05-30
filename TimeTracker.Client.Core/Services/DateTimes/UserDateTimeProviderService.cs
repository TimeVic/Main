using Fluxor;
using TimeTracker.Business.Extensions;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Services.DateTimes;

public class UserDateTimeProviderService: IDisposable
{
    private readonly IState<AuthState> _authState;
    private TimeZoneInfo _userTimeZone;

    public UserDateTimeProviderService(
        IState<AuthState> authState
    )
    {
        _authState = authState;
        _userTimeZone = TimeZoneInfo.Local;
        SetUserTimeZone();
        
        _authState.StateChanged += OnAuthStateChanged;
    }

    /// <summary>
    /// Returns the current moment in the user's workspace timezone as a DateTimeOffset.
    /// The offset (e.g. +05:00) is preserved so Kind is never Unspecified.
    /// </summary>
    public DateTimeOffset GetCurrentTime()
    {
        return DateTime.UtcNow.ToDateTimeOffset(_userTimeZone.Id);
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
