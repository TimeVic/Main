using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Extensions;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao.Workspace;

public class WorkspaceSettingsDao: IWorkspaceSettingsDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public WorkspaceSettingsDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<WorkspaceSettingsClickUpEntity> SetClickUpAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        string? securityKey,
        string? teamId,
        bool isCustomTaskIds,
        bool isFillTimeEntryWithTaskDetails = true
    )
    {
        var clickUpSettings = workspace.GetClickUpSettings(user);
        if (clickUpSettings == null)
        {
            clickUpSettings = new WorkspaceSettingsClickUpEntity
            {
                User = user,
                Workspace = workspace
            };
            workspace.SettingsClickUp.Add(clickUpSettings);
            clickUpSettings.CreatedAt = DateTime.UtcNow;
        }
        clickUpSettings.UpdatedAt = DateTime.UtcNow;
        clickUpSettings.SecurityKey = securityKey;
        clickUpSettings.TeamId = teamId;
        clickUpSettings.IsCustomTaskIds = isCustomTaskIds;
        clickUpSettings.IsFillTimeEntryWithTaskDetails = isFillTimeEntryWithTaskDetails;
        await _sessionProvider.CurrentSession.SaveAsync(clickUpSettings);
        
        return clickUpSettings;
    }
    
    public async Task<WorkspaceSettingsRedmineEntity> SetRedmineAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        string? redmineUrl,
        string? apiKey,
        long? redmineUserId,
        long? redmineActivityId
    )
    {
        var settings = workspace.GetRedmineSettings(user);
        if (settings == null)
        {
            settings = new WorkspaceSettingsRedmineEntity
            {
                User = user,
                Workspace = workspace,
                Url = string.Empty,
                ApiKey = string.Empty
            };
            workspace.SettingsRedmine.Add(settings);
            settings.CreatedAt = DateTime.UtcNow;
        }
        settings.UpdatedAt = DateTime.UtcNow;
        settings.ApiKey = apiKey ?? "";
        settings.RedmineUserId = redmineUserId ?? 0;
        settings.Url = redmineUrl ?? "";
        settings.ActivityId = redmineActivityId ?? 0;
        await _sessionProvider.CurrentSession.SaveAsync(settings);
        
        return settings;
    }
    
    public async Task<WorkspaceSettingsJiraEntity> SetJiraAsync(
        UserEntity user,
        WorkspaceEntity workspace,
        string? url,
        string? apiKey,
        string? userName,
        bool isFillTimeEntryWithTaskDetails = true
    )
    {
        var settings = workspace.GetJiraSettings(user);
        if (settings == null)
        {
            settings = new WorkspaceSettingsJiraEntity
            {
                User = user,
                Workspace = workspace
            };
            workspace.SettingsJira.Add(settings);
            settings.CreatedAt = DateTime.UtcNow;
        }
        settings.UpdatedAt = DateTime.UtcNow;
        settings.Url = url?.ToLower().RemoveTrailingSlash();
        settings.ApiKey = apiKey;
        settings.UserName = userName;
        settings.IsFillTimeEntryWithTaskDetails = isFillTimeEntryWithTaskDetails;
        await _sessionProvider.CurrentSession.SaveAsync(settings);
        
        return settings;
    }
}
