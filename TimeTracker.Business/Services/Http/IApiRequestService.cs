using Domain.Abstractions;
using Domain.Abstractions.Api;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Services.Http;

public interface IApiRequestService: IBaseApiRequestService
{
    Task<UserEntity> GetCurrentUser();

    Task<UserEntity?> GetCurrentUserOrNull();

    Guid? GetCurrentWorkspaceId();
}
