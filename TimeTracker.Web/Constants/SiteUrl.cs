namespace TimeTracker.Web.Constants;

public static class SiteUrl
{
    #region Public
    public static readonly string Main = "/";
    
    public static readonly string Registration_Step1 = "/registration/step1";
    public static readonly string Registration_Step2 = "/registration/step2";
        
    public static readonly string Login = "/login";
        
    public static readonly string Error500 = "/error/500";
    public static readonly string Error404 = "/error/404";
    #endregion
    
    public static readonly string DashboardBase = "/dashboard";
    public static readonly string Dashboard_TimeEntry = "/dashboard/time-entry";
    public static readonly string Dashboard_Projects = "/dashboard/project";
    public static readonly string Dashboard_Project = "/dashboard/project/{0}";
    public static readonly string Dashboard_Clients = "/dashboard/client";
    public static readonly string Dashboard_Payments = "/dashboard/payment";
    
    public static readonly string Dashboard_Reports_Payments = "/dashboard/report/payments";
    public static readonly string Dashboard_Reports_TimeEntries = "/dashboard/report/time-entries";
}
