using Microsoft.AspNetCore.Http;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.FileStorage.Constants;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.FileStorage.Services.Api;

public class FileStorageRequestService: IFileStorageRequestService
{   
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
        if (headers == null || !headers.ContainsKey(HttpConstants.HeaderApiKey) || string.IsNullOrEmpty(headers[HttpConstants.HeaderApiKey]))
        {
            throw new DataValidationException($"{HttpConstants.HeaderApiKey} header was not found");
        }
        return headers[HttpConstants.HeaderApiKey]!;
    }

    public string GetApiSecret()
    {
        var headers = _httpContext.HttpContext?.Request.Headers;
        if (headers == null || !headers.ContainsKey(HttpConstants.HeaderApiSecret) || string.IsNullOrEmpty(headers[HttpConstants.HeaderApiSecret]))
        {
            throw new DataValidationException($"{HttpConstants.HeaderApiSecret} header was not found");
        }
        return headers[HttpConstants.HeaderApiSecret]!;
    }
}
