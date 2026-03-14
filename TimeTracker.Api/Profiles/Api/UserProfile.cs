using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Api.Profiles.Api;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserEntity, UserDto>();
    }
}
