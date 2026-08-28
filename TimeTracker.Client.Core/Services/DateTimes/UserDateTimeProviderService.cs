using Fluxor;
using TimeTracker.Business.Extensions;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Services.DateTimes;

public class UserDateTimeProviderService: IDisposable
{
    private readonly IState<AuthState> _authState;

    public UserDateTimeProviderService(
        IState<AuthState> authState
    )
    {
        _authState = authState;
    }

    /// <summary>
    /// Returns the current moment in the user's workspace timezone as a DateTimeOffset.
    /// The offset (e.g. +05:00) is preserved so Kind is never Unspecified.
    /// </summary>
    public DateTimeOffset GetCurrentTime()
    {
        var tz = GetTimeZone();
        return DateTime.UtcNow.ToDateTimeOffset(tz.Id);
    }
    
    public TimeZoneInfo GetTimeZone()
    {
        var tzId = _authState.Value.Workspace?.TimeZone;
        return ResolveTimeZone(tzId);
    }

    /// <summary>
    /// Resolves timezone by id with UTC fallback.
    /// If id is not provided, uses current workspace timezone.
    /// </summary>
    public TimeZoneInfo ResolveTimeZone(string? timeZoneId = null)
    {
        var tzId = string.IsNullOrWhiteSpace(timeZoneId)
            ? _authState.Value.Workspace?.TimeZone
            : timeZoneId;

        if (string.IsNullOrWhiteSpace(tzId))
        {
            return TimeZoneInfo.Utc;
        }

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(tzId);
        }
        catch (Exception)
        {
            return TimeZoneInfo.Utc;
        }
    }


    /// <summary>
    /// Converts UTC instant to wall-clock value in provided timezone (or workspace timezone).
    /// </summary>
    public DateTime ConvertUtcToWallClock(DateTime value, string? timeZoneId = null)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        var timeZone = ResolveTimeZone(timeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(utc, timeZone);
    }

    public DateTime? ConvertUtcToWallClock(DateTime? value, string? timeZoneId = null)
    {
        return value.HasValue ? ConvertUtcToWallClock(value.Value, timeZoneId) : null;
    }

    public void Dispose()
    {
    }
}
