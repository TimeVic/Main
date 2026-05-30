using TimeTracker.Business.Services.Auth.SocialLogin.Providers;

namespace TimeTracker.Business.Services.Auth.SocialLogin;

public class SocialLoginProviderService: ISocialLoginProviderService
{
    private readonly IGoogleSocialLoginProviderService _googleSocialLoginProviderService;
    private readonly IFacebookSocialLoginProviderService _facebookSocialLoginProviderService;
    private readonly IAppleSocialLoginProviderService _appleSocialLoginProviderService;

    public SocialLoginProviderService(
        IGoogleSocialLoginProviderService googleSocialLoginProviderService,
        IFacebookSocialLoginProviderService facebookSocialLoginProviderService,
        IAppleSocialLoginProviderService appleSocialLoginProviderService
    )
    {
        _googleSocialLoginProviderService = googleSocialLoginProviderService;
        _facebookSocialLoginProviderService = facebookSocialLoginProviderService;
        _appleSocialLoginProviderService = appleSocialLoginProviderService;
    }
    
    public ISocialLoginProvider Provide(SocialLoginProviderTypeEnum providerType)
    {
        if (providerType == SocialLoginProviderTypeEnum.Google)
            return _googleSocialLoginProviderService;
        if (providerType == SocialLoginProviderTypeEnum.Facebook)
            return _facebookSocialLoginProviderService;
        if (providerType == SocialLoginProviderTypeEnum.Apple)
            return _appleSocialLoginProviderService;
        else
        {
            throw new NotImplementedException("Such social login provider not found");
        }
    }
}
