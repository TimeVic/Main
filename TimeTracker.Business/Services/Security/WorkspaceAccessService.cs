using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Orm.Constants;
using TimeTracker.Business.Orm.Dao;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Services.Auth;

namespace TimeTracker.Business.Services.Security;

public class WorkspaceAccessService: IWorkspaceAccessService
{
    private readonly IDbSessionProvider _sessionProvider;
    private readonly IRegistrationService _registrationService;
    private readonly IUserDao _userDao;

    public WorkspaceAccessService(
        IDbSessionProvider sessionProvider,
        IRegistrationService registrationService,
        IUserDao userDao
    )
    {
        _sessionProvider = sessionProvider;
        _registrationService = registrationService;
        _userDao = userDao;
    }

    public async Task<WorkspaceMembershipEntity> ShareAccessAsync(
        WorkspaceEntity workspace,
        string email,
        MembershipAccessType access,
        ICollection<ProjectEntity>? projects = null
    )
    {
        var user = await _userDao.GetByEmail(email);
        if (user is not { IsActivated: true })
        {
            user = await _registrationService.CreatePendingUser(email);
        }

        return await ShareAccessAsync(workspace, user, access, projects);
    }

    public async Task<WorkspaceMembershipEntity> ShareAccessAsync(
        WorkspaceEntity workspace,
        UserEntity user,
        MembershipAccessType access,
        ICollection<ProjectEntity>? projects = null
    )
    {
        var membership = workspace.Memberships.FirstOrDefault(item => item.User.Id == user.Id);
        if (membership == null)
        {
            membership = new WorkspaceMembershipEntity()
            {
                User = user,
                Workspace = workspace,
                CreateTime = DateTime.UtcNow
            };
            workspace.Memberships.Add(membership);
        }
        membership.UpdateTime = DateTime.UtcNow;
        membership.Access = access;

        projects ??= new List<ProjectEntity>();
        membership.ProjectAccesses.Clear();
        if (projects.Any() && membership.Access != MembershipAccessType.Manager)
        {
            foreach (var project in projects)
            {
                var projectAccess = new WorkspaceMembershipProjectAccessEntity()
                {
                    Project = project,
                    CreateTime = DateTime.UtcNow,
                    UpdateTime = DateTime.UtcNow,
                    WorkspaceMembership = membership
                };
                membership.ProjectAccesses.Add(projectAccess);
            }
        }
        await _sessionProvider.CurrentSession.SaveAsync(membership);
        return membership;
    }
}
