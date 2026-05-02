namespace TimeTracker.Web.Constants;

public static class SiteUrl
{
    #region Public
    public static readonly string Main = "/";
    public static readonly string UseCases = "/use-cases";
    public static readonly string Faq = "/faq";
    public static readonly string Pricing = "/pricing";
    
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
    public static readonly string Dashboard_Dashboard = "/board/dashboard";
    public static readonly string Dashboard_Calendar = "/board/calendar";
    public static readonly string Dashboard_TimeEntry = "/board";
    public static readonly string Dashboard_OverdueTasks = "/board/overdue-tasks";
    public static readonly string Dashboard_GoalsTracker = "/board/goals-tracker";
    public static readonly string Dashboard_Projects = "/board/project";
    public static readonly string Dashboard_Project = "/board/project/{0}";
    public static readonly string Dashboard_Clients = "/board/client";
    public static readonly string Dashboard_Tags = "/board/tag";
    public static readonly string Dashboard_Payments = "/board/payment";
    public static readonly string Dashboard_Members = "/board/members";
    public static readonly string Dashboard_Integrations = "/board/integrations";
    public static readonly string Dashboard_Workspace_Settings = "/board/workspace/settings";
    public static readonly string Dashboard_Emoji = "/board/emoji";
    
    public static readonly string Dashboard_Reports_Summary = "/board/report/summary";
    public static readonly string Dashboard_Reports_Payments = "/board/report/payments";
    public static readonly string Dashboard_Reports_TimeEntries = "/board/report/time-entries";
    
    public static readonly string Dashboard_Tasks_Default = "/board/tasks/0";
    public static readonly string Dashboard_Tasks_Main = "/board/tasks";
    public static readonly string Dashboard_Tasks = "/board/tasks/{0}";
    public static readonly string Dashboard_Task = "/board/task/{0}";
    
    public static readonly string Workspace_List = "/board/workspaces";
    public static readonly string Workspace_Change = "/board-change/{0}";
}
