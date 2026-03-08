using Domain.Abstractions;

namespace TimeTracker.Business.Services.Http;

public interface IRequestService: IDomainService
{
    string GetApiToken();
    Guid GetUserIdFromJwt();
}
