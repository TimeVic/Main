using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using TimeTracker.Business.Common.Constants.Http;
using TimeTracker.Client.Core.Services.Http.Cookies;

namespace TimeTracker.Client.Mobile.Services.Http;

public class MobileAuthCookieConfigurator : IAuthCookieConfigurator
{
    private const string JwtCookieNameStorageKey = "TimeVic.Auth.JwtCookieName";
    private const string JwtCookieValueStorageKey = "TimeVic.Auth.JwtCookieValue";
    private const string AccessCookieNameStorageKey = "TimeVic.Auth.AccessCookieName";
    private const string AccessCookieValueStorageKey = "TimeVic.Auth.AccessCookieValue";

    private readonly CookieContainer _cookieContainer;
    private readonly Uri _apiUri;
    private readonly ILogger<MobileAuthCookieConfigurator> _logger;
    private readonly SemaphoreSlim _storageLock = new(1, 1);
    private bool _isRestored;

    public MobileAuthCookieConfigurator(
        CookieContainer cookieContainer,
        IConfiguration configuration,
        ILogger<MobileAuthCookieConfigurator> logger
    )
    {
        _cookieContainer = cookieContainer;
        _logger = logger;
        _apiUri = new Uri(configuration.GetValue<string>("ApiUrl")!);
    }

    public async Task ConfigureRequestAsync(HttpRequestMessage request)
    {
        await RestoreAsync();
    }

    public async Task ProcessResponseAsync(HttpResponseMessage response)
    {
        await RestoreAsync();

        await _storageLock.WaitAsync();
        try
        {
            var changedCookieNames = GetChangedAuthCookieNames(response).ToArray();
            var jwtHeaderToken = GetHeaderValue(response, HttpHeaderKeyEnum.JwtToken.GetKey());

            if (!string.IsNullOrWhiteSpace(jwtHeaderToken))
            {
                var jwtCookieName = await GetStoredValueAsync(JwtCookieNameStorageKey)
                                    ?? FindCookieName(changedCookieNames, HttpCookieKeyEnum.JwtToken.GetKey())
                                    ?? HttpCookieKeyEnum.JwtToken.GetKey();
                AddCookie(jwtCookieName, jwtHeaderToken);
                await SaveCookieAsync(jwtCookieName, jwtHeaderToken, JwtCookieNameStorageKey, JwtCookieValueStorageKey);
            }

            await SyncCookieAsync(
                HttpCookieKeyEnum.JwtToken.GetKey(),
                changedCookieNames,
                JwtCookieNameStorageKey,
                JwtCookieValueStorageKey
            );
            await SyncCookieAsync(
                HttpCookieKeyEnum.AccessToken.GetKey(),
                changedCookieNames,
                AccessCookieNameStorageKey,
                AccessCookieValueStorageKey
            );
        }
        catch (Exception e)
        {
            _logger.LogDebug(e, "Failed to synchronize auth cookies with secure storage");
        }
        finally
        {
            _storageLock.Release();
        }
    }

    public async Task ClearAsync()
    {
        await _storageLock.WaitAsync();
        try
        {
            ExpireCookie(await GetStoredValueAsync(JwtCookieNameStorageKey));
            ExpireCookie(await GetStoredValueAsync(AccessCookieNameStorageKey));
            ExpireCookie(HttpCookieKeyEnum.JwtToken.GetKey());
            ExpireCookie(HttpCookieKeyEnum.AccessToken.GetKey());

            SecureStorage.Default.Remove(JwtCookieNameStorageKey);
            SecureStorage.Default.Remove(JwtCookieValueStorageKey);
            SecureStorage.Default.Remove(AccessCookieNameStorageKey);
            SecureStorage.Default.Remove(AccessCookieValueStorageKey);
            _isRestored = false;
        }
        catch (Exception e)
        {
            _logger.LogDebug(e, "Failed to clear auth cookies from secure storage");
        }
        finally
        {
            _storageLock.Release();
        }
    }

