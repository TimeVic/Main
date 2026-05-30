using Domain.Abstractions;

namespace TimeTracker.Business.Services.Auth.SocialLogin;

public interface ISocialLoginProviderService: IDomainService
{
    public ISocialLoginProvider Provide(SocialLoginProviderTypeEnum providerType);
}
