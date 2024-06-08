using TimeTracker.Business.Orm.Entities.User;

namespace TimeTracker.Business.Dto.Auth;

public record AuthResultDto(
    string JwtToken,
    string AccessToken,
    UserEntity User
);
