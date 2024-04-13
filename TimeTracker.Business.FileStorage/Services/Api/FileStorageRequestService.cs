using Microsoft.AspNetCore.Http;
using TimeTracker.Business.Common.Exceptions.Common;
using TimeTracker.Business.FileStorage.Commons.Constants;

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
        if (headers == null || !headers.ContainsKey(HttpHeader.ApiKey) || string.IsNullOrEmpty(headers[HttpHeader.ApiKey]))
        {
            throw new DataValidationException($"{HttpHeader.ApiKey} header was not found");
        }
        return headers[HttpHeader.ApiKey]!;
    }

    public string GetApiSecret()
    {
        var headers = _httpContext.HttpContext?.Request.Headers;
        if (headers == null || !headers.ContainsKey(HttpHeader.ApiSecret) || string.IsNullOrEmpty(headers[HttpHeader.ApiSecret]))
        {
            throw new DataValidationException($"{HttpHeader.ApiSecret} header was not found");
        }
        return headers[HttpHeader.ApiSecret]!;
    }
}
