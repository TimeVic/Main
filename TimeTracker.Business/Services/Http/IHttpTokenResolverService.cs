using Domain.Abstractions;

namespace TimeTracker.Business.Services.Http;

public interface IHttpTokenResolverService: IScopedDomainService
{
    string? GetApiToken();
    
    string? GetAccessToken();
}
