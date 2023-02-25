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
    public const string TimeEntryGetFilteredList = "dashboard/time-entry/filtered-list";
    public const string TimeEntryDelete = "dashboard/time-entry/delete";
    
    #endregion
    
    #region Project
    
    public const string ProjectAdd = "dashboard/project/add";
    public const string ProjectUpdate = "dashboard/project/update";
    public const string ProjectList = "dashboard/project/list";
    
    #endregion
    
    #region Client
    
    public const string ClientAdd = "dashboard/client/add";
    public const string ClientList = "dashboard/client/list";
    
    #endregion
    
    #region Payment
    
    public const string PaymentAdd = "dashboard/payment/add";
    public const string PaymentUpdate = "dashboard/payment/payment";
    public const string PaymentDelete = "dashboard/payment/delete";
    public const string PaymentList = "dashboard/payment/list";
    
    #endregion
    
    #region Report
    
    public const string ReportPayments = "dashboard/report/payments";

    public const string ReportSummary = "dashboard/report/summary";

    #endregion

    #region Workspace Membership
    
    public const string WorkspaceMembershipAdd = "dashboard/workspace/membership/add";
    public const string WorkspaceMembershipUpdate = "dashboard/workspace/membership/update";
    public const string WorkspaceMembershipDelete = "dashboard/workspace/membership/delete";
    public const string WorkspaceMembershipList = "dashboard/workspace/membership/list";
    
    #endregion
    
    #region Workspace
    
    public const string WorkspaceList = "dashboard/workspace/list";
    public const string WorkspaceAdd = "dashboard/workspace/add";
    public const string WorkspaceUpdate = "dashboard/workspace/update";
    
    #endregion
    
    #region Workspace Integrations
    
    public const string WorkspaceIntegrationSettingsGet = "dashboard/workspace/settings/integrations/get";
    public const string WorkspaceIntegrationSettingsRedmineSet = "dashboard/workspace/settings/set-redmine";
    public const string WorkspaceIntegrationSettingsClickUpSet = "dashboard/workspace/settings/set-clickup";
    
    #endregion
    
    #region Tasks list
    
    public const string TaskListAdd = "dashboard/tasks/list/add";
    public const string TaskListUpdate = "dashboard/tasks/list/update";
    public const string TaskListArchive = "dashboard/tasks/list/archive";
    public const string TaskListList = "dashboard/tasks/list/get-list";
    
    #endregion
    
    #region Tasks
    
    public const string TasksAdd = "dashboard/tasks/add";
    public const string TasksUpdate = "dashboard/tasks/update";
    public const string TasksList = "dashboard/tasks/get-list";
    public const string TasksGetOne = "dashboard/tasks/get-one";
    
    #endregion

    #region Storage

    public const string StorageUpload = "dashboard/storage/upload";
    public const string StorageDelete = "dashboard/storage/delete";

    #endregion
    
    #region Tag
    
    public const string TagAdd = "dashboard/tag/add";
    public const string TagUpdate = "dashboard/tag/update";
    public const string TagDelete = "dashboard/tag/delete";
    public const string TagList = "dashboard/tag/list";
    
    #endregion
}