    private async Task RestoreAsync()
    {
        if (_isRestored)
        {
            return;
        }

        await _storageLock.WaitAsync();
        try
        {
            if (_isRestored)
            {
                return;
            }

            await RestoreCookieAsync(JwtCookieNameStorageKey, JwtCookieValueStorageKey);
            await RestoreCookieAsync(AccessCookieNameStorageKey, AccessCookieValueStorageKey);
            _isRestored = true;
        }
        catch (Exception e)
        {
            _logger.LogDebug(e, "Failed to restore auth cookies from secure storage");
        }
        finally
        {
            _storageLock.Release();
        }
    }

    private async Task RestoreCookieAsync(string nameStorageKey, string valueStorageKey)
    {
        var cookieName = await GetStoredValueAsync(nameStorageKey);
        var cookieValue = await GetStoredValueAsync(valueStorageKey);
        if (string.IsNullOrWhiteSpace(cookieName) || string.IsNullOrWhiteSpace(cookieValue))
        {
            return;
        }

        AddCookie(cookieName, cookieValue);
    }

    private async Task SyncCookieAsync(
        string baseCookieName,
        string[] changedCookieNames,
        string nameStorageKey,
        string valueStorageKey
    )
    {
        var cookie = FindCookie(baseCookieName, changedCookieNames);
        if (!string.IsNullOrWhiteSpace(cookie?.Value))
        {
            await SaveCookieAsync(cookie.Name, cookie.Value, nameStorageKey, valueStorageKey);
            return;
        }

        if (changedCookieNames.Any(cookieName => IsAuthCookieName(cookieName, baseCookieName)))
        {
            SecureStorage.Default.Remove(nameStorageKey);
            SecureStorage.Default.Remove(valueStorageKey);
        }
    }

    private async Task SaveCookieAsync(
        string cookieName,
        string cookieValue,
        string nameStorageKey,
        string valueStorageKey
    )
    {
        await SecureStorage.Default.SetAsync(nameStorageKey, cookieName);
        await SecureStorage.Default.SetAsync(valueStorageKey, cookieValue);
    }

    private Cookie? FindCookie(string baseCookieName, string[] changedCookieNames)
    {
        var cookies = _cookieContainer.GetCookies(_apiUri).Cast<Cookie>().ToArray();
        var changedCookieName = FindCookieName(changedCookieNames, baseCookieName);

        return !string.IsNullOrWhiteSpace(changedCookieName)
            ? cookies.FirstOrDefault(cookie => cookie.Name == changedCookieName)
            : cookies.FirstOrDefault(cookie => IsAuthCookieName(cookie.Name, baseCookieName));
    }

    private static string? FindCookieName(IEnumerable<string> cookieNames, string baseCookieName)
    {
        return cookieNames.FirstOrDefault(cookieName => IsAuthCookieName(cookieName, baseCookieName));
    }

    private static bool IsAuthCookieName(string cookieName, string baseCookieName)
    {
        return cookieName == baseCookieName
               || cookieName.StartsWith($"{baseCookieName}_", StringComparison.Ordinal);
    }

    private static IEnumerable<string> GetChangedAuthCookieNames(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
        {
            return [];
        }

        return setCookieHeaders
            .Select(header => header.Split('=', 2)[0])
            .Where(cookieName =>
                IsAuthCookieName(cookieName, HttpCookieKeyEnum.JwtToken.GetKey())
                || IsAuthCookieName(cookieName, HttpCookieKeyEnum.AccessToken.GetKey())
            );
    }

    private static string? GetHeaderValue(HttpResponseMessage response, string headerName)
    {
        return response.Headers.TryGetValues(headerName, out var values)
            ? values.FirstOrDefault()
            : null;
    }

    private void AddCookie(string cookieName, string cookieValue)
    {
        _cookieContainer.Add(
            _apiUri,
            new Cookie(cookieName, cookieValue, "/")
            {
                Secure = _apiUri.Scheme == Uri.UriSchemeHttps
            }
        );
    }

    private void ExpireCookie(string? cookieName)
    {
        if (string.IsNullOrWhiteSpace(cookieName))
        {
            return;
        }

        _cookieContainer.Add(
            _apiUri,
            new Cookie(cookieName, string.Empty, "/")
            {
                Expires = DateTime.UtcNow.AddDays(-1),
                Secure = _apiUri.Scheme == Uri.UriSchemeHttps
            }
        );
    }

    private static async Task<string?> GetStoredValueAsync(string key)
    {
        return await SecureStorage.Default.GetAsync(key);
    }
}
