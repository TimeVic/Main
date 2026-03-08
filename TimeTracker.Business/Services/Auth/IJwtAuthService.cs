using Domain.Abstractions;

namespace TimeTracker.Business.Services.Auth
{
    public interface IJwtAuthService: IDomainService
    {
        public string BuildJwt(Guid userId);
        public Guid GetUserId(string jwtString);
        bool IsValidJwt(string jwtString);
    }
}
