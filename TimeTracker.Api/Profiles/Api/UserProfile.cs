using AutoMapper;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Api.Shared.Dto.Entity.List;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Api.Profiles.Api;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserEntity, UserDto>()
            .IgnoreAllAndConstructUsing((src, mapper) =>
            {
                var latestAvatar = src.Avatars.OrderByDescending(a => a.CreatedAt).FirstOrDefault();
                return new UserDto
                {
                    Id = src.Id,
                    UserName = src.UserName,
                    Email = src.Email,
                    Login = src.Login,
                    Timezone = src.Timezone,
                    Language = mapper.Mapper.Map<LanguageDto>(src.Language),
                    Avatar = latestAvatar != null ? mapper.Mapper.Map<StoredFileDto>(latestAvatar) : null
                };
            });
    }
}
