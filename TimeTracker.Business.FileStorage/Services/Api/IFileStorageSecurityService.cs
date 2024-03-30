using Domain.Abstractions;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.FileStorage.Services.Api;

public interface IFileStorageSecurityService: IDomainService
{
    public UserEntity GetCurrentUser();
    
    public Task CheckIsAuthenticated();
}
