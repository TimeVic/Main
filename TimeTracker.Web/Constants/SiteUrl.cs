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
    
    public static readonly string DashboardBase = "/board";
    public static readonly string Dashboard_TimeEntry = "/board";
    public static readonly string Dashboard_Projects = "/board/project";
    public static readonly string Dashboard_Project = "/board/project/{0}";
    public static readonly string Dashboard_Clients = "/board/client";
    public static readonly string Dashboard_Tags = "/board/tag";
    public static readonly string Dashboard_Payments = "/board/payment";
    public static readonly string Dashboard_Members = "/board/members";
    public static readonly string Dashboard_Integrations = "/board/integrations";
    
    public static readonly string Dashboard_Reports_Summary = "/board/report/summary";
    public static readonly string Dashboard_Reports_Payments = "/board/report/payments";
    public static readonly string Dashboard_Reports_TimeEntries = "/board/report/time-entries";
    
    public static readonly string Dashboard_Tasks_Default = "/board/tasks/0";
    public static readonly string Dashboard_Tasks = "/board/tasks/{0}/{1}";
    
    public static readonly string Workspace_List = "/board/workspaces";
}
