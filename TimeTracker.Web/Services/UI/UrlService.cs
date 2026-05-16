using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Constants.Storage;

namespace TimeTracker.Web.Services.UI;

public class UrlService
{
    private readonly NavigationManager _navigationManager;

    private readonly string _apiUrl;
    private readonly string _baseUrl;

    public UrlService(
        IConfiguration configuration,
        NavigationManager navigationManager
    )
    {
        _navigationManager = navigationManager;

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
}
