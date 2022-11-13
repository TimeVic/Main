using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Dao.Integrations;
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
        await _sessionProvider.CurrentSession.SaveAsync(clickUpSettings);
        
        return clickUpSettings;
    }
}
