using Domain.Abstractions;

namespace TimeTracker.Business.Services.Http;

public interface IApiRequestService: IDomainService
{
    string GetApiToken();
    Guid GetUserIdFromJwt();
    
    string? GetRequestUrl();
}
