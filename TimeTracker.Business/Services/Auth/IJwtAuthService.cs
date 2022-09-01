using Domain.Abstractions;

namespace TimeTracker.Business.Services.Auth
{
    public interface IJwtAuthService: IDomainService
    {
        public string BuildJwt(long userId);
        public long GetUserId(string jwtString);
        bool IsValidJwt(string jwtString);
    }
}