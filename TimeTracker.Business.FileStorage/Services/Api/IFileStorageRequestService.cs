using Domain.Abstractions;

namespace TimeTracker.Business.FileStorage.Services.Api;

public interface IFileStorageRequestService: IScopedDomainService
{
    string GetApiKey();
    
    string GetApiSecret();
}
