using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.FileStorage.Services.Api;

public interface IFileStorageSecurityService: IScopedDomainService
{
    Task<UserEntity> GetCurrentUser();
    
    Task CheckIsAuthenticated();
}
