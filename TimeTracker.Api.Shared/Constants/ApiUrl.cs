namespace TimeTracker.Api.Shared.Constants;

public class ApiUrl
{
    #region Public
    public const string Login = "user/login";
    public const string LoginAsDemo = "user/login/as-demo";
    public const string LoginMagic = "user/login/magic";
    public const string LoginMagicVerify = "user/login/magic/verify";
    public const string RefreshToken = "user/refresh-token";
    public const string UserCheckIsLoggedIn = "user/check-is-logged-in";
    public const string RegistrationStep1 = "user/registration/step1";
    public const string RegistrationStep2 = "user/registration/step2";
    public const string ResetPasswordStep1 = "user/password/reset";
    public const string ResetPasswordStep2 = "user/password/change";
    public const string Logout = "user/logout";
    public const string UserCurrent = "dashboard/user/current";
    public const string SetNotificationToken = "dashboard/user/set-notification-token";
    public const string UserSelectWorkspace = "dashboard/user/select-workspace";
    public const string UserUpdateSettings = "dashboard/user/update-settings";
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
    public const string ProjectDelete = "dashboard/project/delete";
    public const string ProjectList = "dashboard/project/list";
    
    #endregion
    
    #region Client
    
    public const string ClientAdd = "dashboard/client/add";
    public const string ClientUpdate = "dashboard/client/update";
    public const string ClientList = "dashboard/client/list";
    
    #endregion
    
    #region MemberPayment
    
    public const string MemberPaymentAdd = "dashboard/member-payments/add";
    public const string MemberPaymentUpdate = "dashboard/member-payments/update";
    public const string MemberPaymentDelete = "dashboard/member-payments/delete";
    public const string MemberPaymentList = "dashboard/member-payments/list";
    
    #endregion

    #region ClientPayment

    public const string ClientPaymentAdd = "dashboard/client-payments/add";
    public const string ClientPaymentUpdate = "dashboard/client-payments/update";
    public const string ClientPaymentDelete = "dashboard/client-payments/delete";
    public const string ClientPaymentList = "dashboard/client-payments/list";

    #endregion
    
    #region Report
    
    public const string ReportMemberPayments = "dashboard/report/member-payments";

    public const string ReportSummary = "dashboard/report/summary";

    public const string ReportWorkspaceFinancialSummary = "dashboard/report/workspace-financial-summary";

    #endregion

    #region Workspace Member
    
    public const string WorkspaceMemberAdd = "dashboard/workspace/member/add";
    public const string WorkspaceMemberUpdate = "dashboard/workspace/member/update";
    public const string WorkspaceMemberDelete = "dashboard/workspace/member/delete";
    public const string WorkspaceMemberList = "dashboard/workspace/member/list";
    
    #endregion
    
    #region Workspace
    
    public const string WorkspaceList = "dashboard/workspace/list";
    public const string WorkspaceAdd = "dashboard/workspace/add";
    public const string WorkspaceUpdate = "dashboard/workspace/update";
    
    #endregion

    #region Security

    public const string WorkspacePermissions = "dashboard/security/permissions/workspace";

    #endregion
    
    #region List
    
    public const string ListCurrencyList = "dashboard/list/currency/list";
    
    #endregion
    
    #region Workspace Integrations
    
    public const string WorkspaceIntegrationSettingsGet = "dashboard/workspace/settings/integrations/get";
    public const string WorkspaceIntegrationSettingsRedmineSet = "dashboard/workspace/settings/set-redmine";
    public const string WorkspaceIntegrationSettingsClickUpSet = "dashboard/workspace/settings/set-clickup";
    public const string WorkspaceIntegrationSettingsJiraSet = "dashboard/workspace/settings/set-jira";
    
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
    public const string TasksUpdatePositions = "dashboard/tasks/update-positions";
    public const string TasksList = "dashboard/tasks/get-list";
    public const string TasksGet = "dashboard/tasks/get";
    public const string TasksListForCalendar = "dashboard/tasks/get-for-calendar";
    public const string TasksMyList = "dashboard/tasks/get-my-list";
    public const string TasksGetOne = "dashboard/tasks/get-one";
    public const string TaskCommentAdd = "dashboard/tasks/comment/add";
    public const string TaskCommentUpdate = "dashboard/tasks/comment/update";
    public const string TaskCommentDelete = "dashboard/tasks/comment/delete";
    public const string TaskCommentsList = "dashboard/tasks/comment/get-list";
    
    #endregion

    #region Notes

    public const string NotesGetTree = "dashboard/notes/get-tree";
    public const string NotesGetDocument = "dashboard/notes/get-document";
    public const string NotesGetContent = "dashboard/notes/get-content";
    public const string NotesCreateFolder = "dashboard/notes/create-folder";
    public const string NotesCreateDocument = "dashboard/notes/create-document";
    public const string NotesUpdateDocument = "dashboard/notes/update-document";
    public const string NotesUpdateContent = "dashboard/notes/update-content";
    public const string NotesRenameNode = "dashboard/notes/rename-node";
    public const string NotesMoveNode = "dashboard/notes/move-node";
    public const string NotesArchiveNode = "dashboard/notes/archive-node";
    public const string NotesGetLinkedNotes = "dashboard/notes/get-linked-notes";
    public const string NotesCreateLink = "dashboard/notes/create-link";
    public const string NotesDeleteLink = "dashboard/notes/delete-link";

    #endregion

    #region Storage

    public const string StorageUpload = "dashboard/storage/upload";
    public const string StorageDelete = "dashboard/storage/delete";
    public const string StorageList = "dashboard/storage/list";

    #endregion
    
    #region Tag
    
    public const string TagAdd = "dashboard/tag/add";
    public const string TagUpdate = "dashboard/tag/update";
    public const string TagDelete = "dashboard/tag/delete";
    public const string TagList = "dashboard/tag/list";
    
    #endregion
    
    #region Goals Tracker

    public const string GoalsTrackerGet = "dashboard/goals-tracker/get";
    public const string GoalsTrackerChangePositions = "dashboard/goals-tracker/change-positions";
    public const string GoalsTrackerItemCreate = "dashboard/goals-tracker/item/create";
    public const string GoalsTrackerItemUpdate = "dashboard/goals-tracker/item/update";
    public const string GoalsTrackerItemDelete = "dashboard/goals-tracker/item/delete";
    public const string GoalsTrackerItemSetCompletion = "dashboard/goals-tracker/item/set-completion";
    
    #endregion
    
    #region Workspace
    
    public const string NotificationCenterGetCount = "dashboard/notifications-center/get-count";
    public const string NotificationCenterGetList = "dashboard/notifications-center/get-list";
    public const string NotificationCenterMarkAllAsRead = "dashboard/notifications-center/mark-all-as-read";
    public const string NotificationCenterMarkAsRead = "dashboard/notifications-center/mark-as-read";
    
    #endregion
    
    #region Messaging
    
    public const string MessagingChannelInit = "messaging/channel/init";
    public const string MessagingChannelCreate = "messaging/channel/create";
    public const string MessagingChannelGetList = "messaging/channel/get-list";
    
    public const string MessagingMessageSend = "messaging/message/send";
    public const string MessagingMessageGetList = "messaging/message/get-list";
    
    #endregion
}
