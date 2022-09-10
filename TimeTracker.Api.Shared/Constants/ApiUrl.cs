namespace TimeTracker.Api.Shared.Constants;

public class ApiUrl
{
    #region Public
    public const string Login = "user/login";
    public const string UserCheckIsLoggedIn = "user/check-is-logged-in";
    public const string RegistrationStep1 = "user/registration/step1";
    public const string RegistrationStep2 = "user/registration/step2";
    #endregion
    
    #region Time Entry
    
    public const string TimeEntryStart = "dashboard/time-entry/start";
    public const string TimeEntryStop = "dashboard/time-entry/stop";
    public const string TimeEntrySet = "dashboard/time-entry/set";
    public const string TimeEntryGetList = "dashboard/time-entry/list";
    public const string TimeEntryDelete = "dashboard/time-entry/delete";
    
    #endregion
    
    #region Project
    
    public const string ProjectAdd = "dashboard/project/add";
    public const string ProjectList = "dashboard/project/list";
    
    #endregion
}
