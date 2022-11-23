using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Entities;

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
            clickUpSettings = new WorkspaceSettingsClickUpEntity();
            clickUpSettings.User = user;
            clickUpSettings.Workspace = workspace;
            workspace.SettingsClickUp.Add(clickUpSettings);
            clickUpSettings.CreateTime = DateTime.UtcNow;
        }
        clickUpSettings.UpdateTime = DateTime.UtcNow;
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
            settings = new WorkspaceSettingsRedmineEntity();
            settings.User = user;
            settings.Workspace = workspace;
            workspace.SettingsRedmine.Add(settings);
            settings.CreateTime = DateTime.UtcNow;
        }
        settings.UpdateTime = DateTime.UtcNow;
        settings.ApiKey = apiKey ?? "";
        settings.RedmineUserId = redmineUserId ?? 0;
        settings.Url = redmineUrl ?? "";
        settings.ActivityId = redmineActivityId ?? 0;
        await _sessionProvider.CurrentSession.SaveAsync(settings);
        
        return settings;
    }
}
