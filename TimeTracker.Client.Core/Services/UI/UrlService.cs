using Fluxor;
using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants.Storage;
using TimeTracker.Client.Core.Store.Auth;

namespace TimeTracker.Client.Core.Services.UI;

public class UrlService
{
    private readonly NavigationManager _navigationManager;
    private readonly IState<AuthState> _authState;

    private readonly string _apiUrl;
    private readonly string _baseUrl;

    public UrlService(
        IConfiguration configuration,
        NavigationManager navigationManager,
        IState<AuthState> authState
    )
    {
        _navigationManager = navigationManager;
        _authState = authState;

        _apiUrl = (configuration.GetValue<string>("ApiUrl") ?? string.Empty).TrimEnd('/');
        _baseUrl = (configuration.GetValue<string>("BaseUrl") ?? _navigationManager.BaseUri).TrimEnd('/');
    }

    public string ToAbsoluteUrl(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || relativePath == "/")
        {
            return $"{_baseUrl}/";
        }

        relativePath = relativePath.StartsWith("/") ? relativePath : $"/{relativePath}";
        return $"{_baseUrl}{relativePath}";
    }

    public string GetStorageUrl(string url)
    {
        return ToApiAbsoluteUrl(url);
    }

    public string GetStorageFileUrl(StoredFileDto file)
    {
        return GetStorageFileUrl(file.Id);
    }

    public string GetStorageFileUrl(Guid fileId)
    {
        return ToApiAbsoluteUrl($"dashboard/storage/file/{fileId}");
    }

    public string GetStorageImageUrl(StoredFileDto file, StorageImageSize imageSize)
    {
        return GetStorageImageUrl(file.Id, imageSize);
    }

    public string GetStorageImageUrl(Guid fileId, StorageImageSize imageSize)
    {
        return ToApiAbsoluteUrl($"dashboard/storage/file/{fileId}?imageSize={imageSize}");
    }

    private string ToApiAbsoluteUrl(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return _apiUrl;
        }

        if (Uri.TryCreate(relativePath, UriKind.Absolute, out var absoluteUri))
        {
            if (absoluteUri.Scheme is "http" or "https")
            {
                return absoluteUri.ToString();
            }

            relativePath = absoluteUri.PathAndQuery;
        }

        relativePath = relativePath.TrimStart('/');
        return $"{_apiUrl}/{relativePath}";
    }
    
    public void NavigateToChangeWorkspace(Guid workspaceId, string subUrl)
    {
        subUrl = subUrl.StartsWith("/") ? subUrl : $"/{subUrl}";
        _navigationManager.NavigateTo($"/board-change/{workspaceId}{subUrl}", replace: true);
    }

    public string GetDashboardUrl(string path = "", Guid? workspaceId = null)
    {
        var normalizedPath = path.Trim('/');
        var selectedWorkspaceId = workspaceId is { } id && id != Guid.Empty
            ? id
            : GetWorkspaceIdFromDashboardUrl() ?? _authState.Value.Workspace?.Id;
        if (!selectedWorkspaceId.HasValue)
        {
            return "/error/403";
        }

        return string.IsNullOrEmpty(normalizedPath)
            ? $"/board/{selectedWorkspaceId}"
            : $"/board/{selectedWorkspaceId}/{normalizedPath}";
    }

    public Guid? GetWorkspaceIdFromDashboardUrl(string? url = null)
    {
        var path = new Uri(url ?? _navigationManager.Uri).AbsolutePath.Trim('/');
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var boardSegmentIndex = Array.FindIndex(segments, segment => segment.Equals("board", StringComparison.OrdinalIgnoreCase));
        if (boardSegmentIndex < 0 || segments.Length <= boardSegmentIndex + 1)
        {
            return null;
        }

        return Guid.TryParse(segments[boardSegmentIndex + 1], out var workspaceId)
            ? workspaceId
            : null;
    }

    public string GetDashboardUrlForCurrentPath(Guid workspaceId)
    {
        var currentUri = new Uri(_navigationManager.Uri);
        var segments = currentUri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var boardSegmentIndex = Array.FindIndex(segments, segment => segment.Equals("board", StringComparison.OrdinalIgnoreCase));
        if (boardSegmentIndex < 0)
        {
            if (segments.Length == 2
                && segments[0].Equals("dashboard", StringComparison.OrdinalIgnoreCase)
                && segments[1].Equals("notes", StringComparison.OrdinalIgnoreCase))
            {
                return $"{GetDashboardUrl("notes", workspaceId)}{currentUri.Query}";
            }

            return GetDashboardUrl(workspaceId: workspaceId);
        }

        var pathStartIndex = boardSegmentIndex + 1;
        if (segments.Length > pathStartIndex && Guid.TryParse(segments[pathStartIndex], out _))
        {
            pathStartIndex++;
        }

        var path = string.Join('/', segments.Skip(pathStartIndex));
        return $"{GetDashboardUrl(path, workspaceId)}{currentUri.Query}";
    }
}
