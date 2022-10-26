using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Services.Security;

namespace TimeTracker.Business.Services.Entity;

public class ProjectService: IProjectService
{
    private readonly IWorkspaceAccessService _workspaceAccessService;

    public ProjectService(IWorkspaceAccessService workspaceAccessService)
    {
        _workspaceAccessService = workspaceAccessService;
    }

    public async Task<decimal?> GetUsersHourlyRateForProject(ProjectEntity project, UserEntity user)
    {
        if (project.Workspace.IsOwner(user))
        {
            return project.DefaultHourlyRate;
        }
        var membership = await _workspaceAccessService.GetMembershipAsync(
            user, 
            project.Workspace
        );
        var projectAccessItem = membership?.ProjectAccesses.FirstOrDefault(
            item => item.Project.Id == project.Id
        );
        if (projectAccessItem == null || projectAccessItem.HourlyRate == null)
        {
            return project.DefaultHourlyRate;
        }
        return projectAccessItem.HourlyRate;
    }
}
