namespace TimeTracker.Client.Web.Constants;

public static class SiteUrl
{
    #region Localization
    public static readonly string UkLocalePrefix = "/uk";
    #endregion

    #region Public
    public static readonly string Main = "/";
    public static readonly string UseCases = "/use-cases";
    public static readonly string Faq = "/faq";
    public static readonly string Pricing = "/pricing";
    public static readonly string PrivacyPolicy = "/privacy-policy";
    public static readonly string TermsOfService = "/terms-of-service";
    
    public static readonly string Registration_Step1 = "/signup";
    public static readonly string Registration_Step2 = "/registration/step2";
        
    public static readonly string Login = "/login";
    public static readonly string LoginAsDemo = "/demo";
    public static readonly string LoginMagicVerify = "/login/magic/{0}";
    public static readonly string ForgotPassword = "/user/password-reset";
        
    public static readonly string Error500 = "/error/500";
    public static readonly string Error404 = "/error/404";
    public static readonly string Error403 = "/error/403";
    #endregion
    
    public static readonly string DashboardBase = "/board";
    public static readonly string Workspace_Change = "/board-change/{0}";
}
