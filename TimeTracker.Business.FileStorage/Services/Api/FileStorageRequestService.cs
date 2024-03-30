using Microsoft.AspNetCore.Http;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.FileStorage.Services.Api;

public class FileStorageRequestService: IFileStorageRequestService
{
    private const string HeaderApiKey = "x-tmv-storage-key";
    private const string HeaderApiSecret = "x-tmv-storage-secret";
    
    private readonly IHttpContextAccessor _httpContext;
    
    public FileStorageRequestService(
        IHttpContextAccessor httpContext
    )
    {
        _httpContext = httpContext;
    }

    public string GetApiKey()
    {
        var headers = _httpContext.HttpContext?.Request.Headers;
        if (headers == null || !headers.ContainsKey(HeaderApiKey) || string.IsNullOrEmpty(headers[HeaderApiKey]))
        {
            throw new DataValidationException($"{HeaderApiKey} header was not found");
        }
        return headers[HeaderApiKey]!;
    }

    public string GetApiSecret()
    {
        var headers = _httpContext.HttpContext?.Request.Headers;
        if (headers == null || !headers.ContainsKey(HeaderApiSecret) || string.IsNullOrEmpty(headers[HeaderApiSecret]))
        {
            throw new DataValidationException($"{HeaderApiSecret} header was not found");
        }
        return headers[HeaderApiSecret]!;
    }
}
