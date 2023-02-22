using Persistence.Transactions.Behaviors;
using TimeTracker.Business.Common.Utils;
using TimeTracker.Business.Orm.Dto;
using TimeTracker.Business.Orm.Entities;

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
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow,
        };
        taskList.SetProject(project);
        await _sessionProvider.CurrentSession.SaveAsync(taskList);
        return taskList;
    }
    
    public async Task<TaskListEntity?> GetById(long taskListId)
    {
        return await _sessionProvider.CurrentSession.GetAsync<TaskListEntity>(taskListId);
    }

    public async Task<ListDto<TaskListEntity>> GetList(WorkspaceEntity workspace)
    {
        ProjectEntity projectAlias = null;
        var query = _sessionProvider.CurrentSession.QueryOver<TaskListEntity>()
            .Inner.JoinAlias(item => item.Project, () => projectAlias)
            .Where(() => projectAlias.Workspace.Id == workspace.Id);
        
        var items = await query
            .OrderBy(item => item.Name).Desc
            .ListAsync<TaskListEntity>();
        return new ListDto<TaskListEntity>(
            items,
            await query.RowCountAsync()
        );
    }
}
