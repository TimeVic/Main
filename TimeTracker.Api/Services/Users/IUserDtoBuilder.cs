using Domain.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Api.Services.Users;

public interface IUserDtoBuilder : IDomainService
{
    Task<UserDto> BuildAsync(UserEntity user);
}
