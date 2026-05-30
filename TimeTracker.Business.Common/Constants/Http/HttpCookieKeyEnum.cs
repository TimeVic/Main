namespace TimeTracker.Business.Common.Constants.Http;

public enum HttpCookieKeyEnum
{
    /// <summary>
    /// JWT token cookie parameter key
    /// </summary>
    JwtToken = 1,
    
    /// <summary>
    /// JWT token cookie parameter key
    /// </summary>
    AccessToken,
    
    /// <summary>
    /// Social Login State key
    /// </summary>
    SocialLoginState,
    
    /// <summary>
    /// Social Login Return Url
    /// </summary>
    SocialLoginReturnUrl,
    
    /// <summary>
    /// Social Login Return Url
    /// </summary>
    SocialLoginRegistrationReturnUrl,
    
    /// <summary>
    /// Social Login Error Return Url
    /// </summary>
    SocialLoginErrorReturnUrl,
    
    /// <summary>
    /// Social Login GMT Referer
    /// </summary>
    SocialLoginReferer,
    
    /// <summary>
    /// Social Login Referral Id
    /// </summary>
    SocialLoginReferralId,
}

public static class CookieKeyEnumExtensions
{
    public static string GetKey(this HttpCookieKeyEnum type)
    {
        switch (type)
        {
            case HttpCookieKeyEnum.JwtToken:
                return "tv_jwt_token";
            case HttpCookieKeyEnum.AccessToken:
                return "tv_access_token";
            case HttpCookieKeyEnum.SocialLoginState:
                return "tv_social_login_state";
            case HttpCookieKeyEnum.SocialLoginReturnUrl:
                return "tv_social_login_return_url";
            case HttpCookieKeyEnum.SocialLoginRegistrationReturnUrl:
                return "tv_social_login_registration_return_url";
            case HttpCookieKeyEnum.SocialLoginErrorReturnUrl:
                return "tv_social_login_error_return_url";
            case HttpCookieKeyEnum.SocialLoginReferer:
                return "tv_social_login_referrer";
            case HttpCookieKeyEnum.SocialLoginReferralId:
                return "tv_social_login_referral_id";
        }

        throw new NotImplementedException($"CookieKey was not provided for such type: {type}");
    }
}
