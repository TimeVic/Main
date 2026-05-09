using NHibernate.Criterion;
using NHibernate.Linq;
using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Constants;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;
using TimeTracker.Business.Orm.Entities.Tasks;
using TimeTracker.Business.Orm.Entities.User;
using TimeTracker.Business.Orm.Entities.WorkspaceAccess;
using TimeTracker.Business.Orm.Entities.Workspaces;

namespace TimeTracker.Business.Orm.Dao.Tasks;

public class TaskListDao: ITaskListDao
{
    private readonly IDbSessionProvider _sessionProvider;

    public TaskListDao(IDbSessionProvider sessionProvider)
    {
        _sessionProvider = sessionProvider;
    }

    public async Task<TaskListEntity> CreateTaskListAsync(ProjectEntity project, string name)
    {
        var taskList = new TaskListEntity()
        {
            Name = name,
            Project = project,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        taskList.SetProject(project);
        await _sessionProvider.CurrentSession.SaveAsync(taskList);
        return taskList;
    }
    
    public async Task<TaskListEntity?> GetById(Guid taskListId)
    {
        return await _sessionProvider.CurrentSession.GetAsync<TaskListEntity>(taskListId);
    }

    public async Task<ListDto<TaskListEntity>> GetList(WorkspaceEntity workspace)
    {
        ProjectEntity projectAlias = null!;
        ClientEntity clientAlias = null!;
        WorkspaceEntity workspaceAlias = null!;
        var query = _sessionProvider.CurrentSession.QueryOver<TaskListEntity>()
            .Inner.JoinAlias(item => item.Project, () => projectAlias)
            .Inner.JoinAlias(() => projectAlias!.Client, () => clientAlias)
            .Inner.JoinAlias(() => clientAlias!.Workspace, () => workspaceAlias)
            .Where(() => workspaceAlias!.Id == workspace.Id)
            .Where(taskList => taskList.IsArchived == false);
        
        var items = await query
            .OrderBy(item => item.Name).Desc
            .ListAsync<TaskListEntity>();
        return new ListDto<TaskListEntity>(
            items,
            await query.RowCountAsync()
        );
    }

    public async Task<ListDto<TaskListEntity>> GetAvailableForUserListAsync(
        WorkspaceEntity workspace,
        UserEntity? user = null,
        MembershipAccessType? accessType = null,
        Guid? projectId = null
    )
    {
        ProjectEntity projectAlias = null!;
        ClientEntity clientAlias = null!;
        WorkspaceEntity workspaceAlias = null!;
        var query = _sessionProvider.CurrentSession.QueryOver<TaskListEntity>()
            .Select(
                Projections.Group<TaskListEntity>(item => item.Id)
            )
            .Inner.JoinAlias(item => item.Project, () => projectAlias)
            .Inner.JoinAlias(() => projectAlias!.Client, () => clientAlias)
            .Inner.JoinAlias(() => clientAlias!.Workspace, () => workspaceAlias)
            .Where(() => workspaceAlias!.Id == workspace.Id)
            .Where(() => !projectAlias!.IsArchived)
            .Where(taskList => !taskList.IsArchived);

        if (projectId.HasValue)
        {
            query = query.Where(() => projectAlias!.Id == projectId.Value);
        }

        if (
            user != null
            && accessType != MembershipAccessType.Manager
            && accessType != MembershipAccessType.Owner
        )
        {
            WorkspaceMemberProjectAccessEntity projectAccessAlias = null!;
            WorkspaceMemberEntity workspaceMemberAlias = null!;
            UserEntity userAlias = null!;
            query = query.Inner.JoinAlias(() => projectAlias!.MemberProjectAccess, () => projectAccessAlias)
                .Inner.JoinAlias(() => projectAccessAlias!.WorkspaceMember, () => workspaceMemberAlias)
                .Inner.JoinAlias(() => workspaceMemberAlias!.User, () => userAlias)
                .And(() => userAlias!.Id == user.Id);
        }

        var taskListIds = await query.ListAsync<Guid>();
        if (!taskListIds.Any())
        {
            return new ListDto<TaskListEntity>(new List<TaskListEntity>(), 0);
        }

        var taskLists = await _sessionProvider.CurrentSession.Query<TaskListEntity>()
            .Fetch(item => item.Project)
            .ThenFetch(project => project.Client)
            .Where(item => taskListIds.Contains(item.Id))
            .OrderByDescending(item => item.Name)
            .ToListAsync();

        return new ListDto<TaskListEntity>(taskLists, taskLists.Count);
    }
    
    public async Task ArchiveTaskListAsync(TaskListEntity taskList)
    {
        taskList.IsArchived = true;
        await _sessionProvider.CurrentSession.SaveAsync(taskList);
    }
}
